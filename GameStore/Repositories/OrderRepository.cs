using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class OrderRepository(GameStoreContext context) : IOrderRepository
{
    public async Task<Order?> CheckoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        List<CartItem> cartItems = await context.CartItems
            .Where(item => item.UserId == userId)
            .Include(item => item.Game)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            return null;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        Order order = new()
        {
            UserId = userId,
            Items = cartItems.Select(item => new OrderItem
            {
                GameId = item.GameId,
                GameName = item.Game!.Name,
                UnitPrice = item.Game.Price,
                Quantity = item.Quantity
            }).ToList()
        };
        order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);

        context.Orders.Add(order);
        context.CartItems.RemoveRange(cartItems);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return order;
    }

    public async Task<IReadOnlyCollection<Order>> GetUserOrdersAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await context.Orders
            .Where(order => order.UserId == userId)
            .Include(order => order.Items)
            .OrderByDescending(order => order.OrderedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<Order?> GetUserOrderAsync(int userId, int orderId, CancellationToken cancellationToken = default) =>
        context.Orders
            .Include(order => order.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == orderId && order.UserId == userId, cancellationToken);
}
