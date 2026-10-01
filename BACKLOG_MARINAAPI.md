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
| **#156** | Credenciales de MySQL versionadas en `appsettings.json` | Seguridad | 🔴 | 2 | #171 | ✅ [PR #10](https://github.com/IgorDAM/Proyecto_CSharp/pull/10) |
| **#157** | `Precio` de Amarre como `double` en vez de `decimal` | Bug | 🔴 | 3 | — | ⬜ |
| **#158** | Unit of Work: los repositorios no deben llamar a `SaveChangesAsync` | Deuda técnica | 🟠 | 5 | — | ⬜ |
| **#159** | Completar y versionar los tests de `AssignBarcoAsync` | Tests | 🟠 | 2 | — | ✅ [PR #2] |
| **#160** | Sustituir `FluentValidation.AspNetCore` y añadir validadores | Deuda técnica | 🟠 | 3 | — | ⬜ |
| **#161** | Validar el contenedor de DI al arrancar + test | Calidad | 🟠 | 1 | — | ⬜ |
| **#162** | Logging de SQL activo en todos los entornos | Configuración | 🟠 | 1 | #156 | ⬜ |
| **#163** | Middleware de errores → `IExceptionHandler` + `ProblemDetails` | Deuda técnica | 🟠 | 3 | — | ⬜ |
| **#164** | Spike: decidir excepciones vs Result pattern para errores esperados | Spike | 🟠 | 2 | #163 | ⬜ |
| **#165** | `Amarre` como entidad rica (`AsignarBarco`, `Liberar`) | Refactor | 🟢 | 5 | #158, #164 | ⬜ |
| **#166** | Tests de arquitectura con NetArchTest | Calidad | 🟢 | 2 | — | ⬜ |
| **#167** | Health checks `/health/live` y `/health/ready` | Producción | 🟢 | 2 | — | ⬜ |
| **#168** | CORS restringido por configuración | Producción | 🟢 | 1 | — | ✅ [PR #10](https://github.com/IgorDAM/Proyecto_CSharp/pull/10) |
| **#169** | Versionado de la API (v1 explícita) | Producción | 🟢 | 3 | — | ⬜ |
| **#170** | Formato y comentarios de `MarinaDbContext`, `BarcoRepository` y tests | Chore | 🟢 | 1 | — | ⬜ |
| **#171** | `docker-compose.yml` levanta SQL Server en vez de MySQL | Bug | 🔴 | 2 | — | ✅ [PR #10](https://github.com/IgorDAM/Proyecto_CSharp/pull/10) |
| **#172** | Asignar un barco a un amarre ocupado desaloja al barco anterior | Bug | 🔴 | 2 | — | ✅ [PR #11](https://github.com/IgorDAM/Proyecto_CSharp/pull/11) |
| **#173** | El contenedor `marina-mysql` publica el 3306 en toda la red local | Seguridad | 🟠 | 1 | — | ⬜ |
| **#174** | `DesinscribirBarcoAsync` responde 204 aunque la regata no exista | Bug | 🟢 | 1 | — | ⬜ |
| **#175** | Paginación en los listados `GET` | Producción | 🟢 | 3 | — | ⬜ |
| **#176** | HSTS fuera de Development | Producción | 🟢 | 1 | — | ⬜ |

