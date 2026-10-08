using Microsoft.Extensions.Options;
using RepoRemoto.Api;
using RepoRemoto.Cache;
using RepoRemoto.Config;
using RepoRemoto.Entity;
using RepoRemoto.Repositories;

namespace RepoRemoto.Sync;

public class UserSyncBackgroundService(
    UserRepository repository,
    IJsonPlaceholderApi remoteApi,
    ICacheService cache,
    IOptions<AppConfig> config,
    ILogger<UserSyncBackgroundService> logger) : BackgroundService
{
    private readonly AppConfig _config = config.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Iniciando UserSyncBackgroundService");
        
        await SynchronizeDataAsync(stoppingToken);
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_config.SyncIntervalSeconds));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SynchronizeDataAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("UserSyncBackgroundService detenido");
        }
    }

    private async Task SynchronizeDataAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Iniciando proceso de sincronización con API REST");
            
            await cache.ClearAsync();
            await repository.ClearAsync();
            
            var remoteUsers = await remoteApi.GetAllAsync();

            if (remoteUsers.Count != 0)
            {
                foreach (var u in remoteUsers)
                {
                    var entity = new UserEntity
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Username = u.Username,
                        Email = u.Email,
                        CreatedAt = DateTime.UtcNow
                    };

                    await repository.CreateAsync(entity);
                }

                logger.LogInformation("Sincronización completada con éxito");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la sincronización en UserSyncBackgroundService.");
        }
    }
}