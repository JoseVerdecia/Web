using WEB.Core.Mediator;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Objetivo.Update;

public record UpdateObjetivoCommand(
    int Id,
    string Nombre,
    int NumeroObjetivo
) : IRequest<ObjetivoDto>;

