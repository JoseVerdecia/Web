using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB.Data;
using WEB.Interfaces;

namespace WEB.Core.Services;

public class CurrentUser:ICurrentUser
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly UserManager<ApplicationUser> _userManager;
    private ClaimsPrincipal? _cachedUser;
    private ApplicationUser? _cachedAppUser;
    
    public CurrentUser(  IDbContextFactory<ApplicationDbContext> factory,AuthenticationStateProvider authStateProvider, UserManager<ApplicationUser> userManager)
    {
        _factory = factory;
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }
    public async Task<string?> GetUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;
        return _userManager.GetUserId(principal);
    }
    
    private async Task<ClaimsPrincipal> GetUserClaims()
    {
        if (_cachedUser == null)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            _cachedUser = authState.User;
        }
        return _cachedUser;
    }
    
    public ClaimsPrincipal? User => _cachedUser ??= GetUserClaims().GetAwaiter().GetResult();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
    
    public async Task<ApplicationUser?> GetUserAsync(CancellationToken ct = default)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return null;
    
        await using var context = await _factory.CreateDbContextAsync(ct);
        if (context == null) return null;
    
        return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<string?> GetFullNameAsync()
    {
        var user = await GetUserAsync();
        return user?.FullName;
    }

    public async Task<string?> GetEmailAsync()
    {
        var user = await GetUserAsync();
        return user?.Email;
    }
}