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

        usersGroup.MapPost("/register", async (
            RegisterUserDto registerUser,
            GameStoreContext context,
            PasswordService passwordService) =>
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

            return Results.Created($"/users/{user.Id}", new UserDto(user.Id, user.FullName, user.Email, user.RoleId));
        }).AllowAnonymous();

        usersGroup.MapPost("/login", async (
            LoginUserDto loginUser,
            GameStoreContext context,
            PasswordService passwordService,
            JwtTokenService jwtTokenService) =>
        {
            string email = loginUser.Email.Trim().ToLowerInvariant();
            User? user = await context.Users.FirstOrDefaultAsync(user => user.Email == email);

            if (user is null || !passwordService.VerifyPassword(loginUser.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

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
    }
}
