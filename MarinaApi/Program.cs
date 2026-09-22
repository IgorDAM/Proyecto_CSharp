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
// Migrado de SQL Server a MySQL (Pomelo) para alinear el proyecto con el
// stack de SEIDEL. ServerVersion.AutoDetect() consulta al servidor su
// versión al arrancar, porque Pomelo genera SQL distinto según la versión
// de MySQL (soporte de JSON, funciones de ventana, etc.), igual que
// Hibernate elegía el dialecto (MySQL8Dialect) según la base de datos.
var connectionString = builder.Configuration.GetConnectionString("MarinaDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'MarinaDb' en appsettings.json");

builder.Services.AddDbContext<MarinaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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
