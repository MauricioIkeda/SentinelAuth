using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Infrastructure.Mappings;

public class UserApplicationRoleMap : IEntityTypeConfiguration<UserApplicationRole>
{
    public void Configure(EntityTypeBuilder<UserApplicationRole> builder)
    {
        builder.ToTable("user_application_roles");

        builder.HasKey(userApplicationRole => userApplicationRole.Id);

        builder.Property(userApplicationRole => userApplicationRole.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(userApplicationRole => userApplicationRole.ApplicationClientId)
            .HasColumnName("application_client_id")
            .IsRequired();

        builder.Property(userApplicationRole => userApplicationRole.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.HasIndex(userApplicationRole => new
        {
            userApplicationRole.UserId,
            userApplicationRole.ApplicationClientId,
            userApplicationRole.RoleId
        }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(userApplicationRole => userApplicationRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationClient>()
            .WithMany()
            .HasForeignKey(userApplicationRole => userApplicationRole.ApplicationClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(userApplicationRole => userApplicationRole.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}