using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameStore.Data;
using GameStore.Dtos;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GameStore.EndPoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var usersGroup = app.MapGroup("/users").WithTags("Users");

        usersGroup.MapGet("/", async (GameStoreContext context) =>
        {
            var users = await context.Users
                .Select(user => new UserDto(user.Id, user.FullName, user.Email, user.RoleId))
                .ToListAsync();

            return Results.Ok(users);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        usersGroup.MapPost("/register", async (
            RegisterUserDto registerUser,
            GameStoreContext context,
            PasswordService passwordService,
            ILogService logService) =>
        {
            string email = registerUser.Email.Trim().ToLowerInvariant();
            bool emailExists = await context.Users.AnyAsync(user => user.Email == email);

            if (emailExists)
            {
                return Results.Conflict("A user with this email already exists.");
            }

            User user = new()
            {
                FullName = registerUser.FullName.Trim(),
                Email = email,
                PasswordHash = passwordService.HashPassword(registerUser.Password),
                RoleId = registerUser.RoleId
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            await logService.LogAsync("Information", "User registered.", "Users.Register", user.Id);

            return Results.Created($"/users/{user.Id}", new UserDto(user.Id, user.FullName, user.Email, user.RoleId));
        }).AllowAnonymous();

        usersGroup.MapPost("/login", async (
            LoginUserDto loginUser,
            GameStoreContext context,
            PasswordService passwordService,
            JwtTokenService jwtTokenService,
            ILogService logService) =>
        {
            string email = loginUser.Email.Trim().ToLowerInvariant();
            User? user = await context.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Email == email);

            if (user is null || !passwordService.VerifyPassword(loginUser.Password, user.PasswordHash))
            {
                await logService.LogAsync("Warning", $"Failed login attempt for {email}.", "Users.Login");
                return Results.Unauthorized();
            }

            await logService.LogAsync("Information", "User logged in.", "Users.Login", user.Id);
            return Results.Ok(jwtTokenService.CreateToken(user));
        }).AllowAnonymous();

        usersGroup.MapGet("/me", [Authorize] async (ClaimsPrincipal claimsPrincipal, GameStoreContext context) =>
        {
            string? userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Results.Unauthorized();
            }

            User? user = await context.Users.FindAsync(userId);
            return user is null
                ? Results.NotFound()
                : Results.Ok(new UserDto(user.Id, user.FullName, user.Email, user.RoleId));
        });

        usersGroup.MapPost("/logout", [Authorize] async (
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context,
            ILogService logService) =>
        {
            string? userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            string? jwtId = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Jti);
            string? expiresAtValue = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (string.IsNullOrWhiteSpace(jwtId) ||
                !long.TryParse(expiresAtValue, out long expiresAtUnixSeconds))
            {
                return Results.Unauthorized();
            }

            bool alreadyRevoked = await context.RevokedTokens.AnyAsync(token => token.JwtId == jwtId);
            if (!alreadyRevoked)
            {
                context.RevokedTokens.Add(new RevokedToken
                {
                    JwtId = jwtId,
                    ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnixSeconds).UtcDateTime
                });

                await context.SaveChangesAsync();
            }

            int.TryParse(userIdValue, out int userId);
            await logService.LogAsync("Information", "User logged out.", "Users.Logout", userId == 0 ? null : userId);

            return Results.NoContent();
        });
    }
}
