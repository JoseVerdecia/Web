using WEB.Features.Indicador.Dto;

namespace WEB.Features.Objetivo.Dto;

public record ObjetivoDto(
    int Id,
    string Nombre,
    Enums.Evaluacion Evaluacion,
    int NumeroObjetivo,
    DateTime? DeleteAt,
    IEnumerable<IndicadorSimpleDto> Indicadores){
    public  bool IsSelected { get; set; }
};