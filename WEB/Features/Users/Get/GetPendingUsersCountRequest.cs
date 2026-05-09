using WEB.Core.Mediator;

namespace WEB.Features.Users.Get;

public record GetPendingUsersCountRequest : IRequest<PendingCountDto>;
public record PendingCountDto { public int Count { get; init; } }