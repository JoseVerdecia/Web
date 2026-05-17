
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.IndicadorDeArea.Update;

public class UpdateMetaRealHandler:IRequestHandler<UpdateMetaRealRequest, IndicadorDeAreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public UpdateMetaRealHandler(IUnitOfWorkAccessor uow )
    {
        _uow = uow;
       
    }

    public async Task<AppResult<IndicadorDeAreaDto>> Handle(UpdateMetaRealRequest request, CancellationToken ct)
    {
        var area = await _uow.Current.IndicadorDeArea
            .Get(ia => ia.Id == request.id, includeProperties: "Indicador,Area");
        if (area == null) return AppResult<IndicadorDeAreaDto>.NotFound();

        var padre = area.Indicador;

        if (padre.IsMetaCumplirPorcentaje)
        {
            if (request.ValorTotal == null || request.ValorReal == null)
                return AppResult<IndicadorDeAreaDto>.Fail("Debe proporcionar Valor Total y Valor Real");

            area.ValorTotal = request.ValorTotal;
            area.ValorReal = request.ValorReal;
            area.ValorCualitativo = request.ValoracionCualitativa;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.metaReal))
                return AppResult<IndicadorDeAreaDto>.Fail("Debe proporcionar Meta Real");
            
            var AppResult = EvaluacionHelper.ActualizarMetaReal(area, request.metaReal);
            if (AppResult.IsFailure) return AppResult<IndicadorDeAreaDto>.Fail(AppResult.Errors);
        }

       
        var evalAppResult = padre.IsMetaCumplirPorcentaje
            ? EvaluacionHelper.ActualizarEvaluacionArea(area, padre)
            : AppResult.Success(); 
        if (evalAppResult.IsFailure) return AppResult<IndicadorDeAreaDto>.Fail(evalAppResult.Errors);

        _uow.Current.IndicadorDeArea.Update(area);

       
        var todasLasAreas = await _uow.Current.IndicadorDeArea
            .GetAllBy(ia => ia.IndicadorId == padre.Id);
        
        var recalcAppResult = EvaluacionHelper.RecalcularIndicadorPadre(padre, todasLasAreas.ToList());
        if (recalcAppResult.IsFailure) return AppResult<IndicadorDeAreaDto>.Fail(recalcAppResult.Errors);

        await _uow.Current.SaveAsync();

        return AppResult<IndicadorDeAreaDto>.Success(area.MapToDto());
    }
}