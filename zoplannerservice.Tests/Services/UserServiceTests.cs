using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zoplannerservice.Enums.UserRole;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<ISpringApiClient> _springClientMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _springClientMock = new Mock<ISpringApiClient>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _service = new UserService(_springClientMock.Object, _loggerMock.Object);
    }

    // Helpers
    private CreateUserRequest CreateValidRequest()
    {
        return new CreateUserRequest
        {
            Username = "joel123",
            Email = "joel@test.com",
            Password = "password123",
            Role = UserRole.CONSULTANT,
            City = "Malmö",
            Name = "Joel"
        };
    }

    private User CreateValidUser()
    {
        return new User
        {
            Id = 1,
            Username = "joel123",
            Email = "joel@test.com",
            Password = "password123",
            Role = UserRole.CONSULTANT,
            City = "Malmö",
            Name = "Joel"
        };
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsNull()
    {
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(null!));

        Assert.Equal("User data is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationExceptionWhenUsernameIsMissing()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Username = "";

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("Username is required", exc.Message);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenEmailIsMissing()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = "";

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("Email is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenPasswordIsMissing()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = "";

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("Password is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRoleIsMissing()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Role = null;

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("Role is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenCityIsMissing()
    {
        // Arrange
        var request = CreateValidRequest();
        request.City = "";

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("City is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenNameIsMissing()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = "";

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("Name is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenBackendReturnsNull()
    {
        // ARrrange
        var request = CreateValidRequest();

        _springClientMock
            .Setup(x => x.PostAsync<CreateUserRequest, User>(
                "users",
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act + assert
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("Failed to create User", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenBackEndThrowsHttpRequestException()
    {
        // Arrange
        var request = CreateValidRequest();

        _springClientMock
            .Setup(x => x.PostAsync<CreateUserRequest, User>(
                "users",
                request,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API failure"));

        // Act + assert
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(request));

        Assert.Contains("Spring Boot error while creating User", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedUser_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateValidRequest();
        var createdUser = CreateValidUser();

        _springClientMock
            .Setup(x => x.PostAsync<CreateUserRequest, User>(
                "users",
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act + assert
        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(createdUser.Id, result.Id);
        Assert.Equal(createdUser.Username, result.Username);
        Assert.Equal(createdUser.Email, result.Email);
        Assert.Equal(createdUser.Role, result.Role);
    }

    [Fact]
    public async Task GetByUserNameAsync_ShouldThrowValidationException_WhenUsernameIsMissing()
    {
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetByUsernameAsync(""));

        Assert.Equal("Username is required", exc.Message);
    }

    [Fact]
    public async Task GetUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = CreateValidUser();

        _springClientMock
            .Setup(x => x.GetAsync<User>(
                "users/username/joel123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act + assert
        var result = await _service.GetByUsernameAsync("joel123");

        Assert.NotNull(result);
        Assert.Equal("joel123", result!.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenBackendReturnsNull()
    {
        _springClientMock
            .Setup(x => x.GetAsync<User>(
                "users/username/joel123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.GetByUsernameAsync("joel123");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenBackendThrows404()
    {
        _springClientMock
            .Setup(x => x.GetAsync<User>(
                "users/username/joel123",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("404 Not Found"));

        var result = await _service.GetByUsernameAsync("joel123");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldThrowInvalidOperationException_WhenBackendThrowsUnexpectedException()
    {
        _springClientMock
            .Setup(x => x.GetAsync<User>(
                "users/username/joel123",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something failed"));

        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetByUsernameAsync("joel123"));

        Assert.Equal("Failed to fetch User by username from backend", exc.Message);
    }
}