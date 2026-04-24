/*using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Enums;
using WEB.Features.Dashboard.Dto;

namespace WEB.Features.Dashboard;

public class GetDashboardDataHandler : IRequestHandler<GetDashboardDataRequest,DashboardDto>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GetDashboardDataHandler(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardDataRequest request, CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);

        // 1. Conteo global por evaluación (1 query)
        var evaluaciones = await context.Indicador
            .GroupBy(i => i.Evaluacion)
            .Select(g => new { Evaluacion = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Evaluacion, x => x.Count, ct);

        // 2. Conteo por proceso (1 query con subconsultas)
        var procesosConteo = await context.Proceso
            .Select(p => new ProcesoConteoDto
            {
                Nombre = p.Nombre,
                Sobrecumplidos = p.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.Sobrecumplido),
                Cumplidos = p.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.Cumplido),
                ParcialmenteCumplidos = p.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.ParcialmenteCumplido),
                Incumplidos = p.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.Incumplido),
                NoEvaluados = p.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.NoEvaluado),
            })
            .OrderBy(p => p.Nombre/*#1#)
            .ToListAsync(ct);

        // 3. Conteo por objetivo (1 query con subconsultas a través de M:N)
        var objetivosConteo = await context.Objetivo
            .Select(o => new ObjetivoConteoDto
            {
              
                Sobrecumplidos = o.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.Sobrecumplido),
                Cumplidos = o.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.Cumplido),
                ParcialmenteCumplidos = o.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.ParcialmenteCumplido),
                Incumplidos = o.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.Incumplido),
                NoEvaluados = o.Indicadores.Count(i => i.Evaluacion == Enums.Evaluacion.NoEvaluado),
            })
            .OrderBy(o => o.NumeroObjetivo)
            .ToListAsync(ct);

        // 4. Origen de indicadores (1 query)
        var origenes = await context.Indicador
            .GroupBy(i => i.Origen)
            .Select(g => new { Origen = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Origen, x => x.Count, ct);

        // 5. Tipo de indicadores (1 query)
        var tipos = await context.Indicador
            .GroupBy(i => i.Tipo)
            .Select(g => new { Tipo = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Tipo, x => x.Count, ct);

        // 6. Tipo de áreas (1 query)
        var areaTipos = await context.Area
            .GroupBy(a => a.Tipo)
            .Select(g => new { Tipo = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Tipo, x => x.Count, ct);

        var dto = new DashboardDto
        {
            TotalIndicadores = evaluaciones.Values.Sum(),
            TotalProcesos = procesosConteo.Count,
            TotalObjetivos = objetivosConteo.Count,
            TotalAreas = areaTipos.Values.Sum(),

            ConteoEvaluaciones = new EvaluacionConteoDto
            {
                Sobrecumplidos = evaluaciones.GetValueOrDefault(Enums.Evaluacion.Sobrecumplido),
                Cumplidos = evaluaciones.GetValueOrDefault(Enums.Evaluacion.Cumplido),
                ParcialmenteCumplidos = evaluaciones.GetValueOrDefault(Enums.Evaluacion.ParcialmenteCumplido),
                Incumplidos = evaluaciones.GetValueOrDefault(Enums.Evaluacion.Incumplido),
                NoEvaluados = evaluaciones.GetValueOrDefault(Enums.Evaluacion.NoEvaluado),
            },

            ProcesosConteo = procesosConteo,
            ObjetivosConteo = objetivosConteo,

            IndicadoresMes = origenes.GetValueOrDefault(IndicadorOrigen.MES),
            IndicadoresInternos = origenes.GetValueOrDefault(IndicadorOrigen.Interno),
            IndicadoresEscenciales = tipos.GetValueOrDefault(IndicadorTipo.Escencial),
            IndicadoresNecesarios = tipos.GetValueOrDefault(IndicadorTipo.Necesario),
            AreasFacultad = areaTipos.GetValueOrDefault(AreaTipo.Facultad),
            AreasMunicipio = areaTipos.GetValueOrDefault(AreaTipo.Municipio),
        };

        return Result<DashboardDto>.Success(dto);
    }
}*/