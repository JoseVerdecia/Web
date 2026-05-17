using WEB.Core.Result;

namespace WEB.Core.Interfaces;

public interface IRoleManagementService
{
    Task<AppResult> SetRoleAsync(string userId, string role);
    Task<AppResult> ResetToDefaultRoleAsync(string userId);
    Task<AppResult> UpgradeToJefeProcesoAsync(string userId);
    Task<AppResult> UpgradeToJefeAreaAsync(string userId);
    Task<AppResult> UpgradeToAdminAsync(string userId);
}