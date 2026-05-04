using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.SignalR;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Services;
using WEB.Data.IRepository;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Create;

public class CreateIndicadorHandler : IRequestHandler<CreateIndicadorCommand, IndicadorDto>
{
    private readonly IUnitOfWorkAccessor _uow;
   

    public CreateIndicadorHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
      
    }

    public async Task<Result<IndicadorDto>> Handle(CreateIndicadorCommand command, CancellationToken cancellationToken)
    {
        return await Result<CreateIndicadorCommand>.Success(command)
            .BindAsync(ValidarProceso)
            .BindAsync(ValidarObjetivos)
            .BindAsync(ValidarAreas)
            .BindAsync(req => Task.FromResult(CrearIndicador(req))) 
            .TapAsync(indicador =>  AsignarObjetivos(indicador,command))
            .Tap(indicador => _uow.Current.Indicador.Add(indicador))        
            .TapAsync(_ => _uow.Current.SaveAsync())                  
            .BindAsync(RecargarRelaciones)   
            .Map(indicador => indicador.MapToDto());               
    }
    private async Task AsignarObjetivos(IndicadorModel indicador, CreateIndicadorCommand command)
    {
        if (command.ObjetivoIds == null || command.ObjetivoIds.Count == 0)
            return;
        
        var objetivos = await _uow.Current.Objetivo.GetAllBy(o => command.ObjetivoIds.Contains(o.Id));
        foreach (var obj in objetivos)
        {
            indicador.Objetivos.Add(obj);
        }
    }
    private async Task<Result<CreateIndicadorCommand>> ValidarProceso(CreateIndicadorCommand command)
    {
        if (command.ProcesoId <= 0)
            return Result<CreateIndicadorCommand>.Fail("El ID del proceso es inválido.");

        ProcesoModel? proceso = await _uow.Current.Proceso.Get(p => p.Id == command.ProcesoId);
        return proceso == null
            ? Result<CreateIndicadorCommand>.NotFound("Proceso no encontrado")
            : Result<CreateIndicadorCommand>.Success(command);
    }

    private async Task<Result<CreateIndicadorCommand>> ValidarObjetivos(CreateIndicadorCommand command)
    {
        if (command.ObjetivoIds == null || command.ObjetivoIds.Count == 0)
            return Result<CreateIndicadorCommand>.Fail("Debe proporcionar al menos un objetivo.");

        IEnumerable<ObjetivoModel> objetivos = 
            await _uow.Current.Objetivo.GetAllByAsNoTracking(o => command.ObjetivoIds.Contains(o.Id));

        return objetivos.Count() != command.ObjetivoIds.Count
            ? Result<CreateIndicadorCommand>.Fail("Uno o más objetivos no existen")
            : Result<CreateIndicadorCommand>.Success(command);
    }

    private async Task<Result<CreateIndicadorCommand>> ValidarAreas(CreateIndicadorCommand command)
    {

        if (command.MetaCumplirPorArea == null || command.MetaCumplirPorArea.Count == 0)
            return Result<CreateIndicadorCommand>.Success(command);

        List<int> areaIds = command.MetaCumplirPorArea.Keys.ToList();
        IEnumerable<AreaModel> areas = await _uow.Current.Area.GetAllBy(a => areaIds.Contains(a.Id));

        return areas.Count() != areaIds.Count
            ? Result<CreateIndicadorCommand>.NotFound("Una o más áreas no existen")
            : Result<CreateIndicadorCommand>.Success(command);
    }

    private Result<IndicadorModel> CrearIndicador(CreateIndicadorCommand command)
    {

        return IndicadorDomainService.CrearIndicador(
            nombre: command.Nombre,
            metaCumplir: command.MetaCumplir,
            origen: command.Origen,
            tipo: command.Tipo,
            observacion: command.Observacion,
            procesoId: command.ProcesoId,
            valorTotal: command.ValorTotal,
            valorReal: command.ValorReal,
            objetivoIds: command.ObjetivoIds, 
            metasPorArea: command.MetaCumplirPorArea ?? new Dictionary<int, string>());
    }
    private async Task<Result<IndicadorModel>> RecargarRelaciones(IndicadorModel indicador)
    {
        IndicadorModel? indicadorCompleto = await _uow.Current.Indicador.Get(
            i => i.Id == indicador.Id, 
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea.Area"
        );

        return Result<IndicadorModel>.Success(indicadorCompleto!);
    }
}