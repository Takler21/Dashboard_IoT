using System.Text.Json;

namespace DashboardIoT.Registros.Application.DTOs;

public sealed record CamposRegistroCsv(
    DateTime Fecha,
    string IpOrigen,
    string? IpDestino,
    JsonDocument Datos);
