using System.Text.Json;
using CSharpFunctionalExtensions;
using Refit;
using RepoRemoto.Api;
using RepoRemoto.Cache;
using RepoRemoto.Dto;
using RepoRemoto.Entity;
using RepoRemoto.Errors;
using RepoRemoto.Models;
using RepoRemoto.Notifications;
using RepoRemoto.Repositories;
using RepoRemoto.Validators;

namespace RepoRemoto.Services;

public class UserService(
    UserRepository repository,
    IJsonPlaceholderApi remoteApi,
    ICacheService cache,
    INotificationService notificationService,
    CreateUserRequestValidator validator) : IUserService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<List<User>> GetAllAsync()
    {
        var x = await repository.GetAllAsync();
        return x.Select(e => new User(e.Id, e.Name, e.Username, e.Email)).ToList();
    }
    
    public async Task<Result<User, DomainError>> GetByIdAsync(int id)
    {
        string cacheKey = $"user:{id}";
        
        var cache1 = await cache.GetAsync<User>(cacheKey);
        if (cache1 != null)
        {
            return cache1;
        }
        
        var entity = await repository.GetByIdAsync(id);
        if (entity != null)
        {
            var userDb = new User(entity.Id, entity.Name, entity.Username, entity.Email);
            await cache.SetAsync(cacheKey, userDb, CacheDuration);
            return userDb;
        }
        
        try
        {
            var remoto = await remoteApi.GetByIdAsync(id);
            if (remoto == null)
            {
                return new DomainError.NotFound("User", id);
            }

            var nuevo = new UserEntity
            {
                Id = remoto.Id,
                Name = remoto.Name,
                Username = remoto.Username,
                Email = remoto.Email
            };

            await repository.CreateAsync(nuevo);

            var user = new User(nuevo.Id, nuevo.Name, nuevo.Username, nuevo.Email);
            await cache.SetAsync(cacheKey, user, CacheDuration);

            return user;
        }
        catch (ApiException ex)
        {
            notificationService.Notificar(TipoNotificacion.Error, $"Error API al obtener ID {id}: {ex.StatusCode}");
            return new DomainError.ApiError((int)ex.StatusCode, ex.Message);
        }
    }
    
    public async Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request)
    {
        var validar = validator.Validate(request);
        if (validar.IsFailure)
        {
            return validar.Error;
        }

        try
        {
            var remoto = await remoteApi.CreateAsync(request);
            var entity = new UserEntity
            {
                Id = remoto.Id,
                Name = request.Name,
                Username = request.Username,
                Email = request.Email
            };

            await repository.CreateAsync(entity);

            var user = new User(entity.Id, entity.Name, entity.Username, entity.Email);
            await cache.SetAsync($"user:{user.Id}", user, CacheDuration);

            notificationService.Notificar(TipoNotificacion.Create, $"Usuario creado: {user.Id}");
            return user;
        }
        catch (ApiException ex)
        {
            notificationService.Notificar(TipoNotificacion.Error, $"Error API al crear usuario: {ex.StatusCode}");
            return new DomainError.ApiError((int)ex.StatusCode, ex.Message);
        }
    }
    
    public async Task<Result<User, DomainError>> UpdateAsync(int id, CreateUserRequest request)
    {
        var validar = validator.Validate(request);
        if (validar.IsFailure)
        {
            return validar.Error;
        }

        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            return new DomainError.NotFound("User", id);
        }

        try
        {
            await remoteApi.UpdateAsync(id, request);

            entity.Name = request.Name;
            entity.Username = request.Username;
            entity.Email = request.Email;
            entity.UpdatedAt = DateTime.Now;

            await repository.UpdateAsync(entity);

            var user = new User(entity.Id, entity.Name, entity.Username, entity.Email);
            await cache.SetAsync($"user:{id}", user, CacheDuration);

            notificationService.Notificar(TipoNotificacion.Update, $"Usuario actualizado: {id}");
            return user;
        }
        catch (ApiException ex)
        {
            notificationService.Notificar(TipoNotificacion.Error, $"Error API al actualizar {id}: {ex.StatusCode}");
            return new DomainError.ApiError((int)ex.StatusCode, ex.Message);
        }
    }
    
    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            return new DomainError.NotFound("User", id);
        }

        try
        {
            await remoteApi.DeleteAsync(id);
            await repository.DeleteAsync(entity);
            await cache.RemoveAsync($"user:{id}");

            notificationService.Notificar(TipoNotificacion.Delete, $"Usuario eliminado: {id}");
            return true;
        }
        catch (ApiException ex)
        {
            notificationService.Notificar(TipoNotificacion.Error, $"Error API al eliminar ID {id}: {ex.StatusCode}");
            return new DomainError.ApiError((int)ex.StatusCode, ex.Message);
        }
    }
    
    public async Task<string> ExportarJsonAsync(string path = "users_export.json")
    {
        var lista = await GetAllAsync();
        var json = new JsonSerializerOptions { WriteIndented = true };
        var jsonSerializer = JsonSerializer.Serialize(lista, json);

        await File.WriteAllTextAsync(path, jsonSerializer);
        return Path.GetFullPath(path);
    }
}
