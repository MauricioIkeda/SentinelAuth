using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Infrastructure.Mappings;

public sealed class ApplicationClientMap : IEntityTypeConfiguration<ApplicationClient>
{
    public void Configure(EntityTypeBuilder<ApplicationClient> builder)
    {
        builder.ToTable("application_clients");

        builder.HasKey(applicationClient => applicationClient.Id);

        builder.Property(applicationClient => applicationClient.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(applicationClient => applicationClient.ClientId)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(applicationClient => applicationClient.Audience)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(applicationClient => applicationClient.IsActive)
            .IsRequired();

        builder.HasIndex(applicationClient => applicationClient.ClientId)
            .IsUnique();

        builder.HasIndex(applicationClient => applicationClient.Audience)
            .IsUnique();
    }
}
