using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Users.Constants;

namespace Persistence.Users.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Map to the RefreshTokens table
        builder.ToTable(UserTableNames.RefreshTokens);

        // Configure the primary key
        builder.HasKey(x => x.Id);

        // Configure properties
        builder.Property(x => x.HashedToken).IsRequired().HasMaxLength(1000); // Increased size for hashed tokens
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IpAddress).IsRequired().HasMaxLength(50);
        builder.Property(x => x.UserAgent).IsRequired().HasMaxLength(500);
        builder.Property(x => x.RevokedAt).IsRequired(false);

        // Configure relationships
        builder
            .HasOne<User>()
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraint on HashedToken
        builder.HasIndex(x => x.HashedToken).IsUnique();
    }
}
