using WEB.Core.Mediator;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.GetAll;

public record GetAllIndicadoresByObjetivoRequest(int ObjetivoId) : IRequest<List<IndicadorDto>>;