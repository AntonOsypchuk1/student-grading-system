using Microsoft.AspNetCore.Mvc.Testing;
using SGS.Tests.Integration.Bases;

namespace SGS.Tests.Integration;

public class GradingControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
}