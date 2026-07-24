# Marina API — Puerto a C# / ASP.NET Core

Conversión completa a C#/.NET 8 del proyecto Java (Hibernate → Spring Boot →
API REST) de gestión marítima y regatas. Mismo dominio (Barco, Amarre, Regata),
misma arquitectura por capas, pero con mejoras aplicadas allí donde el
ecosistema .NET lo permite de forma más limpia.

## Alineado con la preparación para las prácticas en Espiral MS

Este proyecto usa deliberadamente el mismo stack que verás en las prácticas,
no el del tutorial Java original:

| | Tutorial Java original | Este proyecto (y Espiral MS) |
|---|---|---|
| Base de datos | MySQL | **SQL Server** |
| Proveedor EF/ORM | Hibernate | **Entity Framework Core** |
| Control de versiones | Git genérico | **Git / Azure DevOps** |

Sirve como práctica directa de varios bloques de tu `GUIA_DEFINITIVA_CSHARP.md`:
- **Lección 6** (Patrones de Diseño: Repository, DI, Factory) → `Repositories/` y `Services/`
- **Lección 8** (Entity Framework Core) → `Data/MarinaDbContext.cs`, migraciones con `dotnet ef`
- **Lección 9** (Git avanzado) → usa este repo para practicar ramas `feature/*`, commits y PRs antes de subirlo a Azure DevOps
- **Lección 10** (Scrum) → buen candidato para trocear en historias de usuario y practicar un sprint tú mismo (ver ejercicio de la Lección 10)

## Cómo ejecutar

```bash
# 1. Levantar SQL Server
docker compose up -d

# 2. Restaurar dependencias y aplicar migraciones
cd MarinaApi
dotnet restore
dotnet tool install --global dotnet-ef   # solo la primera vez
dotnet ef migrations add InitialCreate
dotnet ef database update

# 3. Arrancar la API
dotnet run
```

Swagger UI queda disponible en `https://localhost:PORT/swagger` (equivalente
a `http://localhost:8080/swagger-ui.html` en el proyecto Java).

```bash
# Ejecutar los tests
cd ../MarinaApi.Tests
dotnet test
```

> **Nota de conexión:** la cadena de conexión en `appsettings.json` usa el
> usuario `sa` con la contraseña definida en `docker-compose.yml`
> (`MSSQL_SA_PASSWORD`). Cámbiala en ambos sitios a la vez si la modificas.
> Si en tus prácticas usas SQL Server con autenticación de Windows en vez de
> `sa`/contraseña, cambia la cadena de conexión a
> `Server=localhost;Database=gestion_maritima;Trusted_Connection=True;TrustServerCertificate=True;`.

## Estructura del proyecto

```
MarinaApi/
├── Models/            → Entidades (Barco, Amarre, Regata)
├── Data/               → DbContext (sustituye a hibernate.cfg.xml + HibernateUtil)
├── Dtos/               → DTOs de entrada/salida (records)
├── Mapping/            → Extension methods Entidad ↔ DTO (sustituye a BarcoMapper.java)
├── Repositories/        → Repositorio genérico + específicos (sustituye a los 3 DAO de Java)
├── Services/           → Lógica de negocio, transacciones (equivalente a @Service)
├── Controllers/         → API REST (equivalente a @RestController)
├── Middleware/          → Manejo global de errores (mejora: no existía en Java)
├── Exceptions/          → Excepciones de dominio
└── Program.cs           → Composición de la app (equivalente a application.properties + auto-config de Spring Boot)

MarinaApi.Tests/
└── BarcoServiceTests.cs → xUnit + Moq (equivalente a JUnit 5 + Mockito)
```

## Equivalencias directas con el proyecto Java

| Java / Spring Boot | C# / ASP.NET Core |
|---|---|
| Hibernate / JPA | Entity Framework Core |
| `hibernate.cfg.xml` + anotaciones `@Entity` | `MarinaDbContext` (Fluent API) |
| `@Id @GeneratedValue(IDENTITY)` | `long Id` (convención: EF Core lo detecta solo) |
| `@OneToOne(mappedBy=...)` / `@JoinColumn` | `HasOne().WithOne().HasForeignKey()` en el DbContext |
| `@ManyToMany` + `@JoinTable` + tabla intermedia manual | `HasMany().WithMany()` — EF Core 5+ genera la tabla intermedia solo |
| Lombok `@Data` | `record` (para DTOs) / propiedades auto-implementadas |
| `BarcoDAO` + `BarcoDAOImpl` (uno por entidad, ~150 líneas cada uno) | `GenericRepository<T>` único + interfaces específicas para consultas custom |
| HQL / `@Query` | LINQ (`Where`, `Include`, expresiones lambda) |
| `JpaRepository<Barco, Long>` | `IBarcoRepository : IGenericRepository<Barco>` |
| `@Service` + `@Autowired` | Clase de servicio + inyección por constructor (`AddScoped` en `Program.cs`) |
| `@Transactional` | `SaveChangesAsync()` (transacción implícita) o `BeginTransactionAsync()` explícita |
| `@RestController` + `@RequestMapping` | `[ApiController]` + `[Route]` |
| `ResponseEntity<T>` | `ActionResult<T>` |
| `@RequestBody` / `@PathVariable` | `[FromBody]` / ruta con plantilla `{id:long}` |
| Comprobar `null` y devolver 404 a mano en cada endpoint | `NotFoundException` + middleware global → 404 automático y uniforme |
| Springdoc OpenAPI (`@Tag`, `@Operation`) | Swashbuckle (genera casi todo solo, sin anotar cada método) |
| JUnit 5 + Mockito | xUnit + Moq + FluentAssertions |
| `application.properties` | `appsettings.json` |

## Mejoras aplicadas sobre el proyecto Java original

1. **Todo asíncrono de extremo a extremo** (`async`/`await` + `CancellationToken`
   en cada capa). El proyecto Java era síncrono/bloqueante en el DAO original y
   solo parcialmente asíncrono más adelante.
2. **Repositorio genérico real**: elimina la triplicación de código que había
   en `BarcoDAOImpl` / `AmarreDAOImpl` / `RegataDAOImpl` (~450 líneas casi
   idénticas reducidas a una única clase genérica + 3 interfaces pequeñas con
   solo las consultas específicas de cada entidad).
3. **Manejo de errores centralizado**: un único middleware traduce cualquier
   excepción de dominio a una respuesta HTTP uniforme (formato
   `problem+json`, RFC 7807), en vez de repetir `if (x == null) return 404`
   en cada método de cada controlador.
4. **Transacciones explícitas donde de verdad hacen falta**: `RegataService.DeleteAsync`
   envuelve la desvinculación N:M + el borrado en una única transacción real
   con `commit`/`rollback`, algo que el `deleteWithCleanup` de Java no
   garantizaba (hacía varios `save()` sueltos, cada uno con su propia
   transacción implícita).
5. **DTOs de entrada y salida separados** (`BarcoRequestDto` vs `BarcoDto`):
   el cliente nunca puede fijar el `Id` al crear un barco, cosa que el
   `BarcoDTO` único de Java sí permitía por descuido.
6. **`record` para los DTOs**: inmutables por defecto, con igualdad por valor
   y `ToString()` generados automáticamente — el equivalente en una sola
   palabra clave a `@Data + @AllArgsConstructor` de Lombok, sin depender de
   una librería externa.
7. **CORS configurado** desde el principio, pensando en que un frontend en
   otro origen consuma la API — el proyecto Java no lo contemplaba.
8. **Logging estructurado con `ILogger<T>`** en vez de `System.out.println`.
