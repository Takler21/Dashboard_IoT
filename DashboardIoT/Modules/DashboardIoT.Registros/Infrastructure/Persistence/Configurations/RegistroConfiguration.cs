using DashboardIoT.Registros.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardIoT.Registros.Infrastructure.Persistence.Configurations;

public class RegistroConfiguration : IEntityTypeConfiguration<Registro>
{
    public void Configure(EntityTypeBuilder<Registro> builder)
    {
        builder.HasKey(e => e.IdRegistro);

        builder.Property(e => e.Datos)
               .HasColumnType("jsonb")
               .IsRequired();

        builder.HasOne(e => e.OrigenDatos)
               .WithMany()
               .HasForeignKey(e => e.IdOrigenDatos)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
