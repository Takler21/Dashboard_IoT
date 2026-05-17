using DashboardIoT.Usuarios.Application.Interfaces;
using DashboardIoT.Usuarios.Application.Services;
using DashboardIoT.Usuarios.Infrastructure.Adapters;
using DashboardIoT.Usuarios.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
