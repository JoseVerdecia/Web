using WEB.Core.Result;

namespace WEB.Interfaces;

public interface IRoleManagementService
{
    Task<Result> SetRoleAsync(string userId, string role);
    Task<Result> ResetToDefaultRoleAsync(string userId);
    
    Task<Result> UpgradeToJefeProcesoAsync(string userId);
    Task<Result> UpgradeToJefeAreaAsync(string userId);
    Task<Result> UpgradeToAdminAsync(string userId);
}