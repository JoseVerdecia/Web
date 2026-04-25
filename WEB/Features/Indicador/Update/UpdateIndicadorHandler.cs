using System.Security.Claims;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.SignalR;
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Services;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Features.Indicador.Dto;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Update;

public class UpdateIndicadorHandler : IRequestHandler<UpdateIndicadorCommand, IndicadorDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    /*private readonly IOutputCacheStore _cacheStore;*/
    private readonly ICurrentUser _currentUser;
    //private readonly IHubContext<DashboardHub> _hubContext;

    public UpdateIndicadorHandler(IUnitOfWorkAccessor uow,ICurrentUser currentUser/*,IHubContext<DashboardHub> hubContext*/)
    {
        _uow = uow;
        _currentUser = currentUser;
     //   _hubContext = hubContext;
    }

    public async Task<Result<IndicadorDto>> Handle(UpdateIndicadorCommand command, CancellationToken cancellationToken)
    {
        return await Result<UpdateIndicadorCommand>.Success(command)
            .BindAsync(ValidarIndicador)
            .BindAsync(indicador => ValidarPermisos(indicador, command)) 
            .BindAsync(indicador => ValidarProceso(indicador, command))   
            .BindAsync(indicador => ValidarObjetivos(indicador, command)) 
            .BindAsync(indicador => ValidarAreas(indicador, command))    
            .BindAsync(indicador => Task.FromResult(ActualizarIndicador(indicador, command)))
            .TapAsync(indicador => ActualizarObjetivos(indicador, command))
            .TapAsync(indicador => ActualizarAreas(indicador, command))
            .TapAsync(_ => _uow.Current.SaveAsync())
           // .TapInvalidateCacheAsync(_cacheStore,cancellationToken,CacheTags.AllIndicadores,CacheTags.IndicadorById)
        //    .TapAsync(_=> _hubContext.Clients.Group(GroupNames.Administradores).SendAsync("StatsUpdated", cancellationToken))
            .BindAsync(RecargarRelaciones)
            .Map(indicador => indicador.MapToDto());
    }

    private async Task<Result<IndicadorModel>> ValidarIndicador(UpdateIndicadorCommand command )
    {
        if (command.Id <= 0)
            return Result<IndicadorModel>.Fail("El ID del indicador es inválido.");

        var indicador = await _uow.Current.Indicador.Get(
            i => i.Id == command.Id,
            includeProperties: "Objetivos,IndicadoresDeArea,Proceso"  
        );
        return indicador == null
            ? Result<IndicadorModel>.NotFound("Indicador no encontrado")
            : Result<IndicadorModel>.Success(indicador);
    }

    private async Task<Result<IndicadorModel>> ValidarProceso(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        if (command.ProcesoId <= 0)
            return Result<IndicadorModel>.Fail("El ID del proceso es inválido.");

        var proceso = await _uow.Current.Proceso.Get(p => p.Id == command.ProcesoId);
        return proceso == null
            ? Result<IndicadorModel>.NotFound("Proceso no encontrado")
            : Result<IndicadorModel>.Success(indicador);
    }

    private async Task<Result<IndicadorModel>> ValidarObjetivos(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        if (command.ObjetivoIds == null || command.ObjetivoIds.Count == 0)
            return Result<IndicadorModel>.Fail("Debe proporcionar al menos un objetivo.");

        var objetivos = await _uow.Current.Objetivo.GetAllBy(o => command.ObjetivoIds.Contains(o.Id));
        if (objetivos.Count() != command.ObjetivoIds.Count)
            return Result<IndicadorModel>.NotFound("Algunos objetivos no fueron encontrados");

        return Result<IndicadorModel>.Success(indicador);
    }

    private async Task<Result<IndicadorModel>> ValidarAreas(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        if (command.MetaCumplirPorArea == null || command.MetaCumplirPorArea.Count == 0)
            return Result<IndicadorModel>.Success(indicador);

        var areaIds = command.MetaCumplirPorArea.Keys.ToList();
        var areas = await _uow.Current.Area.GetAllBy(a => areaIds.Contains(a.Id));
        if (areas.Count() != areaIds.Count)
            return Result<IndicadorModel>.NotFound("Algunas áreas no fueron encontradas");

        return Result<IndicadorModel>.Success(indicador);
    }
    
    private async Task<Result<IndicadorModel>> ValidarPermisos(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        var user = _currentUser.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        if (roles.Contains(AppRoles.Administrador))
            return Result<IndicadorModel>.Success(indicador);

        if (roles.Contains(AppRoles.JefeProceso))
        {
            if (indicador.Proceso?.JefeProcesoId != userId)
                return Result<IndicadorModel>.Fail("No tiene permisos para editar este indicador.");
        }

        return Result<IndicadorModel>.Success(indicador);
    }
    private Result<IndicadorModel> ActualizarIndicador(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
      
        indicador.Nombre = command.Nombre;
        indicador.Origen = command.Origen;
        indicador.Tipo = command.Tipo;
        indicador.Observacion = command.Observacion;
        indicador.ProcesoId = command.ProcesoId;
        
        return IndicadorDomainService.AplicarMetasYEvaluar(
            indicador, 
            command.MetaCumplir, 
            command.MetaReal);
    }

    private async Task ActualizarObjetivos(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        indicador.Objetivos.Clear();
        if (command.ObjetivoIds != null && command.ObjetivoIds.Any())
        {
            var objetivos = await _uow.Current.Objetivo.GetAllBy(o => command.ObjetivoIds.Contains(o.Id));
            foreach (var objetivo in objetivos)
                indicador.Objetivos.Add(objetivo);
        }
    }

    private async Task ActualizarAreas(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        var nuevasMetas = command.MetaCumplirPorArea ?? new Dictionary<int, string>();
        var nuevasAreaIds = nuevasMetas.Keys.ToHashSet();

        // Eliminar las que ya no están
        var areasAEliminar = indicador.IndicadoresDeArea
            .Where(a => !nuevasAreaIds.Contains(a.AreaId))
            .ToList();
        foreach (var area in areasAEliminar)
            indicador.IndicadoresDeArea.Remove(area);

        // Actualizar o agregar
        foreach (var kvp in nuevasMetas)
        {
            var areaExistente = indicador.IndicadoresDeArea.FirstOrDefault(a => a.AreaId == kvp.Key);
            if (areaExistente != null)
            {
                //Al actualizar la meta a cumplir, recalcular evaluación automáticamente
                var resultadoEvaluacion = EvaluacionHelper.ActualizarMetaCumplir(areaExistente, kvp.Value);
                if (resultadoEvaluacion.IsFailure)
                {
                    Result<IndicadorModel>.Fail($"Error al actualizar meta para el área ID {kvp.Key}: {resultadoEvaluacion.Errors.Select(e=>e.Message)}");
                }
            }
            else
            {
                var nuevoIndicadorDeArea = new IndicadorDeAreaModel
                {
                    AreaId = kvp.Key,
                    MetaCumplir = kvp.Value
                };
                
                var resultadoParseo = EvaluacionHelper.ActualizarMetaCumplir(nuevoIndicadorDeArea, kvp.Value);
                if (resultadoParseo.IsFailure)
                {
                    // Error en parseo - saltar este indicador
                    continue;
                }
                
                indicador.IndicadoresDeArea.Add(nuevoIndicadorDeArea);
            }
        }
    }
  

    private async Task<Result<IndicadorModel>> RecargarRelaciones(IndicadorModel indicador)
    {
        var indicadorRecargado = await _uow.Current.Indicador.Get(
            i => i.Id == indicador.Id,
            includeProperties: "Objetivos,Proceso,IndicadoresDeArea,IndicadoresDeArea.Area"
        );
        return indicadorRecargado == null
            ? Result<IndicadorModel>.Fail("Error al recargar el indicador")
            : Result<IndicadorModel>.Success(indicadorRecargado);
    }
}