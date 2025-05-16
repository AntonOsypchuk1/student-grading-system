using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;
using SGS.Tests.SeedData;

namespace SGS.Tests.Integration;

public class CriteriaControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/criteria";

    [Fact]
    public async Task GetCriteria_WithValidId_ReturnsCriteria()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidCriteria}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var criteria = await response.Content.ReadFromJsonAsync<GradingCriteriaDto>();
        Assert.NotNull(criteria);
        Assert.Equal(TestDataSeeder.GuidCriteria, criteria!.Id);
        Assert.Equal(TestDataSeeder.Criteria.Title, criteria.Title);
    }

    [Fact]
    public async Task GetCriteria_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllCriteria_ReturnsAllCriteria()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var criteria = await response.Content.ReadFromJsonAsync<IEnumerable<GradingCriteriaDto>>();
        Assert.NotNull(criteria);
        Assert.NotEmpty(criteria!);
    }
    
    [Fact]
    public async Task CreateCriteria_AsTeacher_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsTeacher();
        var command = new CreateGradingCriteriaCommand
        {
            Title = "New Test Criteria",
            Description = "Test criteria description",
            Weight = 0.5,
            AssignmentId = TestDataSeeder.GuidAssignment
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var criteria = await response.Content.ReadFromJsonAsync<GradingCriteriaDto>();
        Assert.NotNull(criteria);
        Assert.Equal(command.Title, criteria!.Title);
    }

    [Fact]
    public async Task CreateCriteria_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new CreateGradingCriteriaCommand
        {
            Title = "Unauthorized Criteria",
            Description = "This should fail",
            Weight = 0.5,
            AssignmentId = TestDataSeeder.GuidAssignment
        };

        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}