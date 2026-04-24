using WEB.Enums;

namespace WEB.Features.Notificacion.Dto;

public record NotificacionDto
{
    public int Id { get; init; }
    public string DestinatarioId { get; init; } = null!;
    public string DestinatarioNombre { get; init; } = null!;
    public string RemitenteId { get; init; } = null!;
    public string RemitenteNombre { get; init; } = null!;
    public string Cabecera { get; init; } = null!;
    public string Cuerpo { get; init; } = null!;
    public TipoNotificacion Tipo { get; init; }
    public EstadoNotificacion Estado { get; init; }
    public bool Leida { get; init; }
    public DateTime? LeidaEn { get; init; }
    public string? MensajePersonalizado { get; init; }
    
    // Datos de solicitud
    public int? IndicadorDeAreaId { get; init; }
    public string? IndicadorNombre { get; init; }
    public string? AreaNombre { get; init; }
    public string? MetaAnterior { get; init; }
    public decimal? MetaAnteriorDecimal { get; init; }
    public bool? IsMetaAnteriorPorcentaje { get; init; }
    public string? NuevaMetaPropuesta { get; init; }
    public decimal? NuevaMetaPropuestaDecimal { get; init; }
    public bool? IsNuevaMetaPorcentaje { get; init; }
    
    public int? SolicitudOriginalId { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public string TiempoTranscurrido => CalcularTiempoTranscurrido();
    
    private string CalcularTiempoTranscurrido()
    {
        var span = DateTime.UtcNow - CreatedAt;
        
        return span switch
        {
            { TotalMinutes: < 1 } => "Ahora mismo",
            { TotalMinutes: < 60 } => $"Hace {span.Minutes} min",
            { TotalHours: < 24 } => $"Hace {span.Hours} h",
            { TotalDays: < 7 } => $"Hace {span.Days} días",
            _ => CreatedAt.ToString("dd/MM/yyyy")
        };
    }
}

