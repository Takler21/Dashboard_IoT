using DashboardIoT.Registros.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardIoT.Registros.Infrastructure.Persistence;

public class RegistrosDbContext(DbContextOptions<RegistrosDbContext> options) : DbContext(options)
{

    public DbSet<Registro> Registro => Set<Registro>();
    public DbSet<OrigenDatos> OrigenDatos => Set<OrigenDatos>();
    public DbSet<Clasificacion> Clasificacion => Set<Clasificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrosDbContext).Assembly);
    }
}
