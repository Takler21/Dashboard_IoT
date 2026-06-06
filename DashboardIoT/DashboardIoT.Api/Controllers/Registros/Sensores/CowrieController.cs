using System.Text.Json;
using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DashboardIoT.Api.Controllers.Registros.Sensores;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("honeypot")]
[AllowAnonymous]
public class CowrieController(IRegistrosService registrosService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RecibirRegistro(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            var registro = ParsearRegistro(json);
            await registrosService.InsertarRegistroSensorAsync(registro, cancellationToken);
            return Ok();
        }
        catch (Exception ex) when (ex is JsonException or FormatException)
        {
            return BadRequest(ex.Message);
        }
    }

    private static RegistroSensorDto ParsearRegistro(string json)
    {
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("event", out var evento))
            throw new FormatException("Formato de registro no válido");

        var (fecha, ipOrigen, ipDestino, nombreSensor) = ExtraerCampos(evento);

        return new RegistroSensorDto
        {
            TipoOrigen = "Cowrie",
            Fecha = fecha.UtcDateTime,
            IpOrigen = ipOrigen,
            IpDestino = ipDestino,
            NombreSensor = nombreSensor,
            Datos = JsonDocument.Parse(evento.GetRawText()),
        };
    }

    private static (DateTimeOffset Fecha, string IpOrigen, string? IpDestino, string NombreSensor) ExtraerCampos(JsonElement root)
    {
        if (!root.TryGetProperty("timestamp", out var fecha) ||
            fecha.ValueKind != JsonValueKind.String)
            throw new FormatException("Falta 'timestamp'");

        if (!root.TryGetProperty("src_ip", out var ipOrigen) ||
            ipOrigen.ValueKind != JsonValueKind.String)
            throw new FormatException("Falta 'src_ip'");

        if (!root.TryGetProperty("sensor", out var sensor) ||
            sensor.ValueKind != JsonValueKind.String)
            throw new FormatException("Falta 'sensor'");

        if (!DateTimeOffset.TryParse(fecha.GetString(), out var fechaParseada))
            throw new FormatException("'timestamp' no es ISO-8601");

        string? ipDestino = null;
        if (root.TryGetProperty("dst_ip", out var dstIp) && dstIp.ValueKind == JsonValueKind.String)
            ipDestino = dstIp.GetString();

        return (fechaParseada, ipOrigen.GetString()!, ipDestino, sensor.GetString()!);
    }
}
