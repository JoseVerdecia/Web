using System.Globalization;
using WEB.Data;
using QuestPDF.Infrastructure;
using WEB.Data.Extensions;


var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDatabase(connectionString);
builder.Services.AddIdentityServices();
builder.Services.AddApplicationServices();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    await DataSeeder.SeedAsync(serviceProvider, configuration);
}

app.ConfigureMiddleware();

app.Run();