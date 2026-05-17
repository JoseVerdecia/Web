using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Services;
using WEB.Features.Objetivo;
using WEB.Features.Objetivo.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Evaluacion.Objetivo;

public class EvaluateObjetivoHandler(
    IUnitOfWorkAccessor _uow)
    :IRequestHandler<EvaluateObjetivoRequest,ObjetivoDto>
{
    public async Task<AppResult<ObjetivoDto>> Handle(EvaluateObjetivoRequest request, CancellationToken cancellationToken)
    {
        ObjetivoModel? objetivo = await _uow.Current.Objetivo.Get(o => o.Id == request.id,includeProperties:"Indicadores");
        
        List<IndicadorEvaluacionData> evaluationData = 
            objetivo.Indicadores.Select(i => new IndicadorEvaluacionData(
                i.Tipo,
                i.Evaluacion
            )).ToList();
        
        Enums.Evaluacion evaluacionFinal = EvaluateObjetivosAndProcesos.Evaluar(evaluationData);
        objetivo.Evaluacion = evaluacionFinal;
        _uow.Current.Objetivo.Update(objetivo);
        await _uow.Current.SaveAsync();
        return AppResult<ObjetivoDto>.Success(objetivo.MapToDto());
    }
}