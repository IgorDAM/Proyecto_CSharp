# Glosario C# / .NET — Marina API

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

- [Sintaxis C#](#sintaxis-c)
- [.NET / ASP.NET Core](#net--aspnet-core)
- [Entity Framework Core](#entity-framework-core)
- [LINQ](#linq)
- [SQL Server](#sql-server)
- [Testing (xUnit / Moq)](#testing-xunit--moq)
- [Git / Azure DevOps](#git--azure-devops)
- [Scrum / Metodología](#scrum--metodología)

---

## Sintaxis C#

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

## .NET / ASP.NET Core

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

_(pendiente de primeras entradas)_

---

## LINQ

_(pendiente de primeras entradas)_

---

## SQL Server

_(pendiente de primeras entradas)_

---

## Testing (xUnit / Moq)

_(pendiente de primeras entradas)_

---

## Git / Azure DevOps

_(pendiente de primeras entradas)_

---

## Scrum / Metodología

_(pendiente de primeras entradas)_
