using Microsoft.FluentUI.AspNetCore.Components.Extensions;
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
            ValorTotal = model.ValorTotal,
            ValorReal = model.ValorReal,
            MetaCumplirIndicadorPadre = model.Indicador.MetaCumplir,
            NombreIndicadorPadre = model.Indicador.Nombre,
            MetaCumplir = model.MetaCumplir,
            ValorTotalLabel = model.Indicador.ValorTotal,
            ValorRealLabel = model.Indicador.ValorReal,
            IsMetaCumplirPorcentual = model.IsMetaCumplirPorcentaje,
            MetaReal = model.MetaReal,
            Evaluacion = model.Evaluacion,
            Tipo = model.Area.Tipo
        };
    }
}