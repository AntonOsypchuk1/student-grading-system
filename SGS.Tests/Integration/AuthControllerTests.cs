using System.Net;
using System.Net.Http.Json;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Bases;

namespace SGS.Tests.Integration;

public class AuthControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/auth";

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
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);

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
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);

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
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", registerCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserDto()
    {
        // Arrange
        await AuthenticateAsTeacher();

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userDto);
        Assert.Equal(TeacherEmail, userDto!.Email);
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
        var loginResponse = await Client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var oldAccessToken = authResponse!.Token;
        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = authResponse.RefreshToken };
        
        // Act
        var refreshResponse = await Client.PostAsJsonAsync($"{BaseUrl}/refresh", refreshRequest);

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
        var loginResponse = await Client.PostAsJsonAsync($"{BaseUrl}/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var logoutRequest = new RefreshTokenRequestDto { RefreshToken = authResponse!.RefreshToken };
        
        // Act
        var logoutResponse = await Client.PostAsJsonAsync($"{BaseUrl}/logout", logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }
}