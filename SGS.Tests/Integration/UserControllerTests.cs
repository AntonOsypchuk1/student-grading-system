using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;
using SGS.Tests.Mocks;

namespace SGS.Tests.Integration;

public class UserControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string BaseUrl = "/api/users";

    [Fact]
    public async Task GetCurrentUser_Authenticated_ReturnsUser()
    {
        // Arrange
        await AuthenticateAsTeacher();

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Email.Should().Be("teacher1@example.com");
    }

    [Fact]
    public async Task GetCurrentUser_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTeachers_ReturnsTeachers()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/teachers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var teachers = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        teachers.Should().NotBeNull();
        teachers!.Should().Contain(t => t.Id == TestDataSeeder.GuidTeacher);
    }

    [Fact]
    public async Task GetStudents_ReturnsStudents()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/students");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var students = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        students.Should().NotBeNull();
        students!.Should().Contain(s => s.Id == TestDataSeeder.GuidStudent);
    }

    [Fact]
    public async Task GetStudentsByGroup_WithValidGroupId_ReturnsStudents()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/groups/{TestDataSeeder.GuidGroup}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var students = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        students.Should().NotBeNull();
        students!.Should().Contain(s => s.Id == TestDataSeeder.GuidStudent);
    }

    [Fact]
    public async Task GetStudentsByGroup_WithInvalidGroupId_ReturnsNotFound()
    {
        // Arrange
        var invalidGroupId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/groups/{invalidGroupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task AuthenticateAsTeacher()
    {
        var loginDto = new LoginDto
        {
            Email = "teacher1@example.com",
            Password = "Password123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }
}