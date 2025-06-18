using System.Net;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.Controllers.Helpers;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers;

public class CriteriaControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests
{
    private readonly HttpClient _client = factory.CreateClient();
    
    private const string BaseUrl = "/api/criteria";

    [Fact]
    public async Task GetCriteria_WithValidId_ReturnsCriteria()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidCriteria}");

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
        var response = await _client.GetAsync($"{BaseUrl}/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllCriteria_ReturnsAllCriteria()
    {
        await AuthenticateAsTeacher(_client);
        var response = await _client.GetAsync($"{BaseUrl}/all");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var criteria = await response.Content.ReadFromJsonAsync<IEnumerable<GradingCriteriaDto>>();
        Assert.NotNull(criteria);
        Assert.NotEmpty(criteria!);
    }
    
    [Fact]
    public async Task CreateCriteria_AsTeacher_ReturnsCreated()
    {
        await AuthenticateAsTeacher(_client);
        
        // Arrange
        var command = new CreateGradingCriteriaCommand
        {
            Title = "New Test Criteria",
            Description = "Test criteria description", 
            MaxGrade = 100,
            Weight = 0.5,
            AssignmentId = TestDataSeeder.GuidAssignment
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var criteria = await response.Content.ReadFromJsonAsync<GradingCriteriaDto>();
        Assert.NotNull(criteria);
        Assert.Equal(command.Title, criteria!.Title);
    }

    [Fact]
    public async Task CreateCriteria_WithoutAuth_ReturnsUnauthorized()
    {
        var command = new CreateGradingCriteriaCommand
        {
            Title = "Unauthorized Criteria",
            Description = "This should fail",
            MaxGrade = 100,
            Weight = 0.5,
            AssignmentId = TestDataSeeder.GuidAssignment
        };

        HttpResponseMessage response;
        using (TestContext.SimulateUnauthorizedScope())
        {
            response = await _client.PostAsJsonAsync($"{BaseUrl}/create", command);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}