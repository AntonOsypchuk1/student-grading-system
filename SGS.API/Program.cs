using Microsoft.EntityFrameworkCore;
using SGS.API.Extensions.Configurations;
using SGS.API.Middleware;
using SGS.Application;
using SGS.DbContext;
using SGS.DbContext.Data;
using SGS.SemanticAnalysis;
using SGS.SemanticAnalysis.Client;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://0.0.0.0:5296");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Optionally: builder.Logging.AddEventSourceLogger(); // for ETW logging

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddAuthServices(builder.Configuration);
}

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

var allowedOrigins = new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(b =>
    {
        b.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseCors();
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "SGS API v1"); });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
// app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public abstract partial class Program;
