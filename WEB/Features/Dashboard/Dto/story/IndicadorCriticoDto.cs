namespace WEB.Features.Dashboard.Dto.story;

public class IndicadorCriticoDto
{
    public string Nombre { get; set; } = "";
    public string Proceso { get; set; } = "";
    public string MetaCumplir { get; set; } = "";
    public decimal MetaRealDecimal { get; set; }
    public double PorcentajeReal { get; set; }
}