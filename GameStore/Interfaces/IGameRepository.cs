using GameStore.Models;

namespace GameStore.Interfaces;

public interface IGameRepository
{
    Task<IReadOnlyCollection<Game>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Game game, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Game game, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
