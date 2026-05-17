using DashboardIoT.Usuarios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardIoT.Usuarios.Infrastructure.Persistence;

public class UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : DbContext(options)
{

    public DbSet<Usuario> Usuario => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsuariosDbContext).Assembly);
    }
}
