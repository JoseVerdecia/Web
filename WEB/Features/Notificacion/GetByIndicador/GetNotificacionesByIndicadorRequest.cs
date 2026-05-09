using WEB.Core.Mediator;
using WEB.Features.Notificacion.Dto;

namespace WEB.Features.Notificacion.GetByIndicador;

public record GetNotificacionesByIndicadorRequest(int IndicadorId) : IRequest<List<NotificacionDto>>;