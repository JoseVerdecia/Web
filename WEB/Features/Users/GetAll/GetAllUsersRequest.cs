using WEB.Common;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Users.Dto;

namespace WEB.Features.Users.GetAll;

public record GetAllUsersRequest(string? Role,string? Name,string? SortBy, string? SortDirection, int Page = 1, int PageSize = 10):IRequest<PagedResult<UserDto>>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}