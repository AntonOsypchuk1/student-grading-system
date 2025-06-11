using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.Controllers.Helpers;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers;

public class AssignmentControllerTests(CustomWebApplicationFactory<Program> factory)
    : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/assignments";

    [Fact]
    public async Task GetAssignmentById_WithValidId_ReturnsAssignment()
    {
        TestContext.SimulationAuthorizedContext();
        
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidAssignment}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var assignment = await response.Content.ReadFromJsonAsync<AssignmentDto>();
        Assert.NotNull(assignment);
        Assert.Equal("Algebra Test", assignment.Title);
    }


    [Fact]
    public async Task GetAssignmentsByGroup_WithValidGroupId_ReturnsAssignments()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/group/{TestDataSeeder.GuidGroup}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var assignments = await response.Content.ReadFromJsonAsync<IEnumerable<AssignmentDto>>();
        Assert.NotNull(assignments);
        Assert.NotEmpty(assignments);
    }

    [Fact]
    public async Task CreateAssignment_AsTeacher_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsTeacher();
        var command = new CreateAssignmentCommand
        {
            GroupId = TestDataSeeder.GuidGroup,
            CourseId = TestDataSeeder.GuidCourse1,
            Title = "New Test Assignment",
            Description = "Test Description",
            MaxAttempts = 2,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var assignment = await response.Content.ReadFromJsonAsync<AssignmentDto>();
        Assert.NotNull(assignment);
        Assert.Equal(command.Title, assignment.Title);

        Dispose();
    }


    [Fact]
    public async Task CreateAssignment_WithoutAuth_ReturnsUnauthorized()
    {
        TestContext.SimulateUnauthorizedScope();
            
        // Arrange
        var command = new CreateAssignmentCommand
        {
            GroupId = TestDataSeeder.GuidGroup,
            CourseId = TestDataSeeder.GuidCourse1,
            Title = "Unauthorized Assignment",
            Description = "This should fail",
            MaxAttempts = 1,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}