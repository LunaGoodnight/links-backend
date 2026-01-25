using System.Text;
using Amazon.S3;
using LinksService.Data;
using LinksService.Models;
using LinksService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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

// Add Entity Framework
builder.Services.AddDbContext<LinksContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 4, 0)),
    o => o.EnableRetryOnFailure()));

// Configure JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
               builder.Configuration["Jwt:Secret"] ??
               "default-development-secret-key-change-in-production";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "LinksService",
            ValidAudience = "LinksAdmin",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Configure S3 with settings from environment variables
builder.Services.AddSingleton<IAmazonS3>(s =>
{
    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY") ??
                   Environment.GetEnvironmentVariable("AWS__AccessKey") ??
                   builder.Configuration["AWS:AccessKey"];
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY") ??
                   Environment.GetEnvironmentVariable("AWS__SecretKey") ??
                   builder.Configuration["AWS:SecretKey"];
    var serviceUrl = Environment.GetEnvironmentVariable("AWS_SERVICE_URL") ??
                    Environment.GetEnvironmentVariable("AWS__ServiceURL") ??
                    builder.Configuration["AWS:ServiceURL"] ??
                    "https://sgp1.digitaloceanspaces.com";

    return new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = false
    });
});

// Register image upload service
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

var app = builder.Build();

// Apply database migrations and seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LinksContext>();
    context.Database.Migrate();

    // Seed admin user if not exists
    if (!context.AdminUsers.Any())
    {
        var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin123";

        context.AdminUsers.Add(new AdminUser
        {
            Username = adminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
