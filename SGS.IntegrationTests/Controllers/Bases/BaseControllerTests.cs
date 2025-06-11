using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.IntegrationTests.Controllers.Helpers;

namespace SGS.IntegrationTests.Controllers.Bases;

public class BaseControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    protected readonly HttpClient Client = factory.CreateClient();
    
    // protected readonly TestAuthHandler AuthHandler = new TestAuthHandler(
    //     factory.Services.GetRequiredService<IOptionsMonitor<AuthenticationSchemeOptions>>(),
    //     factory.Services.GetRequiredService<LoggerFactory>(),
    //     factory.Services.GetRequiredService<UrlEncoder>()
    // );
    
    protected const string TeacherEmail = "teacher1@example.com";
    protected const string StudentEmail = "student1@example.com";
    protected const string UserPassword = "Password123!";

    private string _currentToken = string.Empty;
    private string _currentRefreshToken = string.Empty;

    protected async Task AuthenticateAsTeacher()
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
        
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Authentication failed: {loginResponse.StatusCode}");
        }

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _currentToken = authResponse!.Token;
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
    }
    
    protected async Task AuthenticateAsStudent()
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
        
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Authentication failed: {loginResponse.StatusCode}");
        }
        
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        _currentToken = authResponse!.Token;
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
    }
    
    private async Task RefreshTokensIfNeeded()
    {
        if (string.IsNullOrEmpty(_currentRefreshToken))
            return;
        
        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = _currentRefreshToken };
        var refreshResponse = await Client.PostAsJsonAsync($"/api/auth/refresh", refreshRequest);
    
        if (refreshResponse.IsSuccessStatusCode)
        {
            var authResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            _currentToken = authResponse!.Token;
            _currentRefreshToken = authResponse.RefreshToken;
        
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
        }
    }

    public void Dispose()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }
}