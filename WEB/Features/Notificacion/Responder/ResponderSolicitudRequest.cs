using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Notificacion.Dto;

namespace WEB.Features.Notificacion.Responder;

public record ResponderSolicitudRequest(
    int SolicitudId,
    bool Aceptada,
    string? MensajeRespuesta,
    string RespondeId) : IRequest<NotificacionDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.JefeProceso };
}