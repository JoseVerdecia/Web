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
    IndicadorOrigen origen,
    IndicadorTipo tipo,
    string? observacion,
    int procesoId,
    string? valorTotal,
    string? valorReal,
    IEnumerable<int> objetivoIds,
    Dictionary<int, string> metasPorArea)
{

    if (!MetaHelper.TryParsearMeta(metaCumplir, out decimal metaCumplirDecimal, out bool isCumplirPorcentaje))
        return Result<IndicadorModel>.Fail($"La meta a cumplir '{metaCumplir}' no tiene un formato válido.");

    string? metaRealSincronizada = null;
    decimal metaRealDecimal = 0;
    bool isRealPorcentaje = false;
    

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
        MetaReal = null,
        MetaRealDecimal = metaRealDecimal,
        IsMetaRealPorcentaje = isRealPorcentaje,
        Origen = origen,
        Tipo = tipo,
        Observacion = observacion,
        ValorTotal = valorTotal,
        ValorReal = valorReal,
        ProcesoId = procesoId,
        Evaluacion = Evaluacion.NoEvaluado 
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
    
    public static Result<IndicadorModel> ActualizarMetaCumplir(
        IndicadorModel indicador, 
        string metaCumplir)
    {
        if (!MetaHelper.TryParsearMeta(metaCumplir, out decimal metaCumplirDecimal, out bool isCumplirPorcentaje))
            return Result<IndicadorModel>.Fail($"La meta a cumplir '{metaCumplir}' no tiene un formato válido.");

        string? metaRealSincronizada = null;
        decimal metaRealDecimal = 0;
        bool isRealPorcentaje = false;

        
        
        indicador.MetaCumplir = metaCumplir;
        indicador.MetaCumplirDecimal = metaCumplirDecimal;
        indicador.IsMetaCumplirPorcentaje = isCumplirPorcentaje;
        
        indicador.MetaRealDecimal = metaRealDecimal;
        indicador.IsMetaRealPorcentaje = isRealPorcentaje;

        indicador.Evaluacion = Evaluacion.NoEvaluado;

        return Result<IndicadorModel>.Success(indicador);
    }
}