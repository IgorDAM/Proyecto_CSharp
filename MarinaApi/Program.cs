using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
//
// La cadena de conexión NO está en appsettings.json (que se sube a GitHub):
// vive en user-secrets, fuera del repo, en %APPDATA%\Microsoft\UserSecrets\.
// En Development, CreateBuilder() carga user-secrets automáticamente y su
// valor se superpone al de appsettings.json, igual que un
// application-local.properties que no se versiona en Spring.
// Para configurarla:
//   dotnet user-secrets set "ConnectionStrings:MarinaDb" "Server=...;User=marina_app;Password=..."
var connectionString = builder.Configuration.GetConnectionString("MarinaDb")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'MarinaDb'. Configúrala con: " +
        "dotnet user-secrets set \"ConnectionStrings:MarinaDb\" \"<cadena>\"");

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

// ── Autenticación JWT (equivalente a Spring Security + oauth2ResourceServer().jwt()) ──
// AddJwtBearer() sin opciones lee su configuración de la sección
// "Authentication:Schemes:Bearer": emisor y audiencias válidas en
// appsettings.Development.json, y la clave de firma en user-secrets. Ambas
// las genera `dotnet user-jwts create`, que además emite un token de pruebas.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// FallbackPolicy: cualquier endpoint SIN atributo de autorización exige un
// usuario autenticado. Es "seguro por defecto", como
// .anyRequest().authenticated() en Spring Security: un controlador nuevo
// queda protegido aunque se olvide poner [Authorize]. Para abrir un endpoint
// concreto habría que marcarlo explícitamente con [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

    // Botón "Authorize" en Swagger UI: declara que la API usa tokens Bearer
    // (AddSecurityDefinition) y que se aplican a todas las operaciones
    // (AddSecurityRequirement). En springdoc sería @SecurityScheme + @SecurityRequirement.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega el token de `dotnet user-jwts create` (sin el prefijo 'Bearer')."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS (no existía en el proyecto Java; necesario si un frontend en otro
// origen va a consumir esta API) ──
// Solo se aceptan los orígenes listados en "Cors:AllowedOrigins"
// (appsettings.json). Antes era AllowAnyOrigin(): cualquier web abierta en
// el navegador podía llamar a la API en localhost, incluidos los DELETE.
// Lista vacía = no se permite ningún origen externo. Equivale a
// CorsConfiguration.setAllowedOrigins(...) en Spring.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
              .WithHeaders("Authorization", "Content-Type"));
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
// El orden importa: primero se identifica al usuario leyendo el token
// (UseAuthentication) y después se decide si puede entrar (UseAuthorization).
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necesario para que el proyecto de tests pueda referenciar Program vía
// WebApplicationFactory<Program> (xUnit + tests de integración).
public partial class Program { }
