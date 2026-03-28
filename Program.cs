using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using urban_dukan_product_service.Data;
using urban_dukan_product_service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Key"]; // store in appsettings or env
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Read allowed origins from configuration (falls back to empty array)
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? Array.Empty<string>();

// Register CORS policy using configured origins. If none are provided, default to allowing only local dev origin.
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalUI", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            // Fallback: allow the common Angular dev origin so local dev doesn't break
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Configure DbContext (reads connection string from appsettings.json)
builder.Services.AddDbContext<UrbanDukanProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UrbanDukanProductDb")));

// Register the DB-backed product service
builder.Services.AddScoped<IProductService, ProductService>();

// Http client used only for initial seeding from external dummyjson
builder.Services.AddHttpClient("external", c =>
{
    c.BaseAddress = new Uri("https://dummyjson.com/");
});

// Register seeder
builder.Services.AddScoped<IDbSeeder, DbSeeder>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Urban Dukan Product API",
        Version = "v1",
        Description = "Product API backed by UrbanDukanProductDb.",
        Contact = new OpenApiContact { Name = "Urban Dukan", Email = "dev@example.com" }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Run DB seeding at startup (migrations intentionally NOT run here)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var seeder = services.GetRequiredService<IDbSeeder>();
        logger.LogInformation("Starting DB seeding...");
        seeder.SeedAsync().GetAwaiter().GetResult();
        logger.LogInformation("DB seeding finished.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database seed failed.");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrbanDukan User Service v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// Apply CORS before Authorization and routing
app.UseCors("LocalUI");

app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.Run();
