using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskPage.Domain.Entities;

namespace TaskPage.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the <see cref="TaskItem"/> entity to the Tasks table.
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    /// <summary>Configures columns, constraints and the user relationship.</summary>
    /// <param name="builder">Entity type builder for tasks.</param>
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.UserId).IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired(false);

        builder.Property(t => t.AdditionalData).IsRequired(false);

        builder.HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.UserId, t.Status, t.CreatedAt })
            .IsDescending(false, false, true);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Title);
    }
}
