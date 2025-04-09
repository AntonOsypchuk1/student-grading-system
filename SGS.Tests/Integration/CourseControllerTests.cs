using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;
using SGS.Tests.Mocks;

namespace SGS.Tests.Integration;

public class CourseControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
     private const string BaseUrl = "/api/courses";

    [Fact]
    public async Task GetCourseById_WithValidId_ReturnsCourse()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidCourse1}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>();
        course.Should().NotBeNull();
        course!.Id.Should().Be(TestDataSeeder.GuidCourse1);
        course.Title.Should().Be("Mathematics 101");
    }

    [Fact]
    public async Task GetCoursesByTeacher_AsTeacher_ReturnsCourses()
    {
        // Arrange
        await AuthenticateAsTeacher();

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/teacher/{TestDataSeeder.GuidTeacher}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        courses.Should().NotBeNull();
        courses!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetCoursesByStudent_AsStudent_ReturnsCourses()
    {
        // Arrange
        await AuthenticateAsStudent();

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/student/{TestDataSeeder.GuidStudent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        courses.Should().NotBeNull();
        courses!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task CreateCourse_AsTeacher_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsTeacher();
        var command = new CreateCourseCommand
        {
            Title = "New Test Course",
            Description = "Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>();
        course.Should().NotBeNull();
        course!.Title.Should().Be(command.Title);
    }

    [Fact]
    public async Task CreateCourse_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Title = "Unauthorized Course"
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
            Email = "teacher1@example.com",
            Password = string.Empty
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }

    private async Task AuthenticateAsStudent()
    {
        var loginDto = new LoginDto
        {
            Email = "student1@example.com",
            Password = string.Empty
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }
}