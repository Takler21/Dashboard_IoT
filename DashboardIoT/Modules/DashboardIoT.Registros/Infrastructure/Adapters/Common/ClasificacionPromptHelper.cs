using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;

namespace DashboardIoT.Registros.Infrastructure.Adapters.Common;

internal static class ClasificacionPromptHelper
{
    private static readonly IReadOnlyList<string> _tipos = Enum.GetNames<TipoTrafico>();

    public static string ConstruirPrompt(IReadOnlyList<Registro> registros, AnonimizadorIpHelper anonimizador)
    {
        var tiposLista = string.Join(", ", _tipos);

        var prompt = $"""
            Clasifica los registros de red en uno de estos tipos: {tiposLista}.
            Devuelve {registros.Count} clasificaciones en el mismo orden de entrada.
            Justifica cada una con datos concretos del registro (puertos, comandos, IPs, etc.).
            """;

        for (var i = 0; i < registros.Count; i++)
        {
            var registro = registros[i];
            prompt += $"""

                Registro {i + 1}
                Origen: {registro.OrigenDatos.Nombre}
                Fecha: {registro.Fecha:O}
                IP origen: {anonimizador.Anonimizar(registro.IpOrigen)}
                IP destino: {anonimizador.Anonimizar(registro.IpDestino)}
                Datos: {registro.Datos.RootElement.GetRawText()}
                """;
        }

        return prompt;
    }
}