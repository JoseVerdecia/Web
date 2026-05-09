
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Interfaces;

namespace WEB.Features.IndicadorDeArea.Update;

public class UpdateMetaRealHandler:IRequestHandler<UpdateMetaRealRequest, IndicadorDeAreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public UpdateMetaRealHandler(IUnitOfWorkAccessor uow )
    {
        _uow = uow;
       
    }

    public async Task<Result<IndicadorDeAreaDto>> Handle(UpdateMetaRealRequest request, CancellationToken ct)
    {
        var area = await _uow.Current.IndicadorDeArea
            .Get(ia => ia.Id == request.id, includeProperties: "Indicador,Area");
        if (area == null) return Result<IndicadorDeAreaDto>.NotFound();

        var padre = area.Indicador;

        if (padre.IsMetaCumplirPorcentaje)
        {
            if (request.ValorTotal == null || request.ValorReal == null)
                return Result<IndicadorDeAreaDto>.Fail("Debe proporcionar Valor Total y Valor Real");

            area.ValorTotal = request.ValorTotal;
            area.ValorReal = request.ValorReal;
            area.ValorCualitativo = request.ValoracionCualitativa;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.metaReal))
                return Result<IndicadorDeAreaDto>.Fail("Debe proporcionar Meta Real");
            
            var result = EvaluacionHelper.ActualizarMetaReal(area, request.metaReal);
            if (result.IsFailure) return Result<IndicadorDeAreaDto>.Fail(result.Errors);
        }

       
        var evalResult = padre.IsMetaCumplirPorcentaje
            ? EvaluacionHelper.ActualizarEvaluacionArea(area, padre)
            : Result.Success(); 
        if (evalResult.IsFailure) return Result<IndicadorDeAreaDto>.Fail(evalResult.Errors);

        _uow.Current.IndicadorDeArea.Update(area);

       
        var todasLasAreas = await _uow.Current.IndicadorDeArea
            .GetAllBy(ia => ia.IndicadorId == padre.Id);
        
        var recalcResult = EvaluacionHelper.RecalcularIndicadorPadre(padre, todasLasAreas.ToList());
        if (recalcResult.IsFailure) return Result<IndicadorDeAreaDto>.Fail(recalcResult.Errors);

        await _uow.Current.SaveAsync();

        return Result<IndicadorDeAreaDto>.Success(area.MapToDto());
    }
}