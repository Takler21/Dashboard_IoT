namespace DashboardIoT.Registros.Application.DTOs;

public class FiltroRegistrosDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? DireccionIp { get; set; }
    public int? TipoTrafico { get; set; }
    public int? IdOrigenDatos { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
