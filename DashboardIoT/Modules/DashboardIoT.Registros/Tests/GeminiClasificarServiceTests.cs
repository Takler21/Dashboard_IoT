using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;
using DashboardIoT.Registros.Infrastructure.Adapters.Gemini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace DashboardIoT.Registros.Tests;

public class GeminiClasificarServiceTests
{
    private static readonly IConfiguration _config;

    static GeminiClasificarServiceTests()
    {
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "test");
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gemini:Url"] = "https://fake",
                ["Gemini:Modelo"] = "fake",
            })
            .Build();
    }

    [Test]
    public async Task ClasificarLote_RespuestaValida_DevuelveClasificaciones()
    {
        var registros = new[] { CrearRegistro(), CrearRegistro() };
        var servicio = ConstruirServicio(HttpStatusCode.OK, EnvolverRespuestaGemini(
            """[{"tipoTrafico":"Escaneo","justificacion":"Se ha realizado un escaneo"},{"tipoTrafico":"Ddos","justificacion":"Se produjo un ataque de denegacion"}]"""));

        var resultado = await servicio.ClasificarLoteAsync(registros, CancellationToken.None);

        Assert.That(resultado!.Count, Is.EqualTo(2));
        Assert.That(resultado[0].TipoTrafico, Is.EqualTo(TipoTrafico.Escaneo));
        Assert.That(resultado[1].TipoTrafico, Is.EqualTo(TipoTrafico.Ddos));
    }

    [Test]
    public async Task ClasificarLote_TipoDesconocido_CaeAOtro()
    {
        var registros = new[] { CrearRegistro() };
        var servicio = ConstruirServicio(HttpStatusCode.OK, EnvolverRespuestaGemini(
            """[{"tipoTrafico":"Sin identificar","justificacion":"Es un tipo de ataque que no esta en los enums"}]"""));

        var resultado = await servicio.ClasificarLoteAsync(registros, CancellationToken.None);

        Assert.That(resultado![0].TipoTrafico, Is.EqualTo(TipoTrafico.Otro));
    }

    [Test]
    public void ClasificarLote_RateLimit_LanzaConRetryAfter()
    {
        var servicio = ConstruirServicio(HttpStatusCode.TooManyRequests, "", TimeSpan.FromSeconds(30));

        var ex = Assert.ThrowsAsync<HttpRequestException>(
            () => servicio.ClasificarLoteAsync(new[] { CrearRegistro() }, CancellationToken.None));

        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(ex.Data["RetryAfter"], Is.EqualTo(TimeSpan.FromSeconds(30)));
    }

    private static Registro CrearRegistro()
    {
        var registro = Registro.Crear(1, DateTime.UtcNow, "1.1.1.1", null, JsonDocument.Parse("{}"));
        typeof(Registro).GetProperty(nameof(Registro.OrigenDatos))!.SetValue(registro, new OrigenDatos());
        return registro;
    }

    private static string EnvolverRespuestaGemini(string jsonArray) =>
        JsonSerializer.Serialize(new
        {
            candidates = new[] { new { content = new { parts = new[] { new { text = jsonArray } } } } }
        });

    private static GeminiClasificarService ConstruirServicio(
        HttpStatusCode estado, string contenido, TimeSpan? retryAfter = null)
    {
        var respuesta = new HttpResponseMessage(estado)
        {
            Content = new StringContent(contenido, Encoding.UTF8, "application/json"),
        };
        if (retryAfter is not null)
            respuesta.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter.Value);

        return new GeminiClasificarService(
            new HttpClient(new FakeHttpMessageHandler(respuesta)),
            _config,
            NullLogger<GeminiClasificarService>.Instance);
    }

    private sealed class FakeHttpMessageHandler(HttpResponseMessage respuesta) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respuesta);
    }
}
