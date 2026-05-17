using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Users.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest,Unit>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public DeleteUserHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<AppResult<Unit>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
            return AppResult<Unit>.Fail("Usuario no encontrado.");

        
        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return AppResult<Unit>.Success(Unit.Value);
    }
}