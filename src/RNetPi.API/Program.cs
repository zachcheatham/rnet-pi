using Microsoft.Extensions.DependencyInjection;
using RNetPi.Core.Interfaces;
using RNetPi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add RNetPi services
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
// Note: IRNetController implementation would be added here when available

var app = builder.Build();

// Initialize configuration
var configService = app.Services.GetRequiredService<IConfigurationService>();
await configService.LoadAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();
