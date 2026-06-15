using GameStore.Data;
using GameStore.Dtos;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class CartRepository(GameStoreContext context) : ICartRepository
{
    public async Task<CartDto> GetCartAsync(int userId, CancellationToken cancellationToken = default)
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
                item.Game.Price * item.Quantity))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new CartDto(items, items.Sum(item => item.LineTotal));
    }

    public async Task<bool> AddItemAsync(int userId, AddCartItemDto addCartItem, CancellationToken cancellationToken = default)
    {
        bool gameExists = await context.Games.AnyAsync(game => game.Id == addCartItem.GameId, cancellationToken);
        if (!gameExists)
        {
            return false;
        }

        CartItem? cartItem = await context.CartItems
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.GameId == addCartItem.GameId,
                cancellationToken);

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

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateItemAsync(int userId, int gameId, int quantity, CancellationToken cancellationToken = default)
    {
        CartItem? cartItem = await context.CartItems
            .FirstOrDefaultAsync(item => item.UserId == userId && item.GameId == gameId, cancellationToken);

        if (cartItem is null)
        {
            return false;
        }

        cartItem.Quantity = quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveItemAsync(int userId, int gameId, CancellationToken cancellationToken = default)
    {
        int deletedCount = await context.CartItems
            .Where(item => item.UserId == userId && item.GameId == gameId)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount > 0;
    }

    public async Task ClearAsync(int userId, CancellationToken cancellationToken = default)
    {
        await context.CartItems
            .Where(item => item.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
