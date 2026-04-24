using WEB.Enums;

namespace WEB.Features.Dashboard.Dto.story;

public class AreaConteoDto
{
    public string Nombre { get; set; } = "";
    public AreaTipo Tipo { get; set; }
    public string Label => $"{Nombre} ({Tipo})";
    public int Total { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
}