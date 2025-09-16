using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RNetPi.Core.Interfaces;
using RNetPi.Infrastructure.Services;
using RNetPi.Core.Logging;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced logging
builder.Logging.ClearProviders();
builder.Logging.AddEnhancedConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Set to Debug to see packet details

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RNetPi API",
        Version = "v1",
        Description = "REST API for controlling Russound whole home audio systems via RNet protocol. " +
                      "This API provides endpoints to control zones, sources, volume, and power states.",
        Contact = new OpenApiContact
        {
            Name = "RNetPi Project",
            Url = new Uri("https://github.com/mmackelprang/rnet-pi")
        }
    });

    // Include XML documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add security definition for webhook password
    c.AddSecurityDefinition("WebhookPassword", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Query,
        Name = "pass",
        Description = "Webhook password required for all API calls. Configure this in your config.json file."
    });

    // Apply security requirement globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "WebhookPassword"
                }
            },
            new string[] { }
        }
    });
});

// Add RNetPi services
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
// Note: IRNetController implementation would be added here when available

var app = builder.Build();

// Initialize configuration
var configService = app.Services.GetRequiredService<IConfigurationService>();
await configService.LoadAsync();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (not just Development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RNetPi API V1");
    c.RoutePrefix = "swagger"; // Makes Swagger available at /swagger
    c.DocumentTitle = "RNetPi API Documentation";
    c.DefaultModelsExpandDepth(-1); // Disable model schemas section by default
});

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();
