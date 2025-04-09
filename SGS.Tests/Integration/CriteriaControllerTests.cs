using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;
using SGS.Tests.Mocks;

namespace SGS.Tests.Integration;

public class CriteriaControllerTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string BaseUrl = "/api/criteria";

    [Fact]
    public async Task GetCriteria_WithValidId_ReturnsCriteria()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidCriteria}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var criteria = await response.Content.ReadFromJsonAsync<GradingCriteriaDto>();
        criteria.Should().NotBeNull();
        criteria!.Id.Should().Be(TestDataSeeder.GuidCriteria);
        criteria.CriteriaDescription.Should().Be("Correctness of Solution");
    }

    [Fact]
    public async Task GetCriteria_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{BaseUrl}/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllCriteria_ReturnsAllCriteria()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var criteria = await response.Content.ReadFromJsonAsync<IEnumerable<GradingCriteriaDto>>();
        criteria.Should().NotBeNull();
        criteria!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task CreateCriteria_AsTeacher_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsTeacher();
        var command = new CreateGradingCriteriaCommand
        {
            UserId = TestDataSeeder.GuidTeacher,
            CriteriaDescription = "New Test Criteria"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/create", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criteria = await response.Content.ReadFromJsonAsync<GradingCriteriaDto>();
        criteria.Should().NotBeNull();
        criteria!.CriteriaDescription.Should().Be(command.CriteriaDescription);
    }

    [Fact]
    public async Task CreateCriteria_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new CreateGradingCriteriaCommand
        {
            UserId = TestDataSeeder.GuidTeacher,
            CriteriaDescription = "Unauthorized Criteria"
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
            Password = "Password123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }
}