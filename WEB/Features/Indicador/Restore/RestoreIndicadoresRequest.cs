using WEB.Core.Mediator;
using WEB.Core.Result;

namespace WEB.Features.Indicador.Restore;

public record RestoreIndicadoresRequest(List<int> IndicadorIds) : IRequest<Unit>;