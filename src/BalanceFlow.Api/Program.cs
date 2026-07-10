using BalanceFlow.Application;
using BalanceFlow.Infrastructure;
using BalanceFlow.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

// Enable legacy timestamp behavior in Npgsql to prevent UTC timezone mismatch exceptions
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services from other layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Web API layer services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add JWT Authentication
var secretKey = builder.Configuration["Jwt:Secret"] ?? "SuperSecretSecuritySigningKeyForBalanceFlowApplicationSystemAuditServiceService2026";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BalanceFlowIdentityServer",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BalanceFlowApi",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "BalanceFlow Micro-Accounting Platform API",
        Version = "v1",
        Description = "Production-grade double-entry ledger and auditing system built with Clean Architecture, CQRS, and MediatR."
    });

    // Configure Swagger UI to support Bearer token input
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input your JWT token in this format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BalanceFlow API v1");
});

// Exception handler MUST be configured before routing/controllers to catch all runtime exceptions
app.UseExceptionHandler();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run startup database DDL initialization and seed operations automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BalanceFlow.Infrastructure.Data.ApplicationDbContext>();
    try
    {
        // Execute raw SQL to ensure the Users table exists over PgBouncer
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""Id"" uuid NOT NULL,
                ""Username"" character varying(50) NOT NULL,
                ""PasswordHash"" character varying(250) NOT NULL,
                ""Role"" character varying(20) NOT NULL,
                ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                ""CreatedAt"" timestamp with time zone NOT NULL,
                ""ModifiedAt"" timestamp with time zone,
                ""IsDeleted"" boolean NOT NULL DEFAULT FALSE,
                CONSTRAINT ""PK_Users"" PRIMARY KEY (""Id"")
            );
            
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Username"" ON ""Users"" (""Username"");
        ");

        await BalanceFlow.Infrastructure.Data.DatabaseSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

app.Run();
