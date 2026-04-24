using WEB.Core.Mediator;
using WEB.Features.Users.Dto;

namespace WEB.Features.Users.Get;

public record GetUserByIdRequest(string Id) : IRequest<UserDto>;