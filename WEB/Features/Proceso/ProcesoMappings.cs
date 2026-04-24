using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using WEB.Common;
using WEB.Features.Proceso.Dto;
using WEB.Models;

namespace WEB.Features.Proceso;

public static class ProcesoMappings
{
    public static IEnumerable<ProcesoDto> MapToDto(this IEnumerable<ProcesoModel> procesos)
    {
        var procesosDto = procesos.Select(p => p.MapToDto());
        return procesosDto;
    } 
    public static ProcesoDto MapToDto(this ProcesoModel proceso)
    {
        var procesoDto = new ProcesoDto(
            Id: proceso.Id,
            Nombre: proceso.Nombre,
            Responsable : new UserSummaryDto
            {
                Email = proceso.JefeProceso?.Email,
                FullName = proceso.JefeProceso?.FullName,
                Id = proceso.JefeProcesoId
            },
            ResponsableId: proceso.JefeProcesoId,
            Evaluacion: proceso.Evaluacion,
            Indicadores: proceso.Indicadores.Select(i=> new IndicadoresDeProcesoDto(
                Id:i.Id,
                Nombre:i.Nombre,
                MetaCumplir:i.MetaCumplir,
                MetaReal:i.MetaReal,
                Evaluacion:i.Evaluacion
            )));
        return procesoDto;
    } 
}