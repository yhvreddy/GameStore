using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class GameRepository(GameStoreContext context) : IGameRepository
{
    public async Task<IReadOnlyCollection<Game>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Games
            .Include(game => game.Genre)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.Games.FindAsync([id], cancellationToken);

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        context.Games.Add(game);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        int deletedCount = await context.Games
            .Where(currentGame => currentGame.Id == game.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount > 0;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
