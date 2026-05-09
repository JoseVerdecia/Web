using Microsoft.AspNetCore.SignalR;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.Hub;
using WEB.Interfaces;

namespace WEB.Features.Notificacion.MarcarLeida;

public class MarcarNotificacionLeidaHandler : IRequestHandler<MarcarNotificacionLeidaRequest, bool>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IHubContext<NotificacionHub> _hubContext;

    public MarcarNotificacionLeidaHandler(IUnitOfWorkAccessor uow, IHubContext<NotificacionHub> hubContext)
    {
        _uow = uow;
        _hubContext = hubContext;
    }

    public async Task<Result<bool>> Handle(MarcarNotificacionLeidaRequest request, CancellationToken cancellationToken)
    {
        await _uow.Current.Notificacion.MarcarComoLeidaAsync(request.NotificacionId, cancellationToken);
        await _uow.Current.SaveAsync();
        
        var count = await _uow.Current.Notificacion.CountNoLeidasAsync(request.UsuarioId, cancellationToken);
        
        await _hubContext.Clients.Group($"User_{request.UsuarioId}")
            .SendAsync("NotificacionLeida", count, cancellationToken);

        return Result<bool>.Success(true);
    }
}