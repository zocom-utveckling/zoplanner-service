using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Tests.Services;

public class AssignmentServiceTests {
    private readonly Mock<ISpringApiClient> _springClientMock;
    private readonly Mock<ILogger<AssignmentService>> _loggerMock;
    private readonly AssignmentService _service;

    public AssignmentServiceTests() {
        _springClientMock = new Mock<ISpringApiClient>();
        _loggerMock = new Mock<ILogger<AssignmentService>>();
        _service = new AssignmentService(_springClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsNull(){
        // Act + Assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(null!));

        Assert.Equal("Assignment data is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenDateStartIsNull() {
        // Arrange
        var request = new CreateAssignmentRequest{
            DateStart = null,
            DateEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            CourseId = 1
        };
        
        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

        Assert.Equal("DateStart is required", exc.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenDateEndIsNull() {
        // Arrange
        var request = new CreateAssignmentRequest{
            DateStart = DateOnly.FromDateTime(DateTime.UtcNow),
            DateEnd = null,
            CourseId = 1
        };

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request));

            Assert.Equal("DateEnd is required", exc.Message);
    }

    [Fact]
    public async Task GetByConsultantIdAsync_ShouldThrowValidationException_WhenConsultantIdIsNegative() {
        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetByConsultantIdAsync(-1));

            Assert.Equal("Consultant ID must be greater than 0", exc.Message);
    }

    [Fact]
    public async Task GetByConsultantIdAsync_ShouldReturnEmpty_WhenApiThrows404(){
        // Arrange
        _springClientMock
            .Setup(x => x.GetAsync<IEnumerable<Assignment>>(
                "assignments/consultant/5",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("404 Not Found"));

        // Act
        var result = await _service.GetByConsultantIdAsync(5);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAssignmentsByVisibilityAsync_ShouldReturnEmpty_WhenApiReturnsNull() {
        // Arrange
        _springClientMock
            .Setup(x => x.GetAsync<IEnumerable<Assignment>>(
                "assignments/visibility?published=True",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Assignment>?)null);
        
        // Act
        var result = await _service.GetAssignmentsByVisibilityAsync(true, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedAssignment_AndSetCourseId() {
        // Arrange
        var request = new CreateAssignmentRequest{
            DateStart = DateOnly.FromDateTime(DateTime.UtcNow),
            DateEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            CourseId = 123
        };

        var createdAssignment = new Assignment{
            Id = 1,
            DateStart = request.DateStart,
            DateEnd = request.DateEnd
        };

        _springClientMock
            .Setup(x => x.PostAsync<CreateAssignmentRequest, Assignment>(
                "assignments",
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAssignment);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(123, result.CourseId);
        Assert.Equal(request.DateStart, result.DateStart);
        Assert.Equal(request.DateEnd, result.DateEnd);
    }

    [Fact]
    public async Task PatchAsync_ShouldThrowValidationException_WhenIdIsInvalid(){
        // Arrange
        var request = new PatchAssignmentRequest{
            ConsultantId = 5
        };

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.PatchAsync(0, request));

        Assert.Equal("Assignment ID must be greater than 0", exc.Message);
    }

    [Fact]
    public async Task PatchAsync_ShouldThrowValidationException_WhenRequestIsNull(){
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.PatchAsync(1, null!));

            Assert.Equal("Patch data is required", exc.Message);
    }

    [Fact]
    public async Task PatchAsync_ShouldThrowValidationException_WhenNoFieldsProvided(){
        // Arrange
        var request = new PatchAssignmentRequest{
            ConsultantId = null,
            DateStart = null,
            DateEnd = null
        };

        // Act + assert
        var exc = await Assert.ThrowsAsync<ValidationException>(() => 
            _service.PatchAsync(1, request));

        Assert.Equal("At least one field must be provided for patch", exc.Message);
    }

    [Fact]
    public async Task PatchAsync_ShouldReturnNull_WhenAssignmentDontExist(){
        // Arrange
        var request = new PatchAssignmentRequest{
            ConsultantId = 99
        };

        _springClientMock
            .Setup(x => x.GetAsync<Assignment>(
                "assignments/1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment?)null);

        // Act
        var result = await _service.PatchAsync(1, request);

        // Assert
        Assert.Null(result);

        _springClientMock.Verify(x => x.PatchAsync<Assignment, Assignment>(
            It.IsAny<string>(),
            It.IsAny<Assignment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchAsync_ShouldReturnExisting_WhenNoChangesDetected(){
        // Arramge
        var existing = new Assignment{
            Id = 1,
            ConsultantId = 10,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10)
        };

        var request = new PatchAssignmentRequest{
            ConsultantId = 10,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10)
        };

        _springClientMock
            .Setup(x => x.GetAsync<Assignment>(
                "assignments/1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.PatchAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Id, result!.Id);
        Assert.Equal(existing.ConsultantId, result.ConsultantId);
        Assert.Equal(existing.DateStart, result.DateStart);
        Assert.Equal(existing.DateEnd, result.DateEnd);

        _springClientMock.Verify(x => x.PatchAsync<Assignment, Assignment>(
            It.IsAny<string>(),
            It.IsAny<Assignment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchAsync_ShouldPatchAndReturnUpdated_WhenChanged(){
        // Arrange
        var existing = new Assignment{
            Id = 1,
            ConsultantId = 10,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10),
            CourseId = 100
        };

        var updated = new Assignment{
            Id = 1,
            ConsultantId = 20,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10),
            CourseId = 100
        };

        var request = new PatchAssignmentRequest{
            ConsultantId = 20
        };

        _springClientMock
            .Setup(x => x.GetAsync<Assignment>(
                "assignments/1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _springClientMock
            .Setup(x => x.PatchAsync<Assignment, Assignment>(
                "assignments/1",
                It.IsAny<Assignment>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _service.PatchAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result!.ConsultantId);

        _springClientMock.Verify(x => x.PatchAsync<Assignment, Assignment>(
            "assignments/1",
            It.Is<Assignment>(a =>
                a.Id == 1 &&
                a.ConsultantId == 20 &&
                a.DateStart == new DateOnly(2026, 3, 1) &&
                a.DateEnd == new DateOnly(2026, 3, 10)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchAsync_ShouldThrowValidationException_WhenEndDateIsBeforeStartDate(){
        // Arrange
        var existing = new Assignment{
            Id = 1,
            ConsultantId = 10,
            DateStart = new DateOnly(2026, 3, 10),
            DateEnd = new DateOnly(2026, 3, 20),
            CourseId = 100
        };

        var request = new PatchAssignmentRequest{
            DateEnd = new DateOnly(2026, 3, 5)
        };

        _springClientMock
            .Setup(x => x.GetAsync<Assignment>(
                "assignments/1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var exc = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.PatchAsync(1, request));

        // Assert
        Assert.Equal("End date must be after start date", exc.Message);

        _springClientMock.Verify(x => x.PatchAsync<Assignment, Assignment>(
            It.IsAny<string>(),
            It.IsAny<Assignment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchAsync_ShouldThrowInvalidOperatioException_WhenRequestFails(){
        // Arrange
        var existing = new Assignment{
            Id = 1,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10),
            CourseId = 100
        };

        var request = new PatchAssignmentRequest{
            ConsultantId = 20
        };

        _springClientMock
            .Setup(x => x.GetAsync<Assignment>(
                "assignments/1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _springClientMock
            .Setup(x => x.PatchAsync<Assignment, Assignment>(
                "assignments/1",
                It.IsAny<Assignment>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API failed"));

        // Act + assert
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PatchAsync(1, request));

        Assert.Contains("Spring Boot error while patching Assignment with ID 1", exc.Message);
    }

}