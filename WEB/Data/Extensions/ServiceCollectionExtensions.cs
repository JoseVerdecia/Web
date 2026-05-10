using ApexCharts;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Components.Tooltip;
using WEB.Components;
using WEB.Core.Mediator;
using WEB.Core.Services;
using WEB.Interfaces;
using WEB.Data.Repository;
using WEB.Data.IRepository;
using WEB.Services;

namespace WEB.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddFluentUIComponents();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ProfilePictureService>();
        services.AddScoped<GlobalEvaluationService>();
        services.AddScoped<ExcelExportService>();
        services.AddScoped<JefeAreaExportServices>();
        services.AddScoped<JefeProcesoExportServices>();
        services.AddScoped<ExportPdfService>();
        services.AddSingleton<EvaluationPeriodService>();
        services.AddScoped<AdminPendingUsersState>();
        services.AddScoped<IndicadorUpdateStateService>();
        services.AddScoped<IDeleteCascadeService, DeleteCascadeService>();
        services.AddScoped<ITooltipService, TooltipService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<ThemeService>();
        services.AddScoped<NotificacionStateService>();
        services.AddScoped<App>();
        services.AddScoped<IMediator, Mediator>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkScopeBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorToastBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<INotificationService, NotificationService>();

        services.AddSignalR();
        services.AddApexCharts();

        var assembly = typeof(Program).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));
        foreach (var type in handlerTypes)
        {
            foreach (var @interface in type.GetInterfaces())
            {
                services.AddScoped(@interface, type);
            }
        }

        return services;
    }
}