namespace DashboardIoT.Registros.Application.DTOs;

public class RegistroDetalleDto
{
    public int IdRegistro { get; set; }
    public string IpOrigen { get; set; } = string.Empty;
    public string? IpDestino { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoTrafico { get; set; } = string.Empty;
    public string Justificacion { get; set; } = string.Empty;
    public string? DatosJson { get; set; }
}
