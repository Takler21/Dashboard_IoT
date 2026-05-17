namespace DashboardIoT.Registros.Application.DTOs;

public class ResultadoPaginadoDto<T>
{
    public IList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
}