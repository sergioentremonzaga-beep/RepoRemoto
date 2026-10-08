using Microsoft.EntityFrameworkCore;
using Refit;
using RepoRemoto.Api;
using RepoRemoto.Cache;
using RepoRemoto.Config;
using RepoRemoto.Entity;
using RepoRemoto.Notifications;
using RepoRemoto.Repositories;
using RepoRemoto.Services;
using RepoRemoto.Sync;
using RepoRemoto.Validators;

namespace RepoRemoto.Infraestructure;

public static class DependenciesProvider
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppConfig>(configuration.GetSection(AppConfig.Position));
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=users.db"));
        
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<INotificationService, ConsoleNotificationService>();
        
        services.AddRefitClient<IJsonPlaceholderApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"));
        
        services.AddScoped<UserRepository>();
        services.AddScoped<CreateUserRequestValidator>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddHostedService<UserSyncBackgroundService>();

        return services;
    }
}