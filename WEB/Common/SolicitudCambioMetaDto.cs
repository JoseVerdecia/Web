namespace WEB.Common;

public class SolicitudCambioMetaDto
{
    public int IndicadorDeAreaId { get; set; }
    public string NuevaMeta { get; set; } = null!;
    public string MensajePersonalizado { get; set; } = null!;
}