using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.IndicadorDeArea.Update;

public record UpdateMetaRealRequest(int id , string metaReal):IRequest<IndicadorDeAreaDto>;