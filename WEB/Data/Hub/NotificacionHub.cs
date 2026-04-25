using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using WEB.Interfaces;

namespace WEB.Data.Hub;




public class NotificacionHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ILogger<NotificacionHub> _logger;

    public NotificacionHub(ILogger<NotificacionHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.GetHttpContext()?.Request.Query["userId"].ToString();
    
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogInformation("Usuario {UserId} conectado al grupo. ConnectionId: {ConnectionId}", 
                userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogWarning("Conexión sin userId. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogInformation("Usuario {UserId} desconectado de notificaciones", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}