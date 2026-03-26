using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using urban_dukan_product_service.Data;
using urban_dukan_product_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
});

var app = builder.Build();

// Run automatic migrations and seed DB at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        //var db = services.GetRequiredService<UrbanDukanProductDbContext>();
        //logger.LogInformation("Applying migrations...");
        //db.Database.Migrate();

        //var seeder = services.GetRequiredService<IDbSeeder>();
        //logger.LogInformation("Starting DB seeding...");
        //seeder.SeedAsync().GetAwaiter().GetResult();
        //logger.LogInformation("DB seeding finished.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed failed.");
        // rethrow if you want the app to stop on seed failure:
        // throw;
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

app.UseAuthorization();

app.MapControllers();

app.Run();
