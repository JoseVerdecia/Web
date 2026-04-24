namespace WEB.Features.IndicadorDeArea.Dto;

public class IndicadorDeAreaDto
{
    public int Id { get; set; }
    public int AreaId { get; set; }
    public string AreaNombre { get; set; } = null!;

    public int IndicadorPadreId { get; set; }
    
    public string MetaCumplirIndicadorPadre { get; set; }
    public string NombreIndicadorPadre { get; set; }
    public string? MetaReal { get; set; }
    public string MetaCumplir { get; set; }
    public Enums.Evaluacion Evaluacion { get; set; }
    public string EvaluacionColor { get; set; } = string.Empty;
}