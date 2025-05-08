using SGS.API.Extensions;
using SGS.API.Extensions.Configurations;
using SGS.API.Middleware;
using SGS.Application;
using SGS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://0.0.0.0:5296");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Optionally: builder.Logging.AddEventSourceLogger(); // for ETW logging

builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

var app = builder.Build();

app.UseSwagger();
if (!app.Environment.IsProduction())
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "SGS API v1"); });

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public abstract partial class Program;