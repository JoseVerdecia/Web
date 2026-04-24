namespace WEB.Features.Dashboard.Dto.story;

public class ObjetivoRankingDto
{
    public int NumeroObjetivo { get; set; }
    public string Nombre { get; set; } = "";
    public string Label => $"Objetivo #{NumeroObjetivo}";
    public int Total { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
}