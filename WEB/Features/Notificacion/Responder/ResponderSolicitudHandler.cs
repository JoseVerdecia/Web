using Microsoft.AspNetCore.SignalR;
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.Hub;
using WEB.Data.IRepository;
using WEB.Enums;
using WEB.Features.Notificacion.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Notificacion.Responder;

public class ResponderSolicitudHandler : IRequestHandler<ResponderSolicitudRequest, NotificacionDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IHubContext<NotificacionHub> _hubContext;
    private readonly ILogger<ResponderSolicitudHandler> _logger;

    public ResponderSolicitudHandler(
        IUnitOfWorkAccessor uow,
        IHubContext<NotificacionHub> hubContext,
        ILogger<ResponderSolicitudHandler> logger)
    {
        _uow = uow;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<Result<NotificacionDto>> Handle(ResponderSolicitudRequest request, CancellationToken cancellationToken)
    {
        
        NotificacionModel? solicitud = await _uow.Current.Notificacion.Get(
            n => n.Id == request.SolicitudId,
            cancellationToken,
            "Remitente,IndicadorDeArea,IndicadorDeArea.Indicador,IndicadorDeArea.Area");

        if (solicitud == null)
            return Result<NotificacionDto>.NotFound("Solicitud no encontrada");
        
        if (solicitud.Tipo != TipoNotificacion.SolicitudCambioMeta)
            return Result<NotificacionDto>.Fail("La notificación no es una solicitud de cambio de meta");

        if (solicitud.Estado != EstadoNotificacion.Pendiente)
            return Result<NotificacionDto>.Fail("La solicitud ya fue respondida");

        if (solicitud.DestinatarioId != request.RespondeId)
            return Result<NotificacionDto>.Fail("No tienes permiso para responder esta solicitud");

        if (solicitud.IndicadorDeAreaId == null)
            return Result<NotificacionDto>.Fail("La solicitud no tiene un indicador de área asociado");
        
        solicitud.Estado = request.Aceptada ? EstadoNotificacion.Aceptada : EstadoNotificacion.Rechazada;
        _uow.Current.Notificacion.Update(solicitud);

        
        if (request.Aceptada && solicitud.NuevaMetaPropuestaDecimal.HasValue)
        {
            var indicadorDeArea = await _uow.Current.IndicadorDeArea.Get(
                i => i.Id == solicitud.IndicadorDeAreaId.Value, 
                cancellationToken);

            if (indicadorDeArea != null)
            {
                indicadorDeArea.MetaCumplir = solicitud.NuevaMetaPropuesta;
                indicadorDeArea.MetaCumplirDecimal = solicitud.NuevaMetaPropuestaDecimal.Value;
                indicadorDeArea.IsMetaCumplirPorcentaje = solicitud.IsNuevaMetaPorcentaje ?? false;
                
                if (!string.IsNullOrEmpty(indicadorDeArea.MetaReal))
                {
                    indicadorDeArea.MetaReal = MetaHelper.SincronizarMetaReal(
                        indicadorDeArea.MetaCumplir, 
                        indicadorDeArea.MetaReal);
                }

                _uow.Current.IndicadorDeArea.Update(indicadorDeArea);
            }
        }
        
        
        string areaNombre = solicitud.IndicadorDeArea?.Area?.Nombre ?? "Área desconocida";
        string indicadorNombre = solicitud.IndicadorDeArea?.Indicador?.Nombre ?? "Indicador desconocido";

        string cabecera = request.Aceptada 
            ? $"Solicitud de cambio aprobada"
            : $"Solicitud de cambio rechazada";

        string cuerpo = request.Aceptada
            ? $"Se aprobó el cambio de meta del indicador '{indicadorNombre}' del área '{areaNombre}' de {solicitud.MetaAnterior} a {solicitud.NuevaMetaPropuesta}"
            : $"Se rechazó el cambio de meta del indicador '{indicadorNombre}' del área '{areaNombre}'";

        NotificacionModel notificacionRespuesta = new NotificacionModel
        {
            DestinatarioId = solicitud.RemitenteId, 
            RemitenteId = request.RespondeId,      
            Cabecera = cabecera,
            Cuerpo = cuerpo,
            Tipo = request.Aceptada ? TipoNotificacion.RespuestaAceptada : TipoNotificacion.RespuestaRechazada,
            Estado = request.Aceptada ? EstadoNotificacion.Aceptada : EstadoNotificacion.Rechazada,
            MensajePersonalizado = request.MensajeRespuesta,
            SolicitudOriginalId = solicitud.Id,
            IndicadorDeAreaId = solicitud.IndicadorDeAreaId,
            MetaAnterior = solicitud.MetaAnterior,
            MetaAnteriorDecimal = solicitud.MetaAnteriorDecimal,
            IsMetaAnteriorPorcentaje = solicitud.IsMetaAnteriorPorcentaje,
            NuevaMetaPropuesta = solicitud.NuevaMetaPropuesta,
            NuevaMetaPropuestaDecimal = solicitud.NuevaMetaPropuestaDecimal,
            IsNuevaMetaPorcentaje = solicitud.IsNuevaMetaPorcentaje
        };
        
        _uow.Current.Notificacion.Add(notificacionRespuesta);
      
        
        await _uow.Current.SaveAsync();
        
        int count = await _uow.Current.Notificacion.CountNoLeidasAsync(notificacionRespuesta.DestinatarioId, cancellationToken);

        int countParaRemitente = await _uow.Current.Notificacion.CountNoLeidasAsync(solicitud.RemitenteId, cancellationToken);
        
        await _hubContext.Clients.Group($"User_{notificacionRespuesta.DestinatarioId}")
            .SendAsync("RecibirNotificacion", new
            {
                Id = notificacionRespuesta.Id,
                Cabecera = notificacionRespuesta.Cabecera,
                Cuerpo = notificacionRespuesta.Cuerpo,
                Tipo = notificacionRespuesta.Tipo.ToString(),
                Estado = notificacionRespuesta.Estado.ToString(),
                CreatedAt = notificacionRespuesta.CreatedAt,
                RemitenteNombre = notificacionRespuesta.Remitente?.FullName,
                CountNoLeidas = count
            }, cancellationToken);
        
        await _hubContext.Clients.Group($"User_{solicitud.RemitenteId}")
            .SendAsync("SolicitudActualizada", new
            {
                Id = solicitud.Id,
                Estado = solicitud.Estado.ToString(),
                CountNoLeidas = countParaRemitente
            }, cancellationToken);
        
        if (request.Aceptada)
        {
            _logger.LogInformation("Enviando MetaActualizada al grupo User_{UserId}", solicitud.RemitenteId);
            await _hubContext.Clients.Group($"User_{solicitud.RemitenteId}")
                .SendAsync("MetaActualizada", cancellationToken);
        }

        _logger.LogInformation("Respuesta a solicitud {SolicitudId}: {Estado}", solicitud.Id, request.Aceptada ? "Aceptada" : "Rechazada");

        return Result<NotificacionDto>.Success(notificacionRespuesta.MapToDto());
    }
}