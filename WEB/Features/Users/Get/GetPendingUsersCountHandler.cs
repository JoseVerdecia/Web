using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Users.Get;

public class GetPendingUsersCountHandler : IRequestHandler<GetPendingUsersCountRequest, PendingCountDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetPendingUsersCountHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<PendingCountDto>> Handle(GetPendingUsersCountRequest request, CancellationToken cancellationToken)
    {
        var usuariosConRolNormal = await _userManager.GetUsersInRoleAsync(AppRoles.UsuarioNormal);
        var pending = usuariosConRolNormal.Count();
        return Result<PendingCountDto>.Success(new PendingCountDto { Count = pending });
    }
}