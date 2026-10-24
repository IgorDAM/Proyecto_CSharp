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

- [Sintaxis C#](#sintaxis-c)
- [.NET / ASP.NET Core](#net--aspnet-core)
- [Entity Framework Core](#entity-framework-core)
- [LINQ](#linq)
- [SQL Server](#sql-server)
- [Testing (xUnit / Moq)](#testing-xunit--moq)
- [Git / Azure DevOps](#git--azure-devops)
- [Errores frecuentes (compilación y entorno)](#errores-frecuentes-compilación-y-entorno)
- [Scrum / Metodología](#scrum--metodología)

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

### Operador null-condicional ?.

**Qué es:** `a?.B` devuelve `a.B` si `a` no es `null`, y `null` si lo es, en vez de lanzar una `NullReferenceException`. Se suele combinar con `??` para dar un valor por defecto: `a?.B ?? "sin dato"`.

**Equivalente en Java:** `a != null ? a.getB() : null`, o `Optional.ofNullable(a).map(A::getB).orElse(null)`.

**Ejemplo real (#179, experimento 5, `MarinaApi/Mapping/TripulanteMapper.cs`):**
```csharp
public static TripulanteDto ToDto(this Tripulante tripulante) =>
    new(tripulante.Id, tripulante.Nombre, tripulante.Rol, tripulante.BarcoId,
        tripulante.Barco?.Nombre);
```

**Por qué hace falta aquí:** la navegación está declarada `public Barco? Barco` y solo viene rellena si el Repository la cargó (`Include` o proyección). Sin el `?`, el compilador avisa con CS8602 ("posible desreferencia de una referencia nula") y, en ejecución, explotaría.

**En JavaScript** existen los mismos dos operadores con el mismo significado: `?.` y `??`.

---

### Record posicional: anulable no es opcional

**Qué es:** en un `record` posicional todos los parámetros son obligatorios. Declarar uno como `string?` significa que **admite `null`**, no que se pueda omitir. Para que fuera opcional haría falta un valor por defecto (`string? BarcoNombre = null`).

**Equivalente en Java:** igual que en un `record` de Java o en un constructor: añadir un componente rompe todos los `new` que no lo pasen.

**Ejemplo real (#179, experimento 5):** al añadir `string? BarcoNombre` a `TripulanteDto`, `dotnet build` falló en el Mapper:
```
error CS7036: No se ha dado ningún argumento que corresponda al parámetro
requerido "BarcoNombre" de "TripulanteDto.TripulanteDto(long, string, string, long, string?)"
```
El error es útil: el compilador señala todas las capas que construyen el DTO.

### Argumentos con nombre

**Qué es:** al llamar a un método se puede escribir el nombre del parámetro delante del valor: `metodo(nombre: valor)`. Así la llamada se lee sola y los argumentos se pueden pasar en cualquier orden. Se combina con los **parámetros opcionales** (`CancellationToken ct = default`), porque permite saltarse los opcionales intermedios.

**Equivalente en Java:** no existe. En Java solo cuenta la posición, y para que una llamada con muchos parámetros se entienda se recurre a un *builder* o a sobrecargas.

**Ejemplo real (migración `AmarreSetNullAlBorrarBarco`, #154):**
```csharp
migrationBuilder.AddForeignKey(
    name: "FK_Amarres_Barcos_BarcoId",
    table: "Amarres",
    column: "BarcoId",
    principalTable: "Barcos",
    principalColumn: "Id",
    onDelete: ReferentialAction.SetNull);
```
Sin los nombres serían seis valores seguidos, y habría que abrir la firma del método para saber cuál es cuál.

---

### partial class

**Qué es:** una misma clase repartida entre varios archivos. Cada parte se declara con `partial` y el compilador las junta en una sola clase. Se usa sobre todo cuando una herramienta genera una parte del código y tú escribes la otra: así puedes regenerar la parte automática sin pisar la tuya.

**Equivalente en Java:** no existe. En Java una clase pública vive en un único archivo. Lo más parecido es el código que genera Lombok, pero ese no se ve como archivo aparte.

**Ejemplo real (#154):** cada migración de EF Core son dos archivos con la misma clase:
```csharp
// Migrations/20261005181450_AmarreSetNullAlBorrarBarco.cs: Up() y Down(), lo que se revisa
public partial class AmarreSetNullAlBorrarBarco : Migration { ... }

// Migrations/20261005181450_AmarreSetNullAlBorrarBarco.Designer.cs: generado, no se toca
partial class AmarreSetNullAlBorrarBarco { ... }
```

---

### Operadores con tipos anulables (lifted operators)

**Qué es:** los operadores de un tipo de valor (`==`, `<`, `+`...) también funcionan con su versión anulable (`long?`, `int?`). C# los "eleva" (*lift*) y define qué pasa con el `null`:
- `==` y `!=` comparan sin fallar: `null == 5` es `false` y `null == null` es `true`.
- `<`, `>`, `<=` y `>=` devuelven `false` si algún lado es `null` (ojo: `null < 5` **y** `null > 5` son los dos `false`).
- Los aritméticos propagan el `null`: `null + 1` es `null`.

**Equivalente en Java:** con `Long a = null`, la expresión `a == 5L` obliga a hacer *unboxing* y lanza `NullPointerException`. En C# no hay excepción.

**Ejemplo real (`AmarreService.AssignBarcoAsync`, #172):**
```csharp
// amarre.BarcoId es long? (puede ser null si el amarre está libre); dto.BarcoId es long
if (amarre.BarcoId == dto.BarcoId)   // amarre libre → null == 7 → false, sin excepción
    return amarre.ToDto();           // ya estaba asignado a ese barco: idempotente
```
Para preguntar por el `null` en sí, el proyecto usa los patrones `is null` / `is not null`.

---

### Property pattern (`{ Number: 1062 }`)

**Qué es:** un patrón que comprueba, en una sola expresión, que un objeto es de cierto tipo **y** que algunas de sus propiedades valen lo que dices. Se escribe con llaves y `Propiedad: valor` dentro: `x is MySqlException { Number: 1062 }` significa «x es un `MySqlException` y su `Number` es 1062». Si `x` es `null` o de otro tipo, el resultado es `false`, sin excepción.

**Equivalente en Java:** `x instanceof MySqlException e && e.getNumber() == 1062`. Java 21 tiene *record patterns*, pero solo para `record`; no hay una forma equivalente de comprobar propiedades de una clase normal dentro del propio patrón.

**Ejemplo real (`AmarreRepository.UpdateAsync`, #155):**
```csharp
catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
{
    throw new ConflictException($"El Barco con Id {entity.BarcoId} ya tiene asignado otro Amarre.");
}
```
`ex.InnerException` es de tipo `Exception?` y puede ser `null`: el patrón cubre los tres casos (`null`, otro tipo, otro `Number`) sin `if` anidados. Junto con `when` (filtro de excepción, ver la Guía), el `catch` solo entra si la condición es `true`; si no, la excepción sigue subiendo con la pila intacta. En el #155 se probó quitando el `when`: el test que lanza un `DbUpdateException` sin `MySqlException` dentro se pone rojo, porque el `catch` se tragaría todos los errores.

---

### Tuplas con nombre y deconstrucción

**Qué es:** una tupla agrupa varios valores en uno sin crear una clase. Cada posición puede llevar nombre: `(List<Barco> Elementos, int Total)`. Quien la recibe puede usar `resultado.Elementos` o **deconstruirla** en variables sueltas: `var (elementos, total) = await repo.FindPageAsync(...);`.

**Equivalente en Java:** no hay tuplas en el lenguaje. Se crea un `record Pagina(List<Barco> elementos, int total)` solo para devolver dos cosas, o se usa un `Pair` de una librería. En C# la tupla es del lenguaje y no hace falta declarar el tipo aparte.

**Ejemplo real (`Repositories/PaginacionExtensions.cs` y `Services/AmarreService.cs`):**
```csharp
public static async Task<(List<T> Elementos, int Total)> PaginarAsync<T>(...)

var (amarres, total) = await _repository.FindPageAsync(r => r.Id, pagina, tamanio, ct);
```
**Cuándo usarla:** para devolver dos o tres valores entre capas internas. Hacia fuera (la API) se usa un `record` con nombre, como `PaginaDto<T>`, porque forma parte del contrato JSON.

---

### Genéricos: `record PaginaDto<T>` y métodos con `<T, TResultado>`

**Qué es:** `T` es un tipo que se decide en cada uso. Un `record` genérico (`PaginaDto<BarcoDto>`) y un método genérico (`PaginarAsync<T, TResultado>`) se escriben una vez y valen para cualquier entidad. Con varios parámetros de tipo, el compilador suele deducirlos de los argumentos y no hay que escribirlos en la llamada.

**Equivalente en Java:** `record PaginaDto<T>(...)` y `<T, R> Page<R> paginar(...)`. La diferencia es que los genéricos de C# **no se borran** en tiempo de ejecución (en Java sí, *type erasure*), así que `typeof(T)` y `new T()` son posibles.

**Ejemplo real (`Dtos/PaginaDto.cs`):**
```csharp
public record PaginaDto<T>(IReadOnlyList<T> Elementos, int Pagina, int Tamanio, int TotalElementos)
{
    public int TotalPaginas => (int)Math.Ceiling((double)TotalElementos / Tamanio);
}
```
El `(double)` evita la división entera: `5 / 2` en `int` es `2`, no `2,5`, y `Ceiling` no tendría nada que redondear. `TotalPaginas` es una propiedad calculada: no es parámetro del constructor y aparece igualmente en el JSON.

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

### UseDefaultFiles, UseStaticFiles y el orden de los middlewares

**Qué es:** `UseStaticFiles()` sirve los archivos de `wwwroot` (HTML, JS, CSS) sin pasar por ningún Controller. `UseDefaultFiles()` reescribe `/` a `/index.html`. Los middlewares se ejecutan **en el orden en que se registran** en `Program.cs`, así que `UseDefaultFiles` tiene que ir antes que `UseStaticFiles`: primero se reescribe la ruta y después se sirve el archivo. Si no existe un archivo con esa ruta, `UseStaticFiles` pasa la petición al siguiente middleware.

**Equivalente en Java:** la carpeta `src/main/resources/static/` de Spring Boot, que se sirve sola; el orden de los middlewares es como el orden de los `Filter` de la cadena de Servlets.

**Ejemplo real (#179, `MarinaApi/Program.cs`):**
```csharp
app.UseDefaultFiles();   // "/" → "/index.html"
app.UseStaticFiles();    // sirve el archivo de wwwroot

### User secrets

**Qué es:** un almacén de configuración **fuera del repo**, por proyecto y por usuario, pensado para guardar contraseñas y claves en desarrollo. Vive en `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`, y el `<UserSecretsId>` se declara en el `.csproj`. En Development, `WebApplication.CreateBuilder()` lo carga automáticamente **encima** de `appsettings.json`: si la misma clave está en los dos sitios, gana user-secrets.

**Equivalente en Java/Spring:** un `application-local.properties` metido en `.gitignore`, o variables de entorno. La diferencia es que aquí el archivo ni siquiera está dentro de la carpeta del proyecto, así que es imposible subirlo a git por error.

**Ojo:** no cifra nada, solo saca el secreto del repo. En producción se usan variables de entorno o un almacén real (Azure Key Vault, AWS Secrets Manager).

**Ejemplo (real del proyecto, corrección de seguridad del 2026-09-30):**
```powershell
# La cadena de conexión con contraseña ya no está en appsettings.json
dotnet user-secrets set "ConnectionStrings:MarinaDb" "Server=localhost;Port=3306;Database=gestion_maritima;User=marina_app;Password=..." --project .\MarinaApi
dotnet user-secrets list --project .\MarinaApi
```
```csharp
// Program.cs — se lee igual que antes: la configuración ya mezcla las fuentes
var connectionString = builder.Configuration.GetConnectionString("MarinaDb");
```

---

### JSON en camelCase

**Qué es:** `System.Text.Json`, el serializador de ASP.NET Core, convierte por defecto las propiedades PascalCase de C# a camelCase en el JSON. El DTO tiene `BarcoNombre` y el frontend recibe `barcoNombre`.

**Equivalente en Java:** Jackson ya usa camelCase porque en Java los getters (`getBarcoNombre`) se traducen así; en C# es una conversión explícita del serializador.

**Ejemplo real (#179):** en JavaScript hay que escribir `tripulante.barcoNombre`. Si escribes mal el nombre (`regata.barcosInscritos` en vez de `totalBarcosInscritos`), JS no avisa: devuelve `undefined`.

---

### Restricciones de ruta ({id:long})

**Qué es:** `[HttpGet("{id:long}")]` solo casa si el segmento se puede convertir a `long`. Si no, la ruta **no existe** para esa petición: se responde 404 sin llegar al Controller.

**Equivalente en Java:** en Spring, `@GetMapping("/{id}")` con `Long id` sí entra en el mapping y falla al convertir: da un **400** (`MethodArgumentTypeMismatchException`). Sin la restricción `:long`, .NET también llegaría al Controller y el model binding daría un 400.

**Ejemplo real (#179, experimento 4):** `GET /api/regatas/abc` → 404 con el cuerpo vacío, igual que una URL que no existe (`/api/regatasXX`).

---

### ProblemDetails y application/problem+json

**Qué es:** formato estándar de respuestas de error (RFC 9457, antes RFC 7807): un JSON con `type`, `title`, `status` y `detail`, enviado con `Content-Type: application/problem+json`. Así el cliente sabe que es un error sin tener que interpretar el cuerpo.

**Equivalente en Java:** la clase `ProblemDetail` de Spring 6, devuelta desde un `@ControllerAdvice` con `@ExceptionHandler`.

**Ejemplo real (#179, experimento 4, `MarinaApi/Middleware/ExceptionHandlingMiddleware.cs`):** `GET /api/regatas/9999` → el Service lanza `NotFoundException`, el middleware la captura y responde:
```json
{"type":"https://httpstatuses.com/404","title":"NotFound","status":404,"detail":"Regata con Id 9999 no encontrado."}
```
Un 404 de ruta inexistente, en cambio, llega con el cuerpo vacío: en el frontend no todo error trae JSON, así que hay que mirar `respuesta.ok` antes de hacer `respuesta.json()`.

### Autenticación JWT Bearer (AddJwtBearer y dotnet user-jwts)

**Qué es:** el cliente manda en cada petición la cabecera `Authorization: Bearer <token>`, donde el token es un JWT firmado. `AddJwtBearer()` registra el *handler* que comprueba la firma, el emisor (`iss`), la audiencia (`aud`) y la caducidad. Si todo cuadra, rellena `HttpContext.User`; si no, la petición llega sin usuario y la autorización la rechaza con **401**.

**Equivalente en Java/Spring:** Spring Security con `oauth2ResourceServer().jwt()` y `spring.security.oauth2.resourceserver.jwt.issuer-uri`.

**`dotnet user-jwts`:** herramienta de desarrollo (.NET 7+) que emite tokens de prueba sin necesidad de un servidor de identidad. `dotnet user-jwts create` guarda la clave de firma en user-secrets, escribe el emisor y las audiencias válidas en `appsettings.Development.json` y muestra el token. Por eso `AddJwtBearer()` funciona **sin opciones**: lee todo de la sección `Authentication:Schemes:Bearer`.

**Ejemplo (real del proyecto):**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
// ...
app.UseAuthentication();   // 1º: ¿quién eres? (lee el token)
app.UseAuthorization();    // 2º: ¿puedes entrar?
```
```powershell
dotnet user-jwts create --project .\MarinaApi --name igor   # genera un token nuevo
dotnet user-jwts list   --project .\MarinaApi               # los ya emitidos
```
En Swagger: botón **Authorize**, se pega el token **sin** el prefijo `Bearer`.

**Un JWT está firmado, no cifrado.** Sus tres partes (cabecera, contenido y firma) van en Base64URL: cualquiera que tenga el token puede leer su contenido, por ejemplo en jwt.io. La firma solo garantiza que nadie lo ha modificado. Por eso no se meten datos sensibles en un JWT, y un token **no se pega** en chats, tickets ni capturas: mientras no caduque, quien lo tenga puede usarlo.

---

### FallbackPolicy, [Authorize] y [AllowAnonymous]

**Qué es:** `[Authorize]` exige un usuario autenticado en un controlador o en una acción concreta. La **FallbackPolicy** es la política que se aplica a todo endpoint que *no* tiene ningún atributo de autorización. Si se configura como "requiere usuario autenticado", la API queda **cerrada por defecto** y hay que abrir explícitamente con `[AllowAnonymous]` lo que deba ser público.

**Equivalente en Java/Spring:** `.anyRequest().authenticated()` al final de la configuración de `HttpSecurity`, con `.requestMatchers(...).permitAll()` como equivalente de `[AllowAnonymous]`.

**Por qué FallbackPolicy y no `[Authorize]` en cada controlador:** si mañana se crea un controlador nuevo y se olvida el atributo, con `[Authorize]` quedaría abierto; con la FallbackPolicy queda protegido. Un fallo de este tipo se corrige añadiendo `[AllowAnonymous]`, nunca dejando datos expuestos sin querer.

**Lo que NO cubre:** Swagger (`UseSwagger`) y los archivos estáticos (`UseStaticFiles`) son middleware, no endpoints de controlador, así que la FallbackPolicy no les afecta.

**Ejemplo (real del proyecto):**
```csharp
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```
```csharp
// Así se abriría un endpoint concreto (p.ej. para el frontend de #179)
[AllowAnonymous]
[HttpGet]
public async Task<ActionResult<List<BarcoDto>>> GetAll(CancellationToken ct) => ...
```

---

### CORS

**Qué es:** *Cross-Origin Resource Sharing*. El **navegador** bloquea que una web de un origen (esquema + dominio + puerto) lea respuestas de otro origen, salvo que el servidor lo autorice con cabeceras `Access-Control-Allow-*`. Solo afecta a navegadores: Postman, `curl` o un backend no lo aplican. No sustituye a la autenticación; sirve para impedir que *otra web* use el navegador del usuario contra tu API.

**Equivalente en Java/Spring:** `@CrossOrigin` o un `CorsConfigurationSource` con `setAllowedOrigins(...)`.

**El error que tenía el proyecto:** `AllowAnyOrigin()` permitía que cualquier página abierta en el navegador llamara a la API en `localhost`, incluidos los `DELETE`. Ahora se usa una lista blanca configurable. Un frontend servido **desde la propia API** (`wwwroot`, rama del #179) es el mismo origen, así que no necesita CORS.

**Ejemplo (real del proyecto):**
```csharp
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
              .WithHeaders("Authorization", "Content-Type")));
```
```json
// appsettings.json — vacío = ningún origen externo permitido
"Cors": { "AllowedOrigins": [] }
```

---

### `[FromQuery]` con `[Range]`: parámetros de paginación

**Qué es:** `[FromQuery]` toma el valor de la query string (`?pagina=2&tamanio=10`) y `[Range(1, 100)]` lo valida. Con `[ApiController]` en la clase, si el valor no cumple el rango, el framework responde **400 con `ProblemDetails` antes de entrar al método**. El valor por defecto (`= 1`) se usa cuando el parámetro no viene.

**Equivalente en Java:** `@RequestParam(defaultValue = "1") @Min(1) int pagina`, con `@Validated` en el controller. En Spring hay que acordarse de `@Validated`; en ASP.NET Core la validación va incluida con `[ApiController]`.

**Ejemplo real (`Controllers/RegatasController.cs`):**
```csharp
public async Task<ActionResult<PaginaDto<RegataDto>>> GetAll(
    [FromQuery, Range(1, int.MaxValue)] int pagina = 1,
    [FromQuery, Range(1, 100)] int tamanio = 20,
    CancellationToken ct = default)
```
Hace falta `using System.ComponentModel.DataAnnotations;`. El máximo de 100 evita que un cliente pida `tamanio=1000000` y cargue la tabla entera, que era justo el problema del #175.

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

### Include y la ausencia de lazy loading

**Qué es:** EF Core solo carga lo que se le pide. Una navegación (`tripulante.Barco`) se queda en `null` salvo que la consulta la cargue con `.Include(t => t.Barco)` (un `JOIN` en la misma consulta) o con una proyección. No hay lazy loading salvo que se instale el paquete de proxies.

**Equivalente en Java:** `Include` es el `JOIN FETCH` de JPQL. La diferencia está en el valor por defecto: en JPA, `@ManyToOne` es EAGER, así que Hibernate trae el objeto relacionado sin pedírselo (y a menudo provoca N+1).

**Ejemplo real (#179, experimento 5, `MarinaApi/Repositories/TripulanteRepository.cs`):**
```csharp
public async Task<List<Tripulante>> FindAllWithBarcoAsync(CancellationToken ct = default) =>
    await _context.Tripulantes
        .Include(t => t.Barco)
        .ToListAsync(ct);
```
Sin el `Include`, el JSON traía `barcoNombre: null`. Con él, el log muestra `INNER JOIN Barcos`.

**Coste:** `Include` trae **todas** las columnas de la tabla relacionada (`Capacidad`, `Eslora`, `Manga`...) aunque solo se use una. Si solo hace falta un dato, es mejor una proyección (ver Proyección a DTO con Select, en Entity Framework Core).

### Sin `Include` la colección queda vacía, sin error

**Qué es:** EF Core **no hace lazy loading por defecto**. Una propiedad de navegación de tipo colección solo se rellena si la consulta la carga (`Include`) o si esa entidad ya estaba en el contexto. Si no, conserva el valor con el que la inicializó el constructor (`= new();` → lista vacía), **sin excepción ni aviso**. El código que opera sobre ella (`RemoveAll`, `Add`...) no falla, pero no hace nada útil, y `SaveChangesAsync` no detecta cambios (el *change tracker* no ve modificaciones), así que no se emite SQL. Resultado típico: un endpoint que responde `204` y no ha cambiado nada.

**Equivalente en Java:** con Hibernate y `FetchType.LAZY`, acceder a la colección dentro de la sesión la carga; fuera, lanza `LazyInitializationException`. EF Core no hace ni una cosa ni la otra: calla. El remedio es el mismo que con `JOIN FETCH` (ver MIGRACION, 7.3): `Include()`.

**Ejemplo real (#174, `DesinscribirBarcoAsync`):**
```csharp
// Mal: no carga barco.Regatas; RemoveAll sobre la lista vacía no hace nada
var barco = await _barcoRepository.FindByIdAsync(barcoId, ct);

// Bien: el mismo método que usa InscribirBarcoAsync, con Include(b => b.Regatas)
var barco = await _barcoRepository.FindByIdWithRegatasAsync(barcoId, ct);
```
**Cómo se detecta:** con el log de SQL de la consola de la API. El `DELETE` mostraba dos `SELECT` simples, sin `JOIN` a `BarcoRegata` y sin escritura; el `POST` de inscribir sí tenía `LEFT JOIN BarcoRegata` + `INSERT`. **Cómo se previene:** un test que compruebe el **efecto** (la regata ya no está en `barco.Regatas`, o `Verify` sobre `UpdateAsync`) y no solo el código de estado.

---

### OnDelete y DeleteBehavior: qué pasa al borrar el principal

**Qué es:** `.OnDelete(DeleteBehavior.X)` decide qué les pasa a los **dependientes** (las filas que tienen la FK) cuando se borra el **principal**. Va siempre en ese sentido, del principal al dependiente, aunque configures la relación desde cualquiera de los dos lados. Hace dos cosas:
1. Dice qué hace EF Core con las entidades que tiene cargadas en memoria (*tracked*).
2. Escribe la cláusula `ON DELETE ...` de la FK cuando se genera la migración. A partir de ahí la regla la aplica **la base de datos**, aunque el borrado venga de Workbench o de otro programa.

| `DeleteBehavior` | En la base de datos | Uso típico |
|---|---|---|
| `Cascade` | `ON DELETE CASCADE` | El dependiente no tiene sentido sin el principal. **Por defecto en relaciones obligatorias** (FK no anulable) |
| `SetNull` | `ON DELETE SET NULL` | El dependiente sobrevive sin el principal. Necesita la FK anulable |
| `ClientSetNull` | `NO ACTION` | **Por defecto en relaciones opcionales** (FK anulable). EF pone `null` solo en lo que tiene cargado; si el dependiente no está cargado, la BD rechaza el borrado |
| `Restrict` / `NoAction` | `RESTRICT` / `NO ACTION` | Prohibido borrar el principal si tiene dependientes |
| `ClientCascade` | `NO ACTION` | EF borra en cascada solo lo que tiene cargado (lo más parecido a la cascada de JPA) |

**Equivalente en Java:** `cascade = CascadeType.REMOVE` (o `ALL`) y `orphanRemoval` los ejecuta **Hibernate en memoria**, y solo si el borrado pasa por él; no cambian el esquema. Lo que se parece de verdad a `OnDelete` es la anotación `@OnDelete(action = OnDeleteAction.CASCADE)` de Hibernate (`SET_NULL` desde Hibernate 6.2), que sí escribe el `ON DELETE` en la tabla.

**Ejemplo real (#154, `MarinaApi/Data/MarinaDbContext.cs`):**
```csharp
modelBuilder.Entity<Amarre>()
    .HasOne(a => a.Barco)
    .WithOne(b => b.Amarre)
    .HasForeignKey<Amarre>(a => a.BarcoId)   // la FK está en Amarre: Amarre es el dependiente
    .OnDelete(DeleteBehavior.SetNull);       // antes era Cascade: borrar un barco borraba su amarre
```
El bug del #154 venía de pensar en JPA ("cascada desde Barco hacia Amarre = `CascadeType.ALL`"). En EF Core, `Cascade` borra **el lado que tiene la FK**, que aquí era el amarre: justo el que tenía que sobrevivir. Tabla corregida en [[Migración de Java a CSharp#4.4. Cascade — misma idea, sintaxis distinta]].

---

### Los archivos de una migración

**Qué es:** `dotnet ef migrations add <Nombre>` compara el modelo actual (`OnModelCreating` y las entidades) con `MarinaDbContextModelSnapshot.cs`, que guarda cómo era el modelo en la última migración. Con la diferencia crea o toca tres archivos:

| Archivo | Para qué sirve | ¿Se revisa? |
|---|---|---|
| `<fecha>_<Nombre>.cs` | `Up()` aplica el cambio y `Down()` lo deshace | **Sí, siempre**, antes de aplicarla |
| `<fecha>_<Nombre>.Designer.cs` | Foto del modelo completo en ese momento (`partial class`) | No, es generado |
| `MarinaDbContextModelSnapshot.cs` | El modelo "actual", con el que se comparará la próxima migración | No, pero se commitea con la migración |

**Equivalente en Java:** es como un changeset de Flyway o Liquibase (`V2__...sql`), pero escrito automáticamente a partir de las entidades. No es `ddl-auto=update`: MySQL no se toca hasta lanzar `dotnet ef database update`, que además apunta la migración en la tabla `__EFMigrationsHistory` (el `flyway_schema_history` de Flyway).

**Ejemplo real (#154):** cambiar la acción de una FK genera un `DropForeignKey` seguido de un `AddForeignKey`, porque ningún motor permite modificar una FK en el sitio. Para ver el SQL exacto antes de aplicarlo:
```powershell
dotnet ef migrations script InitialCreate AmarreSetNullAlBorrarBarco --project .\MarinaApi
```
**Por qué revisar `Up()`:** si renombras una propiedad, EF Core a veces genera `DropColumn` + `AddColumn` en lugar de `RenameColumn`, y esa columna pierde los datos sin ningún aviso.

---

### En MySQL el DDL no es transaccional

**Qué es:** en MySQL, cada sentencia DDL (`CREATE`, `ALTER`, `DROP`...) hace un **commit implícito**: no se puede deshacer con un `ROLLBACK`. Si una migración tiene dos `ALTER TABLE` y falla el segundo, el primero ya se ha quedado aplicado, y la migración no consta en `__EFMigrationsHistory`. Hay que arreglarlo a mano.

**Diferencia entre motores:** en **PostgreSQL** y **SQL Server** el DDL sí es transaccional: si algo falla, se deshace toda la migración. Es una de las diferencias reales al pasar de SQL Server a MySQL.

**Equivalente en Java:** igual con Flyway sobre MySQL. Su documentación avisa de que en MySQL una migración fallida a medias deja el esquema en un estado intermedio.

**Ejemplo real (#154):** el script de `AmarreSetNullAlBorrarBarco` acaba en `COMMIT`, pero entre el `DROP FOREIGN KEY` y el `ADD CONSTRAINT` ya ha habido un commit. Si fallara el segundo, la tabla `Amarres` se quedaría **sin FK**. Por eso, en un entorno real, el script se revisa antes (`migrations script --idempotent`) en lugar de lanzar `database update` a ciegas.

---

### Paginar con `CountAsync`, `OrderBy`, `Skip` y `Take`

**Qué es:** una consulta sobre un `DbSet` es un `IQueryable<T>`: una **consulta sin ejecutar**. Cada `Where`, `OrderBy`, `Skip` o `Take` le añade una pieza, y el SQL solo se envía al llamar a `ToListAsync()`, `CountAsync()`, `FirstOrDefaultAsync()`... (ejecución diferida). Paginar son **dos consultas**: un `COUNT` para el total y un `SELECT ... ORDER BY ... LIMIT ... OFFSET ...` para la página.

**Equivalente en Java:** `Pageable` + `Page<T>` de Spring Data (`findAll(PageRequest.of(pagina - 1, tamanio, Sort.by("id")))`), que hace las dos consultas por ti.

**Ejemplo real (`Repositories/PaginacionExtensions.cs`):**
```csharp
var total = await consulta.CountAsync(ct);
var elementos = await consulta
    .OrderBy(ordenarPor)
    .Skip((pagina - 1) * tamanio)
    .Take(tamanio)
    .ToListAsync(ct);
```
**Tres trampas:**
- `Skip` cuenta **filas a descartar desde 0**; la API cuenta **páginas desde 1**. De ahí `(pagina - 1) * tamanio`. Con `pagina * tamanio` la página 1 sale vacía (la caza de bugs del 15-oct).
- Sin `OrderBy`, SQL no garantiza el orden de las filas, y entre una página y otra pueden repetirse o saltarse filas (#180).
- El orden va **antes** de `Skip`/`Take`. En el SQL, `LIMIT`/`OFFSET` se aplican al resultado ya ordenado.

---

### Ordenar sobre un DTO: *could not be translated*

**Qué es:** si proyectas con `Select(r => new RegataDto(...))` **antes** de `OrderBy`/`Skip`/`Take`, el `OrderBy` ya no se aplica sobre una columna de la tabla, sino sobre una propiedad de un objeto construido con un constructor, y EF Core no sabe traducirlo a SQL. Lanza `InvalidOperationException: The LINQ expression ... could not be translated`, y la API responde 500.

**Equivalente en Java:** en JPQL/Criteria se ordena por la entidad (`ORDER BY r.id`), no por el DTO al que luego se mapea. La proyección a DTO es lo último que se hace.

**Ejemplo real (#175, `GET /api/regatas`):**
```csharp
// ❌ 500: OrderBy sobre RegataDto.Id, que no es una columna
consulta.Select(RegataMapper.ToDtoProjection).OrderBy(d => d.Id).Skip(...).Take(...)

// ✅ ordenar y paginar sobre la entidad, proyectar al final
consulta.OrderBy(r => r.Id).Skip(...).Take(...).Select(RegataMapper.ToDtoProjection)
```
Por eso `PaginarAsync` tiene una sobrecarga `<T, TResultado>` que recibe la proyección y la aplica **después de `Take`**.

**Por qué los tests no lo vieron:** `UseInMemoryDatabase` no traduce a SQL (ver «Lo que no prueba el proveedor InMemory»), así que el mismo `OrderBy` sobre el DTO pasaba en los tests y reventaba contra MySQL. Se detectó a mano en el navegador; lo cubrirá el #177 (Testcontainers).

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

### FluentAssertions: `.Should()`

**Qué es:** la librería de asserts del proyecto (`FluentAssertions` 6.12 en `MarinaApi.Tests.csproj`). `Should()` es un *extension method* (ver Extension methods, en Sintaxis C#) que se "pega" a cualquier valor y devuelve un objeto con los asserts propios de su tipo. Se lee como una frase: *"resultado.BarcoId debería ser 5"*.

**Equivalente en Java:** AssertJ, casi uno a uno: `assertThat(x).isEqualTo(5)` ↔ `x.Should().Be(5)`.

| FluentAssertions | AssertJ | Para qué |
|---|---|---|
| `x.Should().Be(5)` | `assertThat(x).isEqualTo(5)` | igualdad |
| `x.Should().BeNull()` / `NotBeNull()` | `isNull()` / `isNotNull()` | nulos |
| `lista.Should().HaveCount(3)` | `hasSize(3)` | tamaño |
| `lista.Should().BeEmpty()` | `isEmpty()` | vacía |
| `lista.Should().OnlyContain(t => t.BarcoNombre == "Aurora")` | `allMatch(...)` | todos cumplen |
| `dto.Should().BeEquivalentTo(esperado)` | `usingRecursiveComparison().isEqualTo(...)` | compara propiedad a propiedad |

**Por qué se usa en vez de `Assert.Equal` de xUnit:** el mensaje de error dice qué expresión falló y con qué valor. `Assert.Equal(5, resultado.BarcoId)` falla con `Expected: 5, Actual: 7`; `resultado.BarcoId.Should().Be(5)` falla con `Expected resultado.BarcoId to be 5, but found 7`.

**Ojo con la versión:** desde la v8 (enero de 2025), FluentAssertions es de pago para uso comercial. El proyecto usa la 6.12, que sigue siendo gratuita. En una empresa conviene preguntar qué versión o alternativa usan (por ejemplo, Shouldly, o los `Assert` de xUnit).

**Ejemplo real (`AssignBarcoServiceTests.cs`):**
```csharp
resultado.BarcoId.Should().Be(5);
amarre.BarcoId.Should().Be(7);   // #172: el barco que ya estaba no se ha tocado
```

---

### `FluentActions.Awaiting(...)` y `.Should().ThrowAsync<T>()`

**Qué es:** la forma de FluentAssertions de comprobar que un método **async** lanza una excepción. Se lee de dentro afuera:

1. **`() => _service.AssignBarcoAsync(1, dto)`**: una lambda que *todavía no se ejecuta*. Es la "acción" que se quiere probar, envuelta para que el assert decida cuándo lanzarla y pueda capturar lo que salga.
2. **`FluentActions.Awaiting(lambda)`**: marca esa acción como asíncrona (algo que hay que esperar con `await`). Para un método **síncrono**, el equivalente es `FluentActions.Invoking(() => ...)`.
3. **`.Should().ThrowAsync<ConflictException>()`**: ejecuta la acción, la espera y comprueba que termina con esa excepción (o una clase hija; para el tipo exacto, `ThrowExactlyAsync<T>()`). Si no lanza nada, o lanza otra cosa, el test falla.
4. **`await` delante de todo**: `ThrowAsync` devuelve un `Task`, y el assert ocurre *dentro* de ese `Task`.

**Equivalente en Java:**
```java
// JUnit 5
assertThrows(ConflictException.class, () -> service.assignBarco(1L, dto));
// AssertJ
assertThatThrownBy(() -> service.assignBarco(1L, dto)).isInstanceOf(ConflictException.class);
```
En Java no hace falta el `Awaiting` ni el `await` porque el método es síncrono. En C# el método devuelve un `Task`, y la excepción "viaja" dentro de él hasta que alguien lo espera.

**⚠️ La trampa:** si se olvida el `await` del principio, el test **pasa siempre**, lance o no la excepción. El `Task` con el assert se crea, nadie lo espera, y el test termina antes de comprobar nada. Es un test que no puede fallar. El compilador avisa con el warning **CS4014**: no lo ignores.

**Extras útiles:**
```csharp
// Comprobar también el mensaje (* = comodín)
await FluentActions.Awaiting(() => _service.AssignBarcoAsync(1, dto))
    .Should().ThrowAsync<ConflictException>()
    .WithMessage("*ya tiene asignado*");

// Comprobar que NO lanza nada
await FluentActions.Awaiting(() => _service.AssignBarcoAsync(1, dto))
    .Should().NotThrowAsync();
```

**Alternativa sin FluentAssertions (xUnit puro):** `await Assert.ThrowsAsync<ConflictException>(() => _service.AssignBarcoAsync(1, dto));`. Ojo: esta exige el tipo **exacto**, no acepta clases hijas.

**Ejemplo real (`AssignBarcoServiceTests.cs`, #172):**
```csharp
await FluentActions.Awaiting(() => _service.AssignBarcoAsync(1, dto))
    .Should().ThrowAsync<ConflictException>();
```

---

### `Times` en `Verify` (Moq)

**Qué es:** el segundo argumento de `Verify` dice **cuántas veces** debía haberse llamado al método:

| Moq | Mockito | Significado |
|---|---|---|
| `Times.Never` | `never()` | ninguna |
| `Times.Once` | `times(1)` | exactamente una |
| `Times.Exactly(3)` | `times(3)` | exactamente 3 |
| `Times.AtLeastOnce` | `atLeastOnce()` | 1 o más |
| `Times.AtMost(2)` | `atMost(2)` | 2 o menos |
| `Times.Between(1, 3, Range.Inclusive)` | — | entre 1 y 3 |

**⚠️ Diferencia con Mockito:** sin segundo argumento, `mock.Verify(r => r.UpdateAsync(...))` significa **al menos una vez** (`AtLeastOnce`), mientras que en Mockito `verify(mock).update(...)` significa **exactamente una** (`times(1)`). Para no depender de eso, en el proyecto se escribe siempre el `Times` explícito.

**Para qué sirve `Times.Never`:** comprobar que un camino de error **corta antes** de hacer efectos. En el #172 no basta con que lance `ConflictException`: además, `UpdateAsync` no debe llamarse, para que no se guarde nada en la base de datos.

**Ejemplo real (`AssignBarcoServiceTests.cs`):**
```csharp
_amarreRepositoryMock.Verify(r => r.UpdateAsync(amarre, It.IsAny<CancellationToken>()), Times.Once);    // caso feliz
_amarreRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Amarre>(), It.IsAny<CancellationToken>()), Times.Never);  // errores
```

---

### `MockBehavior`: mock *loose* frente a *strict* (Moq)

**Qué es:** cómo reacciona un mock cuando se llama a un método **sin `Setup`**:
- **`Loose`** (por defecto, `new Mock<T>()`): no protesta; devuelve el valor por defecto del tipo (`null`, `0`, `false`). Si el método devuelve `Task<T>`, devuelve un `Task` ya terminado con `default(T)`, normalmente `null`.
- **`Strict`** (`new Mock<T>(MockBehavior.Strict)`): cualquier llamada sin `Setup` lanza `MockException`. Obliga a configurar exactamente lo que el código va a usar.

**Equivalente en Java (Mockito):** un mock de Mockito también es *loose* para las llamadas sin configurar: devuelve `null`, `0` o una colección vacía. La "strictness" de Mockito (`STRICT_STUBS`, por defecto con `MockitoExtension`) es otra cosa: avisa de los `when(...)` que **no se usaron**. No hay un equivalente directo a `MockBehavior.Strict`.

**Por qué el proyecto usa `Loose`:**
- En el test del #172 se configuran `ExistsAsync` y `FindByBarcoIdAsync` aunque el servicio corte antes de llamarlos. Con `Loose` no molestan, y el test no depende del orden de las comprobaciones.
- Con `Strict` cada test se rompería al reordenar el código del servicio, aunque el comportamiento fuera el mismo (tests "frágiles").

**La trampa de `Loose`:** un método que se olvidó configurar devuelve `null` en silencio, y el fallo aparece lejos de la causa (un `NullReferenceException` en el servicio, o un test que falla sin motivo aparente). Pasó en el #179 con `FindAllAsync_MapeaTodasLasEntidadesADto`: el servicio empezó a llamar a `FindAllWithBarcoAsync`, que no tenía `Setup`, y Moq devolvía `null`.

---

### Cast de `null` en `ReturnsAsync`: `(Amarre?)null`

**Qué es:** `ReturnsAsync` tiene varias sobrecargas: una recibe el **valor** a devolver y otras reciben una **función** que lo calcula. Un `null` a secas encaja en todas, así que el compilador no sabe cuál elegir y da el error **CS0121** (*"The call is ambiguous between..."*). El cast le da un tipo concreto y elige la sobrecarga de "valor".

**Equivalente en Java:** en Mockito `thenReturn(null)` funciona sin cast, porque no hay sobrecargas que compitan de la misma forma. El concepto es el mismo que cuando en Java se escribe `metodo((String) null)` para deshacer una ambigüedad entre `metodo(String)` y `metodo(Integer)`.

**Formas equivalentes:**
```csharp
.ReturnsAsync((Amarre?)null);     // la que usa el proyecto
.ReturnsAsync(default(Amarre));   // default de un tipo por referencia = null
```

**Ejemplo real (`AssignBarcoServiceTests.cs`):** "el barco no tiene ningún amarre todavía":
```csharp
_amarreRepositoryMock.Setup(r => r.FindByBarcoIdAsync(5, It.IsAny<CancellationToken>()))
    .ReturnsAsync((Amarre?)null);
```

---

### Lo que no prueba el proveedor InMemory

**Qué es:** `UseInMemoryDatabase` guarda las entidades en memoria, sin SQL ni base de datos real. Es rápido para tests, pero:
- **No soporta transacciones.** `BeginTransactionAsync` lanza un error, salvo que se ignore el aviso `TransactionIgnoredWarning`. Por eso `RegataService.DeleteAsync` se quedó sin test en el #152.
- **No traduce a SQL.** Una consulta LINQ que MySQL no sabría ejecutar puede pasar en InMemory.
- **No tiene restricciones de la BD.** No aplica `ON DELETE`, FKs ni índices únicos: solo lo que hace EF en memoria.

**Equivalente en Java:** H2 en modo memoria en los tests de Spring. Con H2 se ejecuta SQL de verdad, pero en otro dialecto, así que también puede dar falsos verdes. La alternativa seria en los dos mundos es **Testcontainers** con el motor real (MySQL).

**Regla del proyecto:** un unit test con el repositorio mockeado prueba la lógica del Service, no la consulta. La consulta y el esquema (como el `ON DELETE SET NULL` del #154) se verifican contra MySQL: a mano en Swagger o con un test de integración.

---

### Mutation testing (la idea)

**Qué es:** una forma de comprobar si los tests sirven. Se cambia el código adrede (se invierte un `if`, un `==` pasa a `!=`, se borra una línea...) y se lanzan los tests. Si alguno se pone en rojo, el "mutante" ha muerto y los tests vigilan esa línea. Si todos siguen en verde, esa línea podría estar mal y nadie se daría cuenta.

**Herramientas:** **Stryker.NET** en .NET y **PIT** (pitest) en Java. Generan los mutantes automáticamente y dan un porcentaje de "mutantes muertos", mucho más fiable que la cobertura de líneas.

**Ejemplo real (#172, a mano):** Igor movió el `if` idempotente de `AssignBarcoAsync` al final del método, y el test `CuandoElMismoBarcoYaEstaEnEseAmarre` se puso en rojo (lo cazaba el `if` de "amarre ocupado"). Ese test mata ese mutante.

---

### Falso verde: un test que pasa por la razón equivocada

**Qué es:** un test que sale en verde aunque el código que dice probar esté mal, porque la condición que comprueba se cumple por otro motivo. Es peor que un test en rojo: da una seguridad que no existe. Con mocks *loose* es fácil caer en él, porque todo lo que no tiene `Setup` devuelve `null`/`false`/`0` sin avisar, y eso puede provocar la misma excepción que esperabas por otro camino.

**Equivalente en Java:** igual con Mockito, que también devuelve `null`/`false` por defecto. Un `assertThrows(NotFoundException.class, ...)` sin mirar el mensaje tiene el mismo riesgo.

**Ejemplo real (#174, `RegataServiceTests.cs`):** al añadir `ExistsAsync` de la regata a `DesinscribirBarcoAsync`, el test `CuandoBarcoNoExiste_LanzaNotFoundException` **siguió en verde**, pero ahora la `NotFoundException` era de la regata (el mock devolvía `false` en `ExistsAsync`), no del barco. Si alguien hubiera borrado la comprobación del barco, el test no se habría enterado.

**Dos defensas:**
1. **En cada test de error, existe todo menos lo que se prueba.** Si el test es "el barco no existe", la regata tiene que existir (`ExistsAsync → true`). Así el error solo puede venir de un sitio.
2. **Comprobar el mensaje, no solo el tipo:**
```csharp
await FluentActions.Awaiting(() => _service.DesinscribirBarcoAsync(1, 999))
    .Should().ThrowAsync<NotFoundException>()
    .WithMessage("*Barco*999*");   // * = comodín; en AssertJ, hasMessageContaining("Barco")
```

**Cómo detectarlo:** escribir el test en rojo **antes** del arreglo (TDD) y comprobar que el rojo dice lo esperado. En el #174, el test nuevo se escribió con el barco existente a propósito: sin eso habría salido en verde desde el principio, por la `NotFoundException` del barco.

**Relacionado (#174):** para afirmar que **no** pasa nada, `.Should().NotThrowAsync()` (`assertDoesNotThrow` en JUnit), y para comprobar el contenido de una colección, `barco.Regatas.Should().ContainSingle(r => r.Id == 2)`: exactamente un elemento y que cumple la condición (`assertThat(lista).singleElement().matches(...)` en AssertJ). Ver [[Guía definitiva de CSharp]] para el resto de aserciones.

**Relacionado (#155, #161):** un verde que no has visto pasar antes por rojo no prueba nada. En el #155 el «verde» eran resultados antiguos del panel de tests (el helper estaba pegado en la clase equivocada y no compilaba, `CS0103`). En el #161 el test positivo `Contenedor_NoTieneDependenciasCautivas` salía verde con `ValidateOnBuild` apagado por un `!` olvidado; solo lo delató el test negativo, que mete a propósito una dependencia cautiva. Comprueba siempre el total de tests (`total: N`) y el resumen final, no solo el color.

---

### `WebApplicationFactory`, `IClassFixture` y `WithWebHostBuilder`

**Qué es:** tres piezas para probar la aplicación **arrancada de verdad** dentro del proceso de test, sin abrir un puerto:
- `WebApplicationFactory<Program>` (paquete `Microsoft.AspNetCore.Mvc.Testing`) ejecuta el `Program.cs` real. `.Services` da acceso al contenedor de DI y `.CreateClient()` a un `HttpClient` contra la API. Requiere que `Program` sea accesible (`public partial class Program`).
- `IClassFixture<T>` (xUnit) crea **una** instancia de `T` por clase de tests y la inyecta en el constructor, para no arrancar la API en cada `[Fact]`.
- `WithWebHostBuilder(...)` devuelve una **fábrica derivada** con cambios (servicios extra, configuración) sin tocar la original: lo que se registra ahí solo existe en ese test.

**Equivalente en Java:** `@SpringBootTest` arranca el contexto real; Spring cachea el contexto entre tests con la misma configuración (el papel de `IClassFixture`); `@TestConfiguration` o `@MockBean` equivalen a `WithWebHostBuilder` + `ConfigureServices`.

**Ejemplo real (#161, `ContenedorDiTests.cs`):**
```csharp
public class ContenedorDiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ContenedorDiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public void Contenedor_ConUnSingletonQueRecibeUnScoped_FallaAlConstruirse()
    {
        var fabricaConCautiva = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddSingleton<ICacheCautiva, CacheCautiva>()));

        var act = () => fabricaConCautiva.Services;   // construye el contenedor

        act.Should().Throw<Exception>().WithMessage("*Cannot consume scoped service*");
    }
}
```
- Las clases de apoyo (`ICacheCautiva`, `CacheCautiva`) van `internal`: visibles solo dentro del proyecto de tests, que es el único que las usa (parecido a *package-private* en Java).
- Este test arranca el `Program.cs` real, y ahí `ServerVersion.AutoDetect` se conecta a MySQL: **necesita el contenedor `marina-mysql` encendido** (lo resuelve el #177, tests de integración).

---

### `DbContext` de pruebas que falla al guardar (y `MySqlException` por reflexión)

**Qué es:** el proveedor InMemory no produce errores de base de datos reales (ver «Lo que no prueba el proveedor InMemory»). Para probar código que **reacciona** a un fallo de la base de datos se hereda de `MarinaDbContext` y se sobrescribe `SaveChangesAsync` (es `virtual`) para que lance la excepción que se le pase. El test decide qué falla.

Hay un segundo obstáculo: `MySqlException` (MySqlConnector 2.3.5) **no tiene constructores públicos**. Se crea por reflexión, llamando al constructor interno `(MySqlErrorCode, string)`. Alternativas: un *wrapper* propio o un test de integración contra MySQL real (#177).

**Equivalente en Java:** `doThrow(new DataIntegrityViolationException("..."))` de Mockito sobre el repositorio. Allí la excepción de Spring sí tiene constructor público y no hace falta reflexión.

**Ejemplo real (#155, `AmarreRepositoryTests.cs`):**
```csharp
internal class DbContextQueFallaAlGuardar : MarinaDbContext
{
    private readonly Exception _excepcion;
    public DbContextQueFallaAlGuardar(DbContextOptions<MarinaDbContext> options, Exception excepcion)
        : base(options) => _excepcion = excepcion;

    public override Task<int> SaveChangesAsync(CancellationToken ct = default) => throw _excepcion;
}

private static MySqlException CrearMySqlException(MySqlErrorCode codigo, string mensaje) =>
    (MySqlException)Activator.CreateInstance(
        typeof(MySqlException), BindingFlags.NonPublic | BindingFlags.Instance,
        binder: null, args: new object[] { codigo, mensaje }, culture: null)!;
```
Tres tests con este montaje: un 1062 → `ConflictException`; un `DbUpdateException` sin `MySqlException` dentro → sale tal cual; y un `MySqlException` 1213 (deadlock) → sale tal cual. Los dos últimos protegen el filtro `{ Number: 1062 }`: sin ellos, un `catch` demasiado ancho pasaría desapercibido.

---

### Sembrar con un contexto y leer con otro

**Qué es:** si un test inserta los datos y luego consulta con el **mismo** `DbContext`, el *change tracker* ya tiene todas las entidades en memoria y **enlaza solo** sus relaciones (*fixup*). Un `Tripulante.Barco` aparece relleno aunque la consulta no tenga `Include`. El test sale verde por la razón equivocada. Para que pruebe el `Include` de verdad, se siembra con un contexto y se consulta con otro nuevo sobre la misma base InMemory.

**Equivalente en Java:** igual que la caché de primer nivel del `EntityManager`: en el mismo `EntityManager`, `em.find` devuelve la instancia ya cargada y no ejecuta SQL. Se hace `em.clear()` para forzar la lectura.

**Ejemplo real (`TripulanteRepositoryPaginacionTests.cs`, #175):**
```csharp
var options = new DbContextOptionsBuilder<MarinaDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())   // base única por test
    .Options;

using (var siembra = new MarinaDbContext(options))     // contexto 1: solo siembra
{
    siembra.Barcos.Add(new Barco { Id = 1, Nombre = "La Bestia", Tipo = "Velero" });
    siembra.Tripulantes.AddRange(/* ids 3, 1, 2 en desorden */);
    await siembra.SaveChangesAsync();
}

using var context = new MarinaDbContext(options);      // contexto 2: nuevo, sin fixup
var repository = new TripulanteRepository(context);
var (elementos, total) = await repository.FindPageWithBarcoAsync(1, 2);
elementos.Should().OnlyContain(t => t.Barco != null && t.Barco.Nombre == "La Bestia");
```
**Comprobado en rojo:** sin `.Include(t => t.Barco)` el test falla con `Barco = <null>, BarcoId = 1`; con él, pasa. El nombre único por test evita que unos tests vean los datos de otros.

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

### git branch -D tras un squash and merge

**Qué es:** con *squash and merge*, GitHub crea en `master` un commit **nuevo** (otro hash) con todo el contenido de la rama. Git compara hashes, no contenido, así que no reconoce tu rama local como mergeada: `git branch -d` (minúscula) se niega a borrarla y hay que usar `-D` (mayúscula, forzado). No se pierde nada, porque el contenido ya está en `master`.

**Ejemplo (PR #10, 2026-09-30):**
```powershell
git switch master
git pull
git branch -D fix/seguridad-secretos
git push origin --delete fix/seguridad-secretos   # o el botón "Delete branch" del PR
```

---

## Errores frecuentes (compilación y entorno)

Errores que ya han salido en el proyecto y que no son de lógica, sino de herramientas o de entorno. Cada uno costó tiempo la primera vez.

| Síntoma | Causa | Solución |
|---|---|---|
| **MSB3027** / MSB3021: *Could not copy ... The file is locked by: "MarinaApi (PID)"* | La API sigue arrancada y Windows bloquea su `.dll` | Parar la API (Ctrl+C) antes de `dotnet build`/`test`, o trabajar con `dotnet watch run` |
| **MSB1003**: *Specify a project or solution file* | `dotnet test` en la raíz de `Marina_C#`, donde no hay `.sln` | `dotnet test MarinaApi.Tests` (o crear la solución con `dotnet new sln` + `dotnet sln add`) |
| **CS7036**: *There is no argument given that corresponds to the required parameter* | Se añadió un parámetro a un `record` posicional y algún `new` no lo pasa. En un record posicional todo parámetro es obligatorio, aunque admita `null` | Pasarlo en todas las llamadas (normalmente en el Mapper) o darle valor por defecto |
| **CS0121**: *The call is ambiguous between...* con `ReturnsAsync(null)` | El `null` encaja en varias sobrecargas | `ReturnsAsync((Amarre?)null)` (ver Testing) |
| `DirectoryNotFoundException: ...\wwwroot` al arrancar la API | Manifiesto `*.staticwebassets.runtime.json` de **otra rama** (la del frontend, `feature/171-frontend-listados`) en `bin/` y `obj/`. Cambiar de rama no limpia `bin/` | `dotnet clean .\MarinaApi` y volver a arrancar (ha pasado tres veces) |
| `dotnet ef ...` falla con *Unable to connect to any of the specified MySQL hosts* | `ServerVersion.AutoDetect(...)` en `Program.cs` consulta la versión a MySQL al crear el `DbContext`, también en tiempo de diseño (`migrations add`) | `docker start marina-mysql` antes de cualquier comando `dotnet ef` |
| `dotnet watch` se para con `Contract.Fail` en `HotReloadMSBuildWorkspace` | Fallo interno de *hot reload* al crear archivos nuevos, no del código propio | Reiniciar `dotnet watch`. Para cambios solo en `wwwroot` basta `dotnet run` + F5 |
| `InvalidOperationException: ... could not be translated` (500 en un listado paginado) | `OrderBy`/`Skip` aplicados **después** de proyectar a un DTO con constructor | Ordenar y paginar sobre la entidad y proyectar al final (ver «Ordenar sobre un DTO»). InMemory no lo detecta |
| El JS falla con `tripulantes is not iterable` tras cambiar el JSON de la API | El navegador sigue con el `.js` antiguo en caché (esperaba una lista y ahora llega `{ elementos, ... }`) | `Ctrl+F5` y comprobar que el código lee `respuesta.elementos` |
| `curl` a `https://localhost:5000` da `SEC_E_INVALID_TOKEN` | El puerto 5000 habla HTTP, no HTTPS; el 5001 solo escucha con el perfil `https` | `curl.exe -i http://localhost:5000/api/...` |
| `git merge` o `git commit` abre un editor (Vim) con `MERGE_MSG` | Git pide confirmar el mensaje del merge commit | `Esc`, `:wq`, `Enter` (o `git merge --no-edit`) |
| `git switch` falla con *Your local changes to ... appsettings.Development.json would be overwritten* | El archivo sale modificado solo por el final de línea (CRLF/LF) o por `user-jwts` | `git restore MarinaApi/appsettings.Development.json` si no hay cambios reales; nunca va en un commit |

**Equivalente en Java:** el MSB3027 es el mismo bloqueo de archivos de Windows que hace fallar `mvn clean` con la aplicación arrancada. El problema de `bin/` y `obj/` equivale a un `target/` viejo, que se arregla con `mvn clean`.

---

## Scrum / Metodología

_(pendiente de primeras entradas)_
