using WEB.Features.Notificacion.Dto;
using WEB.Models;

namespace WEB.Features.Notificacion;

public static class NotificacionMappings
{
    public static NotificacionDto MapToDto(this NotificacionModel notificacion)
    {
        return new NotificacionDto
        {
            Id = notificacion.Id,
            DestinatarioId = notificacion.DestinatarioId,
            DestinatarioNombre = notificacion.Destinatario?.FullName ?? "",
            RemitenteId = notificacion.RemitenteId,
            RemitenteNombre = notificacion.Remitente?.FullName ?? "",
            Cabecera = notificacion.Cabecera,
            Cuerpo = notificacion.Cuerpo,
            Tipo = notificacion.Tipo,
            Estado = notificacion.Estado,
            Leida = notificacion.Leida,
            LeidaEn = notificacion.LeidaAt,
            MensajePersonalizado = notificacion.MensajePersonalizado,
            IndicadorDeAreaId = notificacion.IndicadorDeAreaId,
            IndicadorNombre = notificacion.IndicadorDeArea?.Indicador?.Nombre,
            AreaNombre = notificacion.IndicadorDeArea?.Area?.Nombre,
            MetaAnterior = notificacion.MetaAnterior,
            MetaAnteriorDecimal = notificacion.MetaAnteriorDecimal,
            IsMetaAnteriorPorcentaje = notificacion.IsMetaAnteriorPorcentaje,
            NuevaMetaPropuesta = notificacion.NuevaMetaPropuesta,
            NuevaMetaPropuestaDecimal = notificacion.NuevaMetaPropuestaDecimal,
            IsNuevaMetaPorcentaje = notificacion.IsNuevaMetaPorcentaje,
            SolicitudOriginalId = notificacion.SolicitudOriginalId,
            CreatedAt = notificacion.CreatedAt
        };
    }
}
