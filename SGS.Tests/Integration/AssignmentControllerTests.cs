using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;
using SGS.Tests.SeedData;

namespace SGS.Tests.Integration;

public class AssignmentControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string BaseUrl = "/api/assignments";
    private const string TeacherEmail = "teacher1@example.com";
    private static readonly string TeacherPassword = string.Empty;

    [Fact]
    public async Task GetAssignmentById_WithValidId_ReturnsAssignment()
    {
        // Arrange - Use the existing assignment ID from seeder
        var assignmentId = await GetExistingAssignmentId();

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/{assignmentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignment = await response.Content.ReadFromJsonAsync<AssignmentDto>();
        assignment.Should().NotBeNull();
        assignment!.Title.Should().Be("Algebra Test");
    }

    [Fact]
    public async Task GetAssignmentsByGroup_WithValidGroupId_ReturnsAssignments()
    {
        // Arrange - Use the existing group ID from seeder
        var groupId = TestDataSeeder.GuidGroup;

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/group/{groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignments = await response.Content.ReadFromJsonAsync<IEnumerable<AssignmentDto>>();
        assignments.Should().NotBeNull();
        assignments!.Should().NotBeEmpty();
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
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var assignment = await response.Content.ReadFromJsonAsync<AssignmentDto>();
        assignment.Should().NotBeNull();
        assignment!.Title.Should().Be(command.Title);
    }

    [Fact]
    public async Task CreateAssignment_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new CreateAssignmentCommand
        {
            Title = "Unauthorized Assignment"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task AuthenticateAsTeacher()
    {
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = TeacherPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }

    private async Task<Guid> GetExistingAssignmentId()
    {
        var groupId = TestDataSeeder.GuidGroup;
        var response = await _client.GetAsync($"{BaseUrl}/group/{groupId}");
        var assignments = await response.Content.ReadFromJsonAsync<IEnumerable<AssignmentDto>>();
        return assignments!.First().Id;
    }
}