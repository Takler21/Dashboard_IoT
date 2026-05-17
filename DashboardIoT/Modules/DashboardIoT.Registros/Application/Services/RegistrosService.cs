using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Application.Interfaces;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;

namespace DashboardIoT.Registros.Application.Services;

public class RegistrosService(
    IRegistroRepository registroRepository,
    ICargadorArchivosPort cargadorArchivos,
    IClasificacionTrigger clasificacionTrigger) : IRegistrosService
{
    private readonly IRegistroRepository _registroRepository = registroRepository;
    private readonly ICargadorArchivosPort _cargadorArchivos = cargadorArchivos;
    private readonly IClasificacionTrigger _clasificacionTrigger = clasificacionTrigger;

    private const int TamanoLote = 1000;

    public async Task<ResumenImportacion> InsertarRegistrosDesdeArchivoAsync(Stream archivo, string fileName, CancellationToken cancellationToken)
    {
        var idOrigenDatos = await _registroRepository.InsertarOrigenDatosSiNoExisteAsync(TipoOrigen.Repositorio, fileName, cancellationToken);
        var resumen = new ResumenImportacion();
        var lote = new List<Registro>(TamanoLote);

        await foreach (var campos in _cargadorArchivos.LeerRegistrosAsync(archivo, cancellationToken))
        {
            resumen.TotalFilas++;

            if (campos is null)
            {
                resumen.Rechazadas++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(campos.IpOrigen) ||
                campos.Datos is null)
            {
                resumen.Rechazadas++;
                continue;
            }

            var fechaUtc = campos.Fecha.Kind == DateTimeKind.Utc
                ? campos.Fecha
                : campos.Fecha.ToUniversalTime();

            var registro = Registro.Crear(idOrigenDatos, fechaUtc, campos.IpOrigen, campos.IpDestino, campos.Datos);

            lote.Add(registro);
            if (lote.Count >= TamanoLote)
            {
                await InsertarLoteRegistros(lote, resumen, cancellationToken);
                lote.Clear();
            }
        }

        if (lote.Count > 0)
        {
            await InsertarLoteRegistros(lote, resumen, cancellationToken);
        }

        return resumen;
    }

    private async Task InsertarLoteRegistros(IReadOnlyList<Registro> lote, ResumenImportacion resumen, CancellationToken cancellationToken)
    {
        var (insertadas, duplicadas) = await _registroRepository.InsertarRegistrosAsync(lote, cancellationToken);
        resumen.Importadas += insertadas;
        resumen.Duplicadas += duplicadas;
    }

    public async Task InsertarRegistroSensorAsync(RegistroSensorDto registroSensor, CancellationToken cancellationToken)
    {
        var tipoOrigen = Enum.Parse<TipoOrigen>(registroSensor.TipoOrigen, ignoreCase: true);
        var idOrigenDatos = await _registroRepository.InsertarOrigenDatosSiNoExisteAsync(tipoOrigen, registroSensor.NombreSensor, cancellationToken);
        var registro = Registro.Crear(idOrigenDatos, registroSensor.Fecha, registroSensor.IpOrigen, registroSensor.IpDestino, registroSensor.Datos);
        await _registroRepository.InsertarRegistrosAsync([registro], cancellationToken);
    }

    public async Task<List<RegistrosPorTipoTraficoDto>> SeleccionarTotalRegistrosPorTipoTraficoAsync(CancellationToken cancellationToken)
    {
        return await _registroRepository.SeleccionarTotalRegistrosPorTipoTraficoAsync(cancellationToken);
    }

    public async Task<ResultadoPaginadoDto<RegistroListadoDto>> BuscarRegistrosAsync(FiltroRegistrosDto filtro, CancellationToken cancellationToken)
    {
        var resultado = await _registroRepository.BuscarRegistrosAsync(filtro, cancellationToken);

        var items = resultado.Items.Select(clasificacion => new RegistroListadoDto
        {
            IdRegistro = clasificacion.Registro!.IdRegistro,
            IpOrigen = clasificacion.Registro.IpOrigen,
            IpDestino = clasificacion.Registro.IpDestino,
            Fecha = clasificacion.Registro.Fecha,
            TipoTrafico = clasificacion.TipoTrafico.ToString()
        })
        .ToList();

        return new ResultadoPaginadoDto<RegistroListadoDto>
        {
            Items = items,
            TotalCount = resultado.TotalCount,
            Pagina = resultado.Pagina,
            TamanoPagina = resultado.TamanoPagina
        };
    }

    public async Task<RegistroDetalleDto?> SeleccionarRegistroPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var registro = await _registroRepository.ObtenerPorIdAsync(id, cancellationToken);
        if (registro is null) return null;

        var clasificacion = await _registroRepository.SeleccionarClasificacionPorRegistroIdAsync(id, cancellationToken);

        return new RegistroDetalleDto
        {
            IdRegistro = registro.IdRegistro,
            IpOrigen = registro.IpOrigen,
            IpDestino = registro.IpDestino,
            Fecha = registro.Fecha,
            TipoTrafico = clasificacion?.TipoTrafico.ToString() ?? "",
            Justificacion = clasificacion?.Justificacion ?? "",
            DatosJson = registro.Datos.RootElement.GetRawText()
        };
    }

    public async Task<int> SeleccionarTotalRegistrosPendientesAsync(CancellationToken cancellationToken)
    {
        return await _registroRepository.SeleccionarTotalRegistrosPendientesAsync(cancellationToken);
    }

    public async Task<List<OrigenDatosDto>> SeleccionarOrigenDatosAsync(CancellationToken cancellationToken)
    {
        return await _registroRepository.SeleccionarOrigenDatosAsync(cancellationToken);
    }

    public Task ClasificarPendientesAsync(CancellationToken cancellationToken)
    {
        _clasificacionTrigger.IniciarClasificacion();
        return Task.CompletedTask;
    }

    public async Task<List<Registro>> ObtenerPendientesParaClasificarAsync(int max, CancellationToken cancellationToken)
    {
        return await _registroRepository.ObtenerPendientesOrdenadosAsync(max, cancellationToken);
    }

    public async Task RegistrarClasificacionesAsync(IReadOnlyList<Clasificacion> clasificaciones, CancellationToken cancellationToken)
    {
        if (clasificaciones.Count == 0) return;

        var idsRegistro = clasificaciones.Select(clasificacion => clasificacion.IdRegistro).ToList();
        await _registroRepository.InsertarClasificacionesAsync(clasificaciones, cancellationToken);
        await _registroRepository.MarcarRegistrosComoEstadoAsync(idsRegistro, EstadoRegistro.Procesado, cancellationToken);
    }

    public async Task MarcarRegistrosComoEstadoAsync(IReadOnlyList<int> idsRegistro, EstadoRegistro estado, CancellationToken cancellationToken)
    {
        if (idsRegistro.Count == 0) return;
        await _registroRepository.MarcarRegistrosComoEstadoAsync(idsRegistro, estado, cancellationToken);
    }
}
