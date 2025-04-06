using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SGS.API.DTOs;
using SGS.Application.DTOs;

namespace SGS.Tests.Integration;

public class AuthControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        var loginDto = new LoginDto
        {
            Email = "teacher1@example.com", // Ensure these match seeded test data
            Password = "your_password"      // Password should be set appropriately
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(loginDto.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "invalid"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserDto()
    {
        var loginDto = new LoginDto
        {
            Email = "teacher1@example.com",
            Password = "your_password"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        var token = authResponse.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var meResponse = await _client.GetAsync("/api/auth/me");

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto.Email.Should().Be(loginDto.Email);
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsNewTokens()
    {
        var loginDto = new LoginDto
        {
            Email = "teacher1@example.com",
            Password = "your_password"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        var oldAccessToken = authResponse.Token;
        var refreshToken = authResponse.RefreshToken;

        var refreshRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse.Token.Should().NotBe(oldAccessToken, "a new access token should be issued");
        newAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }
}