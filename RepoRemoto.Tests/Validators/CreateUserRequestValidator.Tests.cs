using FluentAssertions;
using RepoRemoto.Dto;
using RepoRemoto.Errors;
using RepoRemoto.Validators;

namespace RepoRemoto.Tests.Validators;

[TestFixture]
public class CreateUserRequestValidatorTests
{
    private CreateUserRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateUserRequestValidator();
    }

    [Test]
    public void ValidateCasoValido()
    {
        var request = new CreateUserRequest("chema", "chema", "chema@gmail.com");
        
        var result = _validator.Validate(request);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(request);
    }

    [TestCase("", "username", "x@gmail.com", "Name")]
    [TestCase("carlos", "", "x@gmail.com", "Username")]
    [TestCase("carlos", "username", "invalid-email", "Email")]
    public void ValidateCasoError(string name, string username, string email, string expectedField)
    {
        var request = new CreateUserRequest(name, username, email);
        
        var result = _validator.Validate(request);
        
        result.IsFailure.Should().BeTrue();
        var error = result.Error.Should().BeOfType<DomainError.ValidationError>().Subject;
        error.Field.Should().Be(expectedField);
    }
}