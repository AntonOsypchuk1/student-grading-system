using SGS.API.Extensions;
using SGS.API.Middleware;
using SGS.Application;
using SGS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSettings();
builder.Configuration.Bind("JwtSettings", jwtSettings);
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

var app = builder.Build();

app.UseSwagger();
if (!app.Environment.IsProduction())
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "SGS API v1"); });

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public abstract partial class Program;