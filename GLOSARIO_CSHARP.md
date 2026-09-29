# Glosario CSharp / .NET — Marina API

Documento vivo: cada vez que pregunto por una palabra o un trozo de código que no domino, se archiva aquí bajo el tema que le corresponda. No se reescribe, solo se amplía (igual que `GUIA_DEFINITIVA_CSHARP_1.md` y `MIGRACION_JAVA_A_CSHARP.md`).

**Formato de cada entrada:**

### Término / palabra clave

**Qué es:** explicación directa, en el contexto de Marina API.

**Equivalente en Java:** si aplica.

**Ejemplo:**
```csharp
// código real del proyecto o mínimo reproducible
```

---

## Índice de temas

- [[#Sintaxis CSharp]]
- [[#.NET / ASP.NET Core]]
- [[#Entity Framework Core]]
- [[#LINQ]]
- [[#SQL Server]]
- [[#Testing (xUnit / Moq)]]
- [[#Git / Azure DevOps]]
- [[#Scrum / Metodología]]

---

## Sintaxis CSharp

### Expression-bodied member

**Qué es:** sintaxis abreviada con `=>` para definir el cuerpo de un miembro (método, propiedad, constructor, indexador...) cuando ese cuerpo es una única expresión, en vez de un bloque `{ return ...; }`.

**Cuidado, no confundir con una lambda.** Usan el mismo símbolo `=>` pero son cosas distintas:
- Lambda expression: crea una función anónima que se pasa como valor (a un delegado o parámetro) — ej. `options => { options.UseSqlServer(...); }` en `Program.cs`. Es exactamente lo mismo que una lambda de Java (`x -> ...`).
- Expression-bodied member: no crea nada anónimo, es azúcar sintáctico para el cuerpo de un miembro que YA tiene nombre propio, declarado de forma normal.

**Equivalente en Java:** no hay equivalente — Java exige llaves + `return` explícito en todo método con nombre, por corto que sea. Diferencia puramente sintáctica, no cambia rendimiento ni semántica. En Kotlin, `fun foo() = expresion` es el mapeo mental más directo.

**Ejemplos reales del proyecto:**
```csharp
// Método (BarcosController.cs) — en vez de { return Ok(...); }
public async Task<ActionResult<List<BarcoDto>>> GetAll(CancellationToken ct) =>
    Ok(await _barcoService.FindAllAsync(ct));

// Extension method (BarcoMapper.cs)
public static BarcoDto ToDto(this Barco barco) =>
    new(barco.Id, barco.Nombre, barco.Tipo, barco.Eslora, barco.Manga, barco.Capacidad);
```

También se usa en propiedades de solo lectura (aún no en el proyecto, pero de uso muy común):
```csharp
// Sin expression body:
public string NombreCompleto
{
    get { return $"{Nombre} ({Tipo})"; }
}

// Con expression body — mismo resultado:
public string NombreCompleto => $"{Nombre} ({Tipo})";
```

---

### Boilerplate

**Qué es:** código repetitivo que hay que escribir una y otra vez, casi idéntico, sin que aporte lógica de negocio — solo "peaje" que exige el lenguaje.

**Equivalente en Java:** Java es el ejemplo típico de lenguaje con mucho boilerplate: getters/setters manuales, constructores, `equals()`/`hashCode()`/`toString()` a mano, checked exceptions que obligan a `try-catch` en cada llamada. Por eso nació Lombok (`@Data`, `@Getter`, `@AllArgsConstructor`): generar ese código automáticamente vía anotaciones.

C# ataca el mismo problema metiendo la solución dentro del propio lenguaje, sin librería externa: auto-properties (`public string Nombre { get; set; }`), records (constructor + propiedades inmutables + `Equals`/`GetHashCode`/`ToString` en una línea — el equivalente exacto a `@Data + @AllArgsConstructor`), expression-bodied members (`=>` en vez de `return`), `using` declarations (como `try-with-resources` pero sin el bloque completo) y target-typed `new` (`List<string> nombres = new();`).

**Ejemplo (real del proyecto, `Dtos/BarcoDtos.cs`):**
```csharp
/// Un "record" en C# es la forma idiomática de escribir un DTO inmutable:
/// genera automáticamente el equivalente a los @Data + @AllArgsConstructor
/// de Lombok (igualdad por valor, ToString, constructor) sin escribir nada más.
public record BarcoDto(
    long Id,
    string Nombre,
    string Tipo,
    int Eslora,
    int Manga,
    int Capacidad
);
```
Con Lombok en Java: clase + `@Data` + `@AllArgsConstructor` (o los 4 métodos a mano). Aquí, esa única línea ya lo tiene todo.

---

### Extension methods

**Qué es:** una forma de "añadir" un método a un tipo ya existente (una clase tuya, del framework o de un paquete NuGet) sin modificar su código ni heredar de él. Es azúcar sintáctico: el compilador convierte `barco.ToDto()` en `BarcoMapper.ToDto(barco)`.

Reglas para declarar uno:
- Va dentro de una clase `static`.
- El método en sí es `static`.
- El primer parámetro lleva el modificador `this` — ese `this` es lo que le dice al compilador "este método extiende este tipo".
- Para poder llamarlo hace falta un `using` del namespace donde vive la clase estática, aunque no uses la clase por su nombre.

**Equivalente en Java:** no hay una construcción directa. Lo más parecido:
- Un método estático de utilidad al estilo `Collections.sort(list)`, pero ahí escribes `Clase.metodo(obj)`; en C# se invierte a `obj.Metodo()`, aunque por debajo el compilador hace justo lo mismo (una llamada estática).
- Si conoces Kotlin, sus extension functions (`fun String.miMetodo()`) son el mapeo mental más directo — es prácticamente la misma idea.
- No confundir con los `default` methods de las interfaces en Java: esos exigen implementar la interfaz. Un extension method en C# no toca la jerarquía de tipos en absoluto.

**Limitación clave:** al ser azúcar sobre una llamada estática, un extension method NO tiene acceso a los miembros `private` de la clase que extiende — solo ve su API pública. Un método de instancia real en Java sí accedería a los privados.

**Ejemplo (real del proyecto, `Mapping/BarcoMapper.cs`):**
```csharp
public static class BarcoMapper
{
    public static BarcoDto ToDto(this Barco barco) =>
        new(barco.Id, barco.Nombre, barco.Tipo, barco.Eslora, barco.Manga, barco.Capacidad);
}

// Uso:
var dto = barco.ToDto();                            // en vez de BarcoMapper.ToDto(barco)
var dtos = barcos.Select(b => b.ToDto()).ToList();  // se encadena con LINQ sin problema
```

**Dato extra:** LINQ entero (`.Select()`, `.Where()`, `.ToList()`...) son extension methods definidos sobre `IEnumerable<T>` en `System.Linq`. Es exactamente el mismo mecanismo que usas en tus Mappers.

---

### Sobrecarga por tipo de retorno (no existe)

**Qué es:** en C#, el compilador **no** distingue dos miembros por su tipo de retorno — solo por nombre + lista de parámetros (tipo, orden, cantidad). Dos métodos con la misma firma de parámetros pero distinto `return type` no son una sobrecarga válida; es un error de compilación (`CS0111`) si están en el mismo tipo.

**Equivalente en Java:** misma regla exactamente — Java tampoco permite overloading solo por tipo de retorno, por el mismo motivo: la resolución de sobrecarga ocurre en tiempo de compilación mirando la firma de la llamada (argumentos), no lo que se hace con el valor devuelto. No es una diferencia C#/Java, es una regla compartida por la mayoría de lenguajes con overloading estático.

**Dónde apareció (ticket #151):** al implementar `AssignBarcoAsync`, `IAmarreRepository` y `ITripulanteRepository` ya tenían cada uno su propio `FindByBarcoIdAsync(long barcoId, CancellationToken ct = default)` — misma firma de parámetros, pero uno devuelve `Amarre?` (un Amarre busca "su" Barco) y el otro `List<Tripulante>` (un Barco tiene varios Tripulantes). Al vivir en interfaces distintas no hay conflicto real, pero el nombre idéntico + firma idéntica es la clase de situación donde, si por error se intenta declarar ambas sobrecargas dentro de la **misma** interfaz o clase, el compilador lo rechaza de inmediato — no hay forma de que resuelva cuál llamar mirando solo el tipo de retorno esperado.

**Ejemplo (real del proyecto):**
```csharp
// IAmarreRepository.cs
Task<Amarre?> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default);

// ITripulanteRepository.cs
Task<List<Tripulante>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default);

// Esto compila porque son interfaces distintas.
// Si ambas firmas convivieran en la MISMA interfaz, sería CS0111:
// "Type 'X' already defines a member called 'FindByBarcoIdAsync' with the same parameter types"
```

**Cómo se resuelve de verdad** cuando hace falta variar solo el retorno para el mismo concepto: cambiar el nombre (`FindAmarreByBarcoIdAsync` / `FindTripulantesByBarcoIdAsync`), o mover la variación al tipo genérico si el repositorio es genérico. Ver `MIGRACION_JAVA_A_CSHARP.md` → sección de excepciones/middleware → "Ampliación: el mismo patrón para conflictos de negocio (ticket #151)" para el contexto completo del ticket.

---

### virtual / override

**Qué es:** en C# un método solo se puede sobrescribir en una clase hija si la clase base lo marca como `virtual`. La hija lo sustituye con `override`, que es **obligatorio**. Si la base no es `virtual`, el `override` no compila (CS0506).

**Equivalente en Java:** es justo al revés. En Java todo método de instancia es sobrescribible salvo que sea `final`, y `@Override` es una anotación opcional que solo sirve para que el compilador compruebe que de verdad sobrescribes algo. En C# la base tiene que dar permiso (`virtual`) y la hija tiene que declararlo (`override`).

**Ejemplo real (ticket 152, 2026-09-24):**
```csharp
// MarinaApi/Repositories/GenericRepository.cs: la base da permiso
public virtual async Task<List<T>> FindAllAsync(CancellationToken ct = default) =>
    await _dbSet.ToListAsync(ct);

// MarinaApi/Repositories/RegataRepository.cs: la hija lo sustituye
public override async Task<List<Regata>> FindAllAsync(CancellationToken ct = default) =>
    await _context.Regatas.Include(r => r.Barcos).ToListAsync(ct);
```

**Nota del proyecto:** este `override` se revirtió en el commit `6711ea2`, cuando el listado pasó a usar una proyección a DTO (ver Proyección a DTO con Select, en Entity Framework Core). Hoy `FindAllAsync` ya no es `virtual`. El concepto también importa para Moq: solo puede interceptar miembros `virtual` o de interfaz, y por eso en los tests se mockean interfaces (`IRegataRepository`) y no clases. Más detalle en [[Guía definitiva de CSharp#Herencia de clases]].

---

### Tipos anulables y throw expression

**Qué es:** añadir `?` a un tipo de valor (`int`, `long`, `bool`...) lo convierte en anulable: `int?` puede valer un número o `null`. El operador `??` devuelve lo de la izquierda si no es `null` y, si lo es, lo de la derecha. A la derecha de `??` puede ir un `throw` (*throw expression*): C# permite usar `throw` dentro de una expresión, como si fuera un valor.

**Equivalente en Java:** `int?` es como pasar de `int` a `Integer`. `valor ?? throw new X()` es como `optional.orElseThrow(() -> new X())`.

**Ejemplo real (arreglo del N+1, `MarinaApi/Services/RegataService.cs`):**
```csharp
var total = await _regataRepository.ContarTripulantesAsync(regataId, ct); // int?
return total ?? throw new NotFoundException(nameof(Models.Regata), regataId);
```

**Por qué `int?` y no `int`:** hay dos casos que un `int` no podría distinguir:
- `0` significa que la regata existe pero no tiene tripulantes.
- `null` significa que la regata no existe, y eso acaba en un 404.

Con un `int`, los dos casos darían `0`. Al salir por `??`, el compilador sabe que ya no puede ser `null` y convierte el resultado en `int` él solo.

**Truco relacionado:** en Moq, `ReturnsAsync(null)` a secas es ambiguo porque encaja en varias sobrecargas. Hay que escribir `ReturnsAsync((int?)null)`, igual que en Java a veces hay que poner `thenReturn((Integer) null)`.

---

## .NET / ASP.NET Core

### Scoped

**Qué es:** uno de los tres "tiempos de vida" (lifetimes) de un servicio registrado en el contenedor de inyección de dependencias de .NET: **Transient** (instancia nueva cada vez que se pide), **Scoped** (una instancia por scope — en ASP.NET Core, un scope = una petición HTTP; todos los que lo pidan dentro de la misma request reciben la misma instancia) y **Singleton** (una única instancia para toda la vida de la aplicación).

**Equivalente en Java/Spring:** los bean scopes — `@Scope("prototype")` ≈ Transient, `@Scope("singleton")` ≈ Singleton, `@RequestScope` (`@Scope("request")`) ≈ Scoped exactamente.

**La trampa viniendo de Spring:** en Spring el scope por defecto de cualquier bean es `singleton` si no se dice nada explícitamente. En ASP.NET Core no hay valor por defecto: cada `AddScoped`/`AddTransient`/`AddSingleton` es una decisión explícita al registrar el servicio.

**Por qué en el proyecto casi todo es `AddScoped` (`Program.cs`):**
```csharp
builder.Services.AddScoped<IBarcoRepository, BarcoRepository>();
builder.Services.AddScoped<IBarcoService, BarcoService>();
```
El motivo: `AddDbContext<MarinaDbContext>(...)` registra el `DbContext` como Scoped automáticamente (EF Core lo hace así por defecto), porque no es thread-safe y lleva el *change tracker* de esa petición — compartirlo entre requests simultáneas (Singleton) provocaría que dos usuarios se pisaran los cambios.

Regla a respetar: un servicio Scoped no puede ser consumido de forma segura por uno Singleton (*captive dependency* — el framework lanza excepción al arrancar si lo detecta). Como `BarcoRepository` depende del `DbContext` (Scoped), tiene que ser Scoped también; como `BarcoService` depende del repositorio, Scoped otra vez — es un efecto cadena que "empuja" el lifetime hacia arriba.

---

### Model binder

**Qué es:** el mecanismo de ASP.NET Core que coge los datos de una petición HTTP entrante (cuerpo, ruta, query string, cabeceras, formulario) y los convierte en los parámetros del action method, antes de ejecutar el código del método. Tú declaras el parámetro con un atributo que le dice de dónde sacar el valor; el binder hace la conversión/deserialización.

Atributos principales: `[FromRoute]` (de la ruta, ej. `{id:long}`), `[FromQuery]` (query string, ej. `?tipo=Vela`), `[FromBody]` (cuerpo JSON — ver entrada propia), `[FromHeader]` (una cabecera HTTP), `[FromServices]` (inyecta un servicio del contenedor DI directamente como parámetro).

**Equivalente en Java/Spring:** no hay un único concepto que lo agrupe todo con ese nombre — en Spring cada anotación (`@PathVariable`, `@RequestParam`, `@RequestBody`, `@RequestHeader`) hace su parte por separado, pero el mecanismo interno de mapear la petición a parámetros del método es el mismo concepto de fondo.

---

### [FromBody]

**Qué es:** le dice al model binder que deserialice el JSON del cuerpo de la petición contra el tipo del parámetro. Ejemplo real (`Controllers/BarcosController.cs`):
```csharp
public async Task<ActionResult<BarcoDto>> Create([FromBody] BarcoRequestDto dto, CancellationToken ct)
```
El JSON del cliente se deserializa solo en un `BarcoRequestDto`, sin parsing manual.

**Equivalente en Java/Spring:** `@RequestBody`, mismo concepto y mismo sitio:
```java
@PostMapping
public ResponseEntity<BarcoDto> create(@RequestBody BarcoRequestDto dto) { ... }
```

**Diferencia real con Spring — validación automática:** en Spring, validar el body con las anotaciones (`@NotNull`, `@Size`...) exige añadir `@Valid` explícitamente junto a `@RequestBody`; si se te olvida, no se valida nada. En ASP.NET Core, gracias a `[ApiController]` en la clase del controller, la validación del modelo (DataAnnotations como `[Required]`, `[Range]`...) se dispara automáticamente en cuanto termina el binding — sin ningún `[Valid]` en el parámetro. Si falla, el framework devuelve 400 solo, antes de entrar al cuerpo del método. (`BarcoRequestDto` no tiene DataAnnotations todavía, pero en cuanto se le añada una empezará a validarse sin tocar el Controller.)

Nota: con `[ApiController]`, para tipos complejos el `[FromBody]` a veces se puede omitir porque se infiere solo — pero escribirlo explícito, como en el proyecto, es más legible y lo esperable en un code review.

---

### Ok(await ...)

Patrón muy repetido en los Controllers del proyecto — en realidad son dos piezas distintas encajadas.

**`await` dentro de otra llamada:** en C# se puede poner `await` directamente dentro de los paréntesis de otra llamada, sin guardar el resultado en una variable antes:
```csharp
// Equivalentes:
var barcos = await _barcoService.FindAllAsync(ct);
return Ok(barcos);

return Ok(await _barcoService.FindAllAsync(ct));  // más compacto
```

**`Ok(...)`:** método heredado de `ControllerBase` que envuelve el valor recibido en una respuesta HTTP 200 (`OkObjectResult`). Sobrecargas: `Ok()` → 200 sin cuerpo; `Ok(valor)` → 200 con `valor` serializado a JSON.

**Equivalente en Java/Spring:**
```java
@GetMapping
public ResponseEntity<List<BarcoDto>> getAll() {
    return ResponseEntity.ok(barcoService.findAll());
}
```
`Ok(x)` ≈ `ResponseEntity.ok(x)` — mismo helper, pero en Spring se llama como estático de la clase y en ASP.NET Core como método de instancia heredado por el controller. La diferencia real aparece en la versión async: en Java, sin `await`, habría que encadenar `.thenApply(lista -> ResponseEntity.ok(lista))` en vez del `await` inline de C#.

---

### Action

Término ambiguo en C#/.NET — dos sentidos distintos que conviene no mezclar.

**1. Action method (el que ya usas en los Controllers):** en ASP.NET Core, cualquier método público de una clase `Controller` que responde a una petición HTTP. Equivalente conceptual a lo que en Spring se llama informalmente "handler method" (un método anotado con `@GetMapping`/`@PostMapping` dentro de un `@RestController`) — solo que ASP.NET Core sí bautiza el concepto y lo mete en los nombres de sus tipos: `ActionResult<T>`, `IActionResult`, `CreatedAtAction`, `RedirectToAction`.

- `IActionResult`: el action puede devolver cualquier resultado HTTP (`Ok()`, `NotFound()`, `BadRequest()`...) sin comprometerse con un tipo de cuerpo concreto.
- `ActionResult<T>`: deja devolver un `T` directo (se serializa solo) **o** uno de los helpers (`NotFound()`, `Ok(x)`...) — de ahí que los GET del proyecto usen `ActionResult<T>`.

**Ejemplo real (`Controllers/BarcosController.cs`):**
```csharp
return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
```
`CreatedAtAction` construye un 201 con cabecera `Location` apuntando a otro action, resuelto con `nameof()` (a prueba de refactors). En Spring tocaría montar la URI a mano con `ResponseEntity.created(uri)` + `UriComponentsBuilder`.

**2. `Action` como delegado (aún no aparece en el proyecto, pero es de uso común):** `Action` / `Action<T1, T2, ...>` es un delegado incorporado para "un método que no devuelve nada" (void); su contraparte es `Func<..., TResult>`, que sí devuelve algo.

**Equivalente en Java:** las interfaces funcionales de `java.util.function` — `Action` ≈ `Runnable`, `Action<T>` ≈ `Consumer<T>`, `Action<T,U>` ≈ `BiConsumer<T,U>`; mientras que `Func<TResult>` ≈ `Supplier<T>` y `Func<T,TResult>` ≈ `Function<T,R>`.

```csharp
Action<string> log = mensaje => Console.WriteLine(mensaje);
log("hola");
```

---

### Task y CancellationToken

**Qué es `Task`:** el tipo que representa una operación asíncrona en marcha (una "promesa" de resultado). `Task` (sin genérico) = termina sin devolver nada; `Task<T>` = termina y trae un valor `T`. Se consume con `async`/`await`: un método `async Task<X> Foo()` hace `await algoAsync()` y el hilo se libera mientras espera (I/O, DB...), sin quedarse bloqueado; al terminar la operación se reanuda, posiblemente en otro hilo del pool.

**Equivalente en Java:** `CompletableFuture<T>` es el más parecido en concepto, pero encadenar `.thenApply()/.thenCompose()` es más feo que un `await` secuencial. Con Spring MVC clásico (JDBC bloqueante) cada request ocupa un hilo todo el tiempo que dura, incluida la espera a la BD — es lo que `async`/`await` evita. Spring WebFlux (`Mono`/`Flux`) persigue el mismo objetivo con una API totalmente distinta. Los *virtual threads* de Java 21+ (Project Loom) son lo más cercano en espíritu, pero sin cambiar la sintaxis — C# tiene `async`/`await` desde 2012 (C# 5).

**Qué es `CancellationToken`:** un objeto que viaja como parámetro y representa "avísame si alguien pidió cancelar esto". Es cooperativo: NO cancela nada por sí solo. Cada operación que lo recibe decide comprobar `ct.IsCancellationRequested` o delegarlo en algo que ya lo respeta (como los métodos async de EF Core), lanzando `OperationCanceledException` si toca abortar. Si nadie lo mira, no hace nada.

En ASP.NET Core el circuito es automático: si el cliente corta la conexión a mitad de la petición, el framework activa el `CancellationToken` de esa request (`HttpContext.RequestAborted`) y te lo inyecta solo si lo declaras como parámetro del action.

**Equivalente en Java:** no hay nada tan extendido de serie. Lo más cercano: `Future.cancel(true)` (interrumpe el hilo, `InterruptedException` si estaba bloqueado en I/O), o comprobar `Thread.currentThread().isInterrupted()` a mano. Reactor/WebFlux sí tiene cancelación como parte del contrato Reactive Streams — conceptualmente lo más parecido.

**Ejemplo (real del proyecto) — el token viaja Controller → Service → Repository, y solo se "usa" de verdad al final:**
```csharp
// Controller — lo recibe (inyectado automáticamente) y lo reenvía
public async Task<ActionResult<BarcoDto>> GetById(long id, CancellationToken ct) =>
    Ok(await _barcoService.FindByIdAsync(id, ct));

// Service — mismo patrón, solo reenvía
Task<BarcoDto> FindByIdAsync(long id, CancellationToken ct = default);

// Repository (GenericRepository.cs) — aquí EF Core comprueba
// la cancelación mientras espera a SQL Server.
public async Task<T?> FindByIdAsync(long id, CancellationToken ct = default) =>
    await _dbSet.FindAsync(new object[] { id }, ct);
```

El `= default` en la firma significa "si no me pasas token, usa `CancellationToken.None`" — el método sigue funcionando si lo llamas a mano (p.ej. desde un test) sin `HttpContext` de por medio.

---

## Entity Framework Core

### Proyección a DTO con Select

**Qué es:** pedirle a EF Core directamente el DTO (o un único valor calculado) en lugar de entidades. EF traduce la expresión del `Select` a SQL y solo trae las columnas y los cálculos que pides: no carga relaciones en memoria ni las rastrea (*change tracking*). Resuelve a la vez el N+1, el exceso de columnas y el coste del tracking.

**Equivalente en Java:** la proyección de JPQL con constructor (`SELECT new com.marina.RegataDto(r.id, ..., SIZE(r.barcos)) FROM Regata r`) o las *interface projections* de Spring Data.

**Ejemplo real 1: DTO completo (`MarinaApi/Mapping/RegataMapper.cs`, fix de `totalBarcosInscritos`):**
```csharp
public static readonly Expression<Func<Regata, RegataDto>> ToDtoProjection =
    r => new RegataDto(
        r.Id, r.Nombre, r.Lugar, r.Fecha,
        r.Distancia, r.Barcos.Count);   // → COUNT(*) en una subconsulta SQL

// MarinaApi/Repositories/RegataRepository.cs
await _context.Regatas
    .Select(RegataMapper.ToDtoProjection)
    .ToListAsync(ct);
```

**Ejemplo real 2: un único valor (`RegataRepository.ContarTripulantesAsync`, arreglo del N+1):**
```csharp
await _context.Regatas
    .Where(r => r.Id == regataId)
    .Select(r => (int?)r.Barcos.Sum(b => b.Tripulantes.Count))
    .FirstOrDefaultAsync(ct);
```
SQL que genera Pomelo contra MySQL (sacado del log de `dotnet run`):
```sql
SELECT (
    SELECT COALESCE(SUM((
        SELECT COUNT(*)
        FROM `Tripulantes` AS `t`
        WHERE `b0`.`Id` = `t`.`BarcoId`)), 0)
    FROM `BarcoRegata` AS `b`
    INNER JOIN `Barcos` AS `b0` ON `b`.`BarcosId` = `b0`.`Id`
    WHERE `r`.`Id` = `b`.`RegatasId`)
FROM `Regatas` AS `r`
WHERE `r`.`Id` = @__regataId_0
LIMIT 1
```

**Detalles que se ven en ese SQL:**
- `COALESCE(SUM(...), 0)`: en SQL, `SUM` sobre cero filas devuelve `NULL`, pero en C# `Sum()` de una colección vacía devuelve `0`. EF añade el `COALESCE` para que SQL se comporte como C#. Por eso una regata sin barcos da `0` y no un 404.
- `LIMIT 1` sale de `FirstOrDefaultAsync`. Si la regata no existe, no hay fila, y el cast `(int?)` convierte ese "sin fila" en `null`.
- `@__regataId_0` es un parámetro, no el número pegado en el texto: protege de inyección SQL, igual que un `PreparedStatement` de JDBC.

La proyección tiene que declararse como `Expression<Func<...>>` para que EF pueda traducirla (ver Func frente a Expression, en LINQ). Más contexto en [[Guía definitiva de CSharp#8.7 Change tracking, AsNoTracking y el problema N+1]] y en [[Migración de Java a CSharp#7.5. Agregaciones y proyecciones]].

---

### Problema N+1

**Qué es:** un problema de rendimiento en el que se lanza **1** consulta para traer una lista y luego **N** consultas más, una por cada elemento, al acceder a una relación dentro de un bucle. El resultado es correcto, así que no se nota en los tests: solo se ve contando las consultas en el log.

**Equivalente en Java:** el mismo problema de Hibernate con una colección `LAZY` recorrida en un bucle. Se arreglaba con `JOIN FETCH` o con una proyección JPQL.

**Ejemplo real (antes del arreglo, `RegataService.ContarTripulantesTotalesAsync`):**
```csharp
var regata = await _regataRepository.FindByIdWithBarcosAsync(regataId, ct); // 1 consulta
var totalTripulantes = 0;
foreach (var barco in regata.Barcos)
{
    var tripulantes = await _tripulanteRepository.FindByBarcoIdAsync(barco.Id, ct); // +1 por barco
    totalTripulantes += tripulantes.Count;
}
```
Con 20 barcos salían 21 consultas. Después del arreglo (PR #6), sale **1** siempre, tenga los barcos que tenga (ver el ejemplo 2 de la entrada anterior).

**Cómo detectarlo:** en la consola de `dotnet run`, cada consulta aparece como un bloque `Executed DbCommand`. Si una sola petición HTTP dispara muchos bloques casi iguales, es un N+1.

**Cómo arreglarlo:**
- **`Include`**, cuando necesitas los objetos relacionados enteros: los trae en la misma consulta.
- **Proyección con `Select`**, cuando solo necesitas unos datos o un cálculo (un total, un contador). Es la opción preferible.

---

## LINQ

### Func frente a Expression

**Qué es:** la misma lambda se puede guardar de dos formas:
- `Func<Regata, RegataDto>` es **código compilado**: solo se puede ejecutar, en memoria.
- `Expression<Func<Regata, RegataDto>>` es un **árbol de expresión**: la lambda guardada como datos, que otra librería puede leer y traducir. EF Core la traduce a SQL, y Moq la lee para saber qué llamada estás configurando en un `Setup`.

**Equivalente en Java:** no hay un equivalente directo en el lenguaje. Lo más parecido es la diferencia entre un `Predicate<T>` normal (código que se ejecuta) y construir la consulta como objetos con Criteria API o QueryDSL (datos que Hibernate traduce a SQL).

**Ejemplo real (`MarinaApi/Mapping/RegataMapper.cs`):**
```csharp
// ✅ Expression: EF Core la traduce a SQL (COUNT(*) incluido)
public static readonly Expression<Func<Regata, RegataDto>> ToDtoProjection =
    r => new RegataDto(r.Id, r.Nombre, r.Lugar, r.Fecha, r.Distancia, r.Barcos.Count);

// ❌ Si fuera Func<Regata, RegataDto>, .Select() elegiría la versión de IEnumerable:
// EF traería todas las regatas a memoria SIN sus barcos, y r.Barcos.Count daría 0,
// que es el mismo síntoma que tenía el bug de totalBarcosInscritos.
```

**Regla práctica:** todo lo que pases a `Where`, `Select`, `OrderBy`... sobre un `IQueryable` (un `DbSet` de EF) tiene que ser una lambda escrita en el sitio o un `Expression<Func<>>`. Si lo guardas en una variable para reutilizarlo, decláralo como `Expression`. Más detalle en [[Guía definitiva de CSharp#5.2 Func, Action y Predicate — predefinidos]].

---

## SQL Server

_(pendiente de primeras entradas)_

---

## Testing (xUnit / Moq)

### Arrange / Act / Assert (AAA)

**Qué es:** el patrón estándar para estructurar un test unitario en tres bloques separados:
- **Arrange**: preparas lo necesario — datos, mocks configurados, estado inicial.
- **Act**: ejecutas la única acción que se está probando (normalmente una línea).
- **Assert**: compruebas que el resultado es el esperado.

**Ejemplo real (`MarinaApi.Tests/BarcoServiceTests.cs`):**
```csharp
[Fact]
public async Task FindByIdAsync_CuandoExiste_DevuelveDto()
{
    // Arrange
    var barco = new Barco { Id = 1, Nombre = "Estrella del Mar", Tipo = "Velero", Eslora = 12 };
    _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(barco);

    // Act
    var resultado = await _service.FindByIdAsync(1);

    // Assert
    resultado.Nombre.Should().Be("Estrella del Mar");
    resultado.Id.Should().Be(1);
}
```

**Por qué a veces aparecen "Act + Assert" fusionados** (como en el propio proyecto): cuando lo que se comprueba es que el método lanza una excepción, no se puede separar "ejecutar" de "comprobar" — la propia llamada, envuelta en el assert, es la que dispara la excepción que se está verificando:
```csharp
[Fact]
public async Task FindByIdAsync_CuandoNoExiste_LanzaNotFoundException()
{
    // Arrange: el mock devuelve null, como session.find() en Java cuando no hay resultado
    _repositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Barco?)null);

    // Act + Assert
    await FluentActions.Awaiting(() => _service.FindByIdAsync(999))
        .Should().ThrowAsync<NotFoundException>();
}
```

**Equivalente en Java:** el mismo patrón AAA se usa igual con JUnit + Mockito (`when(...).thenReturn(...)` para Arrange, la llamada para Act, `assertEquals`/`assertThrows` para Assert). No es una diferencia de lenguaje sino una convención universal de testing — solo cambia la sintaxis de las librerías (`Mock.Setup` vs `Mockito.when`, FluentAssertions `Should().Be()` vs `assertEquals` de JUnit).

---

### Setup, ReturnsAsync, It.IsAny y Verify (Moq)

**Qué es:** las cuatro piezas básicas para usar un mock de Moq. Un `Mock<IRegataRepository>` implementa la interfaz, pero sus métodos no hacen nada: si llamas a uno sin configurar, devuelve el valor por defecto de su tipo (`null`, `0`, `false`...).
- **`Setup(r => ...)`** describe qué llamada vas a configurar. Moq no ejecuta la lambda: la lee como `Expression` (ver Func frente a Expression, en LINQ).
- **`ReturnsAsync(valor)`** fija lo que devuelve esa llamada, ya envuelto en un `Task` terminado. Es un atajo de `.Returns(Task.FromResult(valor))`.
- **`It.IsAny<T>()`** acepta cualquier valor en ese argumento.
- **`Verify(r => ..., Times.X)`** comprueba **después** del Act si se llamó a un método y cuántas veces.

**Equivalente en Java (Mockito):**

| Moq | Mockito |
|---|---|
| `mock.Setup(r => r.Metodo(1, It.IsAny<CancellationToken>()))` | `when(mock.metodo(eq(1L), any()))` |
| `.ReturnsAsync(2)` | `.thenReturn(2)` |
| `It.IsAny<T>()` | `any(T.class)` |
| `mock.Verify(r => r.Metodo(...), Times.Never)` | `verify(mock, never()).metodo(...)` |
| `mock.Object` (el objeto falso que se inyecta) | el propio `mock` |

La diferencia de estilo: en Mockito llamas al método "de verdad" dentro de `when(...)`; en Moq lo describes con una lambda.

**Ejemplo real (`MarinaApi.Tests/RegataServiceTests.cs`, tras el arreglo del N+1):**
```csharp
// Caso feliz
_regataRepositoryMock.Setup(r => r.ContarTripulantesAsync(1, It.IsAny<CancellationToken>()))
    .ReturnsAsync(2);

// Regata inexistente: (int?)null para que ReturnsAsync no sea ambiguo
_regataRepositoryMock.Setup(r => r.ContarTripulantesAsync(999, It.IsAny<CancellationToken>()))
    .ReturnsAsync((int?)null);
```

**Lecciones del proyecto:**
- Cuando el Service deja de llamar a un método, los `Setup` antiguos siguen compilando, pero ya no configuran nada. El test fallará o, peor, probará algo que ya no existe. Hay que actualizarlos a la vez que el código.
- Un `Verify(..., Times.Never)` sobre un método al que el Service ya no llama nunca pasa siempre. Un test que no puede fallar no aporta nada: se borra.

Más detalle en [[Migración de Java a CSharp#8.5. Mock Tests en profundidad]].

---

## Git / Azure DevOps

### LGTM

**Qué es:** acrónimo de "Looks Good To Me". Comentario/aprobación estándar en un Pull Request que indica que el reviewer ha revisado el código y lo aprueba para mergear.

**Equivalente en Java/Spring:** mismo concepto en cualquier flujo de PR (GitHub, GitLab, Bitbucket, Azure DevOps) — no es específico de .NET, es cultura de Git en general, independiente del lenguaje o framework.

**Ejemplo de uso:** comentario en el PR → `LGTM, buen trabajo con la validación de FK 👍` seguido de pulsar "Approve" (o "Submit review" en GitHub).

**Nota del proyecto:** GitHub no permite auto-aprobar tu propio PR ("You can't approve your own pull request"). En un equipo real esto obliga a que otra persona revise; en la simulación, cuando no hay un segundo reviewer real, se deja el comentario como constancia del review pero se usa "Comment" en vez de "Approve".

---

### Squash and merge

**Qué es:** una de las tres estrategias de merge de un Pull Request en GitHub/Azure DevOps. Coge todos los commits de la rama (p.ej. `wip: Repository`, `wip: DTOs`, `feat: Controller`) y los aplasta en un único commit que se aplica sobre la rama base (`master`), con un solo mensaje limpio.

**Alternativas:**
- *Merge commit*: conserva todos los commits originales de la rama + añade uno de merge encima.
- *Rebase and merge*: reescribe los commits de la rama encima de `master` sin commit de merge (historial lineal, pero cambian los SHA de los commits).

**Equivalente en Java/Spring:** no aplica — es una opción de configuración de la plataforma de Git (GitHub/Azure DevOps/GitLab), totalmente independiente del lenguaje.

**Cuándo usarlo:** cuando la rama tiene commits `wip:`/intermedios de trabajo que no aportan valor individual en el historial final. Común en equipos Scrum: un commit limpio por ticket cerrado.

**Ejemplo (real del proyecto):** rama `feature/crud-tripulante` con commits `wip: TripulanteRepository`, `wip: DTOs, Mapper y Service...`, `feat: TripulantesController...`, `test: TripulanteServiceTests...` → tras squash and merge, en `master` aparece un único commit: `feat: CRUD de Tripulante (#150)`.

---

---

## Scrum / Metodología

_(pendiente de primeras entradas)_
