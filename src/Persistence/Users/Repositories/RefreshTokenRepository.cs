using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Users.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        dbContext.Set<RefreshToken>().Update(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (refreshToken is not null)
        {
            refreshToken.Revoke();
            dbContext.Set<RefreshToken>().Update(refreshToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}