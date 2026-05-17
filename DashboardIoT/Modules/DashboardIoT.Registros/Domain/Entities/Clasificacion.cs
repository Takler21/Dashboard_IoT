using DashboardIoT.Registros.Domain.Enums;

namespace DashboardIoT.Registros.Domain.Entities;

public class Clasificacion
{
    public int IdClasificacion { get; private set; }
    public int IdRegistro { get; private set; }
    public Registro? Registro { get; private set; }
    public TipoTrafico TipoTrafico { get; private set; }
    public string Justificacion { get; private set; } = null!;

    private Clasificacion() { }

    public static Clasificacion Crear(
        int idRegistro,
        Registro registro,
        TipoTrafico tipoTrafico,
        string justificacion)
    {
        return new Clasificacion
        {
            IdRegistro = idRegistro,
            Registro = registro,
            TipoTrafico = tipoTrafico,
            Justificacion = justificacion,
        };
    }
}
