using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;
using TaskEntity = TaskManagement.Api.Models.Task;

namespace TaskManagement.Api.Data;

/// <summary>EF Core database context (SQL Server provider).</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Lookup> Lookups => Set<Lookup>();

    public DbSet<TaskEntity> UserTasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(2000);

            entity.Property(t => t.Status)
                .HasConversion<int>();

            entity.Property(t => t.Priority)
                .HasConversion<int>();

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.DueDate);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(u => u.HashedPassword)
                .IsRequired()
                .HasMaxLength(512);

            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Lookup>(entity =>
        {
            entity.ToTable("Lookups");
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(l => new { l.MajorCode, l.MinorCode }).IsUnique();

            entity.HasData(
                new Lookup { Id = 1, MajorCode = 1, MinorCode = 0, Name = "Task Status" },
                new Lookup { Id = 2, MajorCode = 1, MinorCode = 1, Name = "Initiated" },
                new Lookup { Id = 3, MajorCode = 1, MinorCode = 2, Name = "In Progress" },
                new Lookup { Id = 4, MajorCode = 1, MinorCode = 3, Name = "Completed" },
                new Lookup { Id = 5, MajorCode = 1, MinorCode = 4, Name = "Cancelled" });
        });

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Task");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(2000);

            entity.HasOne(t => t.Status)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.StatusId);
            entity.HasIndex(t => t.UserId);
        });




    }
}
