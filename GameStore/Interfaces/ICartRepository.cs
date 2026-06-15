using GameStore.Dtos;

namespace GameStore.Interfaces;

public interface ICartRepository
{
    Task<CartDto> GetCartAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> AddItemAsync(int userId, AddCartItemDto addCartItem, CancellationToken cancellationToken = default);
    Task<bool> UpdateItemAsync(int userId, int gameId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemAsync(int userId, int gameId, CancellationToken cancellationToken = default);
    Task ClearAsync(int userId, CancellationToken cancellationToken = default);
}
