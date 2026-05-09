using System.Globalization;
using ApexCharts;
using Hangfire;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB.Components;
using WEB.Components.Account;
using WEB.Data;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Components.Tooltip;
using QuestPDF.Infrastructure;
using WEB.Core.Mediator;
using WEB.Core.Services;
using WEB.Data.Hub;
using WEB.Data.Identity;
using WEB.Data.Interceptors;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Interfaces;
using WEB.Services;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

//builder.Services.AddScoped<ICacheInvalidator, MemoryCacheInvalidator>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ProfilePictureService>();
builder.Services.AddScoped<GlobalEvaluationService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<JefeAreaExportServices>();
builder.Services.AddScoped<JefeProcesoExportServices>();
builder.Services.AddScoped<ExportPdfService>();
builder.Services.AddSingleton<EvaluationPeriodService>();
builder.Services.AddScoped<AdminPendingUsersState>();
builder.Services.AddScoped<IndicadorUpdateStateService>();
builder.Services.AddApexCharts();
builder.Services.AddScoped<IDeleteCascadeService, DeleteCascadeService>();
builder.Services.AddScoped<ITooltipService, TooltipService>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<NotificacionStateService>();
builder.Services.AddScoped<App>();
builder.Services.AddScoped<IMediator, Mediator>();

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkScopeBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorToastBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));


/*builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();*/
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddSignalR();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString).AddInterceptors( new NotificacionCleanupInterceptor()),
    ServiceLifetime.Scoped 
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));



builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddHangfireServer(options => 
{
    options.WorkerCount = 1; 
});

var assembly = typeof(Program).Assembly;
var handlerTypes = assembly.GetTypes()
    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));
foreach (var type in handlerTypes)
{
    foreach (var @interface in type.GetInterfaces())
    {
        builder.Services.AddScoped(@interface, type);
    }
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    await DataSeeder.SeedAsync(serviceProvider, configuration);
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

app.Run();