**Total: 50 puntos** (42 iniciales + 8 de la revisión de seguridad del 2026-09-30; ya hechos: #156, #159, #168 y #171). Con una velocidad similar a la del sprint simulado (17 puntos), son unos **tres sprints**.

**Propuesta de reparto:**
- **Sprint 1 (bugs y riesgos, 16 pts):** #154, #155, #156 ✅, #157, #159 ✅, #161, #162, #171 ✅ — más #172, #173 y #174 de la revisión de seguridad
- **Sprint 2 (deuda técnica, 13 pts):** #158, #160, #163, #164
- **Sprint 3 (dominio y producción, 13 pts):** #165, #166, #167, #168 ✅, #169, #170 — más #175 y #176

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
- **Problema:** `AmarreService.AssignBarcoAsync` comprueba con `FindByBarcoIdAsync` que el barco no tiene ya amarre y después actualiza. Si llegan dos peticiones a la vez que asignan el mismo barco a dos amarres distintos, **las dos pasan la comprobación**. La base de datos lo impide gracias al índice único sobre `Amarres.BarcoId` (está en la migración inicial), pero la segunda petición lanza una `DbUpdateException` que el middleware convierte en un **500**, cuando debería ser un **409 Conflict**.
- **Criterios de aceptación:**
  - [ ] Capturar la `DbUpdateException` por violación de índice único (con Pomelo/MySqlConnector: `MySqlException.Number == 1062`, "Duplicate entry" — ya no `SqlException` 2601/2627 de SQL Server) y traducirla a `ConflictException` (o al `Error.Conflicto` si #164 decide usar Result).
  - [ ] La traducción vive en Infrastructure (repositorio o Unit of Work), no en el controlador.
  - [ ] Test que simula la violación y comprueba el 409.
  - [ ] Opcional: evaluar añadir `RowVersion` a `Amarre` para detectar también ediciones concurrentes del mismo amarre.
- **Guía:** Lección 8.8 (concurrencia optimista), Lección 12.4 (la unicidad la garantiza el índice, no el `if`).

### #156 — Credenciales de MySQL versionadas en `appsettings.json` ✅ [PR #10](https://github.com/IgorDAM/Proyecto_CSharp/pull/10)

- **Tipo:** Seguridad · **Puntos:** 2 · **Depende de:** #171
- **Problema (actualizado tras la migración a MySQL del 2026-09-22):** [appsettings.json](MarinaApi/appsettings.json) tiene la cadena de conexión a MySQL con usuario `root` y contraseña en claro, versionada en GitHub. Sigue siendo el mismo hábito que un code review de empresa rechaza (error n.º 8 del resumen final de la guía), solo que ahora la credencial expuesta es la de MySQL en vez de la de SQL Server.
- **Criterios de aceptación:**
  - [x] `appsettings.json` sin credenciales (la clave se ha quitado del todo: si falta, `Program.cs` explica cómo configurarla).
  - [x] Desarrollo: `dotnet user-secrets set "ConnectionStrings:MarinaDb" "..."`.
  - [x] README actualizado con los pasos de arranque.
  - [x] Opcional: no usar el usuario `root` para la aplicación → usuario `marina_app`, con permisos solo sobre `gestion_maritima`.
- **Cierre (2026-09-30):** el repo es público, así que se **rotaron** las credenciales (la de `root` y la de la app). El historial de git no se reescribió: una vez rotadas, las contraseñas antiguas no sirven. Además, JWT Bearer con `FallbackPolicy` (la API no tenía autenticación), sin ticket propio.
- **Guía:** Lección 15 (configuración y secretos).

### #171 — `docker-compose.yml` levanta SQL Server en vez de MySQL ✅ [PR #10](https://github.com/IgorDAM/Proyecto_CSharp/pull/10)

- **Tipo:** Bug · **Puntos:** 2
- **Problema:** [docker-compose.yml](MarinaApi/docker-compose.yml) no se migró junto con el resto del proyecto: sigue levantando `mcr.microsoft.com/mssql/server:2022-latest` (SQL Server 2022) con su propia contraseña (`MSSQL_SA_PASSWORD`), distinta de la que usa `appsettings.json` para MySQL. Quien clone el repo y siga el `docker-compose.yml` no consigue una base de datos que la aplicación pueda usar.
- **Criterios de aceptación:**
  - [x] `docker-compose.yml` levanta un servicio MySQL 8 (imagen `mysql:8.0`), con la contraseña de `root` leída de `.env` (hay un `.env.example`).
  - [x] La base de datos creada se llama `gestion_maritima` y el servicio expone el puerto 3306 (solo en `127.0.0.1`).
  - [~] Desde un clon limpio: `docker compose up` + `dotnet ef database update` funciona, pero hay que crear antes el usuario `marina_app` y guardar la cadena en user-secrets (dos pasos manuales, documentados en el README). Automatizarlo con un script de `docker-entrypoint-initdb.d` queda como mejora opcional.
  - [x] README actualizado con los pasos de arranque.
- **Guía:** Lección 11.4 (proveedores de EF Core), sección 2.6 de `MIGRACION_JAVA_A_CSHARP.md`.

### #172 — Asignar un barco a un amarre ocupado desaloja al barco anterior ✅ [PR #11](https://github.com/IgorDAM/Proyecto_CSharp/pull/11)

- **Tipo:** Bug · **Puntos:** 2 · **Origen:** revisión de seguridad del 2026-09-30
- **Problema:** `AmarreService.AssignBarcoAsync` comprueba que el **barco** no tenga ya otro amarre, pero no que el **amarre** esté libre. Si el amarre 5 tiene el barco A y se hace `PATCH /api/Amarres/5/barco` con el barco B, se sobrescribe `BarcoId` y A se queda sin amarre sin ningún aviso. Es un fallo de integridad: el índice único de `Amarres.BarcoId` no lo detecta, porque B no está en ningún otro amarre.
- **Criterios de aceptación:**
  - [x] Test primero en `AssignBarcoServiceTests`: el amarre ya tiene otro barco → `ConflictException`, y `UpdateAsync` no se llama (`Times.Never`). Además comprueba que el barco anterior sigue en el amarre.
  - [x] Comprobación en `AssignBarcoAsync` antes de las demás, que lanza `ConflictException` → 409.
  - [x] Decidido: reasignar **el mismo** barco → **200 idempotente** (sin `UpdateAsync` y sin consultar el barco, con `Verify(..., Times.Never)`). La comprobación idempotente va antes que la de "ocupado"; un experimento moviéndola confirmó que el test lo detecta.
  - [x] `/// <summary>` de `AssignBarcoAsync` con las cinco reglas en orden.
- **Verificado en Swagger contra MySQL (2026-10-01):** asignación normal 200 → misma asignación 200 → otro barco 409 (`El Amarre con Id 1 ya está ocupado por el Barco 1.`) → el amarre conserva su barco. Tests 37/37.
- **Guía:** Lección 12.2 (invariantes de dominio). Encaja después en #165 (`Amarre.AsignarBarco()`).

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

### #159 — Completar y versionar los tests de `AssignBarcoAsync` ✅ [PR #2](https://github.com/IgorDAM/Proyecto_CSharp/pull/2)

- **Tipo:** Tests · **Puntos:** 2
- **Problema (resuelto):** `MarinaApi.Tests/AssignBarcoServiceTests.cs` no estaba en el control de versiones y solo tenía el caso feliz. Los tres caminos de error del ticket #151 no tenían test.
- **Criterios de aceptación:**
  - [x] Test: el amarre no existe → `NotFoundException`.
  - [x] Test: el barco no existe → `NotFoundException`.
  - [x] Test: el barco ya tiene otro amarre → `ConflictException`, y `UpdateAsync` no se llama (`Times.Never`).
  - [x] Corregida la indentación del primer `[Fact]`.
  - [x] Archivo añadido al repositorio en su propio commit.
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

### #173 — El contenedor `marina-mysql` publica el 3306 en toda la red local

- **Tipo:** Seguridad · **Puntos:** 1 · **Origen:** revisión de seguridad del 2026-09-30
- **Problema:** el contenedor actual se creó a mano con `docker run -p 3306:3306`, que escucha en `0.0.0.0`: MySQL es accesible desde cualquier equipo de la red local, no solo desde este PC. El `docker-compose.yml` nuevo ya publica en `127.0.0.1`, pero el contenedor en uso no sale de ahí.
- **Criterios de aceptación:**
  - [ ] Copia de seguridad antes de tocar nada: `docker exec marina-mysql mysqldump -u root -p --databases gestion_maritima > backup.sql` (fuera del repo).
  - [ ] Averiguar dónde están los datos (`docker inspect marina-mysql --format "{{json .Mounts}}"`): el `docker run` original no declaró volumen, así que es uno anónimo.
  - [ ] Contenedor recreado con el compose (`docker compose up -d`) sin perder datos (reutilizando el volumen o restaurando el dump), con el usuario `marina_app` y su contraseña de siempre.
  - [ ] `docker ps` muestra `127.0.0.1:3306->3306/tcp`; la API arranca y responde 200 con token.
  - [ ] Borrado el contenedor `marina-sqlserver` (parado desde el 2026-09-30) y su volumen, si ya no hace falta.
- **Guía:** sección 2.6 de `MIGRACION_JAVA_A_CSHARP.md`.

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

### #168 — CORS restringido por configuración ✅ [PR #10](https://github.com/IgorDAM/Proyecto_CSharp/pull/10)

- **Tipo:** Producción · **Puntos:** 1
- **Problema:** [Program.cs](MarinaApi/Program.cs) usa `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()`.
- **Criterios de aceptación:**
  - [x] Orígenes permitidos leídos de `Cors:AllowedOrigins` en configuración (lista blanca, vacía por defecto).
  - [x] ~~`AllowAnyOrigin` solo en `Development`~~ → descartado: la lista vacía se usa también en Development, porque con `AllowAnyOrigin` cualquier web abierta en el navegador podía llamar a la API en `localhost`. El frontend de `wwwroot` es el mismo origen y no necesita CORS.
- **Guía:** Lección 13.6.

### #174 — `DesinscribirBarcoAsync` responde 204 aunque la regata no exista

- **Tipo:** Bug · **Puntos:** 1 · **Origen:** revisión de seguridad del 2026-09-30
- **Problema:** `RegataService.DesinscribirBarcoAsync` solo comprueba que exista el barco. `DELETE /api/Regatas/9999/barcos/1` devuelve 204 aunque la regata 9999 no exista, cuando `InscribirBarcoAsync` sí responde 404 en ese caso. El cliente no puede distinguir "retirado" de "no había nada que retirar".
- **Criterios de aceptación:**
  - [ ] Test en `RegataServiceTests`: la regata no existe → `NotFoundException`.
  - [ ] Comprobación con `_regataRepository.ExistsAsync` antes de tocar el barco.
  - [ ] Decidido: si el barco existe pero no estaba inscrito en esa regata, ¿204 (idempotente, como ahora) o 404?

### #175 — Paginación en los listados `GET`

- **Tipo:** Producción · **Puntos:** 3 · **Origen:** revisión de seguridad del 2026-09-30
- **Problema:** `GET /api/Barcos`, `/api/Amarres`, `/api/Regatas` y `/api/Tripulantes` devuelven la tabla entera con `ToListAsync()`. Con muchos registros, una sola petición carga todo en memoria y en la respuesta.
- **Criterios de aceptación:**
  - [ ] Parámetros `?pagina=1&tamanio=20` con `[FromQuery]` y un máximo (p. ej. 100) validado.
  - [ ] `Skip`/`Take` con un `OrderBy` estable (sin él, el orden de las páginas no está garantizado).
  - [ ] Respuesta con los elementos y el total (DTO `PaginaDto<T>` o cabecera `X-Total-Count`).
  - [ ] Tests del repositorio o del servicio para la primera página, la última y una fuera de rango.
- **Guía:** equivalente a `Pageable`/`Page<T>` de Spring Data.

### #176 — HSTS fuera de Development

- **Tipo:** Producción · **Puntos:** 1 · **Origen:** revisión de seguridad del 2026-09-30
- **Problema:** `Program.cs` tiene `UseHttpsRedirection()` pero no `UseHsts()`. Sin HSTS, el navegador puede volver a intentar HTTP en la siguiente visita, y esa primera petición es interceptable.
- **Criterios de aceptación:**
  - [ ] `app.UseHsts()` solo cuando `!app.Environment.IsDevelopment()` (en local rompería `http://localhost:5000`).
  - [ ] Comentario que explique por qué no se activa en Development.

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
  - En [AssignBarcoServiceTests.cs](MarinaApi.Tests/AssignBarcoServiceTests.cs), el 3º y el 4º `[Fact]` (líneas 63 y 84) tienen 8 espacios de indentación en vez de 4.
- **Criterios de aceptación:**
  - [ ] `dotnet format` ejecutado sobre la solución.
  - [ ] Comentarios corregidos para que describan lo que hace el código.
  - [ ] Opcional: añadir `.editorconfig` para que el formato no dependa de cada IDE.
