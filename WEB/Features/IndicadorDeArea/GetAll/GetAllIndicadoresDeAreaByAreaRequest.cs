using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.IndicadorDeArea.GetAll;

public record GetAllIndicadoresDeAreaByAreaRequest(int AreaId) : IRequest<List<IndicadorDeAreaDto>>;
