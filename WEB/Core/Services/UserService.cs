using Microsoft.AspNetCore.Identity;
using WEB.Core.Interfaces;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Core.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWorkAccessor _uow;
    
    public UserService(UserManager<ApplicationUser> userManager,IUnitOfWorkAccessor uow)
    {
        _userManager = userManager;
        _uow = uow;
    }

    public async Task<AppResult<ApplicationUser>> EnsureUserIsAvailableForResponsibilityAsync(string userId,CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AppResult<ApplicationUser>.NotFound("El usuario no existe.");
        
        var isProcessBoss = await _uow.Current.Proceso
            .GetAllBy(p => p.JefeProcesoId == userId,cancellationToken);

        if (isProcessBoss.Any())
            return AppResult<ApplicationUser>.Fail("El usuario ya es responsable de un Proceso y no puede asumir otro cargo.");
        
        var isAreaBoss = await _uow.Current.Area
            .GetAllBy(a => a.JefeAreaId == userId,cancellationToken);

        if (isAreaBoss.Any())
            return AppResult<ApplicationUser>.Fail($"El usuario ya es responsable del Área '{user.FullName}' y no puede asumir otro cargo.");
        
        return AppResult<ApplicationUser>.Success(user);
    }
}