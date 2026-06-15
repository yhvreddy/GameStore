using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class RoleRepository(GameStoreContext context) : IRoleRepository
{
    public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.Roles.FindAsync([id], cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        context.Roles.Add(role);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        int deletedCount = await context.Roles
            .Where(currentRole => currentRole.Id == role.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount > 0;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
