using System.Security.Claims;
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Services;
using WEB.Data;
using WEB.Features.Indicador.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Update;

public class UpdateIndicadorHandler : IRequestHandler<UpdateIndicadorCommand, IndicadorDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateIndicadorHandler(IUnitOfWorkAccessor uow,ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<AppResult<IndicadorDto>> Handle(UpdateIndicadorCommand command, CancellationToken cancellationToken)
    {
        return await AppResult<UpdateIndicadorCommand>.Success(command)
            .BindAsync(ValidarIndicador)
            .BindAsync(indicador => ValidarPermisos(indicador, command)) 
            .BindAsync(indicador => ValidarProceso(indicador, command))   
            .BindAsync(indicador => ValidarObjetivos(indicador, command)) 
            .BindAsync(indicador => ValidarAreas(indicador, command))    
            .BindAsync(indicador => Task.FromResult(ActualizarIndicador(indicador, command)))
            .TapAsync(indicador => ActualizarObjetivos(indicador, command))
            .TapAsync(indicador => ActualizarAreas(indicador, command))
            .TapAsync(RecalcularPadreSiPorcentual)
            .TapAsync(_ => _uow.Current.SaveAsync())
            .BindAsync(RecargarRelaciones)
            .Map(indicador => indicador.MapToDto());
    }

    private async Task<AppResult<IndicadorModel>> ValidarIndicador(UpdateIndicadorCommand command )
    {
        if (command.Id <= 0)
            return AppResult<IndicadorModel>.Fail("El ID del indicador es inválido.");

        var indicador = await _uow.Current.Indicador.Get(
            i => i.Id == command.Id,
            includeProperties: "Objetivos,IndicadoresDeArea,Proceso"  
        );
        return indicador == null
            ? AppResult<IndicadorModel>.NotFound("Indicador no encontrado")
            : AppResult<IndicadorModel>.Success(indicador);
    }
    private async Task RecalcularPadreSiPorcentual(IndicadorModel indicador)
    {
        if (indicador.IsMetaCumplirPorcentaje)
        {
            var areas = indicador.IndicadoresDeArea.ToList();
            var AppResult = EvaluacionHelper.RecalcularIndicadorPadre(indicador, areas);
            if (AppResult.IsFailure)
            {
                Console.WriteLine("Error al recalcular indicador padre: " + AppResult.Errors.FirstOrDefault()?.Message);
            }
        }
    }

    private async Task<AppResult<IndicadorModel>> ValidarProceso(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        if (command.ProcesoId <= 0)
            return AppResult<IndicadorModel>.Fail("El ID del proceso es inválido.");

        var proceso = await _uow.Current.Proceso.Get(p => p.Id == command.ProcesoId);
        return proceso == null
            ? AppResult<IndicadorModel>.NotFound("Proceso no encontrado")
            : AppResult<IndicadorModel>.Success(indicador);
    }

    private async Task<AppResult<IndicadorModel>> ValidarObjetivos(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        if (command.ObjetivoIds == null || command.ObjetivoIds.Count == 0)
            return AppResult<IndicadorModel>.Fail("Debe proporcionar al menos un objetivo.");

        var objetivos = await _uow.Current.Objetivo.GetAllBy(o => command.ObjetivoIds.Contains(o.Id));
        if (objetivos.Count() != command.ObjetivoIds.Count)
            return AppResult<IndicadorModel>.NotFound("Algunos objetivos no fueron encontrados");

        return AppResult<IndicadorModel>.Success(indicador);
    }

    private async Task<AppResult<IndicadorModel>> ValidarAreas(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        if (command.MetaCumplirPorArea == null || command.MetaCumplirPorArea.Count == 0)
            return AppResult<IndicadorModel>.Success(indicador);

        var areaIds = command.MetaCumplirPorArea.Keys.ToList();
        var areas = await _uow.Current.Area.GetAllBy(a => areaIds.Contains(a.Id));
        if (areas.Count() != areaIds.Count)
            return AppResult<IndicadorModel>.NotFound("Algunas áreas no fueron encontradas");

        return AppResult<IndicadorModel>.Success(indicador);
    }
    
    private async Task<AppResult<IndicadorModel>> ValidarPermisos(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
        var user = _currentUser.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        if (roles.Contains(AppRoles.Administrador))
            return AppResult<IndicadorModel>.Success(indicador);

        if (roles.Contains(AppRoles.JefeProceso))
        {
            if (indicador.Proceso?.JefeProcesoId != userId)
                return AppResult<IndicadorModel>.Fail("No tiene permisos para editar este indicador.");
        }

        return AppResult<IndicadorModel>.Success(indicador);
    }
    
    private AppResult<IndicadorModel> ActualizarIndicador(IndicadorModel indicador, UpdateIndicadorCommand command)
    {
      
        indicador.Nombre = command.Nombre;
        indicador.Origen = command.Origen;
        indicador.Tipo = command.Tipo;
        indicador.Observacion = command.Observacion;
        indicador.ProcesoId = command.ProcesoId;
        
        return IndicadorDomainService.ActualizarMetaCumplir(
            indicador, 
            command.MetaCumplir);
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
        
        var areasAEliminar = indicador.IndicadoresDeArea
            .Where(a => !nuevasAreaIds.Contains(a.AreaId))
            .ToList();
        foreach (var area in areasAEliminar)
            indicador.IndicadoresDeArea.Remove(area);
        
        foreach (var kvp in nuevasMetas)
        {
            var areaExistente = indicador.IndicadoresDeArea.FirstOrDefault(a => a.AreaId == kvp.Key);
            if (areaExistente != null)
            {
                var AppResultadoEvaluacion = EvaluacionHelper.ActualizarMetaCumplir(areaExistente, kvp.Value);
                if (AppResultadoEvaluacion.IsFailure)
                {
                    AppResult<IndicadorModel>.Fail($"Error al actualizar meta para el área ID {kvp.Key}: {AppResultadoEvaluacion.Errors.Select(e=>e.Message)}");
                }
            }
            else
            {
                var nuevoIndicadorDeArea = new IndicadorDeAreaModel
                {
                    AreaId = kvp.Key,
                    MetaCumplir = kvp.Value
                };
                
                var AppResultadoParseo = EvaluacionHelper.ActualizarMetaCumplir(nuevoIndicadorDeArea, kvp.Value);
                if (AppResultadoParseo.IsFailure)
                {
                    continue;
                }
                
                indicador.IndicadoresDeArea.Add(nuevoIndicadorDeArea);
            }
        }
        
    }
  

    private async Task<AppResult<IndicadorModel>> RecargarRelaciones(IndicadorModel indicador)
    {
        var indicadorRecargado = await _uow.Current.Indicador.Get(
            i => i.Id == indicador.Id,
            includeProperties: "Objetivos,Proceso,IndicadoresDeArea,IndicadoresDeArea.Area"
        );
        return indicadorRecargado == null
            ? AppResult<IndicadorModel>.Fail("Error al recargar el indicador")
            : AppResult<IndicadorModel>.Success(indicadorRecargado);
    }
}