using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using zoplannerservice.Controllers;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Tests.Controllers;

public class AssignmentControllerTests
{
    private readonly Mock<IAssignmentService> _assignmentServiceMock;
    private readonly Mock<ISpringApiClient> _springClientMock;
    private readonly Mock<ILogger<AssignmentController>> _loggerMock;
    private readonly AssignmentController _controller;

    public AssignmentControllerTests()
    {
        _assignmentServiceMock = new Mock<IAssignmentService>();
        _springClientMock = new Mock<ISpringApiClient>();
        _loggerMock = new Mock<ILogger<AssignmentController>>();

        _controller = new AssignmentController(
            _assignmentServiceMock.Object,
            _springClientMock.Object,
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
            CourseId = 100,
            Course = new Course{ Id = 100 } // För Update -> assignment.CourseId = assignment.Course.Id;
        };
    }

    private CreateAssignmentRequest CreateValidCreateRequest()
    {
        return new CreateAssignmentRequest
        {
            ConsultantId = 10,
            DateStart = new DateOnly(2026, 3, 1),
            DateEnd = new DateOnly(2026, 3, 10),
            CourseId = 100
        };
    }

    [Fact]
    public async Task GetById_ShouldRetornOk_WhenAssignmentExists()
    {
        // Arrage
        var assignment = CreateValidAssignment();

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        // Act + assert
        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAssignment = Assert.IsType<Assignment>(okResult.Value);
        Assert.Equal(1, returnedAssignment.Id);

    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenAssignmentDontExist()
    {
        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment?)null);

        var result = await _controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid ID"));

        var result = await _controller.GetById(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnInternalServerError_WhenInvalidOperationExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service failed"));
        
        var result = await _controller.GetById(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var createdAssignment = CreateValidAssignment();

        _assignmentServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAssignment);

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        var CreatedAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedAssignment = Assert.IsType<Assignment>(CreatedAtActionResult.Value);

        Assert.Equal(nameof(AssignmentController.GetById), CreatedAtActionResult.ActionName);
        Assert.Equal(createdAssignment.Id, returnedAssignment.Id);

    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenValidationEceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _assignmentServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid request"));

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnInternalServerError_WhenInvalidOperationExceptionThrown()
    {
        // Arrange
        var request = CreateValidCreateRequest();

        _assignmentServiceMock
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync( new InvalidOperationException("Create failed"));

        // Act + assert
        var result = await _controller.Create(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenAssignmentDeleted()
    {
        _assignmentServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenAssignmentDontExist()
    {
        _assignmentServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.DeleteAsync(0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid ID"));

        var result = await _controller.Delete(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnServiceUnavailable_WhenInvalidOperationExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backend unavailable"));

        var result = await _controller.Delete(1, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByConsultantId_ShouldReturnOk_WhenAssignmentExist()
    {
        // Arrange
        var assignments = new List<Assignment> { CreateValidAssignment() };

        _assignmentServiceMock
            .Setup(x => x.GetAssignmentsByConsultantAsync(10, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        // Act + assert
        var result = await _controller.GetByConsultantId(10, true, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAssignments = Assert.IsAssignableFrom<IEnumerable<Assignment>>(okResult.Value);
        Assert.Single(returnedAssignments);
    }

    [Fact]
    public async Task GetbyConsultantId_ShouldReturnBadRequest_WhenValidationExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.GetAssignmentsByConsultantAsync(0, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid consultant id"));

        var result = await _controller.GetByConsultantId(0, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByConsultantId_ShouldReturnInternalServerError_WhenInvalidOperationExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.GetAssignmentsByConsultantAsync(10, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service failed"));

        var result = await _controller.GetByConsultantId(10, null, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByVisibility_ShouldReturnOk_WhenAssignmentsExist()
    {
        // arrange
        var assignments = new List<Assignment> { CreateValidAssignment() };

        _assignmentServiceMock
            .Setup(x => x.GetAssignmentsByVisibilityAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        // Act + assert
        var result = await _controller.GetByVisibility(true, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAssignments = Assert.IsAssignableFrom<IEnumerable<Assignment>>(okResult.Value);
        Assert.Single(returnedAssignments);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAssignmentsExist()
    {
        // Arrange
        var assignments = new List<Assignment> { CreateValidAssignment() };

        _assignmentServiceMock
            .Setup(x => x.GetAllSync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);

        // Act + assert
        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAssignments = Assert.IsAssignableFrom<IEnumerable<Assignment>>(okResult.Value);
        Assert.Single(returnedAssignments);
    }

    [Fact]
    public async Task GetAll_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        _assignmentServiceMock
            .Setup(x => x.GetAllSync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.GetAll(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenAssignmentIsUpdated()
    {
        // Arrange
        var existing = CreateValidAssignment();

        var request = new UpdateAssignmentRequest
        {
            ConsultantId = 20,
            DateStart = new DateOnly(2026, 4, 1),
            DateEnd = new DateOnly(2026, 4, 10)
        };

        var updated = new Assignment
        {
            Id = 1,
            ConsultantId = 20,
            DateStart = new DateOnly(2026, 4, 1),
            DateEnd = new DateOnly(2026, 4, 10),
            CourseId = 100,
            Course = new Course { Id = 100 }
        };

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _assignmentServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<Assignment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act + assert
        var result = await _controller.Update(1, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAssignment = Assert.IsType<Assignment>(okResult.Value);
        Assert.Equal(20, returnedAssignment.ConsultantId);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenAssignmentDontExist()
    {
        // Arrange
        var request = new UpdateAssignmentRequest
        {
            ConsultantId = 20,
            DateStart = new DateOnly(2026, 4, 1),
            DateEnd = new DateOnly(2026, 4, 10)
        };

        _assignmentServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Assignment?)null);

        // Act + assert
        var result = await _controller.Update(1, request, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}