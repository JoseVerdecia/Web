namespace WEB.Features.Dashboard.Dto.story;

public class DashboardStoryDto
{
    public double PorcentajeGlobal { get; set; }
    public int TotalIndicadores { get; set; }
    
    public List<ObjetivoRankingDto> ObjetivosRanking { get; set; } = new();
    public List<AreaConteoDto> AreasConteo { get; set; } = new();
    public OrigenComparativoDto OrigenComparativo { get; set; } = new();
    public List<IndicadorCriticoDto> TopCincoCriticos { get; set; } = new();
}