using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace SGS.Tests;

// TEntryPoint should point to your API project's Program class.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Optionally override ConfigureWebHost if you need custom configuration for tests.
    // For example, to use an in-memory database.
}