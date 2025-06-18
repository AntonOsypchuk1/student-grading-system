using System.Net.Http.Headers;
using System.Net.Http.Json;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Helpers;

namespace SGS.IntegrationTests.Controllers.Bases;

public class BaseControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    // , IDisposable
{
    protected const string TeacherEmail = "teacher1@example.com";
    protected const string StudentEmail = "student1@example.com";
    protected const string UserPassword = "Password123!";

    private string _currentToken = string.Empty;
    private string _currentRefreshToken = string.Empty;

    protected async Task AuthenticateAsTeacher(HttpClient client)
    {
        TestAuthHandler.CurrentRole = "Teacher";
        TestAuthHandler.CurrentEmail = TeacherEmail;
        TestAuthHandler.CurrentFullName = "Test Teacher";
        TestAuthHandler.CurrentUserId = "test-teacher-id";
        
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = UserPassword
        };
        
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Authentication failed: {loginResponse.StatusCode}");
        }

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _currentToken = authResponse!.Token;
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
    }
    
    protected async Task AuthenticateAsStudent(HttpClient client)
    {
        TestAuthHandler.CurrentRole = "Student";
        TestAuthHandler.CurrentEmail = StudentEmail;
        TestAuthHandler.CurrentFullName = "Test Student";
        TestAuthHandler.CurrentUserId = "test-student-id";
        
        var loginDto = new LoginDto
        {
            Email = StudentEmail,
            Password = UserPassword
        };
        
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Authentication failed: {loginResponse.StatusCode}");
        }
        
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _currentToken = authResponse!.Token;
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
    }
    
    private async Task RefreshTokensIfNeeded(HttpClient client)
    {
        if (string.IsNullOrEmpty(_currentRefreshToken))
            return;
        
        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = _currentRefreshToken };
        var refreshResponse = await client.PostAsJsonAsync($"/api/auth/refresh", refreshRequest);
    
        if (refreshResponse.IsSuccessStatusCode)
        {
            var authResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            _currentToken = authResponse!.Token;
            _currentRefreshToken = authResponse.RefreshToken;
        
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
        }
    }

    // public void Dispose()
    // {
    //     TestContext.SimulationAuthorizedContext();
    //     TestAuthHandler.CurrentRole = null;
    //     TestAuthHandler.CurrentEmail = null;
    //     TestAuthHandler.CurrentFullName = null;
    //     TestAuthHandler.CurrentUserId = null;
    // }
}