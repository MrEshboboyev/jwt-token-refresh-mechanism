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
        builder.Property(x => x.Token).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevokedAt).IsRequired(false);

        // Configure relationships
        builder
            .HasOne<User>()
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraint on Token
        builder.HasIndex(x => x.Token).IsUnique();
    }
}