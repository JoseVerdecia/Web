using WEB.Enums;

namespace WEB.Common;

public class FilaObjetivo
{
    public string Numero { get; set; }
    public bool EsPrimeraDelGrupo { get; set; }
    public int RowspanProceso { get; set; }
    public string NombreProceso { get; set; }
    public int IndicadorId { get; set; }
    public string NombreIndicador { get; set; }
    public string MetaCumplir { get; set; }
    public string MetaReal { get; set; }
    public bool TieneReal { get; set; }
    public decimal? PorcentajeCumplimiento { get; set; }
    public string PorcentajeCumplimientoTexto { get; set; }
    public string ColorPorcentaje { get; set; }
    public Evaluacion Evaluacion { get; set; }
    public bool EsEsencial { get; set; }
}