using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Infrastructure.Mappings;

public sealed class AuthorizationCodeMap : IEntityTypeConfiguration<AuthorizationCode>
{
    public void Configure(EntityTypeBuilder<AuthorizationCode> builder)
    {
        builder.ToTable("authorization_codes");

        builder.HasKey(authorizationCode => authorizationCode.Id);

        builder.Property(authorizationCode => authorizationCode.UserId)
            .IsRequired();

        builder.Property(authorizationCode => authorizationCode.ApplicationClientId)
            .IsRequired();

        builder.Property(authorizationCode => authorizationCode.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(authorizationCode => authorizationCode.RedirectUri)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(authorizationCode => authorizationCode.ExpiresAt)
            .IsRequired();

        builder.Property(authorizationCode => authorizationCode.ConsumedAt);

        builder.HasIndex(authorizationCode => authorizationCode.CodeHash)
            .IsUnique();
    }
}
