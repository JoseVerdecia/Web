using WEB.Core.Result;
using WEB.Data;

namespace WEB.Core.Interfaces;

public interface IUserService
{
    Task<AppResult<ApplicationUser>> EnsureUserIsAvailableForResponsibilityAsync(string userId,CancellationToken cancellationToken);
}