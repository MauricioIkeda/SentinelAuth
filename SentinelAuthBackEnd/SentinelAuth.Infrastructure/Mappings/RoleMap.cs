using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Infrastructure.Mappings;

public class RoleMap : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.ApplicationClientId)
            .IsRequired();

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(role => new
            {
                role.ApplicationClientId,
                role.NormalizedName
            })
            .IsUnique();

        builder.HasOne<ApplicationClient>()
            .WithMany()
            .HasForeignKey(role => role.ApplicationClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}