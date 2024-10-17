using AspectCore.Api.Domain.Clients.Interfaces;
using AspectCore.Api.Domain.Repositories.Interfaces;
using AspectCore.Api.Domain.Services;
using AspectCore.Api.Domain.Services.Interfaces;
using AspectCore.Api.Infrastructure.Clients;
using AspectCore.Api.Infrastructure.Repositories;
using AspectCore.Api.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddScoped<ICityRepository, CityRepository>()
    .AddScoped<IWeatherForecastClient, WeatherForecastClient>()
    .AddScoped<IWeatherForecastService, WeatherForecastService>()
    .AddTrace();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseTrace();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
