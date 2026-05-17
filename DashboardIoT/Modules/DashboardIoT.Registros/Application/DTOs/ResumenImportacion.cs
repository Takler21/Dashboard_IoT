namespace DashboardIoT.Registros.Application.DTOs;

public class ResumenImportacion
{
    public int TotalFilas { get; set; }
    public int Importadas { get; set; }
    public int Rechazadas { get; set; }
    public int Duplicadas { get; set; }
}
