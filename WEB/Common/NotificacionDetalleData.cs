using WEB.Features.Notificacion.Dto;

namespace WEB.Common;

public record NotificacionDetalleData
{
    public required NotificacionDto Notificacion { get; init; }
    public required string UsuarioId { get; init; }
    public required string UsuarioRol { get; init; }
}