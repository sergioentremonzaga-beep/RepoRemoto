using FluentAssertions;
using Moq;
using RepoRemoto.Api;
using RepoRemoto.Cache;
using RepoRemoto.Dto;
using RepoRemoto.Entity;
using RepoRemoto.Errors;
using RepoRemoto.Models;
using RepoRemoto.Notifications;
using RepoRemoto.Repositories;
using RepoRemoto.Services;
using RepoRemoto.Validators;

namespace RepoRemoto.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<UserRepository> _repositoryMock = null!;
    private Mock<IJsonPlaceholderApi> _remoteApiMock = null!;
    private Mock<ICacheService> _cacheMock = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;
    private CreateUserRequestValidator _validator = null!;
    private UserService _userService = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<UserRepository>(null!);
        _remoteApiMock = new Mock<IJsonPlaceholderApi>();
        _cacheMock = new Mock<ICacheService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _validator = new CreateUserRequestValidator();

        _userService = new UserService(
            _repositoryMock.Object,
            _remoteApiMock.Object,
            _cacheMock.Object,
            _notificationServiceMock.Object,
            _validator);
    }

    [Test]
    public async Task GetByIdAsyncDevuelveCache()
    {
        int id = 1;
        var cacheUser = new User(id, "Manolete", "xxmanolete", "manole@gmail.com");

        _cacheMock
            .Setup(c => c.GetAsync<User>($"user:{id}"))
            .ReturnsAsync(cacheUser);
        
        var result = await _userService.GetByIdAsync(id);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cacheUser);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _remoteApiMock.Verify(a => a.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsyncDevuelveRepo()
    {
        int id = 1;
        var dbEntity = new UserEntity
        {
            Id = id,
            Name = "pepe viyuela",
            Username = "viyuelapepe",
            Email = "pepe@gmail.com"
        };

        _cacheMock.Setup(c => c.GetAsync<User>($"user:{id}")).ReturnsAsync((User?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(dbEntity);
        
        var result = await _userService.GetByIdAsync(id);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be("pepe viyuela");
        
        _cacheMock.Verify(c => c.SetAsync($"user:{id}", It.IsAny<User>(), It.IsAny<TimeSpan?>()), Times.Once);
        _remoteApiMock.Verify(a => a.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsyncNotFound()
    {
        int id = 99;

        _cacheMock.Setup(c => c.GetAsync<User>($"user:{id}")).ReturnsAsync((User?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((UserEntity?)null);
        _remoteApiMock.Setup(a => a.GetByIdAsync(id)).ReturnsAsync((JsonPlaceholderUserDto?)null);
        
        var result = await _userService.GetByIdAsync(id);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<DomainError.NotFound>();
    }

    [Test]
    public async Task CreateAsyncGuardaRemotoLocalCache()
    {
        var request = new CreateUserRequest("natalia quintana", "quintana", "nataliaq@gmail.com");
        var remoteDto = new JsonPlaceholderUserDto(11, request.Name, request.Username, request.Email);

        _remoteApiMock.Setup(a => a.CreateAsync(request)).ReturnsAsync(remoteDto);
         
        var result = await _userService.CreateAsync(request);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(11);
        
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<UserEntity>(e => e.Id == 11)), Times.Once);
        _cacheMock.Verify(c => c.SetAsync("user:11", It.IsAny<User>(), It.IsAny<TimeSpan?>()), Times.Once);
        _notificationServiceMock.Verify(n => n.Notificar(TipoNotificacion.Create, It.Is<string>(s => s.Contains("11"))), Times.Once);
    }

    [Test]
    public async Task CreateAsyncErrorValidacion()
    {
        var invalidRequest = new CreateUserRequest("", "usere", "x@gmail.com");
        
        var result = await _userService.CreateAsync(invalidRequest);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<DomainError.ValidationError>();
        
        _remoteApiMock.Verify(a => a.CreateAsync(It.IsAny<CreateUserRequest>()), Times.Never);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsyncBorraRemotoLocalCache()
    {
        int id = 1;
        var entity = new UserEntity { Id = id, Name = "josete" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        
        var result = await _userService.DeleteAsync(id);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        
        _remoteApiMock.Verify(a => a.DeleteAsync(id), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(entity), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync($"user:{id}"), Times.Once);
        _notificationServiceMock.Verify(n => n.Notificar(TipoNotificacion.Delete, It.IsAny<string>()), Times.Once);
    }
}