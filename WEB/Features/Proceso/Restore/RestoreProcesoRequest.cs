using WEB.Core.Mediator;
using WEB.Core.Result;

namespace WEB.Features.Proceso.Restore;

public record RestoreProcesoRequest(List<int> ProcesoIds) : IRequest<Unit>;