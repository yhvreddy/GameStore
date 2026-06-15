using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class LogRepository(GameStoreContext context) : ILogRepository
{
    public async Task<IReadOnlyCollection<AppLog>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Logs
            .OrderByDescending(log => log.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
