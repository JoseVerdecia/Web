using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.IndicadorDeArea.Update;

public record UpdateMetaRealRequest(int id , string? metaReal, decimal? ValorTotal,decimal? ValorReal,string? ValoracionCualitativa):IRequest<IndicadorDeAreaDto>;