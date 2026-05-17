using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardIoT.Api.Controllers.Registros;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class RegistrosController(IRegistrosService registrosService) : ControllerBase
{
    private readonly IRegistrosService _registrosService = registrosService;

    [HttpGet]
    public async Task<IActionResult> BuscarRegistros([FromQuery] FiltroRegistrosDto filtro, CancellationToken cancellationToken)
    {
        var registros = await _registrosService.BuscarRegistrosAsync(filtro, cancellationToken);
        return Ok(registros);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> SeleccionarRegistroPorId(int id, CancellationToken cancellationToken)
    {
        var registro = await _registrosService.SeleccionarRegistroPorIdAsync(id, cancellationToken);
        if (registro is null) return NotFound();
        return Ok(registro);
    }


    [HttpGet("totales-por-tipo-trafico")]
    public async Task<IActionResult> SeleccionarTotalRegistrosPorTipoTrafico(CancellationToken cancellationToken)
    {
        var resultado = await _registrosService.SeleccionarTotalRegistrosPorTipoTraficoAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> SeleccionarTotalRegistrosPendientes(CancellationToken cancellationToken)
    {
        var total = await _registrosService.SeleccionarTotalRegistrosPendientesAsync(cancellationToken);
        return Ok(new { totalRegistrosPendientes = total });
    }

    [HttpGet("origen-datos")]
    public async Task<IActionResult> SeleccionarOrigenDatos(CancellationToken cancellationToken)
    {
        var resultado = await _registrosService.SeleccionarOrigenDatosAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("clasificar")]
    public async Task<IActionResult> ClasificarPendientes(CancellationToken cancellationToken)
    {
        await _registrosService.ClasificarPendientesAsync(cancellationToken);
        return Accepted();
    }

    [HttpPost("importarcsv")]
    public async Task<IActionResult> Importar(
        [FromForm] IFormFile archivo,
        CancellationToken cancellationToken)
    {
        try
        {
            if (archivo.Length == 0)
                return BadRequest("El archivo está vacío");

            var extension = Path.GetExtension(archivo.FileName);
            if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Solo se admiten archivos CSV");

            await using var stream = archivo.OpenReadStream();
            var resumen = await _registrosService.InsertarRegistrosDesdeArchivoAsync(stream, archivo.FileName, cancellationToken);
            return Ok(resumen);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

