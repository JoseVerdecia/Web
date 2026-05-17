using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Enums;
using WEB.Features.Notificacion.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Notificacion.Crear;

public class CrearSolicitudCambioMetaHandler : IRequestHandler<CrearSolicitudCambioMetaRequest, NotificacionDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly INotificationService _notificacionService;
    private readonly ILogger<CrearSolicitudCambioMetaHandler> _logger;

    public CrearSolicitudCambioMetaHandler(
        IUnitOfWorkAccessor uow,
        INotificationService notificacionService,
        ILogger<CrearSolicitudCambioMetaHandler> logger)
    {
        _uow = uow;
        _notificacionService = notificacionService;
        _logger = logger;
    }

    public async Task<AppResult<NotificacionDto>> Handle(
        CrearSolicitudCambioMetaRequest request, 
        CancellationToken cancellationToken)
    {

        var indicadorDeArea = await _uow.Current.IndicadorDeArea.Get(
            i => i.Id == request.IndicadorDeAreaId,
            cancellationToken,
            "Area,Indicador,Indicador.Proceso");

        if (indicadorDeArea == null)
            return AppResult<NotificacionDto>.NotFound("Indicador de área no encontrado");
        
        if (indicadorDeArea.Area?.JefeAreaId != request.RemitenteId)
            return AppResult<NotificacionDto>.Forbidden("No tienes permiso para modificar este indicador");

       
        if (string.IsNullOrEmpty(indicadorDeArea.Indicador?.Proceso?.JefeProcesoId))
            return AppResult<NotificacionDto>.Fail("El indicador no tiene un jefe de proceso asignado");

       
        if (!MetaHelper.TryParsearMeta(request.NuevaMetaPropuesta, out var nuevaMetaDecimal, out var isNuevaMetaPorcentaje))
            return AppResult<NotificacionDto>.Fail("El formato de la meta propuesta no es válido");
        
        var metaAnteriorFormateada = FormatearMetaParaUI(
            indicadorDeArea.MetaCumplirDecimal, 
            indicadorDeArea.IsMetaCumplirPorcentaje);
        
        var nuevaMetaFormateada = FormatearMetaParaUI(nuevaMetaDecimal, isNuevaMetaPorcentaje);

   
        var notificacion = new NotificacionModel
        {
            DestinatarioId = indicadorDeArea.Indicador.Proceso.JefeProcesoId,
            RemitenteId = request.RemitenteId,
            Cabecera = "Solicitud",
            Cuerpo = $"El área '{indicadorDeArea.Area.Nombre}' solicita cambiar la meta del indicador '{indicadorDeArea.Indicador.Nombre}'",
            Tipo = TipoNotificacion.SolicitudCambioMeta,
            Estado = EstadoNotificacion.Pendiente,
            MensajePersonalizado = request.MensajePersonalizado,
            IndicadorDeAreaId = indicadorDeArea.Id,
            MetaAnterior = metaAnteriorFormateada,
            MetaAnteriorDecimal = indicadorDeArea.MetaCumplirDecimal,
            IsMetaAnteriorPorcentaje = indicadorDeArea.IsMetaCumplirPorcentaje,
            NuevaMetaPropuesta = nuevaMetaFormateada,
            NuevaMetaPropuestaDecimal = nuevaMetaDecimal,
            IsNuevaMetaPorcentaje = isNuevaMetaPorcentaje
        };
        
        await _notificacionService.EnviarNotificacionAsync(notificacion, cancellationToken);

        _logger.LogInformation(
            "Solicitud de cambio de meta creada. IndicadorDeArea: {Id}, MetaAnterior: {Anterior}, NuevaMeta: {Nueva}",
            indicadorDeArea.Id, metaAnteriorFormateada, nuevaMetaFormateada);
        _logger.LogInformation("Enviando a grupo: User_{id}", notificacion.DestinatarioId);
        
        return AppResult<NotificacionDto>.Success(notificacion.MapToDto());
    }

    private static string FormatearMetaParaUI(decimal valor, bool esPorcentaje)
    {
        return esPorcentaje ? $"{valor}%" : valor.ToString();
    }

   
}