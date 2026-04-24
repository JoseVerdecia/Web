using WEB.Features.Proceso.Dto;

namespace WEB.Common;

public class ProcesoDisplayItem
{
    public int Id{get; set; }
    public string Nombre { get; set; }
    public UserSummaryDto Responsable{get;set;}
    public bool IsSelected { get; set; }
    public string ResponsableId {get;set;}
    public Enums.Evaluacion Evaluacion {get;set;}
    IEnumerable<IndicadoresDeProcesoDto> Indicadores{get;set;}


public static ProcesoDisplayItem FromProcesoDto(ProcesoDto procesoDto)
{
    return new ProcesoDisplayItem
    {   
        Id = procesoDto.Id,
        Nombre = procesoDto.Nombre,
        Responsable = procesoDto.Responsable,
        ResponsableId= procesoDto.ResponsableId,
        IsSelected = procesoDto.IsSelected,
        Evaluacion = procesoDto.Evaluacion,
        Indicadores=procesoDto.Indicadores
    };
}

}