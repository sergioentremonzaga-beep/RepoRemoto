using CSharpFunctionalExtensions;
using RepoRemoto.Dto;
using RepoRemoto.Errors;
using RepoRemoto.Models;

namespace RepoRemoto.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    
    Task<Result<User, DomainError>> GetByIdAsync(int id);
    
    Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request);
    
    Task<Result<User, DomainError>> UpdateAsync(int id, CreateUserRequest request);
    
    Task<Result<bool, DomainError>> DeleteAsync(int id);
    
    Task<string> ExportarJsonAsync(string filePath = "users_export.json");
}