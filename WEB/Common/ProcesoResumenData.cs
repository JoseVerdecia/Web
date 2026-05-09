using WEB.Enums;

namespace WEB.Common;

public class ProcesoResumenData
{
    public string NombreProceso { get; set; } = "";
    public List<FilaResumen> Filas { get; set; } = new();
    public Evaluacion EvaluacionProceso { get; set; }
}