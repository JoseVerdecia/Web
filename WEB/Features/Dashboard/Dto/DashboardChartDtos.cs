namespace WEB.Features.Dashboard.Dto;

public class AreaConIndicadoresDto
{
    public int AreaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<IndicadorAreaEvaluacionDto> Indicadores { get; set; } = new();
}

public class IndicadorAreaEvaluacionDto
{
    public int IndicadorDeAreaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal MetaCumplir { get; set; }
    public decimal MetaReal { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
    public string Evaluacion { get; set; } = string.Empty;
}

// ── Para gráfico de procesos por objetivo ──
public class ObjetivoConProcesosDto
{
    public int ObjetivoId { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<ProcesoCumplimientoDto> Procesos { get; set; } = new();
}

public class ProcesoCumplimientoDto
{
    public int ProcesoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int TotalIndicadores { get; set; }
    public int Cumplidos { get; set; }
    public int Sobrecumplidos { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
}

// ── Para gráfico de objetivos/áreas críticas ──
public class EntidadCriticaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int TotalIndicadores { get; set; }
    public int Incumplidos { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public decimal PorcentajeCritico { get; set; }
}