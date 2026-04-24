using Hangfire.Dashboard;

namespace WEB.Data.Identity;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        
        var isAdmin = httpContext.User.IsInRole(AppRoles.Administrador); 
        
        return isAuthenticated && isAdmin;
    }
}