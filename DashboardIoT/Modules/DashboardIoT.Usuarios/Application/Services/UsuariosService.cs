using DashboardIoT.Usuarios.Application.DTOs;
using DashboardIoT.Usuarios.Application.Interfaces;

namespace DashboardIoT.Usuarios.Application.Services;

public class UsuariosService(IUsuarioRepository usuarioRepository, IAutenticacionAdapter autenticacion) : IUsuariosService
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IAutenticacionAdapter _autenticacion = autenticacion;

    public async Task<string?> AutenticarAsync(CredencialesDto credenciales, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.SeleccionarUsuarioPorEmailAsync(credenciales.Email, cancellationToken);
        if (usuario is null || !_autenticacion.VerificarContrasena(credenciales.Contrasena, usuario.HashContrasena))
            return null;

        return _autenticacion.GenerarToken(usuario.IdUsuario, usuario.Email, usuario.Nombre);
    }
}
