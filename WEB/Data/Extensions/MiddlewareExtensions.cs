using Hangfire;
using WEB.Components;
using WEB.Data.Hub;
using WEB.Data.Identity;

namespace WEB.Data.Extensions;


public static class MiddlewareExtensions
{
    public static void ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapAdditionalIdentityEndpoints();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapHub<NotificacionHub>("/hubs/notificaciones");
    }
}