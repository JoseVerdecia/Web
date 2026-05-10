using Hangfire;
using Microsoft.EntityFrameworkCore;
using WEB.Data.Interceptors;

namespace WEB.Data.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<ApplicationDbContext>(
            options => options.UseSqlServer(connectionString).AddInterceptors(new NotificacionCleanupInterceptor()),
            ServiceLifetime.Scoped
        );

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHangfire(config => config
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
        });

        return services;
    }
}