using GameStore.Models;

namespace GameStore.Interfaces;

public interface IGenreRepository
{
    Task<IReadOnlyCollection<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Genre genre, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
