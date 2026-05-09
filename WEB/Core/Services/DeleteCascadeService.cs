using WEB.Interfaces;
using WEB.Models;

namespace WEB.Core.Services;

public class DeleteCascadeService : IDeleteCascadeService
{
    private readonly IUnitOfWorkAccessor _uow;

    public DeleteCascadeService(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

   

    public async Task SoftDeleteProceso(ProcesoModel? proceso)
    {
        if (proceso == null) return;

        var procesoFull = await _uow.Current.Proceso.GetIncludingDeleted(
            p => p.Id == proceso.Id,
            CancellationToken.None,
            includeProperties: "Indicadores");
        
        var indicadorIds = procesoFull.Indicadores.Select(i => i.Id).ToList();

        if (indicadorIds.Any())
        {
            var areas = await _uow.Current.IndicadorDeArea
                .GetAllByIncludingDeleted(i => indicadorIds.Contains(i.IndicadorId), CancellationToken.None);

            _uow.Current.IndicadorDeArea.DeleteRange(areas);
            
            var indicadores = await _uow.Current.Indicador
                .GetAllByIncludingDeleted(i => indicadorIds.Contains(i.Id), CancellationToken.None);

            _uow.Current.Indicador.DeleteRange(indicadores);
        }
        
        _uow.Current.Proceso.SoftDelete(procesoFull);

        await _uow.Current.SaveAsync();
    }

    public async Task SoftDeleteIndicador(IndicadorModel? indicador)
    {
        if (indicador == null || indicador.IsDeleted) return;

        indicador = await _uow.Current.Indicador.GetIncludingDeleted(
            i => i.Id == indicador.Id, 
            CancellationToken.None, 
            includeProperties: "IndicadoresDeArea");

        foreach (var ia in indicador.IndicadoresDeArea)
            if (!ia.IsDeleted) 
                _uow.Current.IndicadorDeArea.SoftDelete(ia);

        _uow.Current.Indicador.SoftDelete(indicador);
        await _uow.Current.SaveAsync();
    }

    public async Task SoftDeleteObjetivo(ObjetivoModel? objetivo)
    {
        if (objetivo == null) return;

        objetivo = await _uow.Current.Objetivo.GetIncludingDeleted(
            o => o.Id == objetivo.Id, 
            CancellationToken.None, 
            includeProperties: "Indicadores");

        foreach (var indicador in objetivo.Indicadores.ToList())
        {
            var indicadorCompleto = await _uow.Current.Indicador.GetIncludingDeleted(
                i => i.Id == indicador.Id, 
                CancellationToken.None, 
                includeProperties: "Objetivos,IndicadoresDeArea");

            
            indicadorCompleto.Objetivos.Remove(
                indicadorCompleto.Objetivos.First(o => o.Id == objetivo.Id));

       
            if (!indicadorCompleto.IsDeleted)
            {
              
                if (!indicadorCompleto.Objetivos.Any(o => !o.IsDeleted))
                {
                    await HardDeleteIndicadorInternal(indicadorCompleto);
                }
            }
           
        }

        _uow.Current.Objetivo.SoftDelete(objetivo);
        await _uow.Current.SaveAsync();
    }

    public async Task HardDeleteObjetivo(ObjetivoModel? objetivo)
    {
        if (objetivo == null) return;

        objetivo = await _uow.Current.Objetivo.GetIncludingDeleted(
            o => o.Id == objetivo.Id, 
            CancellationToken.None, 
            includeProperties: "Indicadores");

        foreach (var indicador in objetivo.Indicadores.ToList())
        {
            var indicadorCompleto = await _uow.Current.Indicador.GetIncludingDeleted(
                i => i.Id == indicador.Id, 
                CancellationToken.None, 
                includeProperties: "Objetivos,IndicadoresDeArea");

           
            indicadorCompleto.Objetivos.Remove(
                indicadorCompleto.Objetivos.First(o => o.Id == objetivo.Id));

           
            if (!indicadorCompleto.Objetivos.Any())
            {
                await HardDeleteIndicadorInternal(indicadorCompleto);
            }
            
            else if (!indicadorCompleto.IsDeleted && indicadorCompleto.Objetivos.All(o => o.IsDeleted))
            {
                await SoftDeleteIndicador(indicadorCompleto);
            }
        }

        _uow.Current.Objetivo.Delete(objetivo);
        await _uow.Current.SaveAsync();
    }

    public async Task SoftDeleteArea(AreaModel? area)
    {
        if (area == null) return;

        area = await _uow.Current.Area.GetIncludingDeleted(
            a => a.Id == area.Id, 
            CancellationToken.None, 
            includeProperties: "IndicadoresDeArea");

        foreach (var ia in area.IndicadoresDeArea.ToList())
            if (!ia.IsDeleted) 
                _uow.Current.IndicadorDeArea.SoftDelete(ia);

        _uow.Current.Area.SoftDelete(area);
        await _uow.Current.SaveAsync();
    }

   
    private async Task HardDeleteIndicadorInternal(IndicadorModel indicador)
    {
        var indicadorFull = await _uow.Current.Indicador.GetIncludingDeleted(
            i => i.Id == indicador.Id, 
            CancellationToken.None, 
            includeProperties: "IndicadoresDeArea");

        foreach (var ia in indicadorFull.IndicadoresDeArea.ToList())
            _uow.Current.IndicadorDeArea.Delete(ia);

        _uow.Current.Indicador.Delete(indicadorFull);
    }

 
}