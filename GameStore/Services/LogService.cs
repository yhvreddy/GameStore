using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;

namespace GameStore.Services;

public class LogService(GameStoreContext context) : ILogService
{
    public async Task<AppLog> LogAsync(
        string level,
        string message,
        string? source = null,
        int? userId = null,
        string? exception = null,
        CancellationToken cancellationToken = default)
    {
        AppLog log = new()
        {
            Level = level,
            Message = message,
            Source = source,
            UserId = userId,
            Exception = exception
        };

        context.Logs.Add(log);
        await context.SaveChangesAsync(cancellationToken);

        return log;
    }
}
