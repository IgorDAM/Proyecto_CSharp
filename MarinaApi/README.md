# Marina API — Puerto a C# / ASP.NET Core

Conversión completa a C#/.NET 8 del proyecto Java (Hibernate → Spring Boot →
API REST) de gestión marítima y regatas. Mismo dominio (Barco, Amarre, Regata),
misma arquitectura por capas, pero con mejoras aplicadas allí donde el
ecosistema .NET lo permite de forma más limpia.

## Preparación para las prácticas en SEIDEL

Stack de SEIDEL frente a este proyecto:

| | Tutorial Java original | Este proyecto | SEIDEL |
|---|---|---|---|
| Lenguaje / framework | Java / Spring Boot | **C# / ASP.NET Core (.NET 8)** | .NET / C# |
| Base de datos | MySQL | **MySQL 8 (Pomelo)** | MySQL y PostgreSQL |
| ORM | Hibernate | **Entity Framework Core** | — |
| Integraciones | — | REST | Incluye sistemas legacy SOAP/XML |
| Despliegue | — | Local (Docker) | Azure y AWS |

El lenguaje, el framework y la base de datos coinciden con los de SEIDEL. El
proyecto empezó con SQL Server y se migró a MySQL el 2026-09-22 (sección 2.6 de
`MIGRACION_JAVA_A_CSHARP.md`); pasar a PostgreSQL se explica en la Lección 11.4
de la guía.

Sirve como práctica directa de varios bloques de `GUIA_DEFINITIVA_CSHARP_1.md`:
- **Lección 6** (Patrones de Diseño: Repository, DI, Factory) → `Repositories/` y `Services/`
- **Lección 8** (Entity Framework Core) → `Data/MarinaDbContext.cs`, migraciones con `dotnet ef`
- **Lecciones 11-13** (arquitectura backend, dominio y API lista para producción) → mejoras pendientes en `BACKLOG_MARINAAPI.md`
- **Lección 16** (Git avanzado) → usa este repo para practicar ramas `feature/*`, commits y PRs
- **Lección 17** (Scrum) → buen candidato para trocear en historias de usuario y practicar un sprint tú mismo

## Cómo ejecutar

Ningún secreto vive en el repo: la contraseña de MySQL va en `.env` y la cadena
de conexión de la API, en **user-secrets**. Todo se ejecuta desde la carpeta
`MarinaApi/`.

```powershell
# 1. Levantar MySQL (solo la primera vez: copia la plantilla y pon una contraseña)
Copy-Item .env.example .env        # edita MYSQL_ROOT_PASSWORD en .env
docker compose up -d

# 2. Crear el usuario de la aplicación (la API no se conecta como root)
docker exec -it marina-mysql mysql -u root -p
#   CREATE USER 'marina_app'@'%' IDENTIFIED BY '<CONTRASEÑA_APP>';
#   GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES
#     ON gestion_maritima.* TO 'marina_app'@'%';
#   exit

# 3. Guardar la cadena de conexión en user-secrets (fuera del repo)
dotnet user-secrets set "ConnectionStrings:MarinaDb" "Server=localhost;Port=3306;Database=gestion_maritima;User=marina_app;Password=<CONTRASEÑA_APP>;"

# 4. Aplicar las migraciones
dotnet tool install --global dotnet-ef   # solo la primera vez
dotnet ef database update

# 5. Generar un token JWT de desarrollo (todos los endpoints lo exigen)
dotnet user-jwts create --name igor

# 6. Arrancar la API
dotnet run --launch-profile http
```

Swagger UI queda en `http://localhost:5000/swagger` (equivalente a
`http://localhost:8080/swagger-ui.html` en el proyecto Java). Pulsa
**Authorize** y pega el token del paso 5, sin el prefijo `Bearer`. Sin token,
la API responde **401**.

```powershell
# Ejecutar los tests (desde la raíz del repo: no hay .sln)
dotnet test MarinaApi.Tests
```

> **Notas:**
> - Si `dotnet run` falla con `DirectoryNotFoundException ... wwwroot` después
>   de cambiar de rama, ejecuta `dotnet clean`: `bin/` conserva archivos
>   generados con la otra rama.
> - CORS no admite ningún origen externo por defecto. Para un frontend en otro
>   origen, añádelo a `Cors:AllowedOrigins` en `appsettings.json`.

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
