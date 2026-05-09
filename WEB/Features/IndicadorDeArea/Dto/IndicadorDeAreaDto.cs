using WEB.Enums;

namespace WEB.Features.IndicadorDeArea.Dto;

public class IndicadorDeAreaDto
{
    public int Id { get; set; }
    public int AreaId { get; set; }
    public string AreaNombre { get; set; } = null!;

    public int IndicadorPadreId { get; set; }
    public IndicadorTipo IndicadorPadreTipo { get; set; }
    public string ProcesoNombre { get; set; }
    
    public string MetaCumplirIndicadorPadre { get; set; }
    public string NombreIndicadorPadre { get; set; }
    public string? MetaReal { get; set; }
    public decimal MetaRealDecimal { get; set; }
    public string MetaCumplir { get; set; }
    public decimal MetaCumplirDecimal { get; set; }
    public bool IsMetaCumplirPorcentual { get; set; }
    public bool IsRealPorcentual { get; set; }
    public decimal? ValorTotal {get; set; }
    public decimal? ValorReal { get; set; }
    public string? ValorCualitativo {get;set;}
    public string? ValorTotalLabel { get; set; }
    public string? ValorRealLabel { get; set; }
    public AreaTipo Tipo { get; set; }
    public Enums.Evaluacion Evaluacion { get; set; }
    public string EvaluacionColor { get; set; } = string.Empty;
}