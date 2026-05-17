using Hangfire;
using Microsoft.Extensions.FileProviders;
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

        var uploadsRootPath = Path.Combine(app.Environment.ContentRootPath, "..", "ArchivosDeUsuario");
        Directory.CreateDirectory(uploadsRootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsRootPath),
            RequestPath = "/uploads"
        });
        
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