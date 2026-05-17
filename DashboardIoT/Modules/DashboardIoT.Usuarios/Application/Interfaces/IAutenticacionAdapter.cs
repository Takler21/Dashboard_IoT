namespace DashboardIoT.Usuarios.Application.Interfaces;

public interface IAutenticacionAdapter
{
    bool VerificarContrasena(string contrasena, string hash);
    string GenerarToken(int usuarioId, string email, string nombre);
}