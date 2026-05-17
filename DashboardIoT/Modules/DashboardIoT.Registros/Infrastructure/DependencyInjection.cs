using DashboardIoT.Registros.Application.Interfaces;
using DashboardIoT.Registros.Application.Services;
using DashboardIoT.Registros.Infrastructure.Adapters.Csv;
using DashboardIoT.Registros.Infrastructure.Adapters.Gemini;
using DashboardIoT.Registros.Infrastructure.Persistence;
using DashboardIoT.Registros.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DashboardIoT.Registros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRegistrosModule(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;

        services.AddDbContext<RegistrosDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IRegistroRepository, RegistroRepository>();

        services.AddScoped<ICargadorArchivosPort, CsvCargadorAdapter>();

        services.AddScoped<IRegistrosService, RegistrosService>();

        services.AddHttpClient<ILlmClasificarService, GeminiClasificarService>();
        services.AddSingleton<ClasificacionWorker>();
        services.AddSingleton<IClasificacionTrigger>(sp => sp.GetRequiredService<ClasificacionWorker>());
        services.AddHostedService(sp => sp.GetRequiredService<ClasificacionWorker>());

        return services;
    }
}
