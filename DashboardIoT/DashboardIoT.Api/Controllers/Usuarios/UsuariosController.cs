using DashboardIoT.Usuarios.Application.DTOs;
using DashboardIoT.Usuarios.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardIoT.Api.Controllers.Usuarios;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class UsuariosController(IUsuariosService usuariosService) : ControllerBase
{
    private readonly IUsuariosService _usuariosService = usuariosService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] CredencialesDto credenciales, CancellationToken cancellationToken)
    {
        var token = await _usuariosService.AutenticarAsync(credenciales, cancellationToken);
        if (token is null)
            return Unauthorized();

        return Ok(new { token });
    }
}
