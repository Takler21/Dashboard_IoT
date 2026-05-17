using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DashboardIoT.Registros.Domain.Enums;
using DashboardIoT.Registros.Domain.Helpers;

namespace DashboardIoT.Registros.Domain.Entities;

public class Registro
{
    public int IdRegistro { get; private set; }
    public DateTime Fecha { get; private set; }
    public string IpOrigen { get; private set; } = null!;
    public string? IpDestino { get; private set; }
    public JsonDocument Datos { get; private set; } = null!;
    public EstadoRegistro Estado { get; private set; }
    public string HashRegistro { get; private set; } = null!;

    public int IdOrigenDatos { get; private set; }
    public OrigenDatos OrigenDatos { get; private set; } = null!;

    private Registro() { }

    public static Registro Crear(
        int idOrigenDatos,
        DateTime fechaUtc,
        string ipOrigen,
        string? ipDestino,
        JsonDocument datos)
    {
        return new Registro
        {
            IdOrigenDatos = idOrigenDatos,
            Fecha = fechaUtc,
            IpOrigen = ipOrigen,
            IpDestino = ipDestino,
            Datos = datos,
            Estado = EstadoRegistro.Pendiente,
            HashRegistro = CalcularHash(idOrigenDatos, fechaUtc, ipOrigen, ipDestino, datos),
        };
    }

    private static string CalcularHash(
        int idOrigenDatos,
        DateTime fechaUtc,
        string ipOrigen,
        string? ipDestino,
        JsonDocument datos)
    {
        var entrada = string.Concat(
            idOrigenDatos,
            fechaUtc.ToString("o"),
            ipOrigen,
            ipDestino,
            JsonHelper.Serializar(datos));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(entrada));
        return Convert.ToHexString(bytes).ToLower();
    }
}
