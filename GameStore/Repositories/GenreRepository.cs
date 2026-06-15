using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class GenreRepository(GameStoreContext context) : IGenreRepository
{
    public async Task<IReadOnlyCollection<Genre>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Genres
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.Genres.FindAsync([id], cancellationToken);

    public async Task AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        context.Genres.Add(genre);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        int deletedCount = await context.Genres
            .Where(currentGenre => currentGenre.Id == genre.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount > 0;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
