using Microsoft.EntityFrameworkCore;
using WEB.Common;
using WEB.Data.IRepository;
using WEB.Enums;
using WEB.Core.Result;
using WEB.Models;

namespace WEB.Core.Helpers;

public static class EvaluacionHelper
{
    public static Evaluacion EvaluarIndicador(decimal? metaReal, decimal metaCumplir)
    {
        if (metaReal == null || metaCumplir == 0)
            return Evaluacion.NoEvaluado;

        decimal porcentajeDeCumplimiento = (metaReal.Value / metaCumplir) * 100;

        if (porcentajeDeCumplimiento > 100)
            return Evaluacion.Sobrecumplido;
        
        if (porcentajeDeCumplimiento == 100)
            return Evaluacion.Cumplido;
        
        if (porcentajeDeCumplimiento >= 80)
            return Evaluacion.ParcialmenteCumplido;

        return Evaluacion.Incumplido;
    }
    public static Result.Result EvaluarIndicadorArea(
        IndicadorDeAreaModel indicadorArea,
        string metaReal)
    {
        if (!MetaHelper.TryParsearMeta(metaReal, out decimal realDecimal, out bool isPorcentaje))
            return Result.Result.Fail("Meta real inválida");

        indicadorArea.MetaReal = metaReal;
        indicadorArea.MetaRealDecimal = realDecimal;
        indicadorArea.IsMetaRealPorcentaje = isPorcentaje;

        indicadorArea.Evaluacion =
            EvaluacionHelper.EvaluarIndicador(
                realDecimal,
                indicadorArea.MetaCumplirDecimal
            );

        return Result.Result.Success();
    }
    
    
    public static (double sobre, double cumple, double parcial, double incumple)
        CalcularPorcentajes(List<IndicadorEvaluacionData> indicadores)
    {
        if (!indicadores.Any())
            return (0, 0, 0, 0);

        double total = indicadores.Count;

        double sobre = indicadores.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido) / total * 100;
        double cumple = indicadores.Count(i => i.Evaluacion == Evaluacion.Cumplido) / total * 100;
        double parcial = indicadores.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido) / total * 100;
        double incumple = indicadores.Count(i => i.Evaluacion == Evaluacion.Incumplido) / total * 100;

        return (sobre, cumple, parcial, incumple);
    }
    public static bool EsSobrecumplido(
        (double sobre, double cumple, double parcial, double incumple) e,
        (double sobre, double cumple, double parcial, double incumple) n)
    {
        return
            e.sobre >= 60 &&
            e.cumple <= 40 &&
            e.parcial == 0 &&
            e.incumple == 0 &&

            n.sobre >= 50 &&
            n.cumple >= 40 &&
            n.parcial <= 10 &&
            n.incumple == 0;
    }
    
    public static bool EsCumplido(
        (double sobre, double cumple, double parcial, double incumple) e,
        (double sobre, double cumple, double parcial, double incumple) n)
    {
        return
            (e.sobre + e.cumple) >= 90 &&
            e.parcial <= 10 &&
            e.incumple == 0 &&

            (n.sobre + n.cumple) >= 70 &&
            n.parcial >= 20 &&
            n.incumple <= 10;
    }
    
    public static bool EsParcial(
        (double sobre, double cumple, double parcial, double incumple) e,
        (double sobre, double cumple, double parcial, double incumple) n)
    {
        return
            (e.sobre + e.cumple + e.parcial) >= 90 &&
            e.incumple <= 10 &&

            (n.sobre + n.cumple + n.parcial) >= 80 &&
            n.incumple <= 20;
    }
    
    /// <summary>
    /// Actualiza la Meta Real de un indicadorDeArea, sincronizando formato, parseando valores y recalculando la evaluación.
    /// </summary>
    public static Result.Result ActualizarMetaReal(IndicadorDeAreaModel indicador, string? nuevaMetaRealRaw)
    {
        string? metaRealSincronizada = null;
        decimal metaRealDecimal = 0;
        bool isRealPorcentaje = false;

        if (!string.IsNullOrWhiteSpace(nuevaMetaRealRaw))
        {
           
            if (!MetaHelper.TryParsearMeta(nuevaMetaRealRaw, out decimal realVal, out bool realPorc))
                return Result.Result.Fail($"La meta real '{nuevaMetaRealRaw}' no tiene un formato válido.");
            
            metaRealSincronizada = MetaHelper.SincronizarMetaReal(indicador.MetaCumplir, nuevaMetaRealRaw);
            
            if (!MetaHelper.TryParsearMeta(metaRealSincronizada, out metaRealDecimal, out isRealPorcentaje))
                return Result.Result.Fail("Error interno al sincronizar la meta real.");
        }
        
        indicador.MetaReal = metaRealSincronizada;
        indicador.MetaRealDecimal = metaRealDecimal;
        indicador.IsMetaRealPorcentaje = isRealPorcentaje;
        
        indicador.Evaluacion = indicador.MetaRealDecimal > 0 && indicador.MetaCumplirDecimal > 0
            ? EvaluarIndicador(indicador.MetaRealDecimal, indicador.MetaCumplirDecimal)
            : Evaluacion.NoEvaluado;

        return Result.Result.Success();
    }
    
    /// <summary>
    /// Actualiza la Meta a Cumplir  de un indicadorDeArea, recalculando la evaluación.
    /// Opcionalmente resincroniza la Meta Real para mantener consistencia de formatos.
    /// </summary>
    public static Result.Result ActualizarMetaCumplir(IndicadorDeAreaModel indicador, string nuevaMetaCumplirRaw)
    {
            if (string.IsNullOrWhiteSpace(nuevaMetaCumplirRaw))
                return Result.Result.Fail("La meta a cumplir no puede estar vacía.");
    
           
            if (!MetaHelper.TryParsearMeta(nuevaMetaCumplirRaw, out decimal metaCumplirDecimal, out bool isCumplirPorcentaje))
                return Result.Result.Fail($"La meta a cumplir '{nuevaMetaCumplirRaw}' no tiene un formato válido.");
    
       
            indicador.MetaCumplir = nuevaMetaCumplirRaw;
            indicador.MetaCumplirDecimal = metaCumplirDecimal;
            indicador.IsMetaCumplirPorcentaje = isCumplirPorcentaje;
            
            if (!string.IsNullOrEmpty(indicador.MetaReal))
            {
                
                var resultadoReal = ActualizarMetaReal(indicador, indicador.MetaReal);
                if (resultadoReal.IsFailure) 
                    return resultadoReal; 
            }
            else 
            {
                 indicador.Evaluacion = Evaluacion.NoEvaluado;
            }
    
            return Result.Result.Success();
    }
    
    /*public static async Task<EvaluationCountsDto> GetEvaluationCountsAsync<T>(IRepository<T> repo) where T : class
    {
        var counts = new EvaluationCountsDto();
        counts.Sobrecumplido = await repo.CountAsync(e => EF.Property<Evaluacion>(e, "Evaluacion") == Evaluacion.Sobrecumplido);
        counts.Cumplido = await repo.CountAsync(e => EF.Property<Evaluacion>(e, "Evaluacion") == Evaluacion.Cumplido);
        counts.ParcialmenteCumplido = await repo.CountAsync(e => EF.Property<Evaluacion>(e, "Evaluacion") == Evaluacion.ParcialmenteCumplido);
        counts.Incumplido = await repo.CountAsync(e => EF.Property<Evaluacion>(e, "Evaluacion") == Evaluacion.Incumplido);
        counts.NoEvaluado = await repo.CountAsync(e => EF.Property<Evaluacion>(e, "Evaluacion") == Evaluacion.NoEvaluado);
        return counts;
    }*/
}