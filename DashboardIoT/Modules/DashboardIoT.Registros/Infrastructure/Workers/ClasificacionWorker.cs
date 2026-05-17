using System.Net;
using DashboardIoT.Registros.Application.Interfaces;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;
using DashboardIoT.Registros.Infrastructure.Adapters.Gemini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DashboardIoT.Registros.Infrastructure.Workers;

public sealed class ClasificacionWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ClasificacionWorker> logger) : BackgroundService, IClasificacionTrigger
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<ClasificacionWorker> _logger = logger;
    private readonly SemaphoreSlim _semaforo = new(0, 1);
    private readonly int _peticionesPorMinuto = configuration.GetValue<int>("ClasificadorWorker:PeticionesPorMinuto");
    private readonly int _maxPendientes = configuration.GetValue<int>("ClasificadorWorker:MaxPendientes");
    private readonly int _tamanoLote = configuration.GetValue<int>("ClasificadorWorker:TamanoLote");
    private readonly int _intervaloMinutos = configuration.GetValue<int>("ClasificadorWorker:IntervaloMinutos");
    private readonly int _pausaTrasLimite = configuration.GetValue<int>("ClasificadorWorker:PausaTrasLimite");

    private DateTime _pausadoHasta = DateTime.MinValue;

    public void IniciarClasificacion()
    {
        try { _semaforo.Release(); }
        catch (SemaphoreFullException) { }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error clasificando");
            }

            await _semaforo.WaitAsync(TimeSpan.FromMinutes(_intervaloMinutos), stoppingToken);
        }
    }

    private async Task ProcesarAsync(CancellationToken cancellationToken)
    {
        if (_pausadoHasta > DateTime.UtcNow) return;

        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var service = sp.GetRequiredService<IRegistrosService>();
        var classifier = sp.GetRequiredService<ILlmClasificarService>();

        var pendientes = await service.ObtenerPendientesParaClasificarAsync(_maxPendientes, cancellationToken);
        if (pendientes.Count == 0) return;

        var esperaEntreLotes = TimeSpan.FromSeconds(60.0 / _peticionesPorMinuto);
        foreach (var lote in AgruparPorOrigen(pendientes))
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (!await ProcesarLoteAsync(lote, classifier, service, cancellationToken)) break;
            await Task.Delay(esperaEntreLotes, cancellationToken);
        }
    }

    private async Task<bool> ProcesarLoteAsync(
        List<Registro> lote,
        ILlmClasificarService classifier,
        IRegistrosService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var clasificaciones = await classifier.ClasificarLoteAsync(lote, cancellationToken);

            if (clasificaciones is null)
            {
                _logger.LogWarning("Lote fallido, registros: {Count} ", lote.Count);
                await service.MarcarRegistrosComoEstadoAsync(
                    lote.Select(registro => registro.IdRegistro).ToList(),
                    EstadoRegistro.Error,
                    cancellationToken);
            }
            else
            {
                await service.RegistrarClasificacionesAsync(clasificaciones, cancellationToken);
            }
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = ex.Data["RetryAfter"] as TimeSpan?
                ?? TimeSpan.FromMinutes(_pausaTrasLimite);
            _pausadoHasta = DateTime.UtcNow + retryAfter;
            _logger.LogWarning("Cuota LLM agotada, reanudo en {ReanudarEn}. Detalle: {Detalle}", _pausadoHasta, ex.Message);
            return false;
        }
    }

    private IEnumerable<List<Registro>> AgruparPorOrigen(List<Registro> pendientes) =>
        pendientes
        .GroupBy(registro => registro.IdOrigenDatos)
        .SelectMany(grupo => grupo.Chunk(_tamanoLote))
        .Select(lote => lote.ToList());
}