using System.Text;
using DashboardIoT.Usuarios.Application.Interfaces;
using DashboardIoT.Usuarios.Application.Services;
using DashboardIoT.Usuarios.Infrastructure.Adapters;
using DashboardIoT.Usuarios.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DashboardIoT.Usuarios.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsuariosModule(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;

        services.AddDbContext<UsuariosDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IUsuariosService, UsuariosService>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddScoped<IAutenticacionAdapter, AutenticacionAdapter>();

        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                };
            });

        return services;
    }
}
