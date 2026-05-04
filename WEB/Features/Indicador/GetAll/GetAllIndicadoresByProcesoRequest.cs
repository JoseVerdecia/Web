using WEB.Core.Mediator;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.GetAll;

public record GetAllIndicadoresByProcesoRequest(int ProcesoId) : IRequest<List<IndicadorDto>>;