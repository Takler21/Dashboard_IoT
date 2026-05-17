using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Application.Interfaces;

namespace DashboardIoT.Registros.Infrastructure.Adapters.Csv;

public sealed class CsvCargadorAdapter : ICargadorArchivosPort
{
    private sealed record ColumnasCsvPorFormato(string ColumnaFecha, string ColumnaIpOrigen, string ColumnaIpDestino);

    private static readonly ColumnasCsvPorFormato[] ColumnasCsvPorFormatos =
    [
        new("ts",    "id.orig_h", "id.resp_h"),
        new("stime", "saddr",     "daddr"),
    ];

    private static readonly CsvConfiguration ConfigCsv = new(CultureInfo.InvariantCulture)
    {
        DetectDelimiter = true,
        HasHeaderRecord = true,
        BadDataFound = null,
        MissingFieldFound = null,
        TrimOptions = TrimOptions.Trim,
    };

    public async IAsyncEnumerable<CamposRegistroCsv?> LeerRegistrosAsync(
        Stream archivo, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(archivo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        using var csv = new CsvReader(reader, ConfigCsv);

        if (!await csv.ReadAsync())
            yield break;

        csv.ReadHeader();
        var cabeceras = csv.HeaderRecord ?? [];
        var formato = DetectarFormato(cabeceras);
        var columnasRequeridas = ObtenerColumnasRequeridas(formato);

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Mapear(csv, cabeceras, formato, columnasRequeridas);
        }
    }

    private static HashSet<string> ObtenerColumnasRequeridas(ColumnasCsvPorFormato formato)
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            formato.ColumnaFecha, formato.ColumnaIpOrigen, formato.ColumnaIpDestino
        };
    }

    private static ColumnasCsvPorFormato DetectarFormato(string[] cabeceras)
    {
        var set = new HashSet<string>(cabeceras, StringComparer.OrdinalIgnoreCase);

        foreach (var formato in ColumnasCsvPorFormatos)
        {
            if (set.Contains(formato.ColumnaFecha) && set.Contains(formato.ColumnaIpOrigen))
                return formato;
        }

        throw new FormatException(
            "CSV no reconocido (cabeceras esperadas: Zeek ts, id.orig_h o Argus stime, saddr)");
    }

    private static CamposRegistroCsv? Mapear(CsvReader csv, string[] cabeceras, ColumnasCsvPorFormato formato, HashSet<string> columnasRequeridas)
    {
        var fechaRaw = csv.GetField(formato.ColumnaFecha);
        var ipOrigen = csv.GetField(formato.ColumnaIpOrigen);
        var ipDestino = csv.GetField(formato.ColumnaIpDestino);

        if (string.IsNullOrWhiteSpace(fechaRaw) ||
            string.IsNullOrWhiteSpace(ipOrigen) ||
            string.IsNullOrWhiteSpace(ipDestino))
            return null;

        var fecha = ParsearFecha(fechaRaw);
        if (fecha is null)
            return null;

        var datosJson = ConstruirDatosJson(csv, cabeceras, columnasRequeridas);
        return new CamposRegistroCsv(fecha.Value, ipOrigen!, ipDestino!, datosJson);
    }

    private static DateTime? ParsearFecha(string valor)
    {
        if (double.TryParse(valor, NumberStyles.Float, CultureInfo.InvariantCulture, out var epoch))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(epoch * 1000d)).UtcDateTime;
        }

        return null;
    }

    private static JsonDocument ConstruirDatosJson(CsvReader csv, string[] cabeceras, HashSet<string> excluidas)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var cabecera in cabeceras)
            {
                if (excluidas.Contains(cabecera)) continue;
                writer.WriteString(cabecera, csv.GetField(cabecera) ?? string.Empty);
            }
            writer.WriteEndObject();
        }
        stream.Position = 0;
        return JsonDocument.Parse(stream);
    }
}
