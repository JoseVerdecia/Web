using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Dashboard.Dto;

namespace WEB.Features.Dashboard;

public record GetDashboardDataRequest : IRequest<DashboardDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}