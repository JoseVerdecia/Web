using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Dashboard.Dto;

namespace WEB.Features.Dashboard;

public record GetDashboardDataRequest : IRequest<DashboardDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}