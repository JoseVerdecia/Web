namespace WEB.Features.Dashboard.Dto;

public class DashboardUnificadoDto
{ 
    public List<EntidadCriticaDto> ObjetivosCriticos { get; set; } = new();
    public List<EntidadCriticaDto> AreasCriticas { get; set; } = new();
    public List<AreaConIndicadoresDto> AreasConIndicadores { get; set; } = new();
    public List<ObjetivoConProcesosDto> ObjetivosConProcesos { get; set; } = new();
    
    // ── Globales ──
    public int TotalIndicadores { get; set; }
    public int TotalProcesos { get; set; }
    public int TotalObjetivos { get; set; }
    public int TotalAreas { get; set; }
    public decimal PorcentajeGlobal { get; set; }

    // ── Conteos por evaluacion ──
    public ConteoEvaluacionesDto ConteoEvaluaciones { get; set; } = new();

    // ── Origen y tipo ──
    public int IndicadoresMes { get; set; }
    public int IndicadoresInternos { get; set; }
    public int IndicadoresEscenciales { get; set; }
    public int IndicadoresNecesarios { get; set; }
    public int AreasFacultad { get; set; }
    public int AreasMunicipio { get; set; }

    // ── Barras apiladas ──
    public List<ProcesoConteoDto> ProcesosConteo { get; set; } = new();
    public List<ObjetivoConteoDto> ObjetivosConteo { get; set; } = new();

    // ── Rankings ──
    public List<ObjetivoRankingDto> ObjetivosRanking { get; set; } = new();
    public List<AreaConteoDto> AreasConteo { get; set; } = new();

    // ── Comparativas ──
    public OrigenComparativoDto OrigenComparativo { get; set; } = new();

    // ── Top criticos ──
    public List<IndicadorCriticoDto> TopCincoCriticos { get; set; } = new();
}

public class ConteoEvaluacionesDto
{
    public int Sobrecumplidos { get; set; }
    public int Cumplidos { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public int Incumplidos { get; set; }
    public int NoEvaluados { get; set; }

    public int TotalEvaluados => Sobrecumplidos + Cumplidos + ParcialmenteCumplidos + Incumplidos;
}

public class ProcesoConteoDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Sobrecumplidos { get; set; }
    public int Cumplidos { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public int Incumplidos { get; set; }
    public int NoEvaluados { get; set; }

    public int Total => Sobrecumplidos + Cumplidos + ParcialmenteCumplidos + Incumplidos + NoEvaluados;
}

public class ObjetivoConteoDto
{
    public int ObjetivoId { get; set; }
    public string NumeroObjetivoString { get; set; } = string.Empty;
    public int Sobrecumplidos { get; set; }
    public int Cumplidos { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public int Incumplidos { get; set; }
    public int NoEvaluados { get; set; }

    public int Total => Sobrecumplidos + Cumplidos + ParcialmenteCumplidos + Incumplidos + NoEvaluados;
}

public class ObjetivoRankingDto
{
    public int ObjetivoId { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal PorcentajeCumplimiento { get; set; }
}

public class AreaConteoDto
{
    public int AreaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal PorcentajeCumplimiento { get; set; }
}

public class OrigenComparativoDto
{
    public int TotalMes { get; set; }
    public int CumplidosMes { get; set; }
    public decimal PorcentajeMes { get; set; }
    public int TotalInternos { get; set; }
    public int CumplidosInternos { get; set; }
    public decimal PorcentajeInternos { get; set; }
}

public class IndicadorCriticoDto
{
    public int IndicadorDeAreaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Proceso { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string MetaCumplir { get; set; } = string.Empty;
    public decimal MetaRealDecimal { get; set; }
    public decimal PorcentajeReal { get; set; }
}
