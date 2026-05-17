using Microsoft.AspNetCore.SignalR;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.Hub;
using WEB.Core.Interfaces;

namespace WEB.Features.Notificacion.Delete;

public class DeleteNotificacionHandler : IRequestHandler<DeleteNotificacionRequest, bool>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IHubContext<NotificacionHub> _hubContext;
    
    public DeleteNotificacionHandler(IUnitOfWorkAccessor uow,IHubContext<NotificacionHub> hubContext)
    {
        _uow = uow;
        _hubContext = hubContext;
    }

    public async Task<AppResult<bool>> Handle(DeleteNotificacionRequest request, CancellationToken cancellationToken)
    {
        var notificacion = await _uow.Current.Notificacion.Get(n => n.Id == request.NotificacionId, cancellationToken);

        if (notificacion == null)
            return AppResult<bool>.NotFound("Notificación no encontrada");

       
        _uow.Current.Notificacion.SoftDelete(notificacion);
        await _uow.Current.SaveAsync();
        
        await _hubContext.Clients.Group($"User_{notificacion.DestinatarioId}")
            .SendAsync("NotificacionEliminada", notificacion.Id, cancellationToken);

        return AppResult<bool>.Success(true);
    }
}