using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;
using SGS.Tests.SeedData;

namespace SGS.Tests.Integration;

public class GradingControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/grading";

    [Fact]
    public async Task GetGradesByAssignment_ReturnsGrades()
    {
        // Arrange
        await AuthenticateAsTeacher();
        
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/assignment/{TestDataSeeder.GuidAssignment}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var grades = await response.Content.ReadFromJsonAsync<IEnumerable<GradingCriteriaDto>>();
        Assert.NotNull(grades);
    }
    
    // [Fact]
    // public async Task CreateGrade_AsTeacher_ReturnsCreated()
    // {
    //     // Arrange
    //     await AuthenticateAsTeacher();
    //     var command = new CreateGradingCriteriaCommand()
    //     {
    //         AssignmentId = TestDataSeeder.GuidAssignment,
    //         StudentId = TestDataSeeder.GuidStudent,
    //         TeacherId = TestDataSeeder.GuidTeacher,
    //         Score = 85,
    //         Comment = "Good work"
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);
    //
    //     // Assert
    //     Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    //     var grade = await response.Content.ReadFromJsonAsync<GradeDto>();
    //     Assert.NotNull(grade);
    //     Assert.Equal(command.Score, grade!.Score);
    // }
}