using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SGS.API.Services;
using SGS.API.Settings;
using SGS.Application.Interfaces;
using SGS.Infrastructure.Data;
using SGS.IntegrationTests.Controllers.Helpers;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers.Bases;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var testConfig = new Dictionary<string, string>
            {
                ["JwtSettings:Secret"] = "test-secret-key-for-development-environment-only-12345",
                ["JwtSettings:Issuer"] = "test-issuer",
                ["JwtSettings:Audience"] = "test-audience",
                ["JwtSettings:ExpirationMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            };

            configBuilder.AddInMemoryCollection(testConfig);
        });


        builder.ConfigureServices(services =>
        {
            services.AddOptions();
        
            services.Configure<JwtSettings>(options =>
            {
                options.Secret = "test-secret-key-for-development-environment-only-12345";
                options.Issuer = "test-issuer";
                options.Audience = "test-audience";
                options.ExpirationMinutes = 60;
                options.RefreshTokenExpirationDays = 7;
            });

            services.RemoveAll<JwtBearerHandler>();
            services.RemoveAll<JwtBearerOptions>();
            services.RemoveAll<AuthenticationSchemeOptions>();
            
            services.AddScoped<ITokenService, JwtTokenService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.AddAuthorizationBuilder()
                .AddPolicy("RequireTeacherRole", policy => policy.RequireRole("Teacher"))
                .AddPolicy("RequireStudentRole", policy => policy.RequireRole("Student"));
            
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            TestDataSeeder.Seed(db);
        }).UseEnvironment("Testing");
        
        return base.CreateHost(builder);
    }
}
