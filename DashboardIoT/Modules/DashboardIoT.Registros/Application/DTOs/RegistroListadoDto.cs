namespace DashboardIoT.Registros.Application.DTOs;

public class RegistroListadoDto
{
    public int IdRegistro { get; set; }
    public string IpOrigen { get; set; } = string.Empty;
    public string? IpDestino { get; set; }
    public string TipoTrafico { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
