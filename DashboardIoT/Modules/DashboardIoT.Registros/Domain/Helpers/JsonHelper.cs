using System.Text;
using System.Text.Json;

namespace DashboardIoT.Registros.Domain.Helpers;

internal static class JsonHelper
{
    public static string Serializar(JsonDocument documento)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            EscribirOrdenado(writer, documento.RootElement);
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void EscribirOrdenado(Utf8JsonWriter writer, JsonElement elemento)
    {
        switch (elemento.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var propiedad in elemento.EnumerateObject().OrderBy(propiedad => propiedad.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(propiedad.Name);
                    EscribirOrdenado(writer, propiedad.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in elemento.EnumerateArray())
                {
                    EscribirOrdenado(writer, item);
                }
                writer.WriteEndArray();
                break;
            default:
                elemento.WriteTo(writer);
                break;
        }
    }
}
