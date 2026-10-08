namespace RepoRemoto.Errors;

public abstract record DomainError
{
    public sealed record NotFound(string Resource, int Id) : DomainError;
    public sealed record ValidationError(string Field, string Message) : DomainError;
    public sealed record ApiError(int StatusCode, string Detail) : DomainError;
};