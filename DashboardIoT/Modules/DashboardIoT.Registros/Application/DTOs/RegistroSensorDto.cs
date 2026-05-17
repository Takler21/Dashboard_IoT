using System.Text.Json;

namespace DashboardIoT.Registros.Application.DTOs;

public sealed class RegistroSensorDto
{
    public string TipoOrigen { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string IpOrigen { get; set; } = null!;
    public string? IpDestino { get; set; }
    public string NombreSensor { get; set; } = null!;
    public JsonDocument Datos { get; set; } = null!;
}
