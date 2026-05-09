using WEB.Enums;

namespace WEB.Common;

public class ObjetivoResumenData
{
    public int NumeroObjetivo { get; set; }
    public string EtiquetaObjetivo => $"Objetivo {NumeroObjetivo}";
    public List<FilaResumen> Filas { get; set; } = new();
    public Evaluacion EvaluacionObjetivo { get; set; }
}