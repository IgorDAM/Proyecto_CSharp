using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Middleware;
using MarinaApi.Repositories;
using MarinaApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Controladores ──
// Equivalente a que Spring escanee automáticamente las clases @RestController.
builder.Services.AddControllers();

// ── DbContext (equivalente a application.properties: spring.datasource.*) ──
// Usamos SQL Server (no MySQL como en el tutorial Java original) para practicar
// EF Core con un motor distinto al del tutorial. En SEIDEL se trabaja con
// MySQL y PostgreSQL: cambiar de proveedor es sustituir el paquete NuGet y
// UseSqlServer por UseMySql (Pomelo) o UseNpgsql (Lección 11.4 de la guía).
var connectionString = builder.Configuration.GetConnectionString("MarinaDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'MarinaDb' en appsettings.json");

builder.Services.AddDbContext<MarinaDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── Inyección de dependencias (equivalente a que Spring detecte @Service/@Repository) ──
// En ASP.NET Core no hay escaneo automático de componentes: cada servicio se
// registra explícitamente aquí. Es más verboso que Spring, pero también más
// explícito sobre qué implementación concreta usa cada interfaz — útil,
// por ejemplo, para intercambiar fácilmente un repositorio por un mock en tests.
builder.Services.AddScoped<IBarcoRepository, BarcoRepository>();
builder.Services.AddScoped<IAmarreRepository, AmarreRepository>();
builder.Services.AddScoped<IRegataRepository, RegataRepository>();
builder.Services.AddScoped<ITripulanteRepository, TripulanteRepository>();
builder.Services.AddScoped<IBarcoService, BarcoService>();
builder.Services.AddScoped<IAmarreService, AmarreService>();
builder.Services.AddScoped<IRegataService, RegataService>();
builder.Services.AddScoped<ITripulanteService, TripulanteService>();

// ── Swagger / OpenAPI (equivalente a springdoc-openapi del Capítulo 15) ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Marina API",
        Version = "v1",
        Description = "API REST de gestión marítima y regatas — puerto C# del proyecto Java original."
    });
});

// ── CORS (no existía en el proyecto Java; necesario si un frontend en otro
// origen va a consumir esta API — mejora de cara a producción) ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Middleware de errores global (ver Middleware/ExceptionHandlingMiddleware.cs) ──
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── Swagger UI, equivalente a http://localhost:8080/swagger-ui.html ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Marina API v1");
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necesario para que el proyecto de tests pueda referenciar Program vía
// WebApplicationFactory<Program> (xUnit + tests de integración).
public partial class Program { }
