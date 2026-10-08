using CSharpFunctionalExtensions;
using RepoRemoto.Dto;
using RepoRemoto.Errors;

namespace RepoRemoto.Validators;

public class CreateUserRequestValidator
{
    public Result<CreateUserRequest, DomainError> Validate(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new DomainError.ValidationError("Nombre","El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new DomainError.ValidationError("Username","El nombre de usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return new DomainError.ValidationError("Email","El formato del correo electrónico no es válido."); 
        }

        return request;
    }
}