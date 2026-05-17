using DashboardIoT.Registros.Application.DTOs;

namespace DashboardIoT.Registros.Application.Interfaces;

public interface ICargadorArchivosPort
{
    IAsyncEnumerable<CamposRegistroCsv?> LeerRegistrosAsync(Stream archivo, CancellationToken cancellationToken);
}
