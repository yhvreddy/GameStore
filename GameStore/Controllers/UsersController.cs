using System.IdentityModel.Tokens.Jwt;
using GameStore.Dtos;
using GameStore.Extensions;
using GameStore.Interfaces;
using GameStore.Mapping;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Route("users")]
[Tags("Users")]
public class UsersController(
    IUserRepository users,
    PasswordService passwordService,
    JwtTokenService jwtTokenService,
    ILogService logService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var userDtos = (await users.GetAllAsync(cancellationToken))
            .Select(user => user.ToDto())
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<UserDto>>.Ok(userDtos, "Users retrieved successfully."));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegisterUserDto registerUser, CancellationToken cancellationToken)
    {
        string email = registerUser.Email.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            return Conflict(ApiResponse<UserDto>.Fail("A user with this email already exists."));
        }

        User user = new()
        {
            FullName = registerUser.FullName.Trim(),
            Email = email,
            PasswordHash = passwordService.HashPassword(registerUser.Password),
            RoleId = registerUser.RoleId
        };

        await users.AddAsync(user, cancellationToken);
        await logService.LogAsync("Information", "User registered.", "Users.Register", user.Id, cancellationToken: cancellationToken);

        return Created($"/users/{user.Id}", ApiResponse<UserDto>.Ok(user.ToDto(), "User registered successfully."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginUserDto loginUser, CancellationToken cancellationToken)
    {
        string email = loginUser.Email.Trim().ToLowerInvariant();
        User? user = await users.GetByEmailWithRoleAsync(email, cancellationToken);

        if (user is null || !passwordService.VerifyPassword(loginUser.Password, user.PasswordHash))
        {
            await logService.LogAsync("Warning", $"Failed login attempt for {email}.", "Users.Login", cancellationToken: cancellationToken);
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid email or password."));
        }

        await logService.LogAsync("Information", "User logged in.", "Users.Login", user.Id, cancellationToken: cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(jwtTokenService.CreateToken(user), "User logged in successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("User is not authorized."));
        }

        User? user = await users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? NotFound(ApiResponse<UserDto>.Fail("User was not found."))
            : Ok(ApiResponse<UserDto>.Ok(user.ToDto(), "User retrieved successfully."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        string? jwtId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        string? expiresAtValue = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (string.IsNullOrWhiteSpace(jwtId) ||
            !long.TryParse(expiresAtValue, out long expiresAtUnixSeconds))
        {
            return Unauthorized(ApiResponse<object>.Fail("User is not authorized."));
        }

        await users.RevokeTokenAsync(new RevokedToken
        {
            JwtId = jwtId,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnixSeconds).UtcDateTime
        }, cancellationToken);

        int? userId = User.TryGetUserId(out int parsedUserId) ? parsedUserId : null;
        await logService.LogAsync("Information", "User logged out.", "Users.Logout", userId, cancellationToken: cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "User logged out successfully."));
    }
}
