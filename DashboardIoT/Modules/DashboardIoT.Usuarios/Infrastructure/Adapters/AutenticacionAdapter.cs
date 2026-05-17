using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DashboardIoT.Usuarios.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DashboardIoT.Usuarios.Infrastructure.Adapters;

public class AutenticacionAdapter(IConfiguration configuration) : IAutenticacionAdapter
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!);
    private readonly int _expiracionHoras = configuration.GetValue<int>("Jwt:TiempoTokenLogin");

    public bool VerificarContrasena(string contrasena, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(contrasena, hash);
    }

    public string GenerarToken(int usuarioId, string email, string nombre)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, nombre),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expiracionHoras),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}