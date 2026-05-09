using WEB.Enums;
using WEB.Features.Objetivo.Dto;

namespace WEB.Common;

public class IndicadorDeAreaExtendido
{
    public int IndicadorPadreId { get; set; }
    public string NombreIndicadorPadre { get; set; }
    public List<ObjetivoSimpleDto> Objetivos { get; set; } = new();
    public bool EsEsencial { get; set; }
    public string MetaCumplir { get; set; }
    public string MetaReal { get; set; }
    public decimal MetaCumplirDecimal { get; set; }
    public decimal MetaRealDecimal { get; set; }
    public bool IsMetaCumplirPorcentual { get; set; }
    public bool IsRealPorcentual { get; set; }
    public decimal? ValorTotal { get; set; }
    public decimal? ValorReal { get; set; }
    public string ValorTotalLabel { get; set; }
    public string ValorRealLabel { get; set; }
    public Evaluacion Evaluacion { get; set; }

    public (string Text, string Color) CalcularPorcentajeCumplimiento()
    {
        if (MetaCumplirDecimal == 0 || MetaRealDecimal == 0)
            return ("0.00%", "#dc2626");
    
        var porcentaje = (MetaRealDecimal / MetaCumplirDecimal) * 100;
        var color = porcentaje switch
        {
            > 100 => "#037036",
            100 => "#05B353",
            >= 80 => "#f97316",
            _ => "#dc2626"
        };
        return ($"{porcentaje:F2}%", color);
    }
}