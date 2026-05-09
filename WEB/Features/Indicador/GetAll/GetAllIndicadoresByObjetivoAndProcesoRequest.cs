using WEB.Core.Mediator;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.GetAll;

public record GetAllIndicadoresByObjetivoAndProcesoRequest(int ObjetivoId, int ProcesoId) 
    : IRequest<List<IndicadorDto>>;