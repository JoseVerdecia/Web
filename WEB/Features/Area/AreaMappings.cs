using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using WEB.Common;
using WEB.Features.Area.Dto;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Models;

namespace WEB.Features.Area;

public static class AreaMappings
{
    public static AreaDto MapToDto(this AreaModel area)
    {
        AreaDto areaDto = new AreaDto(
            Id:area.Id,
            Nombre:area.Nombre,
            JefeAreaId:area.JefeAreaId,
            JefeArea:area.JefeArea != null ? new UserSummaryDto
            {
                Id = area.JefeArea.Id, 
                FullName = area.JefeArea.FullName, 
                Email = area.JefeArea.Email 
            } : null,
            Tipo:area.Tipo,
            IndicadoresDeArea:area.IndicadoresDeArea.Select(ia=> new IndicadorDeAreaDto{
                AreaId = ia.AreaId,
                AreaNombre = ia.Area.Nombre,
                IndicadorPadreId = ia.IndicadorId,
                MetaCumplirIndicadorPadre = ia.Indicador.MetaCumplir,
                NombreIndicadorPadre = ia.Indicador.Nombre,
                MetaReal = ia.MetaReal,
                MetaCumplir = ia.MetaCumplir,
                Evaluacion = ia.Evaluacion
            })
        );
        return areaDto;
    }
    public static IEnumerable<AreaDto> MapToDto(this IEnumerable<AreaModel> areas)
    {
        IEnumerable<AreaDto> areaDto = areas.Select(a => a.MapToDto()); 
        return areaDto;
    }
}  