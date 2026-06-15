using GameStore.Models;

namespace GameStore.Interfaces;

public interface ILogRepository
{
    Task<IReadOnlyCollection<AppLog>> GetAllAsync(CancellationToken cancellationToken = default);
}
