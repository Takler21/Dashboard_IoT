using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;

namespace DashboardIoT.Registros.Application.Interfaces;

public interface IRegistrosService
{
    Task<ResumenImportacion> InsertarRegistrosDesdeArchivoAsync(Stream archivo, string fileName, CancellationToken cancellationToken);
    Task InsertarRegistroSensorAsync(DTOs.RegistroSensorDto registro, CancellationToken cancellationToken);
    Task<List<RegistrosPorTipoTraficoDto>> SeleccionarTotalRegistrosPorTipoTraficoAsync(CancellationToken cancellationToken);
    Task<ResultadoPaginadoDto<RegistroListadoDto>> BuscarRegistrosAsync(FiltroRegistrosDto filtro, CancellationToken cancellationToken);
    Task<RegistroDetalleDto?> SeleccionarRegistroPorIdAsync(int id, CancellationToken cancellationToken);
    Task<int> SeleccionarTotalRegistrosPendientesAsync(CancellationToken cancellationToken);
    Task<List<OrigenDatosDto>> SeleccionarOrigenDatosAsync(CancellationToken cancellationToken);
    Task ClasificarPendientesAsync(CancellationToken cancellationToken);
    Task MarcarRegistrosComoEstadoAsync(IReadOnlyList<int> idsRegistro, EstadoRegistro estado, CancellationToken cancellationToken);
    Task<List<Registro>> ObtenerPendientesParaClasificarAsync(int max, CancellationToken cancellationToken);
    Task RegistrarClasificacionesAsync(IReadOnlyList<Clasificacion> clasificaciones, CancellationToken cancellationToken);
}
