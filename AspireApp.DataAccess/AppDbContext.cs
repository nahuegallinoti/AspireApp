using AspireApp.Entidad;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}