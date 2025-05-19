using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SGS.IntegrationTests.Controllers.Helpers;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TimeProvider timeProvider)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public static string CurrentUserId { get; set; } = "test-user-id";
    public static string CurrentFullName { get; set; } = "Test User";
    public static string CurrentEmail { get; set; } = "test@example.com";
    public static string CurrentRole { get; set; } = "Teacher";
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (TestContext.SimulateUnauthorized)
        {
            return Task.FromResult(AuthenticateResult.Fail("Simulated unauthorized"));
        }
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, CurrentUserId),
            new("fullname", CurrentFullName),
            new(JwtRegisteredClaimNames.Email, CurrentEmail),
            new(ClaimTypes.Role, CurrentRole)
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
            
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}