using AcademiaNovit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region configuracion del Serilog

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

# endregion

#region leer variables de entorno

builder.Configuration.AddEnvironmentVariables();

#endregion


string connectionString;
var secretFilePath = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING_FILE");

if (!string.IsNullOrEmpty(secretFilePath) && File.Exists(secretFilePath))
{
    // Leer desde Docker secret
    connectionString = File.ReadAllText(secretFilePath).Trim();
    Log.Information("Connection string loaded from Docker secret");
}
else
{
    // Fallback a appsettings.json (desarrollo local)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    Log.Information("Connection string loaded from appsettings.json (local development)");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

#region keep alive endpoint

app.MapGet("/keep-alive", () => new
{
    status = "alive",
    timestamp = DateTime.UtcNow
});

#endregion

app.Run();

public partial class Program { } // This partial class is required for the WebApplicationFactory to work properly in tests.