using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.Controllers.Helpers;

namespace SGS.IntegrationTests.Controllers;

public class AuthControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests
{
    private readonly HttpClient _client = factory.CreateClient();
    
    private const string BaseUrl = "/api/auth";
    private string _refreshToken = string.Empty;

    [Fact]
    public async Task TokenLifecycleTest()
    {
        // 1. Login to get initial tokens
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = UserPassword
        };
        
        var loginResponse = await _client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var initialToken = authResponse!.Token;
        _refreshToken = authResponse.RefreshToken;
        
        await Task.Delay(1100);
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);
        
        // 2. Access a protected resource
        var meResponse = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        
        // 3. Refresh the token
        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = _refreshToken };
        var refreshResponse = await _client.PostAsJsonAsync($"{BaseUrl}/refresh", refreshRequest);
        
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var newToken = newAuthResponse!.Token;
        _refreshToken = newAuthResponse.RefreshToken; // Update the refresh token
        
        // 4. Use the new token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        var meResponse2 = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse2.StatusCode);
        
        // 5. Logout
        var logoutRequest = new RefreshTokenRequestDto { RefreshToken = _refreshToken };
        var logoutResponse = await _client.PostAsJsonAsync($"{BaseUrl}/logout", logoutRequest);
        
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        
        // 6. Verify the token is invalidated
        using (TestContext.SimulateUnauthorizedScope())
        {
            var meResponse3 = await _client.GetAsync("/api/users/me");
            Assert.Equal(HttpStatusCode.Unauthorized, meResponse3.StatusCode);
        }
    }
    
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = UserPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse!.Token);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.NotNull(authResponse.User);
        Assert.Equal(loginDto.Email, authResponse.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "wrong@test.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            Email = "newuser@test.com",
            Password = "Password123!",
            FullName = "New Test User",
            Login = "newuser",
            Role = "Student"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/register", registerCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = UserPassword
        };
        var loginResponse = await _client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var oldAccessToken = authResponse!.Token;
        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = authResponse.RefreshToken };
        
        await Task.Delay(1100);

        // Act
        var refreshResponse = await _client.PostAsJsonAsync($"{BaseUrl}/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(newAuthResponse);
        Assert.NotEqual(oldAccessToken, newAuthResponse!.Token);
        Assert.NotEmpty(newAuthResponse.RefreshToken);
    }
    
    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsSuccess()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = UserPassword
        };
        var loginResponse = await _client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var logoutRequest = new RefreshTokenRequestDto { RefreshToken = authResponse!.RefreshToken };
        
        // Act
        var logoutResponse = await _client.PostAsJsonAsync($"{BaseUrl}/logout", logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }
}