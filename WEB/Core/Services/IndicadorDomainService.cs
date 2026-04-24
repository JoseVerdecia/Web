using WEB.Core.Helpers;
using WEB.Core.Result;
using WEB.Enums;
using WEB.Models;

namespace WEB.Core.Services;

public static class IndicadorDomainService
{
    /// <summary>
    /// Metodo para crear un IndicadorModel
    /// </summary>
    /// <param name="nombre">Nombre del Indicador</param>
    /// <param name="metaCumplir">Meta a cumplir del Indicador</param>
    /// <param name="metaReal">MetaReal del Indicador</param>
    /// <param name="origen">Origen del Indicador</param>
    /// <param name="tipo">Tipo de Indicador</param>
    /// <param name="observacion"></param>
    /// <param name="procesoId">Proceso a asignar al Indicador</param>
    /// <param name="objetivoIds">Lista de Objetivos a asignar al Indicador</param>
    /// <param name="metasPorArea">Diccionario<AreaId,MetaCumplirArea> para las metas de los IndicadorDeArea a asignar al Indicador</param>
    /// <returns></returns>
  public static Result<IndicadorModel> CrearIndicador(
    string nombre,
    string metaCumplir,
    string? metaReal,
    IndicadorOrigen origen,
    IndicadorTipo tipo,
    string? observacion,
    int procesoId,
    IEnumerable<int> objetivoIds,
    Dictionary<int, string> metasPorArea)
{

    if (!MetaHelper.TryParsearMeta(metaCumplir, out decimal metaCumplirDecimal, out bool isCumplirPorcentaje))
        return Result<IndicadorModel>.Fail($"La meta a cumplir '{metaCumplir}' no tiene un formato válido.");

    string? metaRealSincronizada = null;
    decimal metaRealDecimal = 0;
    bool isRealPorcentaje = false;

  
    if (!string.IsNullOrWhiteSpace(metaReal))
    {
        if (!MetaHelper.TryParsearMeta(metaReal, out decimal realVal, out bool realPorc))
            return Result<IndicadorModel>.Fail($"La meta real '{metaReal}' no tiene un formato válido.");
        
        metaRealSincronizada = MetaHelper.SincronizarMetaReal(metaCumplir, metaReal);
     
        if (!MetaHelper.TryParsearMeta(metaRealSincronizada, out metaRealDecimal, out isRealPorcentaje))
            return Result<IndicadorModel>.Fail("Error interno al sincronizar la meta real.");
    }

    // Validar metas por área
    foreach (var kvp in metasPorArea)
    {
        if (!MetaHelper.TryParsearMeta(kvp.Value, out _, out _))
            return Result<IndicadorModel>.Fail($"La meta para el área {kvp.Key} '{kvp.Value}' no tiene un formato válido.");
    }

 
    IndicadorModel indicador = new IndicadorModel
    {
        Nombre = nombre,
        MetaCumplir = metaCumplir,
        MetaCumplirDecimal = metaCumplirDecimal,
        IsMetaCumplirPorcentaje = isCumplirPorcentaje,
        MetaReal = metaRealSincronizada ?? metaReal,
        MetaRealDecimal = metaRealDecimal,
        IsMetaRealPorcentaje = isRealPorcentaje,
        Origen = origen,
        Tipo = tipo,
        Observacion = observacion,
        ProcesoId = procesoId,
        Evaluacion = string.IsNullOrWhiteSpace(metaReal) 
            ? Evaluacion.NoEvaluado 
            : EvaluacionHelper.EvaluarIndicador(metaRealDecimal, metaCumplirDecimal)
    };
    
    foreach (var kvp in metasPorArea)
    {
        MetaHelper.TryParsearMeta(kvp.Value, out decimal metaAreaDecimal, out bool isAreaPorcentaje);
        indicador.IndicadoresDeArea.Add(new IndicadorDeAreaModel
        {
            AreaId = kvp.Key,
            MetaCumplir = kvp.Value,
            MetaCumplirDecimal = metaAreaDecimal,
            IsMetaCumplirPorcentaje = isAreaPorcentaje,
            MetaReal = null,
            MetaRealDecimal = 0,
            IsMetaRealPorcentaje = false,
            Evaluacion = Evaluacion.NoEvaluado
        });
    }

    return Result<IndicadorModel>.Success(indicador);
}
    
    public static Result<IndicadorModel> AplicarMetasYEvaluar(
        IndicadorModel indicador, 
        string metaCumplir, 
        string? metaReal)
    {
        if (!MetaHelper.TryParsearMeta(metaCumplir, out decimal metaCumplirDecimal, out bool isCumplirPorcentaje))
            return Result<IndicadorModel>.Fail($"La meta a cumplir '{metaCumplir}' no tiene un formato válido.");

        string? metaRealSincronizada = null;
        decimal metaRealDecimal = 0;
        bool isRealPorcentaje = false;

        if (!string.IsNullOrWhiteSpace(metaReal))
        {
            if (!MetaHelper.TryParsearMeta(metaReal, out decimal realVal, out bool realPorc))
                return Result<IndicadorModel>.Fail($"La meta real '{metaReal}' no tiene un formato válido.");

            metaRealSincronizada = MetaHelper.SincronizarMetaReal(metaCumplir, metaReal);

            if (!MetaHelper.TryParsearMeta(metaRealSincronizada, out metaRealDecimal, out isRealPorcentaje))
                return Result<IndicadorModel>.Fail("Error interno al sincronizar la meta real.");
        }
        
        indicador.MetaCumplir = metaCumplir;
        indicador.MetaCumplirDecimal = metaCumplirDecimal;
        indicador.IsMetaCumplirPorcentaje = isCumplirPorcentaje;
    
        indicador.MetaReal = metaRealSincronizada ?? metaReal;
        indicador.MetaRealDecimal = metaRealDecimal;
        indicador.IsMetaRealPorcentaje = isRealPorcentaje;
        
        indicador.Evaluacion = string.IsNullOrWhiteSpace(metaReal) 
            ? Evaluacion.NoEvaluado 
            : EvaluacionHelper.EvaluarIndicador(metaRealDecimal, metaCumplirDecimal);

        return Result<IndicadorModel>.Success(indicador);
    }
}