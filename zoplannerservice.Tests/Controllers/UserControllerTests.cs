using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zoplannerservice.Controllers;
using zoplannerservice.Enums.UserRole;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<UserController>> _loggerMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UserController>>();

        _controller = new UserController(
            _userServiceMock.Object,
            _loggerMock.Object
        );
    }

    // Helpers
    private User CreateValidUser()
    {
        return new User
        {
            Id = 1,
            Username = "joel123",
            Email = "joel@test.com",
            Password = "secret123",
            Role = UserRole.CONSULTANT,
            City = "Malmö",
            Name = "Joel"
        };
    }

    private CreateUserRequest CreateValidCreateRequest()
    {
        return new CreateUserRequest
        {
            Username = "joel123",
            Email = "joel@test.com",
            Password = "secret123",
            Role = UserRole.CONSULTANT,
            City = "Malmö",
            Name = "Joel"
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenUsers_Exist()
    {
        // ARrange
        var users = new List<User> { CreateValidUser() };

        _userServiceMock
            .Setup(x => x.GetAllSync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act + assert
        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Single(returnedUsers);
    }

    [Fact]
    public async Task GetAll_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.GetAllSync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.GetAll(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);

    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExist()
    {
        // Arrange
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act + assert
        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(1, returnedUser.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDontExist()
    {
        _userServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.GetByIdAsync(0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid ID"));

        var result = await _controller.GetById(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnGatewayTimeout_WhenTimeoutExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Timed out"));

        var result = await _controller.GetById(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(504, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.GetById(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenUserIsCreated(){
        // Arrange
        var request = CreateValidCreateRequest();
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedUser = Assert.IsType<User>(createdResult.Value);

        Assert.Equal(nameof(UserController.GetById), createdResult.ActionName);
        Assert.Equal(user.Id, returnedUser.Id);
    }

    [Fact]
    public async Task Create_ShouldReturnServiceUnavailable_WhenServiceReturnsNull()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _userServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _userServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid request"));

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _userServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUserIsUpdated()
    {
        // Arrange
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.UpdateAsync(1, user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act + assert
        var result = await _controller.Update(1, user, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(user.Id, returnedUser.Id);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDontExist()
    {
        // Arrange
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.UpdateAsync(1, user, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        // Act + assert
        var result = await _controller.Update(1, user, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        // Arrange
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.UpdateAsync(1, user, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid user"));

        // Act + assert
        var result = await _controller.Update(1, user, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnUnavailable_WhenInvalidOperationExceptionThrown()
    {
        // Arrange
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.UpdateAsync(1, user, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        // Act + assert
        var result = await _controller.Update(1, user, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenUserIsDeleted()
    {
        _userServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserDontExist()
    {
        _userServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.DeleteAsync(0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid id"));
        
        var result = await _controller.Delete(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.Delete(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByUsername_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var user = CreateValidUser();

        _userServiceMock
            .Setup(x => x.GetByUsernameAsync("joel123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act + assert
        var result = await _controller.GetByUsername("joel123", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal("joel123", returnedUser.Username);
    }

    [Fact]
    public async Task GetByUsername_ShouldReturnBadRequest_WhenUsernameIsEmpty()
    {
        var result = await _controller.GetByUsername("", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetByUsername_ShouldReturnNotFound_WhenUserDontExist()
    {
        _userServiceMock
            .Setup(x => x.GetByUsernameAsync("joel123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        var result = await _controller.GetByUsername("joel123", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetByUsername_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.GetByUsernameAsync("joel123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid username"));

            var result = await _controller.GetByUsername("joel123", CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetByUsername_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _userServiceMock
            .Setup(x => x.GetByUsernameAsync("joel123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.GetByUsername("joel123", CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }
}