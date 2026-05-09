using WEB.Enums;

namespace WEB.Common;

public record IndicadorEvaluacionData(
    IndicadorTipo Tipo,
    Evaluacion Evaluacion
);

public class EvaluacionProcesoRow
{
    public string Nombre { get; set; }
    public int Total { get; set; }
    public int Sobrecumplidos { get; set; }
    public double PorcentajeS { get; set; }
    public int Cumplidos { get; set; }
    public double PorcentajeC { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public double PorcentajePC { get; set; }
    public int Incumplidos { get; set; }
    public double PorcentajeI { get; set; }
}

public class EvaluacionProcesoData
{
    public string NombreProceso { get; set; }
    public List<EvaluacionProcesoRow> Filas { get; set; }
    public string Evaluacion { get; set; } 
}