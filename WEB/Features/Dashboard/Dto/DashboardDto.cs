namespace WEB.Features.Dashboard.Dto;

public class DashboardDto
{
    // Totales generales
    public int TotalIndicadores { get; set; }
    public int TotalProcesos { get; set; }
    public int TotalObjetivos { get; set; }
    public int TotalAreas { get; set; }

    // Donut: conteo por evaluación
    public EvaluacionConteoDto ConteoEvaluaciones { get; set; } = new();

    // Barras apiladas: conteo por proceso
    public List<ProcesoConteoDto> ProcesosConteo { get; set; } = new();

    // Tabla: conteo por objetivo
    public List<ObjetivoConteoDto> ObjetivosConteo { get; set; } = new();

    // Stats adicionales
    public int IndicadoresMes { get; set; }
    public int IndicadoresInternos { get; set; }
    public int IndicadoresEscenciales { get; set; }
    public int IndicadoresNecesarios { get; set; }
    public int AreasFacultad { get; set; }
    public int AreasMunicipio { get; set; }
}