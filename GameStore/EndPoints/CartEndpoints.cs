using System.Security.Claims;
using GameStore.Data;
using GameStore.Dtos;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GameStore.EndPoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var cartGroup = app.MapGroup("/cart")
            .WithTags("Cart")
            .RequireAuthorization();

        cartGroup.MapGet("/", [Authorize] async (ClaimsPrincipal claimsPrincipal, GameStoreContext context) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(await GetCartDto(context, userId));
        });

        cartGroup.MapPost("/items", [Authorize] async (
            AddCartItemDto addCartItem,
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context,
            ILogService logService) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            if (addCartItem.Quantity <= 0)
            {
                return Results.BadRequest("Quantity must be greater than zero.");
            }

            bool gameExists = await context.Games.AnyAsync(game => game.Id == addCartItem.GameId);
            if (!gameExists)
            {
                return Results.NotFound("Game was not found.");
            }

            CartItem? cartItem = await context.CartItems
                .FirstOrDefaultAsync(item => item.UserId == userId && item.GameId == addCartItem.GameId);

            if (cartItem is null)
            {
                context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    GameId = addCartItem.GameId,
                    Quantity = addCartItem.Quantity
                });
            }
            else
            {
                cartItem.Quantity += addCartItem.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            await logService.LogAsync(
                "Information",
                $"Cart item added or increased for game ID {addCartItem.GameId}.",
                "Cart.AddItem",
                userId);

            return Results.Ok(await GetCartDto(context, userId));
        });

        cartGroup.MapPut("/items/{gameId:int}", [Authorize] async (
            int gameId,
            UpdateCartItemDto updateCartItem,
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context,
            ILogService logService) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            if (updateCartItem.Quantity <= 0)
            {
                return Results.BadRequest("Quantity must be greater than zero.");
            }

            CartItem? cartItem = await context.CartItems
                .FirstOrDefaultAsync(item => item.UserId == userId && item.GameId == gameId);

            if (cartItem is null)
            {
                return Results.NotFound();
            }

            cartItem.Quantity = updateCartItem.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await logService.LogAsync(
                "Information",
                $"Cart item quantity updated for game ID {gameId}.",
                "Cart.UpdateItem",
                userId);

            return Results.Ok(await GetCartDto(context, userId));
        });

        cartGroup.MapDelete("/items/{gameId:int}", [Authorize] async (
            int gameId,
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context,
            ILogService logService) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            int deletedCount = await context.CartItems
                .Where(item => item.UserId == userId && item.GameId == gameId)
                .ExecuteDeleteAsync();

            if (deletedCount == 0)
            {
                return Results.NotFound();
            }

            await logService.LogAsync(
                "Information",
                $"Cart item removed for game ID {gameId}.",
                "Cart.RemoveItem",
                userId);

            return Results.NoContent();
        });

        cartGroup.MapDelete("/", [Authorize] async (
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context,
            ILogService logService) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            await context.CartItems
                .Where(item => item.UserId == userId)
                .ExecuteDeleteAsync();
            await logService.LogAsync("Information", "Cart cleared.", "Cart.Clear", userId);

            return Results.NoContent();
        });
    }

    private static async Task<CartDto> GetCartDto(GameStoreContext context, int userId)
    {
        List<CartItemDto> items = await context.CartItems
            .Where(item => item.UserId == userId)
            .Include(item => item.Game)
            .OrderBy(item => item.Id)
            .Select(item => new CartItemDto(
                item.GameId,
                item.Game!.Name,
                item.Game.Price,
                item.Quantity,
                item.Game.Price * item.Quantity
            ))
            .AsNoTracking()
            .ToListAsync();

        return new CartDto(items, items.Sum(item => item.LineTotal));
    }

    private static bool TryGetUserId(ClaimsPrincipal claimsPrincipal, out int userId)
    {
        string? userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }
}
