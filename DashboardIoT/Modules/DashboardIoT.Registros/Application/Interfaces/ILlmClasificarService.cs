using DashboardIoT.Registros.Domain.Entities;

namespace DashboardIoT.Registros.Application.Interfaces;

public interface ILlmClasificarService
{
    Task<IReadOnlyList<Clasificacion>?> ClasificarLoteAsync(
        IReadOnlyList<Registro> registros, CancellationToken cancellationToken);
}
