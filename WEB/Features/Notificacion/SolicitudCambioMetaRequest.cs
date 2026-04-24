using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Notificacion.Dto;

namespace WEB.Features.Notificacion;

public record CrearSolicitudCambioMetaRequest(
    int IndicadorDeAreaId,
    string NuevaMetaPropuesta,
    string MensajePersonalizado,
    string RemitenteId) : IRequest<NotificacionDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.JefeArea };
}