using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Notificacion.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Notificacion.GetByUser;

public class GetNotificacionesByUsuarioHandler : IRequestHandler<GetNotificacionesByUsuarioRequest, List<NotificacionDto>>
{
    private readonly INotificationService _notificacionService;

    public GetNotificacionesByUsuarioHandler(INotificationService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    public async Task<Result<List<NotificacionDto>>> Handle(
        GetNotificacionesByUsuarioRequest request, 
        CancellationToken cancellationToken)
    {
        var notificaciones = await _notificacionService.GetNotificacionesAsync(
            request.UsuarioId, cancellationToken);

        var dtos = notificaciones.Select(MapToDto).ToList();

        return Result<List<NotificacionDto>>.Success(dtos);
    }

    private static NotificacionDto MapToDto(NotificacionModel n)
    {
        return new NotificacionDto
        {
            Id = n.Id,
            DestinatarioId = n.DestinatarioId,
            DestinatarioNombre = n.Destinatario?.FullName ?? "",
            RemitenteId = n.RemitenteId,
            RemitenteNombre = n.Remitente?.FullName ?? "",
            Cabecera = n.Cabecera,
            Cuerpo = n.Cuerpo,
            Tipo = n.Tipo,
            Estado = n.Estado,
            Leida = n.Leida,
            LeidaEn = n.LeidaAt,
            MensajePersonalizado = n.MensajePersonalizado,
            IndicadorDeAreaId = n.IndicadorDeAreaId,
            IndicadorNombre = n.IndicadorDeArea?.Indicador?.Nombre,
            AreaNombre = n.IndicadorDeArea?.Area?.Nombre,
            MetaAnterior = n.MetaAnterior,
            MetaAnteriorDecimal = n.MetaAnteriorDecimal,
            IsMetaAnteriorPorcentaje = n.IsMetaAnteriorPorcentaje,
            NuevaMetaPropuesta = n.NuevaMetaPropuesta,
            NuevaMetaPropuestaDecimal = n.NuevaMetaPropuestaDecimal,
            IsNuevaMetaPorcentaje = n.IsNuevaMetaPorcentaje,
            SolicitudOriginalId = n.SolicitudOriginalId,
            CreatedAt = n.CreatedAt
        };
    }
}