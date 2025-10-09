using BrightNess.Library.Configuration;
using Microsoft.EntityFrameworkCore;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Infrastructure.Data;
using PersonalCoachAPI.Infrastructure.Services;
using Pgvector.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Personal Coach API", Version = "v1" });
});

// Add Database Context
var connectionString = builder.Configuration.GetRequiredConnectionString("DefaultConnection");
builder.Services.AddDbContext<PersonalCoachDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseVector();
    });
});

// Add HTTP Client
builder.Services.AddHttpClient();

// Add Application Services
builder.Services.AddScoped<IRAGService, RAGService>();
builder.Services.AddScoped<IAIScheduleService, AIScheduleService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Coach API v1");
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

