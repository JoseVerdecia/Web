using WEB.Common;
using WEB.Enums;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.Area.Dto;

public record AreaDto(
    int Id,
    string Nombre,
    string? JefeAreaId,
    UserSummaryDto? JefeArea,
    AreaTipo Tipo,
    IEnumerable<IndicadorDeAreaDto> IndicadoresDeArea
)
{
    public  bool IsSelected { get; set; }
};