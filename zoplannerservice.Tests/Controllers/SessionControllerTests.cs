using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zoplannerservice.Controllers;
using zoplannerservice.Exceptions;
using zoplannerservice.Models;
using zoplannerservice.Services;
using zoplannerservice.Enums.SessionLocation;

namespace zoplannerservice.Tests.Controllers;

public class SessionControllerTests
{
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly Mock<IAssignmentService> _assignmentServiceMock;
    private readonly Mock<ILogger<SessionController>> _loggerMock;
    private readonly SessionController _controller;

    public SessionControllerTests(){
        _sessionServiceMock = new Mock<ISessionService>();
        _assignmentServiceMock = new Mock<IAssignmentService>();
        _loggerMock = new Mock<ILogger<SessionController>>();

        _controller = new SessionController(
            _sessionServiceMock.Object,
            _assignmentServiceMock.Object,
            _loggerMock.Object
        );
    }

    // Helpers
    private Assignment CreateValidAssignment()
    {
        return new Assignment
        {
            Id = 1,
            ConsultantId = 10,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10),
            CourseId = 100
        };
    }

    private CreateSessionRequest CreateValidCreateRequest()
    {
        return new CreateSessionRequest
        {
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddHours(2),
            Location = SessionLocation.ONSITE,
            Comment = "Test session"
        };
    }

    private Session CreateValidSession()
    {
        return new Session
        {
            Id = 1,
            AssignmentId = 1,
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddHours(2),
            Location = SessionLocation.ONSITE,
            Comment = "Test session"
        };
    }

    [Fact]
    public async Task CreateSession_ShouldReturnCreatedAtAction_WhenSessionCreated()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var assignment = CreateValidAssignment();
        var session = CreateValidSession();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _sessionServiceMock
            .Setup(x => x.CreateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act + assert
        var result = await _controller.CreateSession(1, request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedSession = Assert.IsType<Session>(createdResult.Value);

        Assert.Equal(nameof(SessionController.GetById), createdResult.ActionName);
        Assert.Equal(session.Id, returnedSession.Id);
    }

    [Fact]
    public async Task CreateSession_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var assignment = CreateValidAssignment();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _sessionServiceMock
            .Setup(x => x.CreateAsync(1, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid request"));

        // Act + assert
        var result = await _controller.CreateSession(1, request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateSession_ShouldReturnNotFound_WhenNotFoundExceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var assignment = CreateValidAssignment();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _sessionServiceMock
            .Setup(x => x.CreateAsync(1, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Assignment with ID 1 not found"));

        // Act + assert
        var result = await _controller.CreateSession(1, request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateSession_ShouldReturnInternalServerError_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var assignment = CreateValidAssignment();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _sessionServiceMock
            .Setup(x => x.CreateAsync(1, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something failed"));

        // Act + assert
        var result = await _controller.CreateSession(1, request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

    }

    [Fact]
    public async Task CreateSession_ShouldReturnInternalServerError_WhenServiceReturnsNull()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var assignment = CreateValidAssignment();

        _assignmentServiceMock
        .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(assignment);

        _sessionServiceMock
        .Setup(x => x.CreateAsync(1, request, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Session)null!);

        // Act + assert
        var result = await _controller.CreateSession(1, request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenSessionsExist()
    {
        // Arrange
        var sessions = new List<Session> { CreateValidSession() };

        _sessionServiceMock
            .Setup(x => x.GetAllSync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act + assert
        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSessions = Assert.IsAssignableFrom<IEnumerable<Session>>(okResult.Value);
        Assert.Single(returnedSessions);
    }

    [Fact]
    public async Task GetAll_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _sessionServiceMock
            .Setup(x => x.GetAllSync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.GetAll(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldRetornOk_WhenSessionExists()
    {
        // Arrange
        var session = CreateValidSession();

        _sessionServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act + assert
        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSession = Assert.IsType<Session>(okResult.Value);

        Assert.Equal(1, returnedSession.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenSessionDontExist()
    {
        _sessionServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        var result = await _controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnGatewayTimeout_WhenTimeoutExceptionThrown()
    {
        _sessionServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Timed out"));

        var result = await _controller.GetById(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(504, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _sessionServiceMock
            .Setup(x => x.GetByIdAsync(0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid id"));

        var result = await _controller.GetById(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _sessionServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.GetById(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenSessionDeleted()
    {
        _sessionServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenSessionDontExist()
    {
        _sessionServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _sessionServiceMock
            .Setup(x => x.DeleteAsync(0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid id"));

        var result = await _controller.Delete(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _sessionServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        var result = await _controller.Delete(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenSessionUpdated()
    {
        // Arrange
        var session = CreateValidSession();

        _sessionServiceMock
            .Setup(x => x.UpdateAsync(1, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act + assert
        var result = await _controller.Update(1, session, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSession = Assert.IsType<Session>(okResult.Value);

        Assert.Equal(session.Id, returnedSession.Id);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenSessionDontExist()
    {
        // Arrange
        var session = CreateValidSession();

        _sessionServiceMock
            .Setup(x => x.UpdateAsync(1, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act + assert
        var result = await _controller.Update(1, session, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        // Arrange
        var session = CreateValidSession();

        _sessionServiceMock
            .Setup(x => x.UpdateAsync(1, session, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid session"));

        // Act + assert
        var result = await _controller.Update(1, session, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Uodate_ShouldReturnServiceUnavailable_WhenInvalidExceptionThrown()
    {
        // Arrange
        var session = CreateValidSession();

        _sessionServiceMock
            .Setup(x => x.UpdateAsync(1, session, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend failed"));

        // Act + assert
        var result = await _controller.Update(1, session, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }
}