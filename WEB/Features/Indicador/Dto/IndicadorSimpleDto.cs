namespace WEB.Features.Indicador.Dto;

public record IndicadorSimpleDto(
    int Id,
    string Nombre,
    string MetaCumplir,
    string MetaReal,
    Enums.Evaluacion Evaluacion ,
    string ProcesoNombre);