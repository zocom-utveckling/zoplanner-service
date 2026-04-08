using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zoplannerservice.Enums.SessionLocation;
using zoplannerservice.Exceptions;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISpringApiClient> _springClientMock;
    private readonly Mock<ILogger<SessionService>> _loggerMock;
    private readonly Mock<IAssignmentService> _assignmentServiceMock;
    private readonly SessionService _service;

    public SessionServiceTests()
    {
        _springClientMock = new Mock<ISpringApiClient>();
        _loggerMock = new Mock<ILogger<SessionService>>();
        _assignmentServiceMock = new Mock<IAssignmentService>();

        _service = new SessionService(
            _springClientMock.Object,
            _loggerMock.Object,
            _assignmentServiceMock.Object
        );
    }

    // Helpers
    private CreateSessionRequest CreateValidRequest()
    {
        return new CreateSessionRequest
        {
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddHours(2),
            Location = SessionLocation.ONSITE,
            Comment = "Test session"
        };
    }

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

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenAssignmentDontExist()
    {
        // Arrange
        var request = CreateValidRequest();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment?)null);

        // Act + assert
        var exc = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(1, request));

        Assert.Equal("Assignment with ID 1 not found", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsNull()
    {
        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateValidAssignment());

            var exc = await Assert.ThrowsAnyAsync<ValidationException>(() =>
                _service.CreateAsync(1, null!));

        Assert.Equal("Assignment data is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenBackendReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateValidAssignment());

        _springClientMock
            .Setup(x => x.PostAsync<CreateSessionRequest, Session>(
                "sessions/1",
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act + assert
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(1, request));

        Assert.Equal("Backend returned null when creating session", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedSession_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateValidRequest();

        var createdSession = new Session
        {
            Id = 100,
            TimeStart = request.TimeStart,
            TimeEnd = request.TimeEnd,
            Location = request.Location,
            Comment = request.Comment
        };

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateValidAssignment());

        _springClientMock
            .Setup(x => x.PostAsync<CreateSessionRequest, Session>(
                "sessions/1",
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act + assert
        var result = await _service.CreateAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal(100, result!.Id);
        Assert.Equal(request.TimeStart, result.TimeStart);
        Assert.Equal(request.TimeEnd, result.TimeEnd);
        Assert.Equal(request.Location, result.Location);
    }

    
}