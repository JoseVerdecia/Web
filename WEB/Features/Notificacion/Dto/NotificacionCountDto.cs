namespace WEB.Features.Notificacion.Dto;

public record NotificacionCountDto
{
    public int TotalNoLeidas { get; init; }
    public int SolicitudesPendientes { get; init; }
}