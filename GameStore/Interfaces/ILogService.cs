using GameStore.Models;

namespace GameStore.Interfaces;

public interface ILogService
{
    Task<AppLog> LogAsync(
        string level,
        string message,
        string? source = null,
        int? userId = null,
        string? exception = null,
        CancellationToken cancellationToken = default);
}
