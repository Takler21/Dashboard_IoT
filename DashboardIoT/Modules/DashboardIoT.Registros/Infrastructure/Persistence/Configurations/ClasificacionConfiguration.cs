using DashboardIoT.Registros.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardIoT.Registros.Infrastructure.Persistence.Configurations;

public class ClasificacionConfiguration : IEntityTypeConfiguration<Clasificacion>
{
    public void Configure(EntityTypeBuilder<Clasificacion> builder)
    {
        builder.HasKey(e => e.IdClasificacion);

        builder.HasIndex(e => e.IdRegistro).IsUnique();

        builder.HasOne(c => c.Registro)
            .WithOne()
            .HasForeignKey<Clasificacion>(e => e.IdRegistro)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
