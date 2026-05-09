using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Enums;
using WEB.Features.Notificacion.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Notificacion.GetByIndicador;

public class GetNotificacionesByIndicadorHandler : IRequestHandler<GetNotificacionesByIndicadorRequest, List<NotificacionDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetNotificacionesByIndicadorHandler(IUnitOfWorkAccessor uow) => _uow = uow;

    public async Task<Result<List<NotificacionDto>>> Handle(GetNotificacionesByIndicadorRequest request, CancellationToken ct)
    {
        List<NotificacionModel> notificaciones = await _uow.Current.Notificacion
            .GetByIndicadorIdAsync(request.IndicadorId, ct);
        
        var solicitudes = notificaciones
            .Where(n => n.Tipo == TipoNotificacion.SolicitudCambioMeta)
            .Select(n => n.MapToDto())
            .ToList();

        return Result<List<NotificacionDto>>.Success(solicitudes);
    }
}