using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.IndicadorDeArea.Update;

public record UpdateEvaluacionIndicadorDeAreaRequest(
    int Id,
    Enums.Evaluacion? NuevaEvaluacion,
    string? ValoracionCualitativa
) : IRequest<IndicadorDeAreaDto>;