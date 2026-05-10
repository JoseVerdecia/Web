using WEB.Features.Indicador.Dto;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Proceso.Dto;
using WEB.Models;
using EnumExtensions = WEB.Core.Extensions.EnumExtensions;

namespace WEB.Features.Indicador;

public static  class IndicadorMappings
{
    public static IndicadorDto MapToDto(this IndicadorModel model)
    {
        return new IndicadorDto
        {
            Id = model.Id,
            Nombre = model.Nombre,
            MetaCumplir = model.MetaCumplir,
            MetaCumplirDecimal = model.MetaCumplirDecimal,
            IsMetaCumplirPorcentual = model.IsMetaCumplirPorcentaje,
            MetaReal = model.MetaReal,
            MetaRealDecimal = model.MetaRealDecimal,
            Origen = model.Origen,
            Tipo = model.Tipo,
            Observacion = model.Observacion,
            Evaluacion = model.Evaluacion,
            ValorTotalAcumulado = model.ValorTotalAcumulado,
            ValorRealAcumulado = model.ValorRealAcumulado,
            EvaluacionColor = EnumExtensions.GetBadgeColor(model.Evaluacion),
            DeleteAt = model.DeletedAt,
            Proceso = new ProcesoSimpleDto
            {
                Id = model.ProcesoId,
                Nombre = model.Proceso.Nombre 
            },
            
            Objetivos = model.Objetivos.Select(o => new ObjetivoSimpleDto 
            { 
                Id = o.Id, 
                Nombre = o.Nombre,
                NumeroObjetivo = o.NumeroObjetivo
            }).ToList(),
            
            Areas = model.IndicadoresDeArea.Select(ia => new IndicadorDeAreaDto
            {
                Id = ia.Id,
                AreaId = ia.AreaId,
                AreaNombre = ia.Area.Nombre,
                Tipo = ia.Area.Tipo,
                IndicadorPadreId = ia.Indicador.Id,
                IndicadorPadreTipo = ia.Indicador.Tipo,
                MetaCumplirIndicadorPadre = ia.Indicador.MetaCumplir,
                NombreIndicadorPadre = ia.Indicador.Nombre,
                MetaCumplir = ia.MetaCumplir,
                ValorCualitativo = ia.ValorCualitativo,
                ProcesoNombre = ia.Indicador.Proceso.Nombre,
                ValorTotal = ia.ValorTotal,
                ValorReal = ia.ValorReal,
                IsMetaCumplirPorcentual = ia.IsMetaCumplirPorcentaje,
                IsRealPorcentual = ia.IsMetaRealPorcentaje,
                MetaReal = ia.MetaReal,
                Evaluacion = ia.Evaluacion,
                EvaluacionColor = EnumExtensions.GetBadgeColor(ia.Evaluacion)
            }).ToList()
        };
    }
    
    public static IEnumerable<IndicadorDto> MapToDto (this IEnumerable<IndicadorModel> models)
    {
        IEnumerable<IndicadorDto> dtos = models.Select(m => m.MapToDto());
        return dtos;
    }

}