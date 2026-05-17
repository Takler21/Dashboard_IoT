using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;

namespace DashboardIoT.Registros.Application.Interfaces;

public interface IRegistroRepository
{
    Task<Registro?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);

    Task<List<RegistrosPorTipoTraficoDto>> SeleccionarTotalRegistrosPorTipoTraficoAsync(CancellationToken cancellationToken);
    Task<ResultadoPaginadoDto<Clasificacion>> BuscarRegistrosAsync(FiltroRegistrosDto filtro, CancellationToken cancellationToken);
    Task<int> SeleccionarTotalRegistrosPendientesAsync(CancellationToken cancellationToken);

    Task<Clasificacion?> SeleccionarClasificacionPorRegistroIdAsync(int idRegistro, CancellationToken cancellationToken);
    Task<List<OrigenDatosDto>> SeleccionarOrigenDatosAsync(CancellationToken cancellationToken);
    Task<int?> SeleccionarIdOrigenDatosPorTipoAsync(TipoOrigen tipo, CancellationToken cancellationToken);
    Task<int> InsertarOrigenDatosSiNoExisteAsync(TipoOrigen tipo, string nombre, CancellationToken cancellationToken);
    Task<(int Insertadas, int Duplicadas)> InsertarRegistrosAsync(IReadOnlyList<Registro> registros, CancellationToken cancellationToken);
    Task<List<Registro>> ObtenerPendientesOrdenadosAsync(int limite, CancellationToken cancellationToken);
    Task InsertarClasificacionesAsync(IReadOnlyList<Clasificacion> clasificaciones, CancellationToken cancellationToken);
    Task MarcarRegistrosComoEstadoAsync(IReadOnlyList<int> idsRegistro, EstadoRegistro estado, CancellationToken cancellationToken);
}