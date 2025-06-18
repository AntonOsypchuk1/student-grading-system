using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.Controllers.Helpers;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers;

public class UserControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests
{
    private readonly HttpClient _client = factory.CreateClient();
    
    private const string BaseUrl = "/api/users";
    
    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserDto()
    {
        await AuthenticateAsTeacher(_client);
        var response = await _client.GetAsync($"{BaseUrl}/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userDto);
        Assert.Equal(TeacherEmail, userDto!.Email);
    }

    [Fact]
    public async Task GetCurrentUser_Unauthenticated_ReturnsUnauthorized()
    {
        HttpResponseMessage response;
        using (TestContext.SimulateUnauthorizedScope())
        {
            response = await _client.GetAsync($"{BaseUrl}/me");
        }

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTeachers_ReturnsTeachers()
    {
        await AuthenticateAsTeacher(_client);
        var response = await _client.GetAsync($"{BaseUrl}/teachers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var teachers = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        Assert.NotNull(teachers);
        Assert.Contains(teachers!, t => t.Id == TestDataSeeder.GuidTeacher);
    }

    [Fact]
    public async Task GetStudents_ReturnsStudents()
    {
        await AuthenticateAsTeacher(_client);
        var response = await _client.GetAsync($"{BaseUrl}/students");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var students = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        Assert.NotNull(students);
        Assert.Contains(students!, s => s.Id == TestDataSeeder.GuidStudent);
    }

    [Fact]
    public async Task GetStudentsByGroup_WithValidGroupId_ReturnsStudents()
    {
        await AuthenticateAsStudent(_client);
        var response = await _client.GetAsync($"api/groups/{TestDataSeeder.GuidGroup}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        Assert.NotNull(group);
        Assert.Contains(group.Students, s => s.Id == TestDataSeeder.GuidStudent);
    }

    [Fact]
    public async Task GetStudentsByGroup_WithInvalidGroupId_ReturnsNotFound()
    {
        var invalidGroupId = Guid.NewGuid();
        var response = await _client.GetAsync($"{BaseUrl}/groups/{invalidGroupId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}