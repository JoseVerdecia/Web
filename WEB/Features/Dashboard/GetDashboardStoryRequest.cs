using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Dashboard.Dto.story;

namespace WEB.Features.Dashboard.Dto;


public record GetDashboardStoryRequest : IRequest<DashboardStoryDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}