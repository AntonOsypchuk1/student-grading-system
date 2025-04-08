using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SGS.API.DTOs;
using SGS.Application.DTOs;

namespace SGS.Tests.Integration;

public class AuthControllerTests(WebApplicationFactory<Program> webApplicationFactory)
    : BaseWebApplication
{
    private readonly HttpClient _client = webApplicationFactory.CreateClient();
    private const string TeacherEmail = "teacher1@test.com";
    private const string TeacherPassword = "Password123!";

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = TeacherPassword
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(loginDto.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginDto = new LoginDto
        {
            Email = "wrong@test.com",
            Password = "WrongPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserDto()
    {
        // Login first to get token
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = TeacherPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var meResponse = await _client.GetAsync("/api/auth/me");

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Email.Should().Be(TeacherEmail);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsNewTokens()
    {
        var loginDto = new LoginDto
        {
            Email = TeacherEmail,
            Password = TeacherPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        var oldAccessToken = authResponse!.Token;
        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = authResponse.RefreshToken };
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.Token.Should().NotBe(oldAccessToken);
        newAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }
}