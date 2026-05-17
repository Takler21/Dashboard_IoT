using DashboardIoT.Registros.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardIoT.Registros.Infrastructure.Persistence.Configurations;

public class OrigenDatosConfiguration : IEntityTypeConfiguration<OrigenDatos>
{
    public void Configure(EntityTypeBuilder<OrigenDatos> builder)
    {
        builder.HasKey(e => e.IdOrigenDatos);
        builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => new { e.Tipo, e.Nombre }).IsUnique();
    }
}
