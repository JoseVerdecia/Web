using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Users.Get;

public class GetPendingUsersCountHandler : IRequestHandler<GetPendingUsersCountRequest, PendingCountDto>
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public GetPendingUsersCountHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<Result<PendingCountDto>> Handle(GetPendingUsersCountRequest request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var userStore = new UserStore<ApplicationUser>(context);
        
        var normalRole = await context.Roles
            .Where(r => r.Name == AppRoles.UsuarioNormal)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (normalRole == null)
            return Result<PendingCountDto>.Success(new PendingCountDto { Count = 0 });
        
        var count = await context.UserRoles
            .CountAsync(ur => ur.RoleId == normalRole, cancellationToken);

        return Result<PendingCountDto>.Success(new PendingCountDto { Count = count });
    }
}