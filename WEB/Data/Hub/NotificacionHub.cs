using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace WEB.Data.Hub;




public class NotificacionHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ILogger<NotificacionHub> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificacionHub(ILogger<NotificacionHub> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
     if (!string.IsNullOrEmpty(userId))
     {
                 await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                 
                 var user = await _userManager.FindByIdAsync(userId);
                 if (user != null && await _userManager.IsInRoleAsync(user, AppRoles.Administrador))
                 {
                     await Groups.AddToGroupAsync(Context.ConnectionId, "Administradores");
                     _logger.LogInformation("Administrador {UserId} unido al grupo 'Administradores'", userId);
                 }
                 else
                 {
                     _logger.LogInformation("Usuario {UserId} no es administrador", userId);
                 }
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