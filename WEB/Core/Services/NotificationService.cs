using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Data;
using WEB.Data.Hub;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Core.Services;

public class NotificationService : INotificationService
{
    private readonly IToastService _toast;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IHubContext<NotificacionHub> _hubContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IToastService toast,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<NotificationService> logger,
        IHubContext<NotificacionHub> hubContext,
        ICurrentUser currentUser)
    {
        _toast = toast;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _hubContext = hubContext;
        _currentUser = currentUser;
    }

    public void ShowSuccess(string message) => _toast.ShowSuccess(message);
    public void ShowError(string message) => _toast.ShowError(message);

    public async Task<int> GetCountNoLeidasAsync(string usuarioId, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var uow = new UnitOfWork(context);
        return await uow.Notificacion.CountNoLeidasAsync(usuarioId, cancellationToken);
    }

    public async Task<IEnumerable<NotificacionModel>> GetNotificacionesAsync(
        string usuarioId,
        CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var uow = new UnitOfWork(context);
        return await uow.Notificacion.GetByUsuarioAsync(
            usuarioId,
            cancellationToken,
            "Remitente,IndicadorDeArea,IndicadorDeArea.Indicador,IndicadorDeArea.Area,SolicitudOriginal");
    }

    public async Task<IEnumerable<NotificacionModel>> GetPendientesAsync(
        string usuarioId,
        CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var uow = new UnitOfWork(context);
        return await uow.Notificacion.GetPendientesByUsuarioAsync(
            usuarioId,
            cancellationToken,
            "Remitente,IndicadorDeArea,IndicadorDeArea.Indicador,IndicadorDeArea.Area");
    }

    public async Task MarcarComoLeidaAsync(
        int notificacionId,
        string usuarioId,
        CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var uow = new UnitOfWork(context);
        await uow.Notificacion.MarcarComoLeidaAsync(notificacionId, cancellationToken);
        await uow.SaveAsync();

        var count = await GetCountNoLeidasAsync(usuarioId, cancellationToken);
        await _hubContext.Clients.Group($"User_{usuarioId}")
            .SendAsync("NotificacionLeida", count, cancellationToken);
    }

    public async Task MarcarTodasComoLeidasAsync(string usuarioId, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var uow = new UnitOfWork(context);
        await uow.Notificacion.MarcarTodasComoLeidasAsync(usuarioId, cancellationToken);
        await uow.SaveAsync();

        await _hubContext.Clients.Group($"User_{usuarioId}")
            .SendAsync("TodasNotificacionesLeidas", cancellationToken);
    }

    public async Task EnviarNotificacionAsync(NotificacionModel notificacion, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var uow = new UnitOfWork(context);
        uow.Notificacion.Add(notificacion);
        await uow.SaveAsync();

        var count = await GetCountNoLeidasAsync(notificacion.DestinatarioId, cancellationToken);

        await _hubContext.Clients.Group($"User_{notificacion.DestinatarioId}")
            .SendAsync("RecibirNotificacion", new
            {
                Id = notificacion.Id,
                Cabecera = notificacion.Cabecera,
                Cuerpo = notificacion.Cuerpo,
                Tipo = notificacion.Tipo.ToString(),
                Estado = notificacion.Estado.ToString(),
                CreatedAt = notificacion.CreatedAt,
                RemitenteNombre = notificacion.Remitente?.FullName,
                CountNoLeidas = count
            }, cancellationToken);
        
        _logger.LogInformation(
            "Enviando RecibirNotificacion al grupo User_{DestinatarioId}. Count: {Count}",
            notificacion.DestinatarioId, count);


        _logger.LogInformation(
            "Notificación {NotificacionId} enviada a {DestinatarioId}. Tipo: {Tipo}",
            notificacion.Id, notificacion.DestinatarioId, notificacion.Tipo);
    }
}