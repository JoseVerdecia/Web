using WEB.Enums;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Indicador.Dto;

public class IndicadorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string MetaCumplir { get; set; } = null!;
    public decimal MetaCumplirDecimal { get; set; } 
    public bool IsMetaCumplirPorcentual { get; set; }
    public string? MetaReal { get; set; }
    public decimal? MetaRealDecimal { get; set; }
    public IndicadorOrigen Origen { get; set; }
    public IndicadorTipo Tipo { get; set; }
      public bool IsTipoEscencial { get; set; } 
    public string? Observacion { get; set; } = string.Empty;
    public Enums.Evaluacion Evaluacion { get; set; } = Enums.Evaluacion.NoEvaluado;
    public string EvaluacionColor { get; set; } = string.Empty;
    public string? ValorTotal { get; set; }
    public string? ValorReal  { get; set; }
    
    public decimal? ValorTotalAcumulado { get; set; }
    public decimal? ValorRealAcumulado { get; set; }
    public DateTime? DeleteAt { get; set; }
    public ProcesoSimpleDto Proceso { get; set; } = null!;
    public List<ObjetivoSimpleDto> Objetivos { get; set; } = new();
    public List<IndicadorDeAreaDto> Areas { get; set; } = new();
}