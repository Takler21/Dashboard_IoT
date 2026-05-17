namespace DashboardIoT.Registros.Infrastructure.Adapters.Common;

internal sealed class AnonimizadorIpHelper
{
    private readonly Dictionary<string, string> _ips = [];

    public string Anonimizar(string? ipReal)
    {
        if (string.IsNullOrEmpty(ipReal)) return "null";

        if (_ips.TryGetValue(ipReal, out var ipFalsa)) return ipFalsa;

        var nueva = $"10.0.0.{_ips.Count + 1}";
        _ips[ipReal] = nueva;
        return nueva;
    }

    public string RecuperarIpReal(string texto)
    {
        foreach (var ipDiccionario in _ips)
            texto = texto.Replace(ipDiccionario.Value, ipDiccionario.Key);
        return texto;
    }
}
