using GameStore.Models;

namespace GameStore.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string jwtId, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(RevokedToken revokedToken, CancellationToken cancellationToken = default);
}
