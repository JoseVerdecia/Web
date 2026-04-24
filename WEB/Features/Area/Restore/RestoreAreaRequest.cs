using WEB.Core.Mediator;
using WEB.Core.Result;

namespace WEB.Features.Area.Restore;

public record RestoreAreaRequest(List<int> AreaIds) : IRequest<Unit>;