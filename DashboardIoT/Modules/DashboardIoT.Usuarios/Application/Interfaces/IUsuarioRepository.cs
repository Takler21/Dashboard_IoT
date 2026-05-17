using DashboardIoT.Usuarios.Domain.Entities;

namespace DashboardIoT.Usuarios.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> SeleccionarUsuarioPorEmailAsync(string email, CancellationToken cancellationToken);
}
