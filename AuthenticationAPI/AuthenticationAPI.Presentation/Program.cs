using System;
using System.Threading.Tasks;
using Npgsql;
using AuthenticationAPI.Infrastructure.Data;
using AuthenticationAPI.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    // {
    //     Name = "Authorization",
    //     Type = SecuritySchemeType.Http,
    //     Scheme = "bearer",
    //     BearerFormat = "JWT",
    //     In = ParameterLocation.Header,
    //     Description = "JWT Authorization header using the Bearer scheme."
    // });

    // c.AddSecurityDefinition("ApiGatewayHeader", new OpenApiSecurityScheme
    // {
    //     In = ParameterLocation.Header,
    //     Name = "Api-Gateway",
    //     Type = SecuritySchemeType.ApiKey,
    //     Description = "Set to any non-empty value when testing without API Gateway."
    // });

    // c.AddSecurityRequirement(new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    //         },
    //         Array.Empty<string>()
    //     },
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiGatewayHeader" }
    //         },
    //         Array.Empty<string>()
    //     }
    // });
});
builder.Services.AddInfrastructureService(builder.Configuration);

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
const int maxRetryAttempts = 15;
var delay = TimeSpan.FromSeconds(2);
var migrationSucceeded = false;
Exception? lastException = null;

for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
{
    try
    {
        await dbContext.Database.MigrateAsync();
        migrationSucceeded = true;
        break;
    }
    catch (Exception ex) when (IsDatabaseStartingUp(ex) && attempt < maxRetryAttempts)
    {
        lastException = ex;
        app.Logger.LogWarning(ex, "PostgreSQL is not ready yet (attempt {Attempt}/{MaxAttempts}). Waiting {Delay} before retry.", attempt, maxRetryAttempts, delay);
        await Task.Delay(delay);
    }
}

if (!migrationSucceeded)
{
    throw new InvalidOperationException($"Database migration failed because PostgreSQL was not ready after {maxRetryAttempts} attempts.", lastException);
}

//app.UserInfrastructurePolicy();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();

static bool IsDatabaseStartingUp(Exception ex)
{
    return ex switch
    {
        PostgresException postgres when postgres.SqlState == "57P03" => true,
        NpgsqlException npgsql when npgsql.SqlState == "57P03" => true,
        _ when ex.InnerException is not null => IsDatabaseStartingUp(ex.InnerException),
        _ => false
    };
}




