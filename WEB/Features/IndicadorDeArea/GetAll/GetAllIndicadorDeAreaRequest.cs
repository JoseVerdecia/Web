using WEB.Common;
using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.IndicadorDeArea.GetAll;

public record GetAllIndicadorDeAreaRequest(int Page , int PageSize):IRequest<PagedResult<IndicadorDeAreaDto>>;