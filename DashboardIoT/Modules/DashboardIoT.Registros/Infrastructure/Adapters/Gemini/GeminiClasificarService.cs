using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DashboardIoT.Registros.Application.Interfaces;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;
using DashboardIoT.Registros.Infrastructure.Adapters.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DashboardIoT.Registros.Infrastructure.Adapters.Gemini;

public sealed class GeminiClasificarService(
    HttpClient http,
    IConfiguration configuration,
    ILogger<GeminiClasificarService> logger) : ILlmClasificarService
{
    private readonly HttpClient _http = http;
    private readonly ILogger<GeminiClasificarService> _logger = logger;
    private readonly string _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")!;
    private readonly string _url = configuration.GetValue<string>("Gemini:Url")!;
    private readonly string _modelo = configuration.GetValue<string>("Gemini:Modelo")!;
    private static readonly string[] _tiposTrafico = Enum.GetNames<TipoTrafico>();

    private static readonly JsonSerializerOptions _opcionesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly Dictionary<string, object> _esquemaRespuesta = new()
    {
        ["type"] = "ARRAY",
        ["items"] = new Dictionary<string, object>
        {
            ["type"] = "OBJECT",
            ["properties"] = new Dictionary<string, object>
            {
                ["tipoTrafico"] = new Dictionary<string, object>
                {
                    ["type"] = "STRING",
                    ["enum"] = _tiposTrafico,
                },
                ["justificacion"] = new Dictionary<string, object>
                {
                    ["type"] = "STRING",
                },
            },
            ["required"] = new[] { "tipoTrafico", "justificacion" },
        },
    };

    public async Task<IReadOnlyList<Clasificacion>?> ClasificarLoteAsync(
    IReadOnlyList<Registro> registros, CancellationToken cancellationToken)
    {
        var anonimizador = new AnonimizadorIpHelper();
        using var peticion = ConstruirPeticion(registros, anonimizador);

        var respuesta = await _http.SendAsync(peticion, cancellationToken);
        await ValidarRespuesta(respuesta, cancellationToken);

        var json = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        return ParsearRespuesta(json, registros, anonimizador);
    }

    private HttpRequestMessage ConstruirPeticion(IReadOnlyList<Registro> registros, AnonimizadorIpHelper anonimizador)
    {
        var content = new
        {
            contents = new[] { new { parts = new[] { new { text = ClasificacionPromptHelper.ConstruirPrompt(registros, anonimizador) } } } },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = _esquemaRespuesta
            }
        };

        var url = $"{_url}/models/{_modelo}:generateContent";
        var peticion = new HttpRequestMessage(HttpMethod.Post, url);
        peticion.Headers.Add("x-goog-api-key", _apiKey);
        peticion.Content = JsonContent.Create(content);
        return peticion;
    }

    private static async Task ValidarRespuesta(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            var segundosEspera = respuesta.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60;
            var error = new HttpRequestException(
                $"Cuota del LLM agotada ({detalle}), reintentar en {segundosEspera:F0}s",
                null, HttpStatusCode.TooManyRequests);
            error.Data["RetryAfter"] = TimeSpan.FromSeconds(segundosEspera);
            throw error;
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Gemini {(int)respuesta.StatusCode}: {error}",
                null, respuesta.StatusCode);
        }
    }

    private List<Clasificacion>? ParsearRespuesta(
    string json, IReadOnlyList<Registro> registros, AnonimizadorIpHelper anonimizador)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var resultadoLlm = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(resultadoLlm))
            {
                _logger.LogWarning("Respuesta Gemini vacía");
                return null;
            }

            var resultado = JsonSerializer.Deserialize<List<ClasificacionLlm>>(resultadoLlm, _opcionesJson);

            if (resultado is null || resultado.Count != registros.Count)
            {
                _logger.LogWarning("Esperadas: {Esperados}; recibidas: {Recibidos}",
                    registros.Count, resultado?.Count ?? 0);
                return null;
            }

            var clasificaciones = new List<Clasificacion>(registros.Count);
            for (var i = 0; i < registros.Count; i++)
            {
                if (!Enum.TryParse<TipoTrafico>(resultado[i].TipoTrafico, ignoreCase: true, out var tipo))
                {
                    tipo = TipoTrafico.Otro;
                }

                clasificaciones.Add(Clasificacion.Crear(
                    registros[i].IdRegistro,
                    registros[i],
                    tipo,
                    anonimizador.RecuperarIpReal(resultado[i].Justificacion ?? string.Empty)));
            }

            return clasificaciones;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or IndexOutOfRangeException)
        {
            _logger.LogWarning(ex, "Respuesta no parseable");
            return null;
        }
    }

    private sealed record ClasificacionLlm
    {
        [JsonPropertyName("tipoTrafico")] public string? TipoTrafico { get; set; }
        [JsonPropertyName("justificacion")] public string? Justificacion { get; set; }
    }
}
