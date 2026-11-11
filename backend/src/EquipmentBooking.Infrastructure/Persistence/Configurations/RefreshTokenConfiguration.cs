using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", table =>
        {
            table.HasComment("Hashes of refresh tokens; every use rotates the token.");
            table.HasCheckConstraint("ck_refresh_tokens_expiry", "expires_at > created_at");
        });
        builder.Property(token => token.Id).HasComment("Surrogate key.");
        builder.Property(token => token.UserId).HasComment("Owner of the token.");
        builder.HasOne(token => token.User).WithMany().HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired().HasComment("SHA-256 hash of the token in hexadecimal; the token itself is never stored.");
        builder.HasIndex(token => token.TokenHash).IsUnique().HasDatabaseName("ux_refresh_tokens_token_hash");
        builder.Property(token => token.CreatedAt).HasComment("Issue time.");
        builder.Property(token => token.ExpiresAt).HasComment("Expiry time.");
        builder.Property(token => token.RevokedAt).HasComment("Revocation time after rotation, sign out, role change or deactivation.");
        builder.Property(token => token.ReplacedByTokenId).HasComment("Token issued when this one was rotated.");
        builder.HasOne(token => token.ReplacedByToken).WithMany().HasForeignKey(token => token.ReplacedByTokenId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(token => token.UserId).HasDatabaseName("ix_refresh_tokens_user_id");
    }
}
