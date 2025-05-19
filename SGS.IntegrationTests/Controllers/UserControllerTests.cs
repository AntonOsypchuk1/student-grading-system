using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.Controllers.Helpers;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers;

public class UserControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/users";
    
    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserDto()
    {
        // Arrange
        await AuthenticateAsTeacher();

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userDto);
        Assert.Equal(TeacherEmail, userDto!.Email);
    }

    [Fact]
    public async Task GetCurrentUser_Unauthenticated_ReturnsUnauthorized()
    {
        using (TestContext.SimulateUnauthorizedScope())
        {
            var response = await Client.GetAsync($"{BaseUrl}/me");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetTeachers_ReturnsTeachers()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/teachers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var teachers = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        Assert.NotNull(teachers);
        Assert.Contains(teachers!, t => t.Id == TestDataSeeder.GuidTeacher);
    }

    [Fact]
    public async Task GetStudents_ReturnsStudents()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/students");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var students = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        Assert.NotNull(students);
        Assert.Contains(students!, s => s.Id == TestDataSeeder.GuidStudent);
    }

    [Fact]
    public async Task GetStudentsByGroup_WithValidGroupId_ReturnsStudents()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/groups/{TestDataSeeder.GuidGroup}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var students = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        Assert.NotNull(students);
        Assert.Contains(students!, s => s.Id == TestDataSeeder.GuidStudent);
    }

    [Fact]
    public async Task GetStudentsByGroup_WithInvalidGroupId_ReturnsNotFound()
    {
        // Arrange
        var invalidGroupId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/groups/{invalidGroupId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}