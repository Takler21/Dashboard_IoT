using System.Threading.RateLimiting;
using DashboardIoT.Registros.Infrastructure;
using DashboardIoT.Usuarios.Infrastructure;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();

var maxArchivoBytes = config.GetValue<long>("Importacion:MaxArchivoBytes", 1_073_741_824);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = maxArchivoBytes);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    o.MultipartBodyLengthLimit = maxArchivoBytes);

var permitidosPorMinuto = config.GetValue<int>("RateLimit:Honeypot:PermitidosPorMinuto", 600);
builder.Services.AddRateLimiter(options =>
    options.AddPolicy("honeypot", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitidosPorMinuto,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            })));

builder.Services.AddUsuariosModule();
builder.Services.AddRegistrosModule();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
