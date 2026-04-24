namespace WEB.Features.Proceso.Dto;

public record IndicadoresDeProcesoDto(
    int Id,
    string Nombre,
    string MetaCumplir,
    string MetaReal,
    Enums.Evaluacion Evaluacion);