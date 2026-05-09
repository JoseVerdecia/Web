using WEB.Features.Indicador.Dto;
using WEB.Features.Objetivo.Dto;
using WEB.Models;

namespace WEB.Features.Objetivo;

public static class ObjetivoMappings
{
    public static IEnumerable<ObjetivoDto> MapToDto(this IEnumerable<ObjetivoModel> objetivos)
    {
        return objetivos.Select(o => o.MapToDto());
    }
    public static ObjetivoDto MapToDto(this ObjetivoModel objetivo)
    {
        var objetivoDto = new ObjetivoDto(
            Id: objetivo.Id,
            Nombre:objetivo.Nombre,
            NumeroObjetivo:objetivo.NumeroObjetivo,
            Evaluacion:objetivo.Evaluacion,
            DeleteAt:objetivo.DeletedAt,
            Indicadores: objetivo.Indicadores.Select(i=> new IndicadorSimpleDto(
                i.Id,
                i.Nombre,
                i.MetaCumplir,
                i.MetaReal,
                i.Evaluacion,
                i.Proceso.Nombre))
        );
        return objetivoDto;
    }
}