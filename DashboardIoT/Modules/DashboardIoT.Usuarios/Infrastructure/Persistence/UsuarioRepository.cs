using DashboardIoT.Usuarios.Application.Interfaces;
using DashboardIoT.Usuarios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardIoT.Usuarios.Infrastructure.Persistence;

public class UsuarioRepository(UsuariosDbContext context) : IUsuarioRepository
{
    private readonly UsuariosDbContext _context = context;

    public async Task<Usuario?> SeleccionarUsuarioPorEmailAsync(string email, CancellationToken cancellationToken) =>
        await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
