using WEB.Features.IndicadorDeArea.Dto;
using WEB.Models;

namespace WEB.Features.IndicadorDeArea;

public static class IndicadorDeAreaMapping
{
    public static IEnumerable<IndicadorDeAreaDto> MapToDto(this IEnumerable<IndicadorDeAreaModel> models)
    {
        IEnumerable<IndicadorDeAreaDto> dtos = models.Select(m => m.MapToDto());
        return dtos;
    }
    
    public static IndicadorDeAreaDto MapToDto(this IndicadorDeAreaModel model)
    {
        return new IndicadorDeAreaDto
        {
            Id = model.Id,
            AreaId = model.AreaId,
            AreaNombre = model.Area.Nombre,
            IndicadorPadreId = model.Indicador.Id,
            IndicadorPadreTipo = model.Indicador.Tipo,
            ValorTotal = model.ValorTotal,
            ValorReal = model.ValorReal,
            MetaCumplirIndicadorPadre = model.Indicador.MetaCumplir,
            NombreIndicadorPadre = model.Indicador.Nombre,
            ProcesoNombre = model.Indicador.Proceso.Nombre,
            MetaCumplir = model.MetaCumplir,
            MetaCumplirDecimal = model.MetaCumplirDecimal,
            MetaRealDecimal = model.MetaRealDecimal,
            ValorCualitativo =  model.ValorCualitativo,
            IsMetaCumplirPorcentual = model.IsMetaCumplirPorcentaje,
            IsRealPorcentual = model.IsMetaRealPorcentaje,
            MetaReal = model.MetaReal,
            Evaluacion = model.Evaluacion,
            Tipo = model.Area.Tipo
        };
    }
}