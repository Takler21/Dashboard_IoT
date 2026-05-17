namespace DashboardIoT.Usuarios.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string HashContrasena { get; set; } = null!;
}
