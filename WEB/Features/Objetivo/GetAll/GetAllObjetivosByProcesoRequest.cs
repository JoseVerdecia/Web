using WEB.Core.Mediator;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Objetivo.GetAll;

public record GetAllObjetivosByProcesoRequest(int ProcesoId) : IRequest<List<ObjetivoDto>>;