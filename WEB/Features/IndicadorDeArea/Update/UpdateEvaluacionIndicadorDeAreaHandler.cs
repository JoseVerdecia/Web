using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.IndicadorDeArea.Update;

public class UpdateEvaluacionIndicadorDeAreaHandler : IRequestHandler<UpdateEvaluacionIndicadorDeAreaRequest, IndicadorDeAreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public UpdateEvaluacionIndicadorDeAreaHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<IndicadorDeAreaDto>> Handle(UpdateEvaluacionIndicadorDeAreaRequest request, CancellationToken ct)
    {
        var area = await _uow.Current.IndicadorDeArea
            .Get(ia => ia.Id == request.Id, includeProperties: "Indicador,Area");
        if (area == null) return AppResult<IndicadorDeAreaDto>.NotFound("Indicador de área no encontrado.");

        if (request.NuevaEvaluacion.HasValue)
            area.Evaluacion = request.NuevaEvaluacion.Value;

        if (request.ValoracionCualitativa != null)
            area.ValorCualitativo = request.ValoracionCualitativa;

        _uow.Current.IndicadorDeArea.Update(area);
        
        if (request.NuevaEvaluacion.HasValue)
        {
            var todas = await _uow.Current.IndicadorDeArea
                .GetAllBy(ia => ia.IndicadorId == area.IndicadorId);
            var recalculo = EvaluacionHelper.RecalcularIndicadorPadre(area.Indicador, todas.ToList());
            if (recalculo.IsFailure)
                return AppResult<IndicadorDeAreaDto>.Fail(recalculo.Errors);
        }

        await _uow.Current.SaveAsync();
        return AppResult<IndicadorDeAreaDto>.Success(area.MapToDto());
    }
}
