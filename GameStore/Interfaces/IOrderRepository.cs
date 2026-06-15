using GameStore.Models;

namespace GameStore.Interfaces;

public interface IOrderRepository
{
    Task<Order?> CheckoutAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default);
    Task<Order?> GetUserOrderAsync(int userId, int orderId, CancellationToken cancellationToken = default);
}
