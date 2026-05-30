using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelAuth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Infrastructure.Mappings
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id).UseIdentityColumn();

            builder.Property(u => u.CreatedAt).IsRequired();
            builder.Property(u => u.UpdatedAt).IsRequired();
            builder.Property(u => u.PasswordHash).HasColumnName("PasswordHash").IsRequired().HasMaxLength(256);

            builder.OwnsOne(u => u.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value).HasColumnName("Email").IsRequired().HasMaxLength(150);

                emailBuilder.HasIndex(e => e.Value).IsUnique();
            });

        }
    }
}
