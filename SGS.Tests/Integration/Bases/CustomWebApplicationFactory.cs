using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SGS.Infrastructure.Data;
using SGS.Tests.Mocks;

namespace SGS.Tests.Integration.Bases;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    // TODO: Find out how to override the DbContext to use the in-memory database.
    // TODO: Find out how to override the JWT authentication to use a test token.
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all registrations for DbContextOptions<AppDbContext>
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // Remove any DbConnection registrations if they exist.
            services.RemoveAll<DbConnection>();
            
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb");
            });
            
            var sp = services.BuildServiceProvider();
            
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                
                TestDataSeeder.Seed(db);
                db.SaveChanges();
            }
        });
        
        return base.CreateHost(builder);
    }
}
