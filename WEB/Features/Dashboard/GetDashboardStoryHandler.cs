/*
using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Enums;
using WEB.Features.Dashboard.Dto.story;

namespace WEB.Features.Dashboard.Dto;

public class GetDashboardStoryHandler: IRequestHandler<GetDashboardStoryRequest,DashboardStoryDto>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GetDashboardStoryHandler(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }
    public async Task<Result<DashboardStoryDto>> Handle(GetDashboardStoryRequest request, CancellationToken ct)
    {
       await using var context = await _dbFactory.CreateDbContextAsync(ct);

        // 1. Porcentaje Global
        var total = await context.Indicador.CountAsync(ct);
        var cumplidos = await context.Indicador.CountAsync(i => 
            i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido, ct);
        
        double porcentajeGlobal = total == 0 ? 0 : Math.Round((double)(cumplidos * 100) / total, 1);

        // 2. Ranking de Objetivos
        var objetivosRaw = await context.Objetivo.Select(o => new 
            {
                o.NumeroObjetivo,
                o.Nombre,
                Total = o.Indicadores.Count,
                Cumplidos = o.Indicadores.Count(i => 
                    i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido)
            }).ToListAsync(ct);

        var objetivosOrdenados = objetivosRaw.Select(o => new ObjetivoRankingDto
        {
            NumeroObjetivo = o.NumeroObjetivo,
            Nombre = o.Nombre,
            Total = o.Total,
            PorcentajeCumplimiento = o.Total == 0 ? 0 : Math.Round(((decimal)o.Cumplidos / o.Total) * 100, 1)
        }).OrderBy(o => o.PorcentajeCumplimiento).ToList(); // Del peor al mejor

        // 3. Desglose por Áreas
        var areasRaw = await context.Area
            .Select(a => new 
            {
                a.Nombre,
                a.Tipo,
                Total = a.IndicadoresDeArea.Count,
                Cumplidos = a.IndicadoresDeArea.Count(i => 
                    i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido)
            }).ToListAsync(ct);

        var areasOrdenadas = areasRaw.Select(a => new AreaConteoDto
        {
            Nombre = a.Nombre,
            PorcentajeCumplimiento = a.Total == 0 ? 0 : Math.Round(((decimal)a.Cumplidos / a.Total) * 100, 1)
        }).OrderBy(a => a.PorcentajeCumplimiento).ToList();

        // 4. MES vs Internos
        var origenMes = await context.Indicador.Where(i => i.Origen == IndicadorOrigen.MES)
            .GroupBy(i => 1)
            .Select(g => new { Total = g.Count(), Cumplidos = g.Count(i => i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido) })
            .FirstOrDefaultAsync(ct);

        var origenInterno = await context.Indicador.Where(i => i.Origen == IndicadorOrigen.Interno)
            .GroupBy(i => 1)
            .Select(g => new { Total = g.Count(), Cumplidos = g.Count(i => i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido) })
            .FirstOrDefaultAsync(ct);

        var origen = new OrigenComparativoDto
        {
            TotalMes = origenMes?.Total ?? 0, CumplidosMes = origenMes?.Cumplidos ?? 0,
            TotalInternos = origenInterno?.Total ?? 0, CumplidosInternos = origenInterno?.Cumplidos ?? 0
        };
        origen.PorcentajeMes = origen.TotalMes == 0 ? 0 : Math.Round((double)(origen.CumplidosMes * 100) / origen.TotalMes, 1);
        origen.PorcentajeInternos = origen.TotalInternos == 0 ? 0 : Math.Round((double)(origen.CumplidosInternos * 100) / origen.TotalInternos, 1);

        // 5. Top 5 Críticos (Menor porcentaje Real vs Meta)
        var criticosRaw = await context.Indicador
            .Where(i => i.MetaCumplirDecimal > 0)
            .Select(i => new 
            {
                i.Nombre,
                ProcesoNombre = i.Proceso.Nombre,
                i.MetaCumplir,
                i.MetaRealDecimal,
                i.MetaCumplirDecimal
            })
            .ToListAsync(ct);

// PASO 2: Hacer el cálculo y ordenamiento en memoria (C# puro)
        var criticos = criticosRaw
            .Select(i => new IndicadorCriticoDto
            {
                Nombre = i.Nombre,
                Proceso = i.ProcesoNombre,
                MetaCumplir = i.MetaCumplir,
                MetaRealDecimal = i.MetaRealDecimal,
                PorcentajeReal = Math.Round((double)(i.MetaRealDecimal / i.MetaCumplirDecimal) * 100, 1)
            })
            .OrderBy(x => x.PorcentajeReal) // Del peor al mejor
            .Take(5)
            .ToList();

        return Result<DashboardStoryDto>.Success(new DashboardStoryDto
        {
            PorcentajeGlobal = porcentajeGlobal,
            TotalIndicadores = total,
            ObjetivosRanking = objetivosOrdenados,
            AreasConteo = areasOrdenadas,
            OrigenComparativo = origen,
            TopCincoCriticos = criticos
        });
    }
}
*/
