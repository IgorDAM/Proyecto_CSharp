# Backlog técnico de MarinaApi

**Qué es este documento:** tickets pendientes de mejora del código real de MarinaApi. Salen de la revisión hecha al escribir las lecciones 11-13 de `GUIA_DEFINITIVA_CSHARP_1.md`. Siguen la numeración de `SIMULACION_SPRINT_COMPLETO.md` (el último ticket usado es el #153) y se pueden llevar tal cual a un Sprint Planning.

**Cómo usarlo:** cada ticket indica tipo, prioridad, estimación orientativa en story points, el problema con referencia al archivo, los criterios de aceptación y la lección de la guía donde se explica el concepto. Al terminar un ticket, márcalo con ✅ en la tabla y enlaza el commit o PR.

**Leyenda de prioridad:** 🔴 alta (bug o riesgo real) · 🟠 media (deuda técnica que frena) · 🟢 baja (mejora / preparación para producción)

---

## Resumen

| Ticket | Título | Tipo | Prioridad | Puntos | Depende de | Estado |
|---|---|---|---|---|---|---|
| **#154** | Borrar un Barco elimina también su Amarre | Bug | 🔴 | 2 | — | ⬜ |
| **#155** | Asignación concurrente de barco devuelve 500 en vez de 409 | Bug | 🔴 | 2 | — | ⬜ |
| **#156** | Contraseña de SQL Server en `appsettings.json` y `docker-compose.yml` | Seguridad | 🔴 | 2 | — | ⬜ |
| **#157** | `Precio` de Amarre como `double` en vez de `decimal` | Bug | 🔴 | 3 | — | ⬜ |
| **#158** | Unit of Work: los repositorios no deben llamar a `SaveChangesAsync` | Deuda técnica | 🟠 | 5 | — | ⬜ |
| **#159** | Completar y versionar los tests de `AssignBarcoAsync` | Tests | 🟠 | 2 | — | ⬜ |
| **#160** | Sustituir `FluentValidation.AspNetCore` y añadir validadores | Deuda técnica | 🟠 | 3 | — | ⬜ |
| **#161** | Validar el contenedor de DI al arrancar + test | Calidad | 🟠 | 1 | — | ⬜ |
| **#162** | Logging de SQL activo en todos los entornos | Configuración | 🟠 | 1 | #156 | ⬜ |
| **#163** | Middleware de errores → `IExceptionHandler` + `ProblemDetails` | Deuda técnica | 🟠 | 3 | — | ⬜ |
| **#164** | Spike: decidir excepciones vs Result pattern para errores esperados | Spike | 🟠 | 2 | #163 | ⬜ |
| **#165** | `Amarre` como entidad rica (`AsignarBarco`, `Liberar`) | Refactor | 🟢 | 5 | #158, #164 | ⬜ |
| **#166** | Tests de arquitectura con NetArchTest | Calidad | 🟢 | 2 | — | ⬜ |
| **#167** | Health checks `/health/live` y `/health/ready` | Producción | 🟢 | 2 | — | ⬜ |
| **#168** | CORS restringido por configuración | Producción | 🟢 | 1 | — | ⬜ |
| **#169** | Versionado de la API (v1 explícita) | Producción | 🟢 | 3 | — | ⬜ |
| **#170** | Formato y comentarios de `MarinaDbContext`, `BarcoRepository` y tests | Chore | 🟢 | 1 | — | ⬜ |

**Total: 40 puntos.** Con una velocidad similar a la del sprint simulado (17 puntos), son unos **tres sprints**.

**Propuesta de reparto:**
- **Sprint 1 (bugs y riesgos, 14 pts):** #154, #155, #156, #157, #159, #161, #162
- **Sprint 2 (deuda técnica, 13 pts):** #158, #160, #163, #164
- **Sprint 3 (dominio y producción, 13 pts):** #165, #166, #167, #168, #169, #170

---

## 🔴 Prioridad alta

### #154 — Borrar un Barco elimina también su Amarre

- **Tipo:** Bug · **Puntos:** 2
- **Problema:** en [MarinaDbContext.cs](MarinaApi/Data/MarinaDbContext.cs), la relación 1:1 `Amarre → Barco` está configurada con `.OnDelete(DeleteBehavior.Cascade)`. Como la FK `BarcoId` está en `Amarre`, **al borrar un barco se borra la fila del amarre**. Un amarre es infraestructura física del puerto: debería quedar libre (`BarcoId = NULL`), no desaparecer. El comentario del código lo presenta como equivalente a `CascadeType.ALL + orphanRemoval`, pero en la práctica borra el lado que debería sobrevivir.
- **Criterios de aceptación:**
  - [ ] La relación usa `DeleteBehavior.SetNull`.
  - [ ] Nueva migración generada y revisada (`dotnet ef migrations add AmarreSetNullAlBorrarBarco`).
  - [ ] Test de integración: al crear un barco con amarre y hacer `DELETE /api/Barcos/{id}`, el amarre sigue existiendo con `BarcoId = null`.
  - [ ] Actualizado el comentario del `DbContext`.
- **Guía:** Lección 8.4 (entidades y relaciones), Lección 12.2 (invariantes de dominio).

### #155 — Asignación concurrente de barco devuelve 500 en vez de 409

- **Tipo:** Bug · **Puntos:** 2
- **Problema:** `AmarreService.AssignBarcoAsync` comprueba con `FindByBarcoIdAsync` que el barco no tiene ya amarre y después actualiza. Si llegan dos peticiones a la vez que asignan el mismo barco a dos amarres distintos, **las dos pasan la comprobación**. La base de datos lo impide gracias al índice único filtrado sobre `Amarres.BarcoId` (está en la migración inicial), pero la segunda petición lanza una `DbUpdateException` que el middleware convierte en un **500**, cuando debería ser un **409 Conflict**.
- **Criterios de aceptación:**
  - [ ] Capturar la `DbUpdateException` por violación de índice único (SQL Server: `SqlException.Number` 2601 o 2627) y traducirla a `ConflictException` (o al `Error.Conflicto` si #164 decide usar Result).
  - [ ] La traducción vive en Infrastructure (repositorio o Unit of Work), no en el controlador.
  - [ ] Test que simula la violación y comprueba el 409.
  - [ ] Opcional: evaluar añadir `RowVersion` a `Amarre` para detectar también ediciones concurrentes del mismo amarre.
- **Guía:** Lección 8.8 (concurrencia optimista), Lección 12.4 (la unicidad la garantiza el índice, no el `if`).

### #156 — Contraseña de SQL Server en `appsettings.json` y `docker-compose.yml`

- **Tipo:** Seguridad · **Puntos:** 2
- **Problema:** la cadena de conexión con `User Id=sa;Password=TuPassword123!` está en [appsettings.json](MarinaApi/appsettings.json) y la misma contraseña aparece en [docker-compose.yml](MarinaApi/docker-compose.yml). Las dos están versionadas en GitHub. Aunque sea una contraseña de desarrollo, es justo el hábito que un code review de empresa rechaza (error n.º 8 del resumen final de la guía).
- **Criterios de aceptación:**
  - [ ] `appsettings.json` sin credenciales (la clave puede quedar con un valor vacío o un marcador).
  - [ ] Desarrollo: `dotnet user-secrets set "ConnectionStrings:MarinaDb" "..."`.
  - [ ] `docker-compose.yml` lee la contraseña de un archivo `.env` incluido en `.gitignore`, con un `.env.example` versionado.
  - [ ] README actualizado con los pasos de arranque.
  - [ ] Opcional: no usar el usuario `sa` para la aplicación.
- **Guía:** Lección 15 (configuración y secretos).

### #157 — `Precio` de Amarre como `double` en vez de `decimal`

- **Tipo:** Bug · **Puntos:** 3
- **Problema:** `Amarre.Precio`, `AmarreDto` y `AmarreRequestDto` usan `double`. Para importes de dinero, `double` produce errores de redondeo (`0.1 + 0.2 != 0.3`). La guía lo explica desde la Lección 1: el tipo correcto en C# es `decimal`, el equivalente nativo de `BigDecimal`.
- **Criterios de aceptación:**
  - [ ] `decimal` en entidad y DTOs, con `HasPrecision(10, 2)` (o `[Precision(10, 2)]`).
  - [ ] Migración revisada: el `ALTER COLUMN` de `float` a `decimal(10,2)` conserva los datos existentes.
  - [ ] Tests existentes adaptados.
- **Guía:** Lección 1.2 (tipos), Lección 12.2 (value object `Dinero`).

---

## 🟠 Prioridad media

### #158 — Unit of Work: los repositorios no deben llamar a `SaveChangesAsync`

- **Tipo:** Deuda técnica · **Puntos:** 5
- **Problema:** [GenericRepository.cs](MarinaApi/Repositories/GenericRepository.cs) llama a `SaveChangesAsync` dentro de `AddAsync`, `UpdateAsync` y `DeleteAsync`. Cada operación es su propia transacción. En cuanto un caso de uso toque dos repositorios (por ejemplo, asignar un amarre y registrar un movimiento en un histórico), un fallo a mitad deja datos a medias sin rollback posible. Además, el comentario del propio método dice que `SaveChangesAsync` "normalmente se llama una sola vez desde el servicio", justo lo contrario de lo que hace el código.
- **Criterios de aceptación:**
  - [ ] Interfaz `IUnitOfWork` con `SaveChangesAsync`, implementada por `MarinaDbContext` y registrada como `AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MarinaDbContext>())`.
  - [ ] `GenericRepository` sin llamadas a `SaveChangesAsync`; `Add`/`Update`/`Remove` pasan a ser síncronos o solo marcan cambios.
  - [ ] Cada servicio llama a `_unitOfWork.SaveChangesAsync(ct)` una sola vez por caso de uso.
  - [ ] Tests de servicios actualizados: verifican `SaveChangesAsync` en el mock de `IUnitOfWork` en lugar de `UpdateAsync`.
  - [ ] Corregido el comentario engañoso de `GenericRepository`.
- **Guía:** Lección 12.1.

### #159 — Completar y versionar los tests de `AssignBarcoAsync`

- **Tipo:** Tests · **Puntos:** 2
- **Problema:** `MarinaApi.Tests/AssignBarcoServiceTests.cs` **no está en el control de versiones** (aparece como no seguido en `git status`) y solo tiene el caso feliz. Los tres caminos de error del ticket #151 no tienen test.
- **Criterios de aceptación:**
  - [ ] Test: el amarre no existe → `NotFoundException`.
  - [ ] Test: el barco no existe → `NotFoundException`.
  - [ ] Test: el barco ya tiene otro amarre → `ConflictException`, y `UpdateAsync` no se llama (`Times.Never`).
  - [ ] Corregida la indentación del primer `[Fact]`.
  - [ ] Archivo añadido al repositorio en su propio commit.
- **Guía:** Lección 14 (xUnit, Moq, `Verify`).

### #160 — Sustituir `FluentValidation.AspNetCore` y añadir validadores

- **Tipo:** Deuda técnica · **Puntos:** 3
- **Problema:** [MarinaApi.csproj](MarinaApi/MarinaApi.csproj) referencia `FluentValidation.AspNetCore`, un paquete que su autor ha dejado de mantener y desaconseja. Además **no hay ningún validador** en el proyecto: la validación de entrada depende solo de las Data Annotations de las entidades, que no están en los DTOs.
- **Criterios de aceptación:**
  - [ ] Quitar `FluentValidation.AspNetCore`; añadir `FluentValidation` y `FluentValidation.DependencyInjectionExtensions`.
  - [ ] `AddValidatorsFromAssemblyContaining<Program>()` en `Program.cs`.
  - [ ] Validadores para `BarcoRequestDto`, `AmarreRequestDto`, `AsignarBarcoDto` y el DTO de Tripulante, con al menos una regla entre campos.
  - [ ] Validación explícita con `ValidateAsync` que devuelve `ValidationProblem` (400).
  - [ ] Un test por validador con `TestValidate`.
- **Guía:** Lección 12.4.

### #161 — Validar el contenedor de DI al arrancar + test

- **Tipo:** Calidad · **Puntos:** 1
- **Problema:** hoy no hay *captive dependencies*, pero nada impediría introducirlas. La validación automática solo se activa en el entorno `Development`.
- **Criterios de aceptación:**
  - [ ] `builder.Host.UseDefaultServiceProvider(o => { o.ValidateScopes = true; o.ValidateOnBuild = true; })` fuera de producción.
  - [ ] Test con `WebApplicationFactory<Program>` que construye el contenedor y falla si hay dependencias cautivas (`Program` ya expone `public partial class Program`).
- **Guía:** Lección 11.5.

### #162 — Logging de SQL activo en todos los entornos

- **Tipo:** Configuración · **Puntos:** 1
- **Problema:** `appsettings.json` pone `Microsoft.EntityFrameworkCore.Database.Command` en `Information`, así que **cada consulta SQL se registra en todos los entornos**, producción incluida. No existe `appsettings.Development.json`.
- **Criterios de aceptación:**
  - [ ] Crear `appsettings.Development.json` con el nivel `Information` para EF.
  - [ ] En `appsettings.json` base, `Warning` para esa categoría.
- **Guía:** Lección 15.1 (niveles de log), Lección 8.7 (ver el SQL generado).

### #163 — Middleware de errores → `IExceptionHandler` + `ProblemDetails`

- **Tipo:** Deuda técnica · **Puntos:** 3
- **Problema:** [ExceptionHandlingMiddleware.cs](MarinaApi/Middleware/ExceptionHandlingMiddleware.cs) serializa a mano un objeto anónimo con formato *problem+json*. Los errores que genera el propio framework (400 de validación, 404 de rutas inexistentes, 405) salen con otro formato, no llevan `traceId`, y las cancelaciones del cliente (`OperationCanceledException`) se registran como 500.
- **Criterios de aceptación:**
  - [ ] `IExceptionHandler` que traduce `NotFoundException` → 404, `ConflictException` → 409, cancelación del cliente → 499 sin `LogError`, y el resto → 500 con mensaje genérico.
  - [ ] `AddProblemDetails` con `traceId` en `Extensions`; `app.UseExceptionHandler()`.
  - [ ] Eliminado `ExceptionHandlingMiddleware`.
  - [ ] Test de integración: un 404 de ruta inexistente y un 409 de negocio tienen el mismo formato.
- **Guía:** Lección 10.6 y Lección 13.3.

### #164 — Spike: decidir excepciones vs Result pattern para errores esperados

- **Tipo:** Spike (investigación con límite de tiempo, sin código de producción) · **Puntos:** 2
- **Objetivo:** decidir, y dejar por escrito, si MarinaApi sigue con `NotFoundException`/`ConflictException` + manejador global o pasa a `Result<T>`. Lo que no puede pasar es acabar mezclando los dos estilos.
- **Entregable:**
  - [ ] Prototipo de `AssignBarcoAsync` con `Result<AmarreDto>` en una rama descartable.
  - [ ] Nota breve (ADR) en `docs/decisiones/0001-errores-esperados.md` con la decisión, las alternativas valoradas (clases propias, ErrorOr, FluentResults) y las consecuencias.
- **Guía:** Lección 12.3.

---

## 🟢 Prioridad baja

### #165 — `Amarre` como entidad rica (`AsignarBarco`, `Liberar`)

- **Tipo:** Refactor · **Puntos:** 5 · **Depende de:** #158, #164
- **Problema:** todas las entidades de `Models/` son anémicas (`{ get; set; }` públicos). La regla "un amarre ocupado no admite otro barco" vive en `AmarreService`, así que cualquier otro código puede hacer `amarre.BarcoId = x` y saltársela.
- **Criterios de aceptación:**
  - [ ] `Amarre` con setters privados, constructor que valida, y métodos `AsignarBarco(...)`, `Liberar()` y `ActualizarPrecio(...)`.
  - [ ] `AmarreService` orquesta y delega la regla en la entidad.
  - [ ] Tests unitarios de `Amarre` sin mocks.
  - [ ] EF Core sigue leyendo y guardando (constructor privado sin parámetros).
  - [ ] Valorar después si `Barco` lo necesita (capacidad de tripulantes) o puede quedarse anémica.
- **Guía:** Lección 12.2.

### #166 — Tests de arquitectura con NetArchTest

- **Tipo:** Calidad · **Puntos:** 2
- **Criterios de aceptación:**
  - [ ] Paquete `NetArchTest.Rules` en `MarinaApi.Tests`.
  - [ ] Regla: `MarinaApi.Controllers` no depende de `MarinaApi.Repositories` ni de `MarinaApi.Data`.
  - [ ] Regla: `MarinaApi.Models` no depende de `Microsoft.AspNetCore` ni de `MarinaApi.Dtos`.
  - [ ] Regla: `MarinaApi.Services` no depende de `MarinaApi.Data` (acceso a datos solo a través de repositorios).
- **Guía:** Lección 11.6.

### #167 — Health checks `/health/live` y `/health/ready`

- **Tipo:** Producción · **Puntos:** 2
- **Criterios de aceptación:**
  - [ ] Paquete `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`.
  - [ ] `/health/live` sin comprobaciones de dependencias.
  - [ ] `/health/ready` con `AddDbContextCheck<MarinaDbContext>` y respuesta JSON.
  - [ ] Probado parando el contenedor de `docker-compose`: `live` responde 200 y `ready` 503.
- **Guía:** Lección 13.2.

### #168 — CORS restringido por configuración

- **Tipo:** Producción · **Puntos:** 1
- **Problema:** [Program.cs](MarinaApi/Program.cs) usa `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()`.
- **Criterios de aceptación:**
  - [ ] Orígenes permitidos leídos de `Cors:Origenes` en configuración.
  - [ ] `AllowAnyOrigin` solo en `Development`.
- **Guía:** Lección 13.6.

### #169 — Versionado de la API (v1 explícita)

- **Tipo:** Producción · **Puntos:** 3
- **Criterios de aceptación:**
  - [ ] `Asp.Versioning.Mvc` + `Asp.Versioning.Mvc.ApiExplorer`.
  - [ ] Rutas `api/v{version:apiVersion}/[controller]` con `[ApiVersion("1.0")]`, manteniendo temporalmente las rutas sin versión para no romper clientes.
  - [ ] Swagger con un documento por versión.
  - [ ] Actualizados los ejemplos de la guía y de las simulaciones que usan `/api/...` sin versión.
- **Guía:** Lección 13.1.

### #170 — Formato y comentarios de `MarinaDbContext`, `BarcoRepository` y tests

- **Tipo:** Chore · **Puntos:** 1
- **Problema:**
  - En [MarinaDbContext.cs](MarinaApi/Data/MarinaDbContext.cs), el bloque de la relación Barco ↔ Tripulante está sin indentar.
  - La configuración de Organizador ↔ Regata repite el mismo comentario en cada línea encadenada.
  - En `Barco.cs`, el comentario de `Tripulantes` habla de "skip navigations" y de "tabla intermedia", pero es una relación 1:N con FK normal.
  - La última línea de [BarcoRepository.cs](MarinaApi/Repositories/BarcoRepository.cs) está sin indentar.
- **Criterios de aceptación:**
  - [ ] `dotnet format` ejecutado sobre la solución.
  - [ ] Comentarios corregidos para que describan lo que hace el código.
  - [ ] Opcional: añadir `.editorconfig` para que el formato no dependa de cada IDE.
