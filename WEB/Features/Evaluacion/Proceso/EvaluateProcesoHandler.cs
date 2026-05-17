using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Services;
using WEB.Features.Proceso;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Evaluacion.Proceso;

public class EvaluateProcesoHandler(
    IUnitOfWorkAccessor _uow)
    :IRequestHandler<EvaluateProcesoRequest,ProcesoDto>
{
    public async Task<AppResult<ProcesoDto>> Handle(EvaluateProcesoRequest request, CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(o => o.Id == request.id,includeProperties:"Indicadores");
        
        List<IndicadorEvaluacionData> evaluationData = 
            proceso.Indicadores.Select(i => new IndicadorEvaluacionData(
                i.Tipo,
                i.Evaluacion
            )).ToList();
        
        Enums.Evaluacion evaluacionFinal = EvaluateObjetivosAndProcesos.Evaluar(evaluationData);
        proceso.Evaluacion = evaluacionFinal;
        _uow.Current.Proceso.Update(proceso);
        await _uow.Current.SaveAsync();
        return AppResult<ProcesoDto>.Success(proceso.MapToDto());
    }
}