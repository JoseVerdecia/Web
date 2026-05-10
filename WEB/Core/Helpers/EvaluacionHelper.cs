using System.Globalization;
using WEB.Common;
using WEB.Enums;
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
    /// Actualiza la Meta Real de un Indicador de Area, sincronizando formato, parseando valores y recalculando la evaluación.
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
    /// Actualiza la Meta a Cumplir  de un Indicador de Area, recalculando la evaluación.
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
    
    
     /// Calcula la meta real decimal para un área, según el tipo de indicador padre.
     private static decimal ObtenerMetaRealArea(IndicadorDeAreaModel area, IndicadorModel padre)
     {
         if (padre.IsMetaCumplirPorcentaje)
         {
             if (area.ValorTotal.HasValue && area.ValorTotal.Value > 0 &&
                 area.ValorReal.HasValue)
             {
                 return (area.ValorReal.Value / area.ValorTotal.Value) * 100;
             }
             return 0;
         }
         else
         {
             return area.MetaRealDecimal;
         }
     }

    /// <summary>
    /// Actualiza el IndicadorDeAreaModel (MetaReal, MetaRealDecimal, Evaluacion) 
    /// a partir de los valores ya guardados (ValorTotal, ValorReal o MetaReal).
    /// Retorna siempre Success porque no hay validación de entrada en este paso.
    /// </summary>
    public static Result.Result ActualizarEvaluacionArea(IndicadorDeAreaModel area, IndicadorModel padre)
    {
        decimal metaRealDecimal = ObtenerMetaRealArea(area, padre);
        metaRealDecimal = Math.Round(metaRealDecimal, 2);
        area.MetaRealDecimal = metaRealDecimal;
        
        string rawReal = padre.IsMetaCumplirPorcentaje
            ? $"{metaRealDecimal}%"
            : metaRealDecimal.ToString(CultureInfo.InvariantCulture);
        
        area.MetaReal = MetaHelper.SincronizarMetaReal(area.MetaCumplir, rawReal);

        area.Evaluacion = (metaRealDecimal == 0 && area.MetaCumplirDecimal == 0)
            ? Evaluacion.NoEvaluado
            : EvaluarIndicador(metaRealDecimal, area.MetaCumplirDecimal);
        
        area.IsMetaRealPorcentaje = padre.IsMetaCumplirPorcentaje;

        return Result.Result.Success();
    }

    /// <summary>
    /// Recalcula el indicador  a partir de la lista de sus áreas.
    /// Siempre retorna Success porque los datos ya están validados.
    /// </summary>
    public static Result.Result RecalcularIndicadorPadre(IndicadorModel padre, List<IndicadorDeAreaModel> areas)
    {
        if (areas == null || !areas.Any(a => !string.IsNullOrEmpty(a.MetaReal)))
        {
            padre.MetaRealDecimal = 0;
            padre.MetaReal = null;
            padre.IsMetaRealPorcentaje = false;
            padre.Evaluacion = Evaluacion.NoEvaluado;
            return Result.Result.Success();
        }

        decimal realCalculado;

        if (padre.IsMetaCumplirPorcentaje)
        {
            decimal sumTotal = areas.Sum(a => a.ValorTotal ?? 0);
            decimal sumReal  = areas.Sum(a => a.ValorReal  ?? 0);
            realCalculado = sumTotal > 0 ? (sumReal / sumTotal) * 100 : 0;
            padre.ValorTotalAcumulado = sumTotal;
            padre.ValorRealAcumulado  = sumReal;
        }
        else
        {
              realCalculado = areas.Sum(a => a.MetaRealDecimal);
                        padre.ValorTotalAcumulado = null;  
                        padre.ValorRealAcumulado  = null;
        }
        
        realCalculado = Math.Round(realCalculado, 2);

        string rawReal = padre.IsMetaCumplirPorcentaje
            ? $"{realCalculado}%"
            : realCalculado.ToString(CultureInfo.InvariantCulture);

        padre.MetaReal = MetaHelper.SincronizarMetaReal(padre.MetaCumplir, rawReal);
        padre.MetaRealDecimal = realCalculado;
        padre.IsMetaRealPorcentaje = padre.IsMetaCumplirPorcentaje;

        padre.Evaluacion = EvaluarIndicador(realCalculado, padre.MetaCumplirDecimal);
        return Result.Result.Success();
    }
    
    public static string GetEvaluacionClass(this Evaluacion evaluacion)
    {
        return evaluacion switch
        {
            Evaluacion.Sobrecumplido => "ev-sobrecumplido",
            Evaluacion.Cumplido => "ev-cumplido",
            Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
            Evaluacion.Incumplido => "ev-incumplido",
            Evaluacion.NoEvaluado => "ev-noevaluado",
            _ => ""
        };
    }
    
}