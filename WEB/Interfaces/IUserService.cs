using WEB.Core.Result;
using WEB.Data;

namespace WEB.Interfaces;

public interface IUserService
{
    Task<Result<ApplicationUser>> EnsureUserIsAvailableForResponsibilityAsync(string userId,CancellationToken cancellationToken);
}