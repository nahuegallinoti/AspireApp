using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Show> Shows => Set<Show>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
            entity.HasIndex(u => new { u.ExternalProvider, u.ExternalProviderUserId });

            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Name).HasMaxLength(128).IsRequired();
            entity.Property(u => u.Surname).HasMaxLength(128).IsRequired();
            entity.Property(u => u.ExternalProvider).HasMaxLength(64);
            entity.Property(u => u.ExternalProviderUserId).HasMaxLength(256);

            entity.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User!)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User!)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.NormalizedName).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(64).IsRequired();
            entity.Property(r => r.NormalizedName).HasMaxLength(64).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(256);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r!.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.TokenHash).IsUnique();
            entity.HasIndex(rt => new { rt.UserId, rt.RevokedUtc });
            entity.Property(rt => rt.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(rt => rt.CreatedByIp).HasMaxLength(64);
            entity.Property(rt => rt.RevokedByIp).HasMaxLength(64);
            entity.Property(rt => rt.RevokedReason).HasMaxLength(128);
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
