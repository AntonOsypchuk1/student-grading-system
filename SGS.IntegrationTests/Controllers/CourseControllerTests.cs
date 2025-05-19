using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.Controllers.Helpers;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers;

public class CourseControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/courses";

    [Fact]
    public async Task GetCourseById_WithValidId_ReturnsCourse()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidCourse1}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>();
        Assert.NotNull(course);
        Assert.Equal(TestDataSeeder.GuidCourse1, course!.Id);
        Assert.Equal(TestDataSeeder.Course1.Title, course.Title);
    }

    [Fact]
    public async Task GetCoursesByTeacher_AsTeacher_ReturnsCourses()
    {
        // Arrange
        await AuthenticateAsTeacher();

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/teacher/{TestDataSeeder.GuidTeacher}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var courses = await response.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        Assert.NotNull(courses);
        Assert.NotEmpty(courses!);
    }

    [Fact]
    public async Task GetCoursesByStudent_AsStudent_ReturnsCourses()
    {
        // Arrange
        await AuthenticateAsStudent();

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/student/{TestDataSeeder.GuidStudent}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var courses = await response.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        Assert.NotNull(courses);
        Assert.NotEmpty(courses!);
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
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>();
        Assert.NotNull(course);
        Assert.Equal(command.Title, course!.Title);
    }

    [Fact]
    public async Task CreateCourse_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Title = "Unauthorized Course"
        };

        using (TestContext.SimulateUnauthorizedScope())
        {
            // Act
            var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}