using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Notificacion.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.Notificacion.GetCount;

public class GetNotificacionCountHandler : IRequestHandler<GetNotificacionCountRequest, NotificacionCountDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetNotificacionCountHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<NotificacionCountDto>> Handle(
        GetNotificacionCountRequest request, 
        CancellationToken cancellationToken)
    {
        var totalNoLeidas = await _uow.Current.Notificacion.CountNoLeidasAsync(
            request.UsuarioId, cancellationToken);

        var pendientes = await _uow.Current.Notificacion.GetPendientesByUsuarioAsync(
            request.UsuarioId, cancellationToken);

        return AppResult<NotificacionCountDto>.Success(new NotificacionCountDto
        {
            TotalNoLeidas = totalNoLeidas,
            SolicitudesPendientes = pendientes.Count()
        });
    }
}