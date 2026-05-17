using Microsoft.AspNetCore.Identity;
using WEB.Common;
using WEB.Data;
using WEB.Enums;
using WEB.Core.Interfaces;
using WEB.Core.Result;

namespace WEB.Core.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AppResult> SetRoleAsync(string userId, string role)
    {
        
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null) return AppResult.Fail("Usuario no encontrado");
        
        IList<string> currentRoles = await _userManager.GetRolesAsync(user);
        
        if (currentRoles.Contains(role)) return AppResult.Success();
        
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return MapIdentityError(removeResult);
        }
        
        var addResult = await _userManager.AddToRoleAsync(user, role);
        return addResult.Succeeded ? AppResult.Success() : MapIdentityError(addResult);
    }

    public async Task<AppResult> ResetToDefaultRoleAsync(string userId)
    {
        return await SetRoleAsync(userId, AppRoles.UsuarioNormal);
    }


    public Task<AppResult> UpgradeToJefeProcesoAsync(string userId) => SetRoleAsync(userId, AppRoles.JefeProceso);
    public Task<AppResult> UpgradeToJefeAreaAsync(string userId) => SetRoleAsync(userId, AppRoles.JefeArea);
    public Task<AppResult> UpgradeToAdminAsync(string userId) => SetRoleAsync(userId, AppRoles.Administrador);

   
    private AppResult MapIdentityError(IdentityResult result)
    {
        var errors = result.Errors.Select(e => new ErrorDetail
        {
            Type = ErrorType.Validation,
            Field = e.Code,
            Message = e.Description
        }).ToList();

        return AppResult.Fail(errors);
    }
}