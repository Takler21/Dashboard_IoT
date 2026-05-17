using DashboardIoT.Registros.Domain.Enums;

namespace DashboardIoT.Registros.Domain.Entities;

public class OrigenDatos
{
    public int IdOrigenDatos { get; private set; }
    public TipoOrigen Tipo { get; private set; }
    public string Nombre { get; private set; } = null!;
}