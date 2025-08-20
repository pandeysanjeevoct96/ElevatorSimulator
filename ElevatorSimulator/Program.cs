using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using ElevatorSimulator.Services;

var builder = WebApplication.CreateBuilder(args);

// Register your custom service
builder.Services.AddSingleton<ElevatorSimulationService>();

// Allow Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Elevator Simulator API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elevator Simulator API v1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();
