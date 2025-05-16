using System.Net.Http.Headers;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.Tests.Integration.Helpers;
using SGS.Tests.SeedData;

namespace SGS.Tests.Integration.Bases;

public class BaseControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly CustomWebApplicationFactory<Program> Factory = factory;
    
    protected const string TeacherEmail = "teacher1@example.com";
    protected const string StudentEmail = "student1@example.com";
    protected const string UserPassword = "Password123!";


    protected async Task AuthenticateAsTeacher()
    {
        TestAuthHandler.CurrentRole = "Teacher";
        
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
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }
    
    protected async Task AuthenticateAsStudent()
    {
        TestAuthHandler.CurrentRole = "Student";
        
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
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
    }
}