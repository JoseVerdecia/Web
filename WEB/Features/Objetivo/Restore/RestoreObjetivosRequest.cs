using WEB.Core.Mediator;
using WEB.Core.Result;

namespace WEB.Features.Objetivo.Restore;

public record RestoreObjetivosRequest(List<int> Ids) : IRequest<Unit>;