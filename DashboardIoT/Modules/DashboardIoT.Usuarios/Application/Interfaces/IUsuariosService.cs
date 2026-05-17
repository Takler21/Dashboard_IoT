using DashboardIoT.Usuarios.Application.DTOs;

namespace DashboardIoT.Usuarios.Application.Interfaces;

public interface IUsuariosService
{
    Task<string?> AutenticarAsync(CredencialesDto credenciales, CancellationToken cancellationToken);
}
