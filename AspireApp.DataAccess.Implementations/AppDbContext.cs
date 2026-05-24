using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Show> Shows => Set<Show>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Name).HasMaxLength(128).IsRequired();
            entity.Property(u => u.Surname).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(256).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<Show>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(256).IsRequired();
            entity.Property(s => s.Description).HasMaxLength(2000);
        });
    }
}
