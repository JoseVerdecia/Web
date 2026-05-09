﻿using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Dashboard.Dto;
using WEB.Models;

namespace WEB.Features.Dashboard;

public record GetDashboardUnificadoRequest : IRequest<DashboardUnificadoDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador, AppRoles.JefeProceso, AppRoles.JefeArea };
}

public class GetDashboardUnificadoHandler : IRequestHandler<GetDashboardUnificadoRequest, DashboardUnificadoDto>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public GetDashboardUnificadoHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<DashboardUnificadoDto>> Handle(GetDashboardUnificadoRequest request, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        try
        {
            var dto = new DashboardUnificadoDto();

            // ── Consulta base: indicadores de area con sus relaciones ──
            var indicadoresDeArea = await context.IndicadorDeArea
                .Include(i => i.Indicador)
                    .ThenInclude(ind => ind!.Proceso)
                .Include(i => i.Indicador)
                    .ThenInclude(ind => ind!.Objetivos)
                .Include(i => i.Area)
                .Where(i => !i.IsDeleted && i.Indicador != null && !i.Indicador.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Entidades distintas no eliminadas
            var indicadores = indicadoresDeArea
                .Select(i => i.Indicador!)
                .Where(ind => !ind.IsDeleted)
                .Distinct()
                .ToList();

            var procesos = await context.Proceso
                .Where(p => !p.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var objetivos = await context.Objetivo
                .Where(o => !o.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var areas = await context.Area
                .Where(a => !a.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // ── Totales globales ──
            dto.TotalIndicadores = indicadoresDeArea.Count;
            dto.TotalProcesos = procesos.Count;
            dto.TotalObjetivos = objetivos.Count;
            dto.TotalAreas = areas.Count;

            // ── Conteo de evaluaciones ──
            dto.ConteoEvaluaciones = new ConteoEvaluacionesDto
            {
                Sobrecumplidos = indicadoresDeArea.Count(i => i.Evaluacion == Enums.Evaluacion.Sobrecumplido),
                Cumplidos = indicadoresDeArea.Count(i => i.Evaluacion == Enums.Evaluacion.Cumplido),
                ParcialmenteCumplidos = indicadoresDeArea.Count(i => i.Evaluacion == Enums.Evaluacion.ParcialmenteCumplido),
                Incumplidos = indicadoresDeArea.Count(i => i.Evaluacion == Enums.Evaluacion.Incumplido),
                NoEvaluados = indicadoresDeArea.Count(i => i.Evaluacion == Enums.Evaluacion.NoEvaluado)
            };

            // Porcentaje global: (cumplidos + sobrecumplidos) / total evaluados
            var totalEvaluados = dto.TotalIndicadores - dto.ConteoEvaluaciones.NoEvaluados;
            if (totalEvaluados > 0)
            {
                dto.PorcentajeGlobal = Math.Round(
                    (decimal)(dto.ConteoEvaluaciones.Cumplidos + dto.ConteoEvaluaciones.Sobrecumplidos)
                    / totalEvaluados * 100, 1);
            }

            // ── Origen y tipo de indicadores ──
            dto.IndicadoresMes = indicadores.Count(i => i.Origen == Enums.IndicadorOrigen.MES);
            dto.IndicadoresInternos = indicadores.Count(i => i.Origen == Enums.IndicadorOrigen.Interno);
            dto.IndicadoresEscenciales = indicadores.Count(i => i.Tipo == Enums.IndicadorTipo.Escencial);
            dto.IndicadoresNecesarios = indicadores.Count(i => i.Tipo == Enums.IndicadorTipo.Necesario);
            dto.AreasFacultad = areas.Count(a => a.Tipo == Enums.AreaTipo.Facultad);
            dto.AreasMunicipio = areas.Count(a => a.Tipo == Enums.AreaTipo.CUMFUM);

            // ── ProcesosConteo: solo procesos que tienen al menos un indicador asociado ──
            var procesoIdsConIndicadores = new HashSet<int>(
                indicadores
                    .Select(i => i.ProcesoId)
                    .Distinct());

            dto.ProcesosConteo = procesos
                .Where(p => procesoIdsConIndicadores.Contains(p.Id))
                .Select(p => new ProcesoConteoDto
                {
                    Nombre = p.Nombre,
                    Sobrecumplidos = indicadoresDeArea.Count(i =>
                        i.Indicador?.ProcesoId == p.Id && i.Evaluacion == Enums.Evaluacion.Sobrecumplido),
                    Cumplidos = indicadoresDeArea.Count(i =>
                        i.Indicador?.ProcesoId == p.Id && i.Evaluacion == Enums.Evaluacion.Cumplido),
                    ParcialmenteCumplidos = indicadoresDeArea.Count(i =>
                        i.Indicador?.ProcesoId == p.Id && i.Evaluacion == Enums.Evaluacion.ParcialmenteCumplido),
                    Incumplidos = indicadoresDeArea.Count(i =>
                        i.Indicador?.ProcesoId == p.Id && i.Evaluacion == Enums.Evaluacion.Incumplido),
                    NoEvaluados = indicadoresDeArea.Count(i =>
                        i.Indicador?.ProcesoId == p.Id && i.Evaluacion == Enums.Evaluacion.NoEvaluado)
                })
                .OrderBy(p => p.Nombre)
                .ToList();

            // ── ObjetivosConteo: solo objetivos con al menos un indicador ──
            var objetivoIdsConIndicadores = new HashSet<int>(
                indicadores
                    .SelectMany(i => i.Objetivos)
                    .Select(o => o.Id)
                    .Distinct());

            dto.ObjetivosConteo = objetivos
                .Where(o => objetivoIdsConIndicadores.Contains(o.Id))
                .Select(o =>
                {
                    var delObjetivo = indicadoresDeArea
                        .Where(i => i.Indicador != null && i.Indicador.Objetivos.Any(oi => oi.Id == o.Id))
                        .ToList();

                    return new ObjetivoConteoDto
                    {
                        ObjetivoId = o.Id,
                        NumeroObjetivoString = $"OBJETIVO#:{o.NumeroObjetivo}",
                        Sobrecumplidos = delObjetivo.Count(i => i.Evaluacion == Enums.Evaluacion.Sobrecumplido),
                        Cumplidos = delObjetivo.Count(i => i.Evaluacion == Enums.Evaluacion.Cumplido),
                        ParcialmenteCumplidos = delObjetivo.Count(i => i.Evaluacion == Enums.Evaluacion.ParcialmenteCumplido),
                        Incumplidos = delObjetivo.Count(i => i.Evaluacion == Enums.Evaluacion.Incumplido),
                        NoEvaluados = delObjetivo.Count(i => i.Evaluacion == Enums.Evaluacion.NoEvaluado)
                    };
                })
                .Where(o => o.Sobrecumplidos + o.Cumplidos + o.ParcialmenteCumplidos + o.Incumplidos + o.NoEvaluados > 0)
                .ToList();

            // ── Ranking Objetivos (peor a mejor) ──
            dto.ObjetivosRanking = objetivos
                .Select(o =>
                {
                    var delObjetivo = indicadoresDeArea
                        .Where(i => i.Indicador != null && i.Indicador.Objetivos.Any(oi => oi.Id == o.Id))
                        .ToList();

                    var totalConEvaluacion = delObjetivo.Count(i => i.Evaluacion != Enums.Evaluacion.NoEvaluado);
                    var cumplidos = delObjetivo.Count(i => 
                        i.Evaluacion == Enums.Evaluacion.Cumplido || 
                        i.Evaluacion == Enums.Evaluacion.Sobrecumplido);

                    return new ObjetivoRankingDto
                    {
                        ObjetivoId = o.Id,
                        Label = $"OBJETIVO#: {o.NumeroObjetivo}",
                        PorcentajeCumplimiento = totalConEvaluacion == 0
                            ? 0
                            : Math.Round((decimal)cumplidos / totalConEvaluacion * 100, 1)
                    };
                })
                .OrderBy(r => r.PorcentajeCumplimiento)
                .ToList();

            // ── Rendimiento por Area: solo areas con al menos un indicador evaluado ──
            var areaIdsConIndicadores = new HashSet<int>(
                indicadoresDeArea
                    .Select(i => i.AreaId)
                    .Distinct());

            dto.AreasConteo = areas
                .Where(a => areaIdsConIndicadores.Contains(a.Id))
                .Select(a =>
                {
                    var delArea = indicadoresDeArea
                        .Where(i => i.AreaId == a.Id)
                        .ToList();

                    var totalEvaluados = delArea.Count(i => i.Evaluacion != Enums.Evaluacion.NoEvaluado);
                    var cumplidos = delArea.Count(i =>
                        i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido);

                    return new AreaConteoDto
                    {
                        AreaId = a.Id,
                        Nombre = a.Nombre,
                        Label = a.Nombre,
                        PorcentajeCumplimiento = totalEvaluados == 0
                            ? 0
                            : Math.Round((decimal)cumplidos / totalEvaluados * 100, 1)
                    };
                })
                .OrderBy(a => a.PorcentajeCumplimiento)
                .ToList();

            // ── Comparativa MES vs Interno ──
            var indicadoresMesIds = new HashSet<int>(
                indicadores
                    .Where(i => i.Origen == Enums.IndicadorOrigen.MES)
                    .Select(i => i.Id));

            var indicadoresInternosIds = new HashSet<int>(
                indicadores
                    .Where(i => i.Origen == Enums.IndicadorOrigen.Interno)
                    .Select(i => i.Id));

            var cumplidosMesIds = indicadoresDeArea
                .Where(i => indicadoresMesIds.Contains(i.IndicadorId)
                         && (i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido))
                .Select(i => i.IndicadorId)
                .Distinct()
                .Count();

            var cumplidosInternosIds = indicadoresDeArea
                .Where(i => indicadoresInternosIds.Contains(i.IndicadorId)
                         && (i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido))
                .Select(i => i.IndicadorId)
                .Distinct()
                .Count();

            dto.OrigenComparativo = new OrigenComparativoDto
            {
                TotalMes = indicadoresMesIds.Count,
                CumplidosMes = cumplidosMesIds,
                PorcentajeMes = indicadoresMesIds.Count == 0
                    ? 0
                    : Math.Round((decimal)cumplidosMesIds / indicadoresMesIds.Count * 100, 1),
                TotalInternos = indicadoresInternosIds.Count,
                CumplidosInternos = cumplidosInternosIds,
                PorcentajeInternos = indicadoresInternosIds.Count == 0
                    ? 0
                    : Math.Round((decimal)cumplidosInternosIds / indicadoresInternosIds.Count * 100, 1)
            };

            // ── Top 5 Criticos: mayor brecha (menor porcentaje real), solo evaluados con meta > 0 ──
            dto.TopCincoCriticos = indicadoresDeArea
                .Where(i => i.Evaluacion != Enums.Evaluacion.NoEvaluado
                         && i.MetaCumplirDecimal > 0)
                .Select(i => new IndicadorCriticoDto
                {
                    IndicadorDeAreaId = i.Id,
                    Nombre = i.Indicador?.Nombre ?? "Sin nombre",
                    Proceso = i.Indicador?.Proceso?.Nombre ?? "",
                    Area = i.Area?.Nombre ?? "",
                    MetaCumplir = i.MetaCumplir ?? "",
                    MetaRealDecimal = i.MetaRealDecimal,
                    PorcentajeReal = Math.Round(i.MetaRealDecimal / i.MetaCumplirDecimal * 100, 1)
                })
                .OrderBy(i => i.PorcentajeReal)
                .Take(5)
                .ToList();
            
             dto.AreasConIndicadores = areas
                .Where(a => areaIdsConIndicadores.Contains(a.Id))
                .Select(a => new AreaConIndicadoresDto
                {
                    AreaId = a.Id,
                    Nombre = a.Nombre,
                    Indicadores = indicadoresDeArea
                        .Where(ia => ia.AreaId == a.Id && ia.Indicador != null)
                        .Select(ia => new IndicadorAreaEvaluacionDto
                        {
                            IndicadorDeAreaId = ia.Id,
                            Nombre = ia.Indicador!.Nombre,
                            MetaCumplir = ia.MetaCumplirDecimal,
                            MetaReal = ia.MetaRealDecimal,
                            PorcentajeCumplimiento = ia.MetaCumplirDecimal > 0 
                                ? Math.Round(ia.MetaRealDecimal / ia.MetaCumplirDecimal * 100, 1) 
                                : 0,
                            Evaluacion = ia.Evaluacion.ToString()
                        }).ToList()
                })
                .ToList();


             dto.ObjetivosConProcesos = objetivos
                 .Select(o => new ObjetivoConProcesosDto
                 {
                     ObjetivoId = o.Id,
                     Label = $"OBJETIVO#: {o.NumeroObjetivo}", 
                     Procesos = procesos
                         .Where(p => indicadores.Any(i => 
                             i.ProcesoId == p.Id && 
                             i.Objetivos.Any(oi => oi.Id == o.Id)))
                         .Select(p =>
                         {
                             // ✅ FIX CRÍTICO: Obtener solo los IDs de los indicadores que pertenecen a ESTE objetivo
                             var indicadoresIdsDelObjetivo = indicadores
                                 .Where(i => i.ProcesoId == p.Id && i.Objetivos.Any(oi => oi.Id == o.Id))
                                 .Select(i => i.Id)
                                 .ToHashSet();

                             // ✅ FIX: Filtrar las áreas usando solo los indicadores del objetivo seleccionado
                             var indsDelProceso = indicadoresDeArea
                                 .Where(ia => ia.Indicador != null && indicadoresIdsDelObjetivo.Contains(ia.IndicadorId))
                                 .ToList();

                             var totalProceso = indsDelProceso.Count;
                             var buenosProceso = indsDelProceso.Count(ia => 
                                 ia.Evaluacion == Enums.Evaluacion.Cumplido || ia.Evaluacion == Enums.Evaluacion.Sobrecumplido);

                             return new ProcesoCumplimientoDto
                             {
                                 ProcesoId = p.Id,
                                 Nombre = p.Nombre,
                                 TotalIndicadores = totalProceso,
                                 Cumplidos = indsDelProceso.Count(ia => ia.Evaluacion == Enums.Evaluacion.Cumplido),
                                 Sobrecumplidos = indsDelProceso.Count(ia => ia.Evaluacion == Enums.Evaluacion.Sobrecumplido),
                                 PorcentajeCumplimiento = totalProceso > 0 
                                     ? Math.Round((decimal)buenosProceso / totalProceso * 100, 1) 
                                     : 0
                             };
                         })
                         .ToList()
                 })
                 .Where(o => o.Procesos.Any())
                 .ToList();


            // ── 3. Objetivos Críticos (Los que tienen más incumplidos/parciales) ──
            dto.ObjetivosCriticos = dto.ObjetivosConteo
                .Where(o => o.Incumplidos > 0 || o.ParcialmenteCumplidos > 0)
                .Select(o => new EntidadCriticaDto
                {
                    Id = o.ObjetivoId,
                    Nombre = o.NumeroObjetivoString,
                    TotalIndicadores = o.Sobrecumplidos + o.Cumplidos + o.ParcialmenteCumplidos + o.Incumplidos + o.NoEvaluados,
                    Incumplidos = o.Incumplidos,
                    ParcialmenteCumplidos = o.ParcialmenteCumplidos,
                    PorcentajeCritico = (o.Sobrecumplidos + o.Cumplidos + o.ParcialmenteCumplidos + o.Incumplidos) > 0 
                        ? Math.Round((decimal)(o.Incumplidos + o.ParcialmenteCumplidos) / (o.Sobrecumplidos + o.Cumplidos + o.ParcialmenteCumplidos + o.Incumplidos) * 100, 1) 
                        : 0
                })
                .OrderByDescending(o => o.PorcentajeCritico)
                .ToList();


            // ── 4. Áreas Críticas (Las que tienen más incumplidos/parciales) ──
            dto.AreasCriticas = areas
                .Where(a => areaIdsConIndicadores.Contains(a.Id))
                .Select(a =>
                {
                    var indsArea = indicadoresDeArea.Where(i => i.AreaId == a.Id).ToList();
                    var inc = indsArea.Count(i => i.Evaluacion == Enums.Evaluacion.Incumplido);
                    var par = indsArea.Count(i => i.Evaluacion == Enums.Evaluacion.ParcialmenteCumplido);
                    var total = indsArea.Count;
                    
                    return new EntidadCriticaDto
                    {
                        Id = a.Id,
                        Nombre = a.Nombre,
                        TotalIndicadores = total,
                        Incumplidos = inc,
                        ParcialmenteCumplidos = par,
                        PorcentajeCritico = total > 0 ? Math.Round((decimal)(inc + par) / total * 100, 1) : 0
                    };
                })
                .Where(a => a.Incumplidos > 0 || a.ParcialmenteCumplidos > 0)
                .OrderByDescending(a => a.PorcentajeCritico)
                .ToList();

            

            return Result<DashboardUnificadoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DashboardUnificadoDto>.Fail($"Error al generar el dashboard: {ex.Message}");
        }
        
    }

    /// <summary>
    /// Calcula el porcentaje de cumplimiento de un area.
    /// Solo considera indicadores que han sido evaluados (distinto de NoEvaluado).
    /// </summary>
    private static decimal CalcularPorcentajeCumplimientoArea(int areaId, List<IndicadorDeAreaModel> indicadores)
    {
        var delArea = indicadores
            .Where(i => i.AreaId == areaId && i.Evaluacion != Enums.Evaluacion.NoEvaluado)
            .ToList();

        if (delArea.Count == 0)
            return 0;

        var cumplidos = delArea.Count(i =>
            i.Evaluacion == Enums.Evaluacion.Cumplido || i.Evaluacion == Enums.Evaluacion.Sobrecumplido);

        return Math.Round((decimal)cumplidos / delArea.Count * 100, 1);
    }
}