using Refit;
using RepoRemoto.Dto;

namespace RepoRemoto.Api;

public interface IJsonPlaceholderApi
{
    [Get("/users")]
    Task<List<JsonPlaceholderUserDto>> GetAllAsync();
    
    [Get("/users/{id}")]
    Task<JsonPlaceholderUserDto?> GetByIdAsync(int id);
    
    [Post("/users")]
    Task<JsonPlaceholderUserDto> CreateAsync([Body] CreateUserRequest request);
    
    [Put("/users/{id}")]
    Task<JsonPlaceholderUserDto> UpdateAsync(int id, [Body] CreateUserRequest request);

    [Delete("/users/{id}")]
    Task DeleteAsync(int id);
}