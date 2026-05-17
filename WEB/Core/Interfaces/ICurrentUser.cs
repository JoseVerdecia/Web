using System.Security.Claims;
using WEB.Data;

namespace WEB.Core.Interfaces;

public interface ICurrentUser
{
    ClaimsPrincipal? User { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    Task<ApplicationUser?> GetUserAsync(CancellationToken ct = default);
    Task<string?> GetUserIdAsync();
    Task<string?> GetFullNameAsync();             
    Task<string?> GetEmailAsync();                  
}