using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using SGS.Infrastructure.Data;
using SGS.Tests.Mocks;

namespace SGS.Tests.Integration;

public class BaseWebApplication(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly AppDbContext _context = null!;
    protected BaseWebApplication() : this(new WebApplicationFactory<Program>())
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _context = new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        TestDataSeeder.Seed(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}