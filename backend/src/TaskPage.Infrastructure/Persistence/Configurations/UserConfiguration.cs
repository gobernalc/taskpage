using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskPage.Domain.Entities;

namespace TaskPage.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the <see cref="User"/> entity to the Users table.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>Configures columns, constraints and the tasks relationship.</summary>
    /// <param name="builder">Entity type builder for users.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired(false);

        builder.HasMany(u => u.Tasks)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
