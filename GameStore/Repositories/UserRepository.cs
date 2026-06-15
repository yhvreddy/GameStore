using GameStore.Data;
using GameStore.Interfaces;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Repositories;

public class UserRepository(GameStoreContext context) : IUserRepository
{
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.Users.FindAsync([id], cancellationToken);

    public async Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default) =>
        await context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsTokenRevokedAsync(string jwtId, CancellationToken cancellationToken = default) =>
        context.RevokedTokens.AnyAsync(token => token.JwtId == jwtId, cancellationToken);

    public async Task RevokeTokenAsync(RevokedToken revokedToken, CancellationToken cancellationToken = default)
    {
        bool alreadyRevoked = await IsTokenRevokedAsync(revokedToken.JwtId, cancellationToken);
        if (alreadyRevoked)
        {
            return;
        }

        context.RevokedTokens.Add(revokedToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
