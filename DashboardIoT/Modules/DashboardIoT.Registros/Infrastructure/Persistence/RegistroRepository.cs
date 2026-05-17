using System.Data;
using System.Text;
using DashboardIoT.Registros.Application.DTOs;
using DashboardIoT.Registros.Application.Interfaces;
using DashboardIoT.Registros.Domain.Entities;
using DashboardIoT.Registros.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DashboardIoT.Registros.Infrastructure.Persistence;

public class RegistroRepository(RegistrosDbContext context) : IRegistroRepository
{
    private readonly RegistrosDbContext _context = context;

    public async Task<Registro?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Registro
            .FirstOrDefaultAsync(r => r.IdRegistro == id, cancellationToken);
    }

    public async Task<ResultadoPaginadoDto<Clasificacion>> BuscarRegistrosAsync(FiltroRegistrosDto filtro, CancellationToken cancellationToken)
    {
        var query = _context.Clasificacion
        .AsNoTracking()
        .Include(clasificacion => clasificacion.Registro)
        .Where(clasificacion => clasificacion.Registro!.Estado == EstadoRegistro.Procesado);

        if (filtro.FechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(filtro.FechaDesde.Value, DateTimeKind.Utc);
            query = query.Where(clasificacion => clasificacion.Registro!.Fecha >= desde);
        }
        if (filtro.FechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(filtro.FechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(clasificacion => clasificacion.Registro!.Fecha <= hasta);
        }
        if (!string.IsNullOrWhiteSpace(filtro.DireccionIp))
        {
            query = query.Where(clasificacion => clasificacion.Registro!.IpOrigen == filtro.DireccionIp || clasificacion.Registro!.IpDestino == filtro.DireccionIp);
        }
        if (filtro.TipoTrafico.HasValue)
        {
            var tipoTrafico = (TipoTrafico)filtro.TipoTrafico.Value;
            query = query.Where(clasificacion => clasificacion.TipoTrafico == tipoTrafico);
        }
        if (filtro.IdOrigenDatos.HasValue)
        {
            query = query.Where(clasificacion => clasificacion.Registro!.IdOrigenDatos == filtro.IdOrigenDatos.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
        .OrderByDescending(clasificacion => clasificacion.Registro!.Fecha)
        .ThenByDescending(clasificacion => clasificacion.Registro!.IdRegistro)
        .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
        .Take(filtro.TamanoPagina)
        .ToListAsync(cancellationToken);

        return new ResultadoPaginadoDto<Clasificacion>
        {
            Items = items,
            TotalCount = totalCount,
            Pagina = filtro.Pagina,
            TamanoPagina = filtro.TamanoPagina
        };
    }

    public async Task<List<RegistrosPorTipoTraficoDto>> SeleccionarTotalRegistrosPorTipoTraficoAsync(CancellationToken cancellationToken)
    {
        return await _context.Clasificacion
        .AsNoTracking()
        .GroupBy(clasificacion => clasificacion.TipoTrafico)
        .Select(grupo => new RegistrosPorTipoTraficoDto
        {
            TipoTrafico = grupo.Key.ToString(),
            NumeroRegistros = grupo.Count()
        })
        .ToListAsync(cancellationToken);
    }

    public async Task<int> SeleccionarTotalRegistrosPendientesAsync(CancellationToken cancellationToken)
    {
        return await _context.Registro
        .AsNoTracking()
        .CountAsync(registro => registro.Estado == EstadoRegistro.Pendiente, cancellationToken);
    }

    public async Task<Clasificacion?> SeleccionarClasificacionPorRegistroIdAsync(int idRegistro, CancellationToken cancellationToken)
    {
        return await _context.Clasificacion
        .AsNoTracking()
        .FirstOrDefaultAsync(clasificacion => clasificacion.IdRegistro == idRegistro, cancellationToken);
    }

    public async Task<List<OrigenDatosDto>> SeleccionarOrigenDatosAsync(CancellationToken cancellationToken)
    {
        return await _context.OrigenDatos
            .AsNoTracking()
            .Select(origenDatos => new OrigenDatosDto { IdOrigenDatos = origenDatos.IdOrigenDatos, Nombre = origenDatos.Nombre })
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> SeleccionarIdOrigenDatosPorTipoAsync(TipoOrigen tipo, CancellationToken cancellationToken)
    {
        return await _context.OrigenDatos
        .AsNoTracking()
        .Where(origenDatos => origenDatos.Tipo == tipo)
        .Select(origenDatos => (int?)origenDatos.IdOrigenDatos)
        .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> InsertarOrigenDatosSiNoExisteAsync(TipoOrigen tipo, string nombre, CancellationToken cancellationToken)
    {
        var ids = await _context.Database
            .SqlQuery<int>($"INSERT INTO origen_datos (tipo, nombre) VALUES ({(int)tipo}, {nombre}) ON CONFLICT (tipo, nombre) DO UPDATE SET nombre = EXCLUDED.nombre RETURNING id_origen_datos")
            .ToListAsync(cancellationToken);
        return ids[0];
    }

    public async Task<(int Insertadas, int Duplicadas)> InsertarRegistrosAsync(IReadOnlyList<Registro> registros, CancellationToken cancellationToken)
    {
        var (sql, parametros) = ConstruirInsertLote(registros);
        var insertadas = await _context.Database.ExecuteSqlRawAsync(sql, parametros, cancellationToken);
        return (insertadas, registros.Count - insertadas);
    }

    private static (string Sql, object[] Parametros) ConstruirInsertLote(IReadOnlyList<Registro> registros)
    {
        const string cabecera = "INSERT INTO registro (fecha, ip_origen, ip_destino, datos, estado, id_origen_datos, hash_registro) VALUES ";
        const string sufijo = " ON CONFLICT (hash_registro) DO NOTHING;";
        const int columnasPorFila = 7;

        var sb = new StringBuilder(cabecera, capacity: cabecera.Length + registros.Count * 60 + sufijo.Length);
        var parametros = new object[registros.Count * columnasPorFila];

        for (var i = 0; i < registros.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            var primerRegistroFila = i * columnasPorFila;
            sb.Append($"({{{primerRegistroFila}}}, {{{primerRegistroFila + 1}}}, {{{primerRegistroFila + 2}}}, {{{primerRegistroFila + 3}}}::jsonb, {{{primerRegistroFila + 4}}}, {{{primerRegistroFila + 5}}}, {{{primerRegistroFila + 6}}})");

            var registro = registros[i];
            parametros[primerRegistroFila] = registro.Fecha;
            parametros[primerRegistroFila + 1] = registro.IpOrigen;
            parametros[primerRegistroFila + 2] = (object?)registro.IpDestino ?? new NpgsqlParameter { Value = DBNull.Value, DbType = DbType.String };
            parametros[primerRegistroFila + 3] = registro.Datos.RootElement.GetRawText();
            parametros[primerRegistroFila + 4] = (int)registro.Estado;
            parametros[primerRegistroFila + 5] = registro.IdOrigenDatos;
            parametros[primerRegistroFila + 6] = registro.HashRegistro;
        }

        sb.Append(sufijo);
        return (sb.ToString(), parametros);
    }

    public async Task<List<Registro>> ObtenerPendientesOrdenadosAsync(int limite, CancellationToken cancellationToken)
    {
        return await _context.Registro
            .Include(registro => registro.OrigenDatos)
            .Where(registro => registro.Estado == EstadoRegistro.Pendiente)
            .OrderBy(registro => registro.Fecha)
            .ThenBy(registro => registro.IdOrigenDatos)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task InsertarClasificacionesAsync(
        IReadOnlyList<Clasificacion> clasificaciones, CancellationToken cancellationToken)
    {
        _context.Clasificacion.AddRange(clasificaciones);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarcarRegistrosComoEstadoAsync(
        IReadOnlyList<int> idsRegistro, EstadoRegistro estado, CancellationToken cancellationToken)
    {
        await _context.Registro
            .Where(registro => idsRegistro.Contains(registro.IdRegistro))
            .ExecuteUpdateAsync(setter => setter.SetProperty(registro => registro.Estado, estado), cancellationToken);
    }
}
