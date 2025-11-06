using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Users.Repositories;

internal sealed class RefreshTokenRepository(
    ApplicationDbContext dbContext,
    ITokenHasher tokenHasher,
    IRefreshTokenBlacklistService refreshTokenBlacklistService
) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        // First check if the token is blacklisted
        var hashedToken = tokenHasher.HashToken(token);
        if (await refreshTokenBlacklistService.IsTokenBlacklistedAsync(hashedToken))
        {
            return null;
        }

        // Load all refresh tokens (temporary solution until we can properly index)
        // TODO: This is inefficient and should be optimized with proper database indexing
        var refreshTokens = await dbContext.Set<RefreshToken>()
            .ToListAsync(cancellationToken);
            
        // Find the token that matches the provided token
        return refreshTokens.FirstOrDefault(rt => tokenHasher.VerifyToken(token, rt.HashedToken));
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
        var refreshToken = await GetAsync(token, cancellationToken);

        if (refreshToken is not null)
        {
            refreshToken.Revoke();
            dbContext.Set<RefreshToken>().Update(refreshToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            // Also add to blacklist
            await refreshTokenBlacklistService.BlacklistTokenAsync(refreshToken);
        }
    }
}
