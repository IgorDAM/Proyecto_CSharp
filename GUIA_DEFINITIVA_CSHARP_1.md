**Nivel:** Transición Java → C# (Intermedio)  
**Duración estimada:** 4-5 semanas, 5-8h/semana

---

# Índice

1. [[#Introducción]]
2. [[#Lección 1: Sintaxis básica para quien viene de Java|Lección 1: Sintaxis C#]]
3. [[#Lección 2: Colecciones y LINQ]]
4. [[#Lección 3: Async/Await y manejo de excepciones|Lección 3: Async/Await]]
5. [[#Mini-proyecto: Gestor de Biblioteca]]
6. [[#Lección 4: Generics]]
7. [[#Lección 5: Delegates y Eventos]]
8. [[#Lección 6: Patrones de Diseño]]
9. [[#Lección 7: SQL — Oracle y T-SQL|Lección 7: SQL]]
10. [[#Lección 8: Entity Framework Core|Lección 8: Entity Framework]]
11. [[#Lección 9: Records y Pattern Matching|Lección 9: Records y Pattern Matching]]
12. [[#Lección 10: ASP.NET Core Web API|Lección 10: ASP.NET Core Web API]]
13. [[#Lección 11: Testing con xUnit y Moq|Lección 11: Testing]]
14. [[#Lección 12: Logging y Configuración en .NET|Lección 12: Logging y Configuración]]
15. [[#Lección 13: Git Avanzado|Lección 13: Git]]
16. [[#Lección 14: Scrum y Agile|Lección 14: Scrum]]
17. [[#Lección 15: Frontend]]
18. [[#Lección 16: TypeScript]]
19. [[#Resumen Final]]

---

## Cómo leer esta guía

- Los bloques `> 💡 **Tip:**` son atajos, trucos y cosas que un compañero senior te diría por encima del hombro.
- Los bloques `> ⚠️ **Cuidado:**` son errores reales, de los que cuestan una tarde de depuración o un comentario en code review.
- Los bloques `> 🧠 **Mentalidad Java → C#:**` señalan el punto exacto donde tu intuición de Java te va a engañar.

No intentes memorizar la guía. Lee la lección, escribe el código de los ejemplos **a mano** (no copiar-pegar) y haz los ejercicios. Lo que no se teclea, no se aprende.

---

# Introducción

[[#Índice|↑ Volver al índice]]

Bienvenido a tu plan de capacitación en C# para las prácticas de DAM en Espiral MS. Este curso está diseñado específicamente para alguien que viene de Java y necesita estar preparado para un entorno profesional de .NET.

**Lo que lograrás:**
- Dominar las diferencias clave entre Java y C#
- Escribir código idiomático en C# (LINQ, async/await, records, pattern matching)
- Entender patrones empresariales (Repository, Dependency Injection, Strategy, Decorator)
- Construir y testear una API real con ASP.NET Core y Entity Framework Core
- Manejarte en el entorno de trabajo: Git en equipo, Scrum, logs y configuración

**Estructura del aprendizaje:**
- Semanas 1-2: Fundamentos de C# (sintaxis, colecciones, async)
- Semana 2: Proyecto integrador (juntar todo en algo real)
- Semana 3: Profundización (generics, delegates, patrones, records y pattern matching)
- Semana 4: El stack de la empresa (Web API, testing, logging y configuración) + entorno de equipo (Git, Scrum, frontend)

---

# Lección 1: Sintaxis básica para quien viene de Java

[[#Índice|↑ Volver al índice]]

## 1.1 Estructura básica de un programa

### Java
```java
public class Main {
    public static void main(String[] args) {
        System.out.println("Hola mundo");
    }
}
```

### C# (estilo clásico)
```csharp
using System;

namespace MiApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hola mundo");
        }
    }
}
```

### C# moderno (top-level statements, .NET 6+)
```csharp
Console.WriteLine("Hola mundo");
```

**Diferencias clave:**
- `System.out.println` → `Console.WriteLine`
- C# usa `namespace` en vez de paquetes
- Desde .NET 6, se permite código directo sin clase `Program` explícita
- El punto de entrada `Main` puede ser `static void Main()` o `static async Task Main()` si necesitas `await` desde el arranque

---

## 1.2 Tipos y variables

| Concepto | Java | C# |
|---|---|---|
| Inferencia de tipo | `var x = 5;` (Java 10+) | `var x = 5;` |
| Entero | `int` | `int` |
| Entero largo | `long` | `long` |
| Decimal | `double` | `double` |
| Decimal preciso | `BigDecimal` | `decimal` (recomendado para dinero) |
| Cadena | `String` | `string` (alias de `System.String`) |
| Carácter | `char` | `char` |
| Booleano | `boolean` | `bool` |
| Constante | `final int X = 5;` | `const int X = 5;` |
| Solo-lectura tras construcción | No existe directo | `readonly` |

### Value types vs Reference types

Esta es una distinción más marcada en C# que en Java:

- **Value types** (`int`, `double`, `bool`, `struct`, `enum`): se copian por valor. Al asignar, se duplica el dato.
- **Reference types** (`class`, `string`, `array`, `interface`): se copian por referencia. Al asignar, ambas variables apuntan al mismo objeto.

```csharp
// Value type: cada variable tiene su propia copia
int a = 5;
int b = a;
b = 10;
Console.WriteLine(a); // 5 (no cambia)

// Reference type: ambas apuntan al mismo objeto
var libro1 = new Libro { Titulo = "1984" };
var libro2 = libro1;
libro2.Titulo = "Otro título";
Console.WriteLine(libro1.Titulo); // "Otro título" (¡cambió también libro1!)
```

`decimal` merece mención aparte: en Java usarías `BigDecimal` para cálculos monetarios exactos (evitar errores de coma flotante). En C#, `decimal` es un tipo nativo del lenguaje pensado exactamente para eso — lo verás en cualquier campo de precio o importe en Espiral MS.

> 🧠 **Mentalidad Java → C#:** en Java, `==` sobre `String` compara referencias y por eso siempre usas `.equals()`. **En C# `==` sobre `string` compara el contenido**, porque `string` sobrecarga el operador. `"hola" == "hola"` es `true` sin sorpresas. Es de las poquísimas veces que C# es *menos* traicionero que Java, y casi nadie te lo cuenta.

> ⚠️ **Cuidado:** eso solo vale para `string`. Para tus propias clases, `==` sigue comparando referencias salvo que sobrecargues el operador o uses un `record` (ver [[#Lección 9: Records y Pattern Matching|Lección 9]]). Si quieres comparar por contenido, sobrescribe `Equals` y `GetHashCode` — **siempre los dos juntos**: si sobrescribes uno sin el otro, tu objeto se comportará mal dentro de un `Dictionary` o un `HashSet` y el bug será durísimo de encontrar.

> 💡 **Tip:** para comparar texto ignorando mayúsculas, **nunca** hagas `a.ToLower() == b.ToLower()` (crea dos strings nuevos en memoria y falla con idiomas como el turco). Usa `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`. Lo mismo con `Contains`, `StartsWith` y `EndsWith`: todos aceptan un `StringComparison` como segundo parámetro.

> 💡 **Tip de rendimiento:** concatenar strings en un bucle (`texto += algo;`) crea un objeto nuevo en cada vuelta, igual que en Java. Si vas a concatenar más de un puñado de veces, usa `StringBuilder` (idéntico al de Java). Para unir una colección, `string.Join(", ", lista)` es más rápido y más legible que cualquier bucle.

---

## 1.3 Propiedades — La diferencia más importante

En Java, para encapsular un campo usas getters/setters manuales:

```java
public class Persona {
    private String nombre;
    public String getNombre() { return nombre; }
    public void setNombre(String nombre) { this.nombre = nombre; }
}
```

En C#, esto se simplifica con **propiedades**:

```csharp
public class Persona
{
    public string Nombre { get; set; }
}
```

Uso:
```csharp
var p = new Persona();
p.Nombre = "Ana";
Console.WriteLine(p.Nombre);
```

### Propiedades con lógica personalizada

```csharp
public class Persona
{
    private string _nombre = string.Empty;

    public string Nombre
    {
        get { return _nombre; }
        set { _nombre = value.Trim(); } // "value" es una palabra reservada aquí
    }
}
```

### Propiedades de solo lectura y auto-inicializadas

```csharp
public class Libro
{
    // Solo se puede asignar en el constructor, nunca después
    public string Isbn { get; init; } = string.Empty;

    // Solo lectura calculada, sin campo de respaldo
    public string TituloCompleto => $"{Titulo} ({Anio})";
}
```

`init` (C# 9+) es útil para propiedades que deben fijarse al crear el objeto y no cambiar después — más estricto que `set` pero más flexible que un constructor obligatorio.

### `required` — obligar sin escribir un constructor

El problema de `init` es que nada obliga a rellenarlo: `new Libro()` compila aunque el ISBN quede vacío. Desde C# 11 existe `required`:

```csharp
public class Libro
{
    public required string Isbn { get; init; }   // el compilador EXIGE asignarlo al construir
    public required string Titulo { get; set; }
    public string? Autor { get; set; }           // opcional, puede quedar null
}

var ok  = new Libro { Isbn = "978-...", Titulo = "1984" }; // compila
// var ko = new Libro { Titulo = "1984" };                  // ERROR de compilación: falta Isbn
```

Es el equivalente moderno a tener un constructor con parámetros obligatorios, pero conservando la sintaxis de inicializador de objeto (mucho más legible cuando hay 8 propiedades).

> 💡 **Tip:** `required` + `init` es la combinación que usarás para DTOs y objetos de configuración: obligatorio al crear, inmutable después. Es la forma de conseguir en C# lo que en Java haces con un constructor largo o con el patrón Builder de Lombok — sin escribir ni una línea extra.

> ⚠️ **Cuidado:** una propiedad auto-implementada `{ get; set; }` no es un campo. Si en Java escribías `private String nombre;` y accedías directamente, en C# la convención es **siempre** exponer propiedades, nunca campos públicos. Un `public string Nombre;` (campo público) compila, pero es un *code smell* claro en code review: rompe la compatibilidad binaria, no se puede interceptar, y varios frameworks (EF Core, serializadores JSON, binding de ASP.NET) lo ignoran directamente. Si tus datos "desaparecen" al serializar a JSON, lo primero que hay que mirar es si son campos en vez de propiedades.

---

## 1.4 Nulabilidad

Java moderno usa `Optional<T>` para expresar "esto puede no tener valor". C# resuelve esto de forma distinta, con soporte nativo del lenguaje:

```csharp
// Nullable value types: value types normalmente no pueden ser null
int? edad = null;          // el "?" habilita null para un value type
if (edad.HasValue)
{
    Console.WriteLine(edad.Value);
}

// Nullable reference types (activado por defecto en proyectos nuevos)
string? nombre = null;     // el compilador avisa si lo usas sin comprobar

// Operador de coalescencia nula (??): valor por defecto si es null
string nombreFinal = nombre ?? "Sin nombre";

// Operador de navegación segura (?.): evita NullReferenceException
int? longitud = nombre?.Length;

// Combinando ambos: cadena de accesos seguros con valor por defecto
string resultado = persona?.Direccion?.Ciudad ?? "Ciudad desconocida";
```

**Por qué importa:** en Java, un `NullPointerException` puede aparecer en cualquier punto sin aviso previo del compilador. En C# con *nullable reference types* activado, el compilador te avisa en tiempo de compilación de posibles nulos no comprobados — mucho antes de llegar a producción.

> ⚠️ **Cuidado con el operador `!` (null-forgiving):** verás código como `var x = obtener()!;`. Ese `!` significa "compilador, cállate, yo sé que no es null". **No comprueba nada en tiempo de ejecución.** Si te equivocas, tienes exactamente el mismo `NullReferenceException` que en Java, pero ahora además habiendo silenciado el aviso. Úsalo solo cuando sepas algo que el compilador no puede saber (típicamente propiedades de navegación de EF Core: `public Cliente Cliente { get; set; } = null!;`), nunca para "quitar warnings molestos".

> 💡 **Tip:** `??=` asigna solo si la variable es null — `_cache ??= CargarDatos();` es el idioma habitual para inicialización perezosa, y evita el clásico `if (_cache == null) _cache = ...`.

> 💡 **Tip de code review:** `string.IsNullOrWhiteSpace(texto)` cubre de una sola vez null, cadena vacía y cadena de solo espacios. Es lo que se espera ver al validar entrada de usuario; `texto == null || texto.Length == 0` se considera código de principiante en C#.

---

## 1.5 Convenciones de nombres

- Java: métodos y variables en `camelCase`, clases en `PascalCase`
- C#: **todo lo público** (clases, métodos, propiedades) en `PascalCase`. Variables locales y parámetros en `camelCase`. Campos privados con `_` de prefijo.

```csharp
public class GestorPedidos
{
    private int _totalPedidos;          // campo privado: _camelCase

    public int TotalPedidos { get; set; } // propiedad pública: PascalCase

    public void ProcesarPedido(int idPedido) // método: PascalCase, parámetro: camelCase
    {
        int cantidadProcesada = 0;        // variable local: camelCase
    }
}
```

---

## 1.6 Interfaces y herencia

```csharp
public interface IRepositorio<T>
{
    T ObtenerPorId(int id);
}

public class RepositorioUsuarios : IRepositorio<Usuario>
{
    public Usuario ObtenerPorId(int id)
    {
        return new Usuario();
    }
}
```

**Nota de nomenclatura:** en C# las interfaces se prefijan con `I` por convención (`IRepositorio`, `IEnumerable`, `IDisposable`). En Java esto no es obligatorio, aunque algunos equipos lo adoptan igualmente.

### Herencia de clases

```csharp
public class Animal
{
    public string Nombre { get; set; } = string.Empty;
    public virtual void HacerSonido() => Console.WriteLine("...");
}

public class Perro : Animal // ":" en vez de "extends"
{
    public override void HacerSonido() => Console.WriteLine("Guau");
}
```

Diferencias con Java: `extends` → `:`, `@Override` → `override` (y el método base debe marcarse `virtual` explícitamente para poder sobrescribirse — en Java todos los métodos son sobrescribibles por defecto salvo `final`).

---

## 1.7 Control de flujo — pequeñas diferencias útiles

```csharp
// switch expression (C# 8+), mucho más compacto que switch clásico
string categoria = anio switch
{
    < 1900 => "Clásico",
    >= 1900 and < 2000 => "Siglo XX",
    _ => "Contemporáneo"
};

// pattern matching con "is"
if (objeto is Libro libro && libro.Anio > 2000)
{
    Console.WriteLine(libro.Titulo);
}

// range operator para arrays/listas
int[] numeros = { 1, 2, 3, 4, 5 };
int[] mitad = numeros[1..3]; // elementos en posiciones 1 y 2
int ultimo = numeros[^1];    // índice desde el final: 5 (equivale a numeros[numeros.Length - 1])
```

> 💡 **Tip:** `switch` expression obliga al compilador a comprobar que cubres todos los casos posibles. Si te falta el descarte `_` y el compilador no puede demostrar que has cubierto todo, te avisa. Un `switch` clásico con `default` olvidado no te avisa de nada. Prefiere siempre la versión expresión cuando el objetivo es **calcular un valor**; deja el `switch` clásico para cuando el objetivo es **ejecutar acciones**.

> ⚠️ **Cuidado:** en C# el `switch` clásico **no tiene fall-through**: cada `case` debe terminar en `break`, `return` o `goto case`. El compilador te obliga. Es lo contrario a Java, donde olvidar un `break` compila perfectamente y produce el bug clásico.

Este apartado es solo la superficie: el pattern matching de C# llega mucho más lejos (patrones de propiedad, de tipo, de lista, `when`). Se trata a fondo en la [[#Lección 9: Records y Pattern Matching|Lección 9]].

---

## 1.8 Ejercicios Lección 1

1. Crea una clase `Libro` con propiedades `Titulo`, `Autor` y `Anio`
2. Crea una interfaz `IPrestable` con un método `Prestar()`
3. Implementa la interfaz en `Libro`
4. Crea una lista de libros y recórrela con `foreach`
5. Añade una propiedad calculada `EsAntiguo` que devuelva `true` si `Anio < 1950`
6. Usa `switch` expression para clasificar libros por siglo (XIX, XX, XXI)
7. Practica `?.` y `??` con una propiedad `Autor` que pueda ser `null`

---

# Lección 2: Colecciones y LINQ

[[#Índice|↑ Volver al índice]]

## 2.1 Colecciones principales

| Java | C# | Uso |
|---|---|---|
| `ArrayList<T>` / `List<T>` | `List<T>` | Lista dinámica, la más usada |
| `HashMap<K,V>` | `Dictionary<K,V>` | Clave-valor, acceso O(1) |
| `HashSet<T>` | `HashSet<T>` | Elementos únicos, sin orden garantizado |
| `LinkedList<T>` | `LinkedList<T>` | Lista enlazada, inserciones rápidas en extremos |
| `T[]` | `T[]` | Array de tamaño fijo |
| `Queue<T>` | `Queue<T>` | Cola FIFO (primero en entrar, primero en salir) |
| `Stack<T>` (Deque) | `Stack<T>` | Pila LIFO (último en entrar, primero en salir) |
| `TreeMap<K,V>` | `SortedDictionary<K,V>` | Diccionario ordenado por clave |

### Dictionary en profundidad

```csharp
Dictionary<string, int> edades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Luis", 30 }
};

edades["Ana"] = 26;           // actualizar
edades["Carlos"] = 40;        // añadir nueva clave

// Acceso seguro sin lanzar excepción si no existe la clave
if (edades.TryGetValue("Pedro", out int edadPedro))
{
    Console.WriteLine(edadPedro);
}
else
{
    Console.WriteLine("Pedro no está en el diccionario");
}

foreach (var par in edades)
{
    Console.WriteLine($"{par.Key} tiene {par.Value} años");
}

// Solo claves o solo valores
foreach (var clave in edades.Keys) { /* ... */ }
foreach (var valor in edades.Values) { /* ... */ }
```

> ⚠️ **Cuidado (viniendo de Java):** `HashMap.get("noExiste")` devuelve `null`. **`Dictionary["noExiste"]` lanza `KeyNotFoundException`.** No devuelve null, revienta. Ese es el error número uno de quien llega de Java. Usa `TryGetValue` (como arriba), o `GetValueOrDefault(clave)` si te vale el valor por defecto del tipo.

> 💡 **Tip:** el patrón `if (dic.TryGetValue(k, out var v))` declara `v` en la propia condición y la deja disponible dentro del `if`. Ese `out var` en línea es idiomático en C# moderno; declarar la variable antes en una línea aparte es estilo antiguo.

> 💡 **Tip:** ¿necesitas comprobar si algo existe en una colección muchas veces? Un `List.Contains()` es O(n) — recorre la lista entera cada vez. Un `HashSet.Contains()` es O(1). Si tienes un bucle que hace `Contains` sobre una lista grande, convertirla a `HashSet` antes del bucle (`var set = lista.ToHashSet();`) puede llevar un proceso de minutos a milisegundos. Es uno de los arreglos de rendimiento más rentables y más fáciles de detectar en code review.

### Queue y Stack

```csharp
// Queue: útil para colas de procesamiento (ej: tareas pendientes)
Queue<string> colaTareas = new();
colaTareas.Enqueue("Tarea 1");
colaTareas.Enqueue("Tarea 2");
string siguiente = colaTareas.Dequeue(); // "Tarea 1"

// Stack: útil para deshacer/rehacer, o recorridos en profundidad
Stack<string> historial = new();
historial.Push("Página A");
historial.Push("Página B");
string ultima = historial.Pop(); // "Página B"
```

---

## 2.2 LINQ — Streams pero mejor

En Java usarías:
```java
List<String> nombres = personas.stream()
    .filter(p -> p.getEdad() > 18)
    .map(Persona::getNombre)
    .collect(Collectors.toList());
```

En C# tienes **dos sintaxis** para LINQ:

### Sintaxis de método (la que usarás casi siempre)
```csharp
var nombres = personas
    .Where(p => p.Edad > 18)
    .Select(p => p.Nombre)
    .ToList();
```

### Sintaxis de consulta (parecida a SQL, poco usada en la práctica)
```csharp
var nombres = (from p in personas
               where p.Edad > 18
               select p.Nombre).ToList();
```

La sintaxis de método es la estándar en código profesional de .NET — es la que verás en Espiral MS.

---

## 2.3 Métodos LINQ más usados

| Método | Equivalente Java | Ejemplo |
|---|---|---|
| `Where` | `.filter()` | `libros.Where(l => l.Anio > 2000)` |
| `Select` | `.map()` | `libros.Select(l => l.Titulo)` |
| `OrderBy` / `OrderByDescending` | `.sorted()` | `libros.OrderBy(l => l.Anio)` |
| `ThenBy` | `.thenComparing()` | `libros.OrderBy(l => l.Autor).ThenBy(l => l.Anio)` |
| `First` / `FirstOrDefault` | `.findFirst()` | `libros.FirstOrDefault(l => l.Autor == "Cervantes")` |
| `Single` / `SingleOrDefault` | — | Espera exactamente 1 resultado, lanza excepción si hay más de uno |
| `Any` | `.anyMatch()` | `libros.Any(l => l.Anio < 1700)` |
| `All` | `.allMatch()` | `libros.All(l => l.Autor != null)` |
| `Count` | `.count()` | `libros.Count(l => l.Anio > 1900)` |
| `Sum` / `Average` / `Max` / `Min` | `.sum()` / `.average()` | `libros.Sum(l => l.Anio)` |
| `GroupBy` | `.collect(groupingBy())` | `libros.GroupBy(l => l.Autor)` |
| `Distinct` | `.distinct()` | `libros.Select(l => l.Autor).Distinct()` |
| `Skip` / `Take` | `.skip()` / `.limit()` | `libros.Skip(10).Take(5)` (paginación) |
| `ToList` / `ToArray` / `ToDictionary` | `.collect(toList())` | `.ToList()` |

> ⚠️ **`First` vs `FirstOrDefault`, el error clásico:** `First()` lanza `InvalidOperationException` si no encuentra nada; `FirstOrDefault()` devuelve `null` (o `0`, o `default`). **No elijas `FirstOrDefault` solo para "que no pete"**: si el elemento *debe* existir, `First()` falla ruidosamente en el sitio exacto del problema, mientras que `FirstOrDefault()` te devuelve un null que explotará tres capas más arriba, donde ya no hay contexto para depurarlo. Regla: `FirstOrDefault` solo cuando *no encontrar nada* sea un caso legítimo que vas a tratar de verdad.

> 💡 **Tip:** `Single()` no es un `First()` más elegante. `Single()` recorre **toda** la secuencia para comprobar que no hay un segundo elemento, y lanza excepción si lo hay. Úsalo cuando quieras *afirmar* unicidad (buscar por clave primaria, por ejemplo); usa `First()` cuando solo te interese el primero.

> ⚠️ **Cuidado con `Count`:** si solo quieres saber si hay elementos, usa `.Any()`, no `.Count() > 0`. `Count()` puede recorrer la secuencia entera (y en Entity Framework lanza un `SELECT COUNT(*)` a la base de datos), mientras que `Any()` para en cuanto encuentra el primero. Sobre una `List<T>` la diferencia es despreciable; sobre una consulta a BD o una secuencia perezosa, es abismal.

> 💡 **Tip:** `.OrderBy()` en C# es un ordenamiento **estable** (los elementos con la misma clave conservan su orden relativo), igual que `Collections.sort` en Java. Puedes fiarte de eso al encadenar `ThenBy`.

---

## 2.4 Encadenando operaciones

```csharp
var resultado = libros
    .Where(l => l.Anio > 1900)
    .OrderByDescending(l => l.Anio)
    .Select(l => l.Titulo)
    .ToList();
```

### Paginación con Skip + Take (patrón muy común en APIs)

```csharp
int pagina = 2;
int tamanoPagina = 10;

var librosPagina = libros
    .OrderBy(l => l.Titulo)
    .Skip((pagina - 1) * tamanoPagina)
    .Take(tamanoPagina)
    .ToList();
```

### Agregaciones combinadas

```csharp
var estadisticas = new
{
    Total = libros.Count,
    AnioMasAntiguo = libros.Min(l => l.Anio),
    AnioMasReciente = libros.Max(l => l.Anio),
    PromedioAnio = libros.Average(l => l.Anio)
};
```

---

## 2.5 Ejecución diferida (deferred execution)

**Esto es crucial y suele confundir a quien viene de Java:** la mayoría de operadores LINQ (`Where`, `Select`, `OrderBy`) son *perezosos*. No se ejecutan hasta que alguien recorre el resultado.

```csharp
var libros2020 = libros.Where(l => l.Anio >= 2020); // NO filtra todavía, solo define "la receta"

libros.Add(new Libro { Anio = 2021 }); // si añades algo antes de iterar, SÍ se incluirá

foreach (var libro in libros2020) // aquí se ejecuta el filtro realmente
{
    Console.WriteLine(libro.Titulo);
}
```

Para forzar la ejecución inmediata (y "congelar" el resultado en ese momento), usa `.ToList()`, `.ToArray()` o `.Count()`:

```csharp
var libros2020 = libros.Where(l => l.Anio >= 2020).ToList(); // se ejecuta AHORA, resultado fijo
```

**Por qué importa con Entity Framework (Lección 8):** cuando trabajes con base de datos, la ejecución diferida significa que la consulta SQL real no se lanza hasta que iteras el resultado — puedes seguir añadiendo `.Where()` encima sin penalización, y EF Core combina todo en una sola consulta SQL optimizada.

### El coste oculto: re-enumerar sin darte cuenta

Esto es lo que la ejecución diferida tiene de peligroso, y casi nunca se explica:

```csharp
var caros = productos.Where(p => EsCaro(p)); // "receta", no resultado

Console.WriteLine(caros.Count());  // 1ª ejecución: recorre y evalúa EsCaro para cada producto
Console.WriteLine(caros.First());  // 2ª ejecución: vuelve a recorrer desde cero
foreach (var p in caros) { }       // 3ª ejecución: otra vez
```

**Ese código ejecuta el filtro tres veces.** Si `productos` es una consulta de Entity Framework, son **tres viajes a la base de datos**. Si `EsCaro` es un cálculo caro, lo pagas tres veces. Y nada en el código lo sugiere visualmente.

La regla práctica: **materializa una vez con `.ToList()` en cuanto vayas a usar el resultado más de una vez.**

```csharp
var caros = productos.Where(p => EsCaro(p)).ToList(); // una sola ejecución, resultado en memoria
Console.WriteLine(caros.Count);   // ojo: ahora es la PROPIEDAD Count, no el método Count()
Console.WriteLine(caros[0]);
foreach (var p in caros) { }
```

> ⚠️ **Cuidado:** el bug más desagradable de la ejecución diferida es la **captura de variable en un bucle**. Si construyes lambdas dentro de un `for` capturando la variable del bucle, todas comparten la misma variable. En `foreach` C# lo arregló hace años (cada iteración tiene su propia variable), pero en un `for` clásico sigue mordiendo.

### `IEnumerable<T>` vs `List<T>` como tipo de retorno

Esta decisión aparece en todos los code reviews, y merece entenderla bien:

| | `IEnumerable<T>` | `List<T>` / `IReadOnlyList<T>` |
|---|---|---|
| Qué promete | "Puedes recorrer esto" | "Esto ya está calculado en memoria" |
| Ejecución | Puede ser diferida | Ya materializada |
| `.Count` | Método `Count()`, puede recorrer todo | Propiedad `Count`, O(1) |
| Acceso por índice | No | Sí |
| Riesgo | Que el consumidor la recorra varias veces sin saberlo | Ninguno, pero fuerza materializar |

```csharp
// ❌ Peligroso: el consumidor no sabe que cada recorrido vuelve a tocar la BD
public IEnumerable<Libro> ObtenerDisponibles()
    => _contexto.Libros.Where(l => !l.Prestado);

// ✅ Honesto: el método promete un resultado ya calculado
public async Task<List<Libro>> ObtenerDisponiblesAsync()
    => await _contexto.Libros.Where(l => !l.Prestado).ToListAsync();
```

**Criterio que puedes defender en una revisión:**
- Si el método **consulta y devuelve un resultado terminado** (lo habitual en un repositorio o servicio): devuelve `List<T>` o `IReadOnlyList<T>`. Materializa dentro del método, donde controlas el contexto.
- Si el método es una **transformación intermedia** que se va a seguir componiendo, o genera elementos perezosamente (`yield return`): `IEnumerable<T>` es correcto y deseable.
- Nunca devuelvas `IEnumerable<T>` que por dentro sea una consulta a base de datos sin materializar: el `DbContext` puede estar ya cerrado cuando el consumidor intente iterarla, y obtendrás un `ObjectDisposedException` en un sitio que no tiene nada que ver con la causa.

> 💡 **Tip:** `IReadOnlyList<T>` es el mejor punto medio para devolver colecciones: garantiza que está materializada, permite `Count` y acceso por índice, y **comunica que el llamante no debe modificarla**. En Java devolverías `Collections.unmodifiableList(...)`; en C# basta con el tipo de retorno.

> 💡 **Tip:** `yield return` te permite escribir tus propios métodos perezosos, generando elementos uno a uno sin construir la lista completa en memoria. Es lo que hace `Where` por dentro:

```csharp
public static IEnumerable<int> Pares(IEnumerable<int> numeros)
{
    foreach (var n in numeros)
        if (n % 2 == 0)
            yield return n;   // devuelve este y "pausa" hasta que pidan el siguiente
}
```

---

## 2.6 Ejercicios Lección 2

Con una lista de al menos 5 libros:

1. Usa `Where` para obtener libros después de 1900
2. Usa `OrderBy` + `Select` para títulos ordenados por año
3. Usa `Any` para comprobar si existe algún libro sin autor
4. Usa `GroupBy` para agrupar por década (pista: `l.Anio / 10 * 10`)
5. Encadena `Where` + `OrderByDescending` + `Select` + `ToList`
6. Implementa paginación con `Skip` + `Take` (página de 2 elementos)
7. Calcula estadísticas: total, año más antiguo, año más reciente, promedio
8. Usa un `Dictionary<string, List<Libro>>` para agrupar libros por autor manualmente (sin GroupBy) y compara con la versión LINQ
9. **Demuestra la re-enumeración:** crea un método `bool EsAntiguo(Libro l)` que imprima "evaluando..." antes de devolver el resultado. Filtra con `Where(EsAntiguo)` sin `ToList()`, luego llama a `.Count()` y después recorre con `foreach`. Cuenta cuántos "evaluando..." salen por consola. Repite añadiendo `.ToList()` y compara.
10. Escribe un método con `yield return` que devuelva los libros de una lista de uno en uno, imprimiendo un mensaje en cada `yield`. Obsérva en qué momento exacto se imprime.

---

# Lección 3: Async/Await y manejo de excepciones

[[#Índice|↑ Volver al índice]]

## 3.1 Manejo de excepciones

```csharp
try
{
    int resultado = 10 / int.Parse("0");
}
catch (DivideByZeroException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Formato inválido: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error general: {ex.Message}");
}
finally
{
    Console.WriteLine("Siempre se ejecuta");
}
```

**Diferencias con Java:**
- No existen las "checked exceptions" — ningún método te obliga a declarar `throws`
- No hay obligación de declarar excepciones en la firma del método
- `throw new IllegalArgumentException("msg")` → `throw new ArgumentException("msg")`
- El orden de los `catch` importa: de más específico a más genérico (igual que en Java)

### Relanzar excepciones correctamente

```csharp
try
{
    ProcesarLibro();
}
catch (Exception ex)
{
    LoguearError(ex);
    throw;        // CORRECTO: conserva el stack trace original
    // throw ex;  // INCORRECTO: reinicia el stack trace, pierdes info de dónde falló
}
```

### Excepciones personalizadas

```csharp
public class LibroNoEncontradoException : Exception
{
    public LibroNoEncontradoException(string mensaje) : base(mensaje) { }
}

// Con datos adicionales
public class StockInsuficienteException : Exception
{
    public int StockDisponible { get; }

    public StockInsuficienteException(string mensaje, int stockDisponible) : base(mensaje)
    {
        StockDisponible = stockDisponible;
    }
}

// Uso
throw new StockInsuficienteException("No hay suficiente stock", stockDisponible: 2);
```

> 💡 **Tip:** ese `stockDisponible: 2` es un **argumento con nombre**. No existe en Java y es de las cosas que más mejora la legibilidad: `CrearPedido(cliente, urgente: true, notificar: false)` se entiende sin ir a mirar la firma. Úsalo siempre que pases un `bool` o un número "mágico" como parámetro — es lo primero que un revisor pide en C#.

> 💡 **Tip:** para validar argumentos, C# moderno tiene atajos que ahorran tres líneas cada vez:
> ```csharp
> ArgumentNullException.ThrowIfNull(libro);              // .NET 6+
> ArgumentException.ThrowIfNullOrWhiteSpace(titulo);      // .NET 8+
> ArgumentOutOfRangeException.ThrowIfNegative(cantidad);  // .NET 8+
> ```
> Además, estos capturan automáticamente el **nombre del parámetro** para el mensaje de error, sin que tengas que escribirlo a mano.

### Filtros de excepción (`when`) — no existen en Java

```csharp
try
{
    await LlamarApiAsync();
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    // solo entra aquí si es un 404
}
catch (HttpRequestException ex) when (ex.StatusCode >= HttpStatusCode.InternalServerError)
{
    // solo para errores 5xx: aquí sí tiene sentido reintentar
}
```

La diferencia respecto a hacer un `if` dentro del `catch` no es solo estética: si el filtro `when` devuelve `false`, **la pila no se desenrolla** y la excepción sigue buscando otro `catch` con su stack trace intacto. Con un `if` dentro del catch y un `throw;`, ya has "capturado y relanzado", y depurar es más difícil.

> ⚠️ **Cuidado — el antipatrón más penalizado en code review:** un `catch (Exception) { }` vacío, o un `catch` que solo hace `Console.WriteLine(ex.Message)` y sigue como si nada. Se llama *swallowing* y convierte un fallo detectable en un comportamiento incorrecto silencioso. Si capturas, o **resuelves** el problema, o lo **registras y relanzas**. Y captura siempre el tipo más específico que puedas: `catch (Exception)` genérico solo tiene sentido en la frontera de la aplicación (el middleware de errores de una API, por ejemplo).

> 💡 **Tip:** `ex.Message` es solo la punta del iceberg. En logs, registra `ex.ToString()` — incluye el tipo, el mensaje, el stack trace **y las excepciones internas** (`InnerException`), que es exactamente donde suele estar la causa real cuando EF Core o HttpClient envuelven un error.

### using — gestión automática de recursos

Equivalente a `try-with-resources` de Java:

```java
// Java
try (FileReader fr = new FileReader("archivo.txt")) {
    // usar fr
}
```

```csharp
// C# clásico
using (var fr = new StreamReader("archivo.txt"))
{
    // usar fr
} // se libera automáticamente al salir del bloque

// C# moderno (C# 8+), sin llaves, se libera al final del método/bloque contenedor
using var fr = new StreamReader("archivo.txt");
```

---

## 3.2 async/await

```csharp
public async Task<string> ObtenerDatosAsync()
{
    Console.WriteLine("Empezando...");
    await Task.Delay(2000); // simula una operación que tarda (ej: llamada a API)
    Console.WriteLine("Terminado");
    return "Datos obtenidos";
}

public static async Task Main()
{
    string resultado = await ObtenerDatosAsync();
    Console.WriteLine(resultado);
}
```

**Reglas clave:**
- Un método que usa `await` debe ser `async`
- Su retorno es `Task` (sin valor) o `Task<T>` (con valor de tipo T)
- `await` pausa la ejecución del método **sin bloquear el hilo** — el hilo queda libre para atender otras peticiones mientras espera
- Por convención, métodos async terminan en `Async` (`ObtenerDatosAsync`, `GuardarAsync`)

### ¿Qué es Task\<T\> exactamente?

`Task<T>` representa una operación en curso que eventualmente producirá un valor de tipo `T`. Es como un resguardo de tintorería: el resguardo (`Task`) es la promesa de que la ropa (`T`) estará lista; `await` es ir a recogerla cuando esté.

### Ejecutar tareas en paralelo con Task.WhenAll

```csharp
public async Task ProcesarVariosLibrosAsync()
{
    Task<Libro> tarea1 = ObtenerLibroAsync(1);
    Task<Libro> tarea2 = ObtenerLibroAsync(2);
    Task<Libro> tarea3 = ObtenerLibroAsync(3);

    // Las 3 operaciones se ejecutan EN PARALELO, no una tras otra
    Libro[] libros = await Task.WhenAll(tarea1, tarea2, tarea3);

    foreach (var libro in libros)
        Console.WriteLine(libro.Titulo);
}
```

Si hicieras `await` de cada una por separado (`await tarea1; await tarea2; await tarea3;`), se ejecutarían **secuencialmente**, tardando la suma de los tres tiempos en vez del máximo de ellos.

> ⚠️ **Cuidado — la trampa número uno de `Task.WhenAll`:** funciona porque las tareas se **lanzan** antes (`var t1 = ObtenerLibroAsync(1);` sin `await`). Si escribes `await Task.WhenAll(await A(), await B())`, ya has esperado cada una antes de llegar a `WhenAll` y no has paralelizado nada. Y si una tarea falla, `WhenAll` lanza solo **la primera** excepción, aunque hayan fallado varias: para verlas todas hay que inspeccionar `task.Exception.InnerExceptions`.

> ⚠️ **Cuidado con `Task.WhenAll` y Entity Framework:** un `DbContext` **no es thread-safe**. Lanzar varias consultas en paralelo sobre el mismo `DbContext` provoca el error *"A second operation was started on this context before a previous operation completed"*. Con EF Core, las consultas van secuenciales salvo que crees un contexto por tarea (con `IDbContextFactory`). Este error aparece constantemente en proyectos reales.

### Los tres pecados capitales de async

```csharp
// ❌ 1. .Result / .Wait() — bloquea el hilo y puede provocar DEADLOCK
var libro = ObtenerLibroAsync(1).Result;

// ✅ correcto
var libro = await ObtenerLibroAsync(1);
```

`.Result` y `.Wait()` bloquean el hilo mientras esperan. En ciertos contextos (interfaces gráficas, ASP.NET clásico) esto provoca un **deadlock** completo de la aplicación: el hilo bloqueado es justo el que la tarea necesita para terminar. La regla es **"async all the way"**: si algo es async, todo lo que lo llama debe ser async hasta arriba.

```csharp
// ❌ 2. async void — sus excepciones NO se pueden capturar, tumban el proceso
public async void ProcesarPedido() { ... }

// ✅ correcto
public async Task ProcesarPedidoAsync() { ... }
```

`async void` solo es aceptable en manejadores de eventos de UI (`button_Click`), donde la firma lo impone. En cualquier otro sitio es un bug esperando a ocurrir: nadie puede hacer `await` de él, y una excepción dentro no la captura ningún `try/catch` del llamante.

```csharp
// ❌ 3. async sin await — envolver algo síncrono en una Task para "parecer" async
public async Task<int> SumarAsync(int a, int b) => a + b;  // el compilador avisa

// ✅ si no hay I/O real, no lo hagas async
public int Sumar(int a, int b) => a + b;
```

> 🧠 **Mentalidad Java → C#:** `async/await` **no crea hilos**. Es fácil pensar que `await` "lanza esto en otro hilo" como haría un `ExecutorService`. No: `await` libera el hilo actual para que atienda otro trabajo mientras el sistema operativo se encarga de la operación de I/O, y lo retoma cuando termina. Por eso async brilla en I/O (base de datos, red, ficheros) y **no aporta nada** en cálculo puro de CPU — para eso sí existe `Task.Run`, que es lo más parecido a lanzar un hilo.

> 💡 **Tip:** si de verdad necesitas mover un cálculo pesado de CPU fuera del hilo actual, ese es el caso de `Task.Run(() => CalculoPesado())`. Pero en una API web no suele ser lo que quieres: el hilo sale del pool igualmente y no ganas capacidad de atención.

### Manejo de excepciones en código async

```csharp
public async Task<string> LlamarApiAsync()
{
    try
    {
        var resultado = await AlgunaOperacionAsync();
        return resultado;
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Fallo de red: {ex.Message}");
        throw; // relanza conservando el stack trace
    }
}
```

### CancellationToken — cancelar operaciones en curso

En aplicaciones reales (APIs web, por ejemplo), necesitas poder cancelar una operación async si el cliente cierra la conexión o pasa un timeout:

```csharp
public async Task<Libro> ObtenerLibroAsync(int id, CancellationToken token)
{
    token.ThrowIfCancellationRequested(); // comprobar si ya se pidió cancelar
    await Task.Delay(2000, token);         // Task.Delay también respeta el token
    return new Libro { Id = id };
}

// Uso con timeout de 5 segundos
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
try
{
    var libro = await ObtenerLibroAsync(1, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("La operación se canceló por timeout");
}
```

---

## 3.3 Por qué importa esto para la empresa

En Espiral MS, cuando accedas a base de datos con Entity Framework (Lección 8), verás patrones como este constantemente:

```csharp
public async Task<List<Cliente>> ObtenerClientesAsync()
{
    return await _contexto.Clientes
        .Where(c => c.Activo)
        .ToListAsync(); // ← el mismo LINQ que ya conoces, con Async + await
}
```

Es exactamente el LINQ de la Lección 2, combinado con lo que acabas de aprender aquí.

---

## 3.4 Ejercicios Lección 3

1. Crea `DividirAsync(int a, int b)` con `Task.Delay(1000)`
2. Llama desde `Main` con `try/catch` capturando `DivideByZeroException`
3. Prueba con valores válidos y con `b = 0`
4. Añade un `finally` que imprima "Operación finalizada"
5. Crea 3 métodos async independientes (por ejemplo, simulando 3 llamadas a servicios distintos) y ejecútalos en paralelo con `Task.WhenAll`, comparando el tiempo total con hacerlo secuencial
6. Crea una excepción personalizada `LibroNoDisponibleException` con una propiedad adicional `FechaDisponible`
7. Practica `using var` con un `StreamWriter` para escribir texto a un archivo
8. Escribe dos `catch` del mismo tipo de excepción diferenciados con filtros `when`, y comprueba cuál entra según el caso
9. **Reproduce el fallo de `.Result`:** llama a un método async con `.Result` desde `Main` y compara el comportamiento con `await`. Mide el tiempo de ambos con `Stopwatch`
10. Convierte un `async void` en `async Task` y comprueba, con un `try/catch` en el llamante, la diferencia: con `async void` la excepción no se captura

---

# Mini-proyecto: Gestor de Biblioteca

[[#Índice|↑ Volver al índice]]

## Estructura

```
GestorBiblioteca/
├── Program.cs
├── modelo/
│   └── Libro.cs
├── excepciones/
│   ├── LibroNoEncontradoException.cs
│   └── LibroYaPrestadoException.cs
└── repositorio/
    └── BibliotecaRepositorio.cs
```

## Modelo: Libro.cs

```csharp
namespace modelo
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Autor { get; set; }
        public int Anio { get; set; }
        public bool Prestado { get; set; } = false;

        public Libro(int id, string titulo, string? autor, int anio, bool prestado)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            Anio = anio;
            Prestado = prestado;
        }

        public Libro() { }

        public event Action<string>? LibroPrestado;

        public void NotificarPrestamo(string mensaje)
        {
            LibroPrestado?.Invoke(mensaje);
        }
    }
}
```

## Excepciones

```csharp
// excepciones/LibroNoEncontradoException.cs
namespace excepciones
{
    public class LibroNoEncontradoException : Exception
    {
        public LibroNoEncontradoException(string mensaje) : base(mensaje) { }
    }
}

// excepciones/LibroYaPrestadoException.cs
namespace excepciones
{
    public class LibroYaPrestadoException : Exception
    {
        public LibroYaPrestadoException(string mensaje) : base(mensaje) { }
    }
}
```

## Repositorio

```csharp
using modelo;
using excepciones;

namespace repositorio
{
    public class BibliotecaRepositorio
    {
        private List<Libro> _libros = new List<Libro>();

        public BibliotecaRepositorio()
        {
            _libros.Add(new Libro(1, "Cien años de soledad", "Gabriel García Márquez", 1967, false));
            _libros.Add(new Libro(2, "1984", "George Orwell", 1949, false));
            _libros.Add(new Libro(3, "El Principito", "Antoine de Saint-Exupéry", 1943, false));
            _libros.Add(new Libro(4, "Don Quijote", "Miguel de Cervantes", 1605, false));
            _libros.Add(new Libro(5, "Moby Dick", "Herman Melville", 1851, false));
            _libros.Add(new Libro(6, "La Odisea", "Homero", -800, false));
        }

        public List<Libro> ObtenerTodos() => _libros;

        public List<Libro> BuscarPorTitulo(string texto)
            => _libros.Where(l => l.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();

        public Libro ObtenerPorId(int id)
            => _libros.FirstOrDefault(l => l.Id == id) 
               ?? throw new LibroNoEncontradoException($"Libro con ID {id} no encontrado.");

        public async Task PrestarAsync(int id)
        {
            var libro = ObtenerPorId(id);
            if (libro.Prestado)
                throw new LibroYaPrestadoException($"El libro '{libro.Titulo}' ya está prestado.");

            await Task.Delay(500);
            libro.Prestado = true;
            libro.NotificarPrestamo($"El libro '{libro.Titulo}' ha sido prestado.");
        }

        public async Task DevolverAsync(int id)
        {
            var libro = ObtenerPorId(id);
            if (!libro.Prestado)
                throw new LibroYaPrestadoException($"El libro '{libro.Titulo}' no está prestado.");

            await Task.Delay(500);
            libro.Prestado = false;
        }

        public List<Libro> ObtenerDisponibles() 
            => _libros.Where(l => !l.Prestado).ToList();

        public List<Libro> ObtenerOrdenadosPorAnio() 
            => _libros.OrderBy(l => l.Anio).ToList();
    }
}
```

---

# Lección 4: Generics

[[#Índice|↑ Volver al índice]]

## 4.1 Generics "reificados"

En Java usan *type erasure*: en tiempo de ejecución, `List<String>` y `List<Integer>` son indistinguibles (ambas son solo `List`). En C# los generics son **reificados** — se mantiene la información del tipo en runtime:

```csharp
public static void MostrarTipo<T>(T valor)
{
    Console.WriteLine($"El tipo es: {typeof(T).Name}");
}

MostrarTipo(42);        // "El tipo es: Int32"
MostrarTipo("hola");    // "El tipo es: String"
```

Esto tiene una consecuencia práctica importante: en C#, `List<int>` no necesita autoboxing de cada elemento (a diferencia de Java, donde `List<Integer>` convierte cada `int` primitivo en un objeto `Integer`). Resultado: mejor rendimiento con tipos value type en colecciones genéricas.

> 🧠 **Mentalidad Java → C#:** olvídate de los wildcards `List<? extends Number>`. En C# la varianza se declara **en la interfaz**, no en cada uso: `IEnumerable<out T>` es covariante (por eso puedes asignar un `List<Perro>` a un `IEnumerable<Animal>`), y `Action<in T>` es contravariante. No tienes que escribir nada; simplemente funciona donde tiene sentido. Y ojo: `List<Perro>` **no** es un `List<Animal>`, porque `List<T>` no es covariante (si lo fuera, podrías meter un `Gato` en una lista de perros).

> 💡 **Tip:** `default(T)` te da el valor por defecto del tipo sin saber cuál es: `null` para clases, `0` para `int`, `false` para `bool`. En C# moderno puedes escribir solo `default` si el compilador puede inferir el tipo: `T resultado = default;`.

> ⚠️ **Cuidado:** dentro de un método genérico no puedes comparar con `==` de forma fiable si `T` no tiene restricción (el compilador no sabe si existe ese operador). Usa `EqualityComparer<T>.Default.Equals(a, b)`, que es lo que hacen internamente `List<T>.Contains` y `Dictionary<K,V>`.

---

## 4.2 Clase genérica

```csharp
public class Contenedor<T>
{
    private T _valor;

    public Contenedor(T valor)
    {
        _valor = valor;
    }

    public T ObtenerValor() => _valor;

    public void MostrarTipo()
    {
        Console.WriteLine($"Tipo: {typeof(T).Name}");
    }
}

// Uso
var contenedorTexto = new Contenedor<string>("Hola");
var contenedorNumero = new Contenedor<int>(42); // en Java sería Contenedor<Integer>
```

### Método genérico independiente

```csharp
public static T ObtenerPrimero<T>(List<T> lista)
{
    if (lista.Count == 0)
        throw new InvalidOperationException("La lista está vacía");
    return lista[0];
}

// El compilador infiere T automáticamente por el tipo del argumento
Libro primero = ObtenerPrimero(misLibros); // T = Libro, sin especificarlo explícitamente
```

---

## 4.3 Constraints

```csharp
public class Repositorio<T> where T : class, new()
{
    public T CrearNuevo() => new T();
}
```

**Constraints comunes:**

| Constraint | Significado |
|---|---|
| `where T : class` | T debe ser un tipo referencia |
| `where T : struct` | T debe ser un value type |
| `where T : new()` | T debe tener constructor público sin parámetros |
| `where T : IComparable<T>` | T debe implementar esa interfaz |
| `where T : Libro` | T debe ser `Libro` o una clase derivada |
| `where T : class, IEntidad, new()` | Se pueden combinar varios constraints |

### Ejemplo aplicado — repositorio genérico reutilizable

```csharp
public interface IEntidad
{
    int Id { get; set; }
}

public class RepositorioGenerico<T> where T : IEntidad
{
    private List<T> _elementos = new();

    public void Agregar(T elemento) => _elementos.Add(elemento);

    public T ObtenerPorId(int id)
    {
        return _elementos.FirstOrDefault(e => e.Id == id)
            ?? throw new InvalidOperationException($"Elemento {id} no encontrado.");
    }

    public List<T> ObtenerTodos() => new List<T>(_elementos);
}

// Uso: la misma clase sirve para Libro, Cliente, Pedido...
public class Libro : IEntidad
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
}

var repo = new RepositorioGenerico<Libro>();
```

Este patrón es exactamente la base sobre la que funciona `DbSet<T>` en Entity Framework (Lección 8) — un repositorio genérico que sabe operar con cualquier entidad.

> 💡 **Tip de diseño (nivel senior):** un repositorio genérico es tentador, pero no lo conviertas en dogma. Si `RepositorioGenerico<T>` acaba lleno de métodos que solo usa una entidad, o te obliga a exponer `IQueryable<T>` hacia fuera para poder consultar algo específico, ya no te está ahorrando trabajo: te está ocultando la intención. El patrón sano es **genérico para el CRUD trivial, repositorio específico para las consultas de negocio** (`ObtenerMorososConDeudaMayorQue(...)`). En Espiral MS verás las dos cosas conviviendo.

---

## 4.4 Múltiples parámetros de tipo

```csharp
public class Par<TClave, TValor>
{
    public TClave Clave { get; set; } = default!;
    public TValor Valor { get; set; } = default!;
}

var par = new Par<string, int> { Clave = "edad", Valor = 30 };
```

Esto es literalmente cómo está construido `Dictionary<TKey, TValue>` internamente.

---

## 4.5 Ejercicios Lección 4

1. Crea una interfaz `IEntidad` con propiedad `Id`
2. Haz que `Libro` implemente `IEntidad`
3. Crea `RepositorioGenerico<T> where T : IEntidad`
4. Implementa `Agregar(T)`, `ObtenerPorId(int)`, `ObtenerTodos()`
5. Instancia con `RepositorioGenerico<Libro>`
6. Añade un método genérico `Filtrar<T>(List<T> lista, Func<T, bool> criterio)` y pruébalo con distintos tipos
7. Crea una clase `Par<TClave, TValor>` y úsala para representar una estadística (ej: `Par<string, int>` para "autor" → "cantidad de libros")

---

# Lección 5: Delegates y Eventos

[[#Índice|↑ Volver al índice]]

## 5.1 ¿Qué es un delegate?

Un **delegate** es un tipo que representa una referencia a un método. En Java, lo más parecido son las interfaces funcionales (`Function<T,R>`, `Predicate<T>`) combinadas con lambdas.

```csharp
public delegate bool Criterio(Libro libro);

public static bool EsAntiguo(Libro libro) => libro.Anio < 1950;

Criterio delegado = EsAntiguo;
bool resultado = delegado(unLibro); // llama internamente a EsAntiguo
```

---

## 5.2 Func, Action y Predicate — predefinidos

En la práctica, casi nunca se crean delegates personalizados. Se usan los genéricos predefinidos del framework:

| Delegate | Firma | Ejemplo |
|---|---|---|
| `Action` | nada → nada | `Action saludar = () => Console.WriteLine("Hola");` |
| `Action<T>` | T → nada | `Action<string> imprimir = s => Console.WriteLine(s);` |
| `Func<T, TResult>` | T → TResult | `Func<int, int> cuadrado = x => x * x;` |
| `Func<T1, T2, TResult>` | T1, T2 → TResult | `Func<int, int, int> sumar = (a, b) => a + b;` |
| `Predicate<T>` | T → bool | `Predicate<Libro> antiguo = l => l.Anio < 1950;` |

### La conexión con LINQ

```csharp
public static List<T> Filtrar<T>(List<T> lista, Func<T, bool> criterio)
{
    return lista.Where(criterio).ToList();
}
```

`Func<T, bool>` es exactamente lo que recibe `.Where()` por debajo — cuando escribes `libros.Where(l => l.Anio > 2000)`, esa lambda ES un `Func<Libro, bool>`.

> ⚠️ **Cuidado — `Predicate<T>` vs `Func<T, bool>`:** son *funcionalmente* lo mismo pero son **tipos distintos** y no se convierten automáticamente. LINQ usa `Func<T, bool>`; `List<T>.FindAll` y `RemoveAll` usan `Predicate<T>`. Si te da un error raro de conversión, es casi seguro esto. En código nuevo, usa siempre `Func<T, bool>`: `Predicate<T>` es un resto histórico de .NET 2.0.

> 💡 **Tip (muy importante para Entity Framework):** hay una diferencia enorme entre `Func<Libro, bool>` y `Expression<Func<Libro, bool>>`. El primero es **código compilado**: solo se puede ejecutar. El segundo es un **árbol de expresión**: la lambda guardada como datos que se pueden *inspeccionar y traducir*. Por eso EF Core puede convertir `Where(l => l.Anio > 2000)` en un `WHERE Anio > 2000` de SQL. Si accidentalmente fuerzas un `Func` (por ejemplo llamando a `.AsEnumerable()` antes del `Where`), EF se trae **toda la tabla a memoria** y filtra en C#. La consulta sigue dando el resultado correcto y tarda mil veces más. Es uno de los problemas de rendimiento más comunes en proyectos .NET.

> 💡 **Tip:** una lambda que usa variables de su entorno se llama *closure*, y esas variables se mantienen vivas mientras la lambda exista. Si guardas lambdas en una lista o en un evento, estás manteniendo vivos también los objetos que capturan — una fuente clásica de fugas de memoria.

---

## 5.3 Delegates multicast

Un delegate puede apuntar a **varios métodos a la vez**, ejecutándose todos en orden al invocarlo:

```csharp
Action miAccion = () => Console.WriteLine("Primero");
miAccion += () => Console.WriteLine("Segundo");
miAccion += () => Console.WriteLine("Tercero");

miAccion(); // ejecuta los tres, en el orden en que se añadieron
```

Esto es la base técnica sobre la que funcionan los **eventos**.

---

## 5.4 Eventos

Un evento es un delegate especializado para el patrón publicador/suscriptor (Observer):

```csharp
public class GestorNotificaciones
{
    public event Action<string>? NotificacionEnviada;

    public void EnviarNotificacion(string mensaje)
    {
        NotificacionEnviada?.Invoke(mensaje);
    }
}

// Uso
var gestor = new GestorNotificaciones();
gestor.NotificacionEnviada += msg => Console.WriteLine($"Notificación: {msg}");
gestor.EnviarNotificacion("Hola"); // Dispara el evento
```

**¿Por qué `?.Invoke`?** Si nadie se ha suscrito todavía, el evento es `null`. El operador `?.` evita un `NullReferenceException` en ese caso.

### Diferencia entre un delegate público normal y un event

```csharp
public class Ejemplo
{
    public Action<string>? DelegatePublico;   // cualquiera puede hacer .Invoke() desde fuera
    public event Action<string>? EventoReal;   // solo la propia clase puede invocarlo
}
```

Declarar con `event` restringe: código externo solo puede suscribirse (`+=`) o desuscribirse (`-=`), nunca disparar el evento directamente. Es una protección de encapsulación que un delegate público normal no ofrece.

### Desuscribirse de un evento

```csharp
void ManejarNotificacion(string mensaje) => Console.WriteLine(mensaje);

gestor.NotificacionEnviada += ManejarNotificacion;
// ...más tarde...
gestor.NotificacionEnviada -= ManejarNotificacion; // deja de recibir notificaciones
```

**Nota:** solo puedes desuscribir con `-=` si guardaste una referencia nombrada al método — una lambda anónima (`msg => ...`) no se puede desuscribir después porque no tienes forma de referenciarla de nuevo.

> ⚠️ **Cuidado — la fuga de memoria de los eventos:** cuando `A` se suscribe a un evento de `B`, **`B` guarda una referencia a `A`**. Si `B` vive mucho (un servicio singleton, por ejemplo) y `A` debería morir, el recolector de basura no puede liberar `A` porque `B` lo sigue referenciando. Es la fuga de memoria clásica de .NET. Regla: **si te suscribes, planifica la desuscripción** (normalmente en `Dispose()`).

> ⚠️ **Cuidado:** si un suscriptor lanza una excepción, **los siguientes suscriptores no se ejecutan** — la excepción sube por la cadena desde el `Invoke()`. Si necesitas que todos se ejecuten pase lo que pase, hay que recorrer `GetInvocationList()` a mano y envolver cada llamada en su propio `try/catch`.

> 💡 **Tip:** la convención del framework para eventos "de verdad" no es `Action<string>`, sino `EventHandler<TEventArgs>`, con la firma `(object? sender, TEventArgs e)`. Con `Action<T>` el suscriptor no sabe **quién** disparó el evento; con `EventHandler<T>` sí. Para aprender, `Action<T>` está bien; en código de producción de la empresa verás `EventHandler<T>`.

---

## 5.5 En el mundo empresarial

- **Func/Action/Predicate**: constantemente en LINQ, callbacks, configuración de servicios y middlewares
- **Eventos**: comunes en interfaces gráficas (WinForms, WPF, Blazor) y en arquitecturas orientadas a eventos en backend (ej: "cuando se crea un pedido, notificar por email")

---

## 5.6 Ejercicios Lección 5

1. Crea `GestorNotificaciones` con evento `NotificacionEnviada`
2. Añade método `EnviarNotificacion(string mensaje)`
3. Suscribe 3 lambdas diferentes
4. Envía una notificación y verifica que se ejecutan todas
5. Aplica esto a `Libro`: evento `LibroPrestado`
6. Crea un método con nombre (no lambda) y practica suscribir/desuscribir con `+=` / `-=`
7. Diferencia en código un delegate público normal de un `event`, e intenta invocar el evento desde fuera de la clase (comprueba que el compilador no te deja)

---

# Lección 6: Patrones de Diseño

[[#Índice|↑ Volver al índice]]

## 6.1 ¿Por qué patrones de diseño?

Un patrón de diseño es una solución probada a un problema recurrente. No es código específico, es una **estructura reutilizable**. Beneficios: código más legible para el resto del equipo, más fácil de testear, y más fácil de modificar sin romper otras partes.

---

## 6.2 Repository Pattern

Abstrae el acceso a datos detrás de una interfaz, para que el resto del código no sepa (ni le importe) si los datos vienen de memoria, un archivo o una base de datos.

```csharp
public interface ILibroRepositorio
{
    List<Libro> ObtenerTodos();
    Libro ObtenerPorId(int id);
    Task PrestarAsync(int id);
}

public class BibliotecaRepositorio : ILibroRepositorio
{
    // Implementación (en memoria, en tu proyecto actual)
}

// Uso: el código consumidor depende de la interfaz, no de la implementación concreta
ILibroRepositorio repo = new BibliotecaRepositorio();
```

**Ventaja real:** si mañana cambias de "datos en memoria" a "SQL Server con Entity Framework", solo creas `BibliotecaRepositorioEF : ILibroRepositorio` y cambias una línea en `Program.cs`. Ningún otro archivo se entera del cambio.

---

## 6.3 Dependency Injection (DI)

Pasar a un objeto lo que necesita (sus dependencias) por constructor, en vez de que él mismo las cree.

### Mal (acoplado)
```csharp
public class GestorPrestamos
{
    private BibliotecaRepositorio _repo = new(); // crea su propia dependencia, acoplamiento fuerte

    public async Task Prestar(int id)
    {
        await _repo.PrestarAsync(id);
    }
}
```

### Bien (inyectado)
```csharp
public class GestorPrestamos
{
    private readonly ILibroRepositorio _repo; // depende de la interfaz, no de una clase concreta

    public GestorPrestamos(ILibroRepositorio repo) // inyección por constructor
    {
        _repo = repo;
    }

    public async Task Prestar(int id)
    {
        await _repo.PrestarAsync(id);
    }
}

// Uso
ILibroRepositorio repo = new BibliotecaRepositorio();
var gestor = new GestorPrestamos(repo);
```

Ahora `GestorPrestamos` funciona con **cualquier** implementación de `ILibroRepositorio` — incluida una falsa para tests.

### Los 3 "lifetimes" de DI en ASP.NET Core (lo que verás en las prácticas)

Cuando llegues a ASP.NET Core, registrarás las dependencias con `builder.Services.Add...`, y elegirás uno de estos tres ciclos de vida:

| Lifetime | Comportamiento | Cuándo usarlo |
|---|---|---|
| `AddTransient<T>` | Nueva instancia cada vez que se solicita | Servicios ligeros, sin estado |
| `AddScoped<T>` | Una instancia por petición HTTP | El más común — ej: `DbContext` |
| `AddSingleton<T>` | Una única instancia para toda la app | Configuración, caché compartida |

```csharp
// Ejemplo de cómo se verá en Program.cs de un proyecto ASP.NET Core real
builder.Services.AddScoped<ILibroRepositorio, BibliotecaRepositorio>();
builder.Services.AddScoped<GestorPrestamos>();
```

Esto automatiza exactamente lo que hicimos a mano arriba: cuando algo pida un `ILibroRepositorio`, el framework crea automáticamente un `BibliotecaRepositorio` y lo inyecta.

> ⚠️ **Cuidado — *captive dependency*, el bug de DI que más cuesta encontrar:** si un servicio **Singleton** recibe por constructor un servicio **Scoped** (por ejemplo un `DbContext`), ese `DbContext` queda "atrapado" y vive para siempre, compartido entre todas las peticiones de todos los usuarios. Resultado: datos de un usuario que aparecen en la sesión de otro, o un `ObjectDisposedException` aleatorio. **Regla mental: un servicio nunca debe depender de otro con un ciclo de vida más corto que el suyo.** En .NET moderno, `builder.Build()` en desarrollo detecta muchos de estos casos y falla al arrancar — no desactives esa validación.

> 💡 **Tip:** ¿cuál elegir cuando dudas? `AddScoped` es el valor por defecto sensato para casi todo (servicios, repositorios, `DbContext`). `AddSingleton` solo para cosas realmente sin estado o inmutables (configuración, cachés pensadas para ser compartidas, clientes HTTP). `AddTransient` para objetos muy ligeros y de usar y tirar. Ante la duda: Scoped.

> 💡 **Tip de code review:** si un constructor recibe 8 dependencias, el problema no es DI — es que esa clase hace demasiadas cosas. Un constructor sobrecargado es el detector de violaciones de responsabilidad única más fiable que existe, y es literalmente lo primero que mira un revisor con experiencia.

> ⚠️ **Cuidado:** inyectar `IServiceProvider` y pedirle servicios a mano (`provider.GetService<X>()`) se llama *Service Locator* y se considera un antipatrón: oculta las dependencias reales de la clase, que ya no se ven en el constructor, y hace los tests mucho más difíciles. Se usa solo en casos muy concretos (factorías, servicios en segundo plano que crean su propio scope).

---

## 6.4 Factory Pattern

Una factory es un método (o clase) que crea objetos por ti, encapsulando la lógica de construcción cuando esta es compleja.

```csharp
public static class BibliotecaFactory
{
    public static GestorPrestamos CrearGestor()
    {
        var repo = new BibliotecaRepositorio();
        return new GestorPrestamos(repo);
    }
}

// Uso
var gestor = BibliotecaFactory.CrearGestor();
```

En ASP.NET Core moderno, esto se automatiza con el contenedor de DI (visto arriba), pero el concepto de "encapsular la construcción compleja de un objeto" sigue siendo el mismo.

---

## 6.5 Singleton Pattern

Garantiza que solo existe **una instancia** de una clase en toda la aplicación.

```csharp
public class Configuracion
{
    public static Configuracion Instancia { get; } = new();

    private Configuracion() { } // constructor privado: nadie puede hacer "new" desde fuera

    public string ObtenerRutaBaseDatos() => "Server=...";
}

// Uso
var config = Configuracion.Instancia; // siempre el mismo objeto
```

**Cuidado:** los singletons manuales pueden complicar los tests unitarios (estado compartido global). En ASP.NET Core, se prefiere `AddSingleton<T>` con DI, que resuelve este problema porque el propio framework gestiona el ciclo de vida y puede sustituirlo por un mock en tests.

> ⚠️ **Cuidado:** un Singleton **debe ser thread-safe**. Si guarda estado mutable (un `Dictionary` que se va llenando, un contador), varias peticiones simultáneas lo tocarán a la vez y tendrás corrupción de datos o excepciones esporádicas imposibles de reproducir. Si necesitas estado compartido mutable, usa las colecciones concurrentes: `ConcurrentDictionary<K,V>` en vez de `Dictionary<K,V>`.

---

## 6.6 Strategy y Decorator — dos patrones que verás sin que nadie los nombre

### Strategy: intercambiar el algoritmo, no el objeto

Cuando tienes un `if/else` o un `switch` largo eligiendo *cómo* hacer algo, casi siempre es un Strategy disfrazado.

```csharp
// ❌ Antes: cada nuevo tipo de descuento obliga a tocar este método
public decimal CalcularDescuento(Pedido p, string tipoCliente)
{
    if (tipoCliente == "VIP") return p.Total * 0.20m;
    if (tipoCliente == "Empleado") return p.Total * 0.35m;
    return 0m;
}

// ✅ Después: una estrategia por caso, cerradas a modificación
public interface IEstrategiaDescuento
{
    string TipoCliente { get; }
    decimal Calcular(Pedido pedido);
}

public class DescuentoVip : IEstrategiaDescuento
{
    public string TipoCliente => "VIP";
    public decimal Calcular(Pedido pedido) => pedido.Total * 0.20m;
}

public class DescuentoEmpleado : IEstrategiaDescuento
{
    public string TipoCliente => "Empleado";
    public decimal Calcular(Pedido pedido) => pedido.Total * 0.35m;
}

public class CalculadoraDescuentos
{
    private readonly IEnumerable<IEstrategiaDescuento> _estrategias;

    // ASP.NET Core inyecta AUTOMÁTICAMENTE todas las implementaciones registradas
    public CalculadoraDescuentos(IEnumerable<IEstrategiaDescuento> estrategias)
        => _estrategias = estrategias;

    public decimal Calcular(Pedido pedido, string tipoCliente)
        => _estrategias.FirstOrDefault(e => e.TipoCliente == tipoCliente)?.Calcular(pedido) ?? 0m;
}
```

```csharp
// Registro: varias implementaciones de la MISMA interfaz
builder.Services.AddScoped<IEstrategiaDescuento, DescuentoVip>();
builder.Services.AddScoped<IEstrategiaDescuento, DescuentoEmpleado>();
```

> 💡 **Tip:** ese truco de inyectar `IEnumerable<IInterfaz>` para recibir **todas** las implementaciones registradas es una de las cosas más útiles y menos conocidas del contenedor de DI de .NET. Añadir un descuento nuevo pasa a ser crear una clase y registrarla: cero modificaciones en código existente (principio abierto/cerrado, en la práctica).

En un caso tan simple como este, un `switch` expression es perfectamente aceptable y más corto. Strategy gana cuando cada rama tiene lógica real, dependencias propias, o hay que testearlas por separado. **Elegir mal aquí es el error típico del que acaba de aprender patrones: aplicarlos donde no hacen falta complica más de lo que ordena.**

### Decorator: añadir comportamiento sin tocar la clase original

Un decorador implementa la misma interfaz que decora, y envuelve a la instancia real:

```csharp
public class LibroRepositorioConCache : ILibroRepositorio
{
    private readonly ILibroRepositorio _interno;          // el repositorio de verdad
    private readonly Dictionary<int, Libro> _cache = new();

    public LibroRepositorioConCache(ILibroRepositorio interno) => _interno = interno;

    public Libro ObtenerPorId(int id)
    {
        if (_cache.TryGetValue(id, out var libro))
            return libro;                                  // servido desde caché

        libro = _interno.ObtenerPorId(id);                 // delega en el real
        _cache[id] = libro;
        return libro;
    }

    public List<Libro> ObtenerTodos() => _interno.ObtenerTodos();
    public Task PrestarAsync(int id) => _interno.PrestarAsync(id);
}
```

El consumidor sigue pidiendo un `ILibroRepositorio` y no se entera de nada. Así se añaden caché, logging, reintentos o métricas **sin modificar ni una línea** de la implementación original.

> 💡 **Tip:** el middleware de ASP.NET Core (Lección 10) es literalmente el patrón Decorator aplicado a la petición HTTP: cada middleware envuelve al siguiente. Reconocer el patrón hace que el pipeline deje de parecer magia.

---

## 6.7 Mock para testing

```csharp
public class RepositorioMock : ILibroRepositorio
{
    private List<Libro> _libros = new()
    {
        new Libro(1, "Test Book", "Test Author", 2020, false)
    };

    public List<Libro> ObtenerTodos() => _libros;
    public Libro ObtenerPorId(int id) => _libros.First(l => l.Id == id);
    public async Task PrestarAsync(int id)
    {
        await Task.Delay(10); // sin latencia real, tests rápidos
        _libros.First(l => l.Id == id).Prestado = true;
    }
}

// En un test
ILibroRepositorio repo = new RepositorioMock();
var gestor = new GestorPrestamos(repo);
await gestor.Prestar(1);
// se puede comprobar el resultado sin tocar una base de datos real
```

Esto es lo que hace posible testear `GestorPrestamos` sin necesitar una base de datos de verdad — el corazón de por qué Repository + DI se usan juntos casi siempre.

### Moq: la versión "de verdad" de este ejemplo

El `RepositorioMock` de arriba sirve para entender el concepto, pero escribir una clase fake a mano por cada interfaz que se quiera testear no escala. En la práctica (y en este mismo proyecto, MarinaApi) se usa una **librería de mocking**, Moq, que genera el fake automáticamente y permite configurar su comportamiento por test:

```csharp
var repoMock = new Mock<ILibroRepositorio>();
repoMock.Setup(r => r.ObtenerPorId(1))
    .Returns(new Libro(1, "Test Book", "Test Author", 2020, false));

var gestor = new GestorPrestamos(repoMock.Object);
// gestor.ObtenerPorId(1) devuelve el libro configurado arriba,
// sin necesitar una clase RepositorioMock escrita a mano
```

- `new Mock<ILibroRepositorio>()` genera el fake en tiempo de ejecución — equivalente a escribir `RepositorioMock`, pero sin código.
- `.Setup(...).Returns(...)` configura la respuesta **por test**, en vez de tener una única implementación fake fija compartida por todos los tests.
- `repoMock.Object` es el objeto fake real, el que se le pasa al constructor — igual que antes se pasaba una instancia de `RepositorioMock`.

Sigue siendo el mismo patrón (interfaz + implementación de test intercambiable vía DI); Moq solo genera el fake por ti y te deja configurarlo con más precisión (`Verify` para comprobar que se llamó a un método, `Setup` distinto en cada test, etc.).

> Ejemplo completo, línea a línea, con los tests reales de MarinaApi (`Mock<T>`, `Setup`, `ReturnsAsync`, `Verify`, mockear interfaz vs. clase concreta, y cuándo NO hace falta mock): ver **Cap. 8.5 "Mock Tests en profundidad"** en `MIGRACION_JAVA_A_CSHARP.md`.

---

Esta lección te da el patrón; la [[#Lección 11: Testing con xUnit y Moq|Lección 11]] te da el marco de tests completo donde encaja.

---

## 6.8 Cómo se combinan estos patrones en la práctica

```
Program.cs (composición)
    │
    ├─ registra: ILibroRepositorio → BibliotecaRepositorio  (Repository)
    ├─ registra: GestorPrestamos                             (recibe DI automática)
    │
    └─ en tests: ILibroRepositorio → RepositorioMock          (mismo contrato, otra implementación)
```

En Espiral MS verás esta combinación constantemente: interfaz + implementación real + implementación de test, todas intercambiables gracias a DI.

---

## 6.9 Ejercicios Lección 6

1. Crea `ILibroRepositorio` con todos los métodos
2. Haz que `BibliotecaRepositorio` lo implemente
3. Crea `GestorPrestamos` con inyección de DI
4. Crea `RepositorioMock` para testing
5. Refactoriza `Program.cs` para usar patrones
6. Crea una `BibliotecaFactory` que construya un `GestorPrestamos` completamente configurado
7. Implementa un `Configuracion` como Singleton con al menos 2 propiedades (ej: nombre de la app, versión)
8. Escribe un pequeño programa que use `RepositorioMock` en vez de `BibliotecaRepositorio` y verifica que `GestorPrestamos` funciona igual con ambos
9. Convierte un `if/else` de cálculo de recargo por retraso en tres implementaciones de `IEstrategiaRecargo` e inyéctalas todas con `IEnumerable<IEstrategiaRecargo>`
10. Escribe un decorador `LibroRepositorioConLog : ILibroRepositorio` que imprima por consola cada método invocado antes de delegar en el repositorio real. Comprueba que `GestorPrestamos` funciona sin cambiar ni una línea

---

# Lección 7: SQL — Oracle y T-SQL

[[#Índice|↑ Volver al índice]]

## 7.1 DDL — Crear la estructura de la base de datos

```sql
CREATE TABLE Clientes (
    IdCliente INT PRIMARY KEY IDENTITY(1,1),  -- IDENTITY = auto-incremento (T-SQL)
    NombreCliente NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Pais NVARCHAR(50) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Pedidos (
    IdPedido INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT NOT NULL,
    FechaPedido DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(10, 2) NOT NULL DEFAULT 0,
    FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente)
);

-- Modificar una tabla existente
ALTER TABLE Clientes ADD Telefono NVARCHAR(20);
ALTER TABLE Clientes DROP COLUMN Telefono;

-- Eliminar una tabla
DROP TABLE Pedidos;
```

En Oracle, el equivalente de `IDENTITY` es una `SEQUENCE` combinada con un `TRIGGER`, o directamente `GENERATED ALWAYS AS IDENTITY` en versiones modernas (Oracle 12c+).

---

## 7.2 DML — Manipular datos

```sql
-- INSERT
INSERT INTO Clientes (NombreCliente, Email, Pais, Activo)
VALUES ('Ana García', 'ana@example.com', 'España', 1);

-- UPDATE
UPDATE Clientes
SET Activo = 0
WHERE IdCliente = 5;

-- DELETE
DELETE FROM Clientes WHERE Activo = 0 AND FechaRegistro < '2020-01-01';
```

> ⚠️ **Cuidado — la regla que te salvará el puesto:** antes de ejecutar un `UPDATE` o un `DELETE`, escribe primero la consulta como `SELECT * FROM ... WHERE ...` con **exactamente el mismo WHERE**, míralo, y solo entonces cambia el `SELECT *` por el `UPDATE`/`DELETE`. Un `WHERE` mal escrito (o directamente olvidado) afecta a la tabla entera. Si estás en una base de datos compartida, envuélvelo además en una transacción explícita para poder hacer `ROLLBACK`:
> ```sql
> BEGIN TRANSACTION;
>   UPDATE Clientes SET Activo = 0 WHERE IdCliente = 5;
>   -- comprueba el número de filas afectadas antes de decidir
> ROLLBACK;  -- o COMMIT; si es correcto
> ```

> 💡 **Tip:** `NULL` no es igual a nada, ni siquiera a sí mismo. `WHERE Email = NULL` **nunca** devuelve filas, aunque haya emails nulos. Hay que escribir `WHERE Email IS NULL`. Y ojo con `NOT IN (subconsulta)`: si la subconsulta devuelve un solo `NULL`, el resultado completo es vacío. Por eso en código profesional se prefiere `NOT EXISTS`, que no tiene ese problema.

---

## 7.3 Consultas básicas

```sql
SELECT * FROM Clientes WHERE Activo = 1 ORDER BY Nombre;

SELECT Nombre, Email FROM Clientes WHERE Pais = 'España';

SELECT COUNT(*) AS Total FROM Clientes;

-- Combinando condiciones
SELECT * FROM Clientes
WHERE Pais = 'España' AND Activo = 1 AND FechaRegistro >= '2025-01-01';

-- LIKE para búsquedas parciales
SELECT * FROM Clientes WHERE NombreCliente LIKE 'Ana%';  -- empieza por "Ana"
SELECT * FROM Clientes WHERE Email LIKE '%@gmail.com';    -- termina en "@gmail.com"

-- IN para listas de valores
SELECT * FROM Clientes WHERE Pais IN ('España', 'México', 'Argentina');

-- BETWEEN para rangos
SELECT * FROM Pedidos WHERE Total BETWEEN 50 AND 200;
```

---

## 7.4 Joins — los 4 tipos principales

```sql
-- INNER JOIN: solo filas que coinciden en ambas tablas
SELECT c.NombreCliente, p.Total
FROM Clientes c
INNER JOIN Pedidos p ON c.IdCliente = p.IdCliente;

-- LEFT JOIN: todas las filas de la izquierda, coincidan o no
SELECT c.NombreCliente, COUNT(p.IdPedido) AS TotalPedidos
FROM Clientes c
LEFT JOIN Pedidos p ON c.IdCliente = p.IdCliente
GROUP BY c.IdCliente, c.NombreCliente;
-- (clientes sin pedidos aparecen igual, con TotalPedidos = 0)

-- RIGHT JOIN: todas las filas de la derecha, coincidan o no (menos usado)
SELECT c.NombreCliente, p.Total
FROM Clientes c
RIGHT JOIN Pedidos p ON c.IdCliente = p.IdCliente;

-- FULL OUTER JOIN: todas las filas de ambas tablas
SELECT c.NombreCliente, p.Total
FROM Clientes c
FULL OUTER JOIN Pedidos p ON c.IdCliente = p.IdCliente;
```

### Join múltiple (3+ tablas), el más común en consultas reales

```sql
SELECT c.NombreCliente, pr.Nombre AS Producto, dp.Cantidad
FROM Clientes c
INNER JOIN Pedidos pe ON c.IdCliente = pe.IdCliente
INNER JOIN DetallesPedido dp ON pe.IdPedido = dp.IdPedido
INNER JOIN Productos pr ON dp.IdProducto = pr.IdProducto
WHERE c.Activo = 1;
```

---

## 7.5 GroupBy y agregaciones

```sql
SELECT Pais, COUNT(*) AS Total
FROM Clientes
GROUP BY Pais
HAVING COUNT(*) > 5   -- HAVING filtra DESPUÉS de agrupar (WHERE filtra ANTES)
ORDER BY Total DESC;

-- Varias agregaciones a la vez
SELECT
    Pais,
    COUNT(*) AS TotalClientes,
    AVG(TotalGastado) AS GastoPromedio,
    MAX(TotalGastado) AS GastoMaximo,
    MIN(FechaRegistro) AS PrimerRegistro
FROM Clientes
GROUP BY Pais;
```

**Diferencia clave WHERE vs HAVING:** `WHERE` filtra filas individuales antes de agrupar; `HAVING` filtra grupos ya formados. Por eso `HAVING COUNT(*) > 5` funciona pero `WHERE COUNT(*) > 5` da error.

---

## 7.6 Subconsultas

```sql
-- Subconsulta en el WHERE
SELECT NombreCliente
FROM Clientes
WHERE IdCliente IN (
    SELECT IdCliente FROM Pedidos WHERE Total > 100
);

-- Subconsulta correlacionada (se ejecuta por cada fila externa)
SELECT c.NombreCliente,
    (SELECT COUNT(*) FROM Pedidos p WHERE p.IdCliente = c.IdCliente) AS TotalPedidos
FROM Clientes c;

-- Common Table Expression (CTE) — más legible que subconsultas anidadas
WITH ClientesActivos AS (
    SELECT * FROM Clientes WHERE Activo = 1
)
SELECT NombreCliente, Pais FROM ClientesActivos WHERE Pais = 'España';
```

---

## 7.7 Índices — por qué importan para el rendimiento

```sql
-- Crear un índice sobre una columna consultada frecuentemente
CREATE INDEX IX_Clientes_Email ON Clientes(Email);

-- Índice único (además de optimizar, impide duplicados)
CREATE UNIQUE INDEX UX_Clientes_Email ON Clientes(Email);

-- Índice compuesto (varias columnas, útil si sueles filtrar por ambas juntas)
CREATE INDEX IX_Pedidos_Cliente_Fecha ON Pedidos(IdCliente, FechaPedido);
```

Sin índice, `WHERE Email = '...'` recorre toda la tabla fila por fila (*table scan*). Con índice, la búsqueda es casi instantánea incluso con millones de filas. La contrapartida: cada índice ralentiza ligeramente los `INSERT`/`UPDATE`, así que no se indexa todo indiscriminadamente.

> ⚠️ **Cuidado — cómo anular un índice sin querer:** si aplicas una función a la columna indexada en el `WHERE`, el índice **deja de usarse**. `WHERE YEAR(FechaPedido) = 2026` hace table scan; `WHERE FechaPedido >= '2026-01-01' AND FechaPedido < '2027-01-01'` usa el índice. Lo mismo con `WHERE UPPER(Email) = '...'` o `WHERE Email LIKE '%algo'` (el comodín al principio impide usar el índice; al final, `'algo%'`, sí lo usa).

> 💡 **Tip:** en un índice compuesto **el orden de las columnas importa**. `INDEX (IdCliente, FechaPedido)` sirve para filtrar por `IdCliente` solo, o por `IdCliente + FechaPedido`, pero **no** para filtrar solo por `FechaPedido`. Piénsalo como una guía telefónica ordenada por apellido y luego nombre: buscar "García" es inmediato, buscar a todos los "Ana" no.

> 💡 **Tip:** en SQL Server, activa `Include Actual Execution Plan` (Ctrl+M en SSMS) antes de ejecutar una consulta lenta. Si ves "Table Scan" o "Clustered Index Scan" sobre una tabla grande, ahí está tu problema. Saber leer un plan de ejecución, aunque sea por encima, te distingue inmediatamente de un becario medio.

> 💡 **Tip:** `SELECT *` está bien para explorar a mano, pero en código de producción es un problema: trae columnas que no necesitas (más red, más memoria), y se rompe silenciosamente si alguien reordena o añade columnas. Nombra siempre las columnas que usas.

---

## 7.8 Procedimientos almacenados

### Oracle

```sql
CREATE PROCEDURE sp_ObtenerClientes(
    p_Pais VARCHAR2,
    p_Resultado OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_Resultado FOR
        SELECT IdCliente, Nombre, Email
        FROM Clientes
        WHERE Pais = p_Pais AND Activo = 1;
END;
```

### SQL Server (T-SQL)

```sql
CREATE PROCEDURE sp_InsertarCliente
    @Nombre NVARCHAR(100),
    @Email NVARCHAR(100),
    @Pais NVARCHAR(50),
    @IdNuevo INT OUTPUT
AS
BEGIN
    INSERT INTO Clientes (NombreCliente, Email, Pais, Activo)
    VALUES (@Nombre, @Email, @Pais, 1);

    SET @IdNuevo = @@IDENTITY;
END;

-- Ejecutar el procedimiento
DECLARE @NuevoId INT;
EXEC sp_InsertarCliente 'Ana García', 'ana@example.com', 'España', @NuevoId OUTPUT;
SELECT @NuevoId;
```

**Por qué se usan:** centralizan lógica de acceso a datos en la propia BD, reducen el tráfico de red (una sola llamada en vez de varias consultas), y en muchas empresas siguen siendo el estándar para operaciones críticas.

---

## 7.9 Diferencias prácticas Oracle vs SQL Server

| Aspecto | Oracle | SQL Server (T-SQL) |
|---|---|---|
| Auto-incremento | `SEQUENCE` + `TRIGGER`, o `GENERATED ALWAYS AS IDENTITY` | `IDENTITY(1,1)` |
| Limitar filas | `WHERE ROWNUM <= 10` (o `FETCH FIRST 10 ROWS ONLY`) | `SELECT TOP 10` |
| Concatenar texto | `\|\|` | `+` o `CONCAT()` |
| Fecha actual | `SYSDATE` | `GETDATE()` |
| Booleano | No existe nativo, se usa `NUMBER(1)` | `BIT` |
| Cadena de texto | `VARCHAR2` | `NVARCHAR` |

---

## 7.10 Ejercicios Lección 7

1. Crea las tablas `Clientes` y `Pedidos` con sus relaciones (FK)
2. Inserta al menos 5 clientes y 8 pedidos
3. Escribe una consulta que obtenga clientes de "España" con más de 1 pedido (usando `HAVING`)
4. Escribe un `LEFT JOIN` que muestre todos los clientes, incluso los que no tienen pedidos
5. Crea un índice sobre la columna `Email` de `Clientes`
6. Escribe una subconsulta que obtenga el nombre de los clientes cuyo gasto total supere el promedio de todos los clientes
7. Escribe un procedimiento almacenado que reciba un país y devuelva el número de clientes activos en ese país

---

# Lección 8: Entity Framework Core

[[#Índice|↑ Volver al índice]]

## 8.1 ¿Qué es EF Core?

Un ORM que traduce LINQ a SQL automáticamente:

```csharp
// Escribes esto
var activos = await _contexto.Clientes
    .Where(c => c.Activo)
    .ToListAsync();

// EF Core genera:
// SELECT * FROM Clientes WHERE Activo = 1;
```

---

## 8.2 Instalación de paquetes NuGet

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## 8.3 DbContext

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Libro> Libros { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<DetallePedido> DetallesPedido { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=localhost;Database=MiBD;Trusted_Connection=true;TrustServerCertificate=true;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración avanzada de relaciones
        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Pedidos)
            .WithOne(p => p.Cliente)
            .HasForeignKey(p => p.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices y restricciones
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // Valores por defecto
        modelBuilder.Entity<Cliente>()
            .Property(c => c.FechaRegistro)
            .HasDefaultValueSql("GETDATE()");
    }
}
```

**Nota importante:** en el entorno real de Espiral MS, la cadena de conexión NO se hardcodea así. Se lee desde `appsettings.json` mediante `IConfiguration` inyectado (esto lo verás con ASP.NET Core en las prácticas).

---

## 8.4 Definir las entidades (modelos)

```csharp
public class Cliente
{
    public int IdCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Pais { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; }

    // Propiedad de navegación: relación uno a muchos
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}

public class Pedido
{
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }

    // Propiedad de navegación: relación inversa
    public Cliente Cliente { get; set; } = null!;
    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}

public class DetallePedido
{
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdLibro { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
}
```

---

## 8.5 CRUD completo con un servicio

```csharp
public class LibroServicio
{
    private readonly AppDbContext _contexto;

    public LibroServicio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // CREATE
    public async Task<int> CrearLibroAsync(Libro libro)
    {
        _contexto.Libros.Add(libro);
        await _contexto.SaveChangesAsync();
        return libro.Id;
    }

    // READ - por Id
    public async Task<Libro?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Libros.FindAsync(id);
    }

    // READ - todos
    public async Task<List<Libro>> ObtenerTodosAsync()
    {
        return await _contexto.Libros.ToListAsync();
    }

    // READ - con filtro LINQ (idéntico a lo que ya sabes)
    public async Task<List<Libro>> ObtenerDisponiblesAsync()
    {
        return await _contexto.Libros
            .Where(l => !l.Prestado)
            .OrderBy(l => l.Titulo)
            .ToListAsync();
    }

    // UPDATE
    public async Task ActualizarAsync(Libro libro)
    {
        _contexto.Libros.Update(libro);
        await _contexto.SaveChangesAsync();
    }

    // DELETE
    public async Task EliminarAsync(int id)
    {
        var libro = await _contexto.Libros.FindAsync(id);
        if (libro != null)
        {
            _contexto.Libros.Remove(libro);
            await _contexto.SaveChangesAsync();
        }
    }
}
```

**Comparación directa con tu repositorio actual:** fíjate que `ObtenerDisponiblesAsync` usa exactamente el mismo LINQ (`Where`, `OrderBy`) que ya escribiste en `BibliotecaRepositorio.ObtenerDisponibles()`. La única diferencia es `await` + `ToListAsync()` en vez de `ToList()`, porque ahora la consulta viaja a una base de datos real en vez de trabajar en memoria.

---

## 8.6 Consultas complejas: Include, proyecciones y agregaciones

```csharp
// Eager loading: cargar relaciones con Include
var clienteConPedidos = await _contexto.Clientes
    .Include(c => c.Pedidos)
        .ThenInclude(p => p.Detalles)
    .FirstOrDefaultAsync(c => c.IdCliente == 1);

// Proyección con Select (traer solo lo necesario, más eficiente)
var resumenClientes = await _contexto.Clientes
    .Where(c => c.Activo)
    .Select(c => new
    {
        c.NombreCliente,
        TotalPedidos = c.Pedidos.Count,
        GastoTotal = c.Pedidos.Sum(p => p.Total)
    })
    .OrderByDescending(c => c.GastoTotal)
    .ToListAsync();

// GroupBy traducido a SQL real
var clientesPorPais = await _contexto.Clientes
    .GroupBy(c => c.Pais)
    .Select(g => new { Pais = g.Key, Total = g.Count() })
    .ToListAsync();

// Combinar Where + Join explícito (cuando Include no basta)
var pedidosRecientes = await _contexto.Pedidos
    .Where(p => p.FechaPedido >= DateTime.Now.AddDays(-30))
    .Join(_contexto.Clientes,
          pedido => pedido.IdCliente,
          cliente => cliente.IdCliente,
          (pedido, cliente) => new { cliente.NombreCliente, pedido.Total, pedido.FechaPedido })
    .ToListAsync();
```

---

## 8.7 Change tracking, AsNoTracking y el problema N+1

Esta sección es la que separa "sé usar EF Core" de "sé usar EF Core en producción". Casi todos los problemas de rendimiento de una aplicación .NET salen de aquí.

### El Change Tracker: por qué `SaveChangesAsync()` sabe qué guardar

Cuando EF Core trae una entidad de la base de datos, guarda internamente una **copia del estado original**. Al llamar a `SaveChangesAsync()`, compara lo que hay ahora con esa copia y genera el `UPDATE` solo de las columnas que cambiaron.

```csharp
var libro = await _contexto.Libros.FirstAsync(l => l.Id == 1);
libro.Titulo = "Nuevo título";     // no hace falta llamar a Update()
await _contexto.SaveChangesAsync(); // EF detecta el cambio y genera: UPDATE Libros SET Titulo = ...
```

> 🧠 **Mentalidad Java → C#:** si vienes de Hibernate, esto es exactamente el *dirty checking* de una entidad en estado *managed*. Y sí: **modificar una entidad rastreada y llamar a `SaveChanges` basta**; no hace falta `Update()`. De hecho `Update()` marca **todas** las columnas como modificadas, así que genera un `UPDATE` más grande de lo necesario y puede pisar cambios de otro usuario.

### `AsNoTracking()` — el arreglo de rendimiento más fácil que existe

Ese seguimiento tiene un coste: memoria para las copias originales y tiempo de comparación. En una consulta de **solo lectura** (listados, informes, respuestas de una API GET) no lo necesitas para nada.

```csharp
// Lectura pura: nada se va a modificar → desactiva el tracking
var libros = await _contexto.Libros
    .AsNoTracking()
    .Where(l => !l.Prestado)
    .ToListAsync();
```

En listados grandes esto puede reducir el tiempo y la memoria de forma muy notable, y el cambio es una sola línea.

> 💡 **Tip:** regla mental sencilla — **si el resultado se va a serializar a JSON y devolverse, lleva `AsNoTracking()`. Si lo vas a modificar y guardar, no.**

> ⚠️ **Cuidado:** una entidad traída con `AsNoTracking()` **no se guardará** si la modificas y llamas a `SaveChangesAsync()`: EF no la está vigilando y no se entera del cambio. No da error: simplemente no pasa nada. Ese silencio es lo que lo hace desconcertante la primera vez.

### El problema N+1

Es *el* problema clásico de cualquier ORM. Se ve mejor con un ejemplo:

```csharp
// ❌ N+1: 1 consulta para los clientes + 1 consulta POR CADA cliente
var clientes = await _contexto.Clientes.ToListAsync();      // 1 consulta
foreach (var c in clientes)
{
    // Cada acceso a c.Pedidos dispara otro SELECT (lazy loading)
    Console.WriteLine($"{c.NombreCliente}: {c.Pedidos.Count} pedidos");
}
// 500 clientes = 501 viajes a la base de datos
```

Tres formas de arreglarlo, de mejor a peor según el caso:

```csharp
// ✅ A) Include: trae clientes y pedidos juntos (1 consulta con JOIN)
var clientes = await _contexto.Clientes
    .Include(c => c.Pedidos)
    .AsNoTracking()
    .ToListAsync();

// ✅ B) Proyección: si solo necesitas el número, ni siquiera traigas los pedidos
var resumen = await _contexto.Clientes
    .Select(c => new { c.NombreCliente, NumPedidos = c.Pedidos.Count })
    .ToListAsync();
// SQL: SELECT NombreCliente, (SELECT COUNT(*) ...) — traspasa el trabajo a la BD, que es donde debe estar

// ⚠️ C) AsSplitQuery: cuando un Include múltiple genera una "explosión cartesiana"
var clientes = await _contexto.Clientes
    .Include(c => c.Pedidos).ThenInclude(p => p.Detalles)
    .AsSplitQuery()      // varias consultas pequeñas en vez de un JOIN gigante duplicando filas
    .ToListAsync();
```

> 💡 **Tip (el de más valor de toda esta lección):** la **proyección con `Select` a un DTO** resuelve el N+1, el tracking y el exceso de columnas de una sola vez, porque EF traduce a SQL exactamente las columnas que pides y nada más. Si solo hay una cosa que recuerdes de EF Core, que sea esta: **proyecta pronto, trae poco**.

> ⚠️ **Cuidado:** `Include` con `Where` **no filtra los hijos**. `Include(c => c.Pedidos).Where(c => c.Activo)` filtra *clientes*, no pedidos. Para filtrar la colección hija se usa un *filtered include*: `Include(c => c.Pedidos.Where(p => p.Total > 100))`.

### Cómo ver el SQL que se está ejecutando

No adivines: míralo.

```csharp
// En OnConfiguring / Program.cs, solo en desarrollo
options.UseSqlServer(cadena)
       .LogTo(Console.WriteLine, LogLevel.Information)
       .EnableSensitiveDataLogging();   // muestra también los valores de los parámetros
```

```csharp
// O puntualmente, para inspeccionar una consulta concreta sin ejecutarla
var sql = _contexto.Libros.Where(l => !l.Prestado).ToQueryString();
Console.WriteLine(sql);
```

> ⚠️ **Cuidado:** `EnableSensitiveDataLogging()` escribe en los logs los valores reales de los parámetros (que pueden ser datos personales o contraseñas). **Nunca** debe llegar activado a producción. Actívalo solo bajo `if (app.Environment.IsDevelopment())`.

---

## 8.8 Transacciones explícitas

`SaveChangesAsync()` ya es transaccional: todos los cambios acumulados en el contexto se guardan en una sola transacción implícita (todo o nada). Necesitas una transacción **explícita** solo cuando quieres agrupar **varias llamadas a `SaveChanges`** o mezclar EF con SQL directo.

```csharp
public async Task TransferirPrestamoAsync(int idLibro, int idUsuarioOrigen, int idUsuarioDestino)
{
    using var transaccion = await _contexto.Database.BeginTransactionAsync();
    try
    {
        var prestamo = await _contexto.Prestamos
            .FirstAsync(p => p.IdLibro == idLibro && p.IdUsuario == idUsuarioOrigen);

        prestamo.Activo = false;
        await _contexto.SaveChangesAsync();          // 1ª operación

        _contexto.Prestamos.Add(new Prestamo
        {
            IdLibro = idLibro,
            IdUsuario = idUsuarioDestino,
            Activo = true
        });
        await _contexto.SaveChangesAsync();          // 2ª operación

        await transaccion.CommitAsync();             // ambas se confirman juntas
    }
    catch
    {
        await transaccion.RollbackAsync();           // si algo falla, NINGUNA se aplica
        throw;
    }
}
```

> 💡 **Tip:** si todo tu trabajo cabe en un único `SaveChangesAsync()`, **no escribas una transacción explícita**: ya la tienes gratis. Envolver una sola llamada a `SaveChanges` en `BeginTransaction` es ruido que un revisor te señalará.

> ⚠️ **Cuidado:** no hagas llamadas HTTP externas ni operaciones lentas dentro de una transacción abierta. Mientras esté abierta, mantiene bloqueos en la base de datos; si además esperas 5 segundos a una API de terceros, estás bloqueando filas (o tablas enteras) para todo el mundo durante esos 5 segundos.

### Concurrencia optimista: dos usuarios editando lo mismo

```csharp
public class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;

    [Timestamp]                      // columna especial que la BD actualiza en cada UPDATE
    public byte[]? RowVersion { get; set; }
}
```

Con esa columna, si dos usuarios cargan el mismo libro y ambos guardan, el segundo recibe una `DbUpdateConcurrencyException` en vez de pisar silenciosamente el cambio del primero:

```csharp
try
{
    await _contexto.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    throw new InvalidOperationException(
        "Otro usuario modificó este libro mientras lo editabas. Recarga y vuelve a intentarlo.");
}
```

**Por qué importa en Espiral MS:** en cualquier aplicación con varios usuarios trabajando a la vez, el problema de "el último que guarda gana y borra el trabajo del otro" aparece tarde o temprano. Saber que existe la concurrencia optimista y cómo se activa es algo que no se espera de un becario — y por eso destaca.

---

## 8.9 Migraciones — versionar la base de datos

Las migraciones son como "commits de Git pero para el esquema de la BD": cada cambio en tus clases se traduce en un script versionado.

```bash
# Primera migración: crea todas las tablas según tus entidades
dotnet ef migrations add InitialCreate

# Tras modificar una entidad (por ejemplo, añadir una propiedad)
dotnet ef migrations add AnadirCampoEmail

# Aplicar los cambios pendientes a la base de datos real
dotnet ef database update

# Revertir a una migración anterior
dotnet ef database update NombreMigracionAnterior

# Ver el SQL que generaría una migración sin aplicarlo
dotnet ef migrations script
```

Archivo de migración generado automáticamente:

```csharp
public partial class AnadirCampoEmail : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Clientes",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revertir el cambio, por si haces rollback
        migrationBuilder.DropColumn(name: "Email", table: "Clientes");
    }
}
```

**Por qué importa en Espiral MS:** cuando trabajes en equipo, cada desarrollador genera sus propias migraciones al cambiar el modelo. Git las versiona igual que el código, y `dotnet ef database update` las aplica en orden. Nadie edita la BD a mano.

> ⚠️ **Cuidado:** **abre siempre el archivo de migración generado antes de aplicarlo.** EF Core a veces interpreta un renombrado de propiedad como "borrar columna + crear columna", lo que en producción significa **perder todos los datos de esa columna**. Si ves un `DropColumn` que no esperabas, cámbialo a mano por `RenameColumn`. Es un caso real y frecuente.

> 💡 **Tip:** ¿te has equivocado en la última migración y **aún no la has aplicado**? `dotnet ef migrations remove` la borra limpiamente. Si ya la aplicaste, primero `dotnet ef database update MigracionAnterior` y luego `remove`. Lo que **nunca** debes hacer es borrar el archivo a mano: la tabla `__EFMigrationsHistory` de la base de datos se quedará descuadrada.

> ⚠️ **Cuidado en equipo:** dos personas creando migraciones a la vez en ramas distintas generan un conflicto que Git no sabe resolver (el archivo `ModelSnapshot.cs`). La solución habitual: quien mergea el segundo borra su migración, hace `pull`, y la regenera sobre el modelo actualizado. Anticiparlo evita una tarde perdida.

---

## 8.10 Async en toda la cadena — por qué importa

Fíjate que **todos** los métodos de EF Core que tocan la base de datos terminan en `Async` y se usan con `await`: `ToListAsync()`, `FindAsync()`, `SaveChangesAsync()`, `FirstOrDefaultAsync()`. Esto es consistente con lo que aprendiste en la Lección 3 — cualquier operación de I/O (y una consulta a BD lo es) debe ser asíncrona para no bloquear el hilo mientras espera respuesta del servidor de base de datos.

---

## 8.11 Ejercicios Lección 8

1. Crea un `AppDbContext` con `DbSet<Libro>`, `DbSet<Cliente>`, `DbSet<Pedido>`.
2. Configura la relación uno-a-muchos entre `Cliente` y `Pedido` en `OnModelCreating`.
3. Escribe un `LibroServicio` con los 5 métodos CRUD mostrados arriba.
4. Genera la migración inicial y aplícala con `dotnet ef database update`.
5. Escribe una consulta con `Include` que traiga un cliente junto a todos sus pedidos.
6. Escribe una proyección con `Select` que devuelva solo nombre de cliente y su gasto total.
7. **Provoca un N+1 a propósito:** activa `LogTo(Console.WriteLine)`, recorre los clientes accediendo a `c.Pedidos` dentro del bucle y cuenta cuántos `SELECT` aparecen en consola. Arréglalo con `Include` y vuelve a contar.
8. Compara con `Stopwatch` el tiempo de la misma consulta con y sin `AsNoTracking()` sobre al menos 1.000 filas.
9. Usa `ToQueryString()` para imprimir el SQL de tres consultas LINQ distintas y explica qué hace cada una.
10. Escribe un método con transacción explícita que cree un `Pedido` y sus `DetallePedido`, y fuerza un error a mitad para comprobar que el `ROLLBACK` deja la base de datos intacta.
11. Añade una columna `[Timestamp] RowVersion` a `Libro` y simula un conflicto de concurrencia usando dos instancias de `DbContext`.

---

# Lección 9: Records y Pattern Matching

[[#Índice|↑ Volver al índice]]

Las lecciones anteriores te han dado el C# que se corresponde casi línea a línea con Java. Esta lección es distinta: aquí están las cosas que **no tienen equivalente directo en Java clásico** y que hacen que el código C# moderno se lea diferente. Es también lo que más rápido te delata como "programador de Java escribiendo C#" si no lo conoces.

---

## 9.1 `record` — inmutabilidad e igualdad por valor, en una línea

En Java, para una clase de datos inmutable escribes el constructor, los getters, `equals()`, `hashCode()` y `toString()` (o dejas que Lombok o un `record` de Java 16+ lo hagan). En C#:

```csharp
public record Libro(string Titulo, string Autor, int Anio);
```

Esa única línea genera automáticamente:
- Un constructor `Libro(string, string, int)`
- Las propiedades `Titulo`, `Autor`, `Anio` como `{ get; init; }` — es decir, **inmutables tras la construcción**
- `Equals` y `GetHashCode` **por valor** (comparan el contenido, no la referencia)
- Un `ToString()` legible: `Libro { Titulo = 1984, Autor = Orwell, Anio = 1949 }`
- Un método `Deconstruct` para poder desestructurar el objeto

### La diferencia que más importa: igualdad por valor

```csharp
public record LibroRecord(string Titulo, int Anio);
public class  LibroClase  { public string Titulo = ""; public int Anio; }

var r1 = new LibroRecord("1984", 1949);
var r2 = new LibroRecord("1984", 1949);
Console.WriteLine(r1 == r2);        // True  — mismo contenido, son "iguales"

var c1 = new LibroClase { Titulo = "1984", Anio = 1949 };
var c2 = new LibroClase { Titulo = "1984", Anio = 1949 };
Console.WriteLine(c1 == c2);        // False — son dos objetos distintos en memoria
```

> 💡 **Tip:** esto hace que los tests sean muchísimo más limpios. Con un `record`, `Assert.Equal(esperado, obtenido)` funciona comparando todo el contenido de golpe, sin tener que comprobar propiedad a propiedad ni sobrescribir `Equals` a mano.

### Expresiones `with` — copiar cambiando algo

Como un record es inmutable, no puedes modificarlo. Lo que haces es **crear una copia con un cambio**:

```csharp
var original = new Libro("1984", "Orwell", 1949);
var reedicion = original with { Anio = 2024 };   // copia idéntica salvo el año

Console.WriteLine(original.Anio);   // 1949 — el original NO se toca
Console.WriteLine(reedicion.Anio);  // 2024
```

Esto se llama *non-destructive mutation* y es el equivalente conceptual del spread de JavaScript (`{ ...persona, edad: 30 }`, que verás en la Lección 15).

### Cuándo usar `record` y cuándo `class`

| Usa `record` | Usa `class` |
|---|---|
| DTOs de entrada y salida de una API | Entidades de Entity Framework |
| Objetos de configuración | Servicios, repositorios, controladores |
| Value objects de dominio (`Dinero`, `Coordenada`, `Rango`) | Cualquier cosa con estado mutable e identidad propia |
| Mensajes, eventos, resultados de consulta | Clases con lógica de comportamiento, no solo datos |

La regla mental: **`record` para datos, `class` para comportamiento.** Si el objeto se define por *lo que contiene*, record. Si se define por *lo que hace* o por *quién es* (tiene un Id que lo identifica aunque cambien sus datos), class.

> ⚠️ **Cuidado — no uses `record` para entidades de EF Core.** Una entidad tiene identidad (su clave primaria) y estado mutable por diseño; el change tracker necesita modificarla. Un `record` con propiedades `init` choca frontalmente con eso. Para los DTOs que envuelven esa entidad, en cambio, `record` es perfecto — que es exactamente el patrón que verás en el proyecto MarinaApi.

### Variantes: `record struct` y sintaxis extendida

```csharp
// record struct: igualdad por valor Y semántica de value type (se copia al asignar)
// Ideal para objetos pequeños creados en gran cantidad: evita presión sobre el recolector de basura
public readonly record struct Coordenada(double Latitud, double Longitud);

// Sintaxis extendida: un record puede tener cuerpo, métodos y propiedades adicionales
public record Pedido(int Id, decimal Subtotal)
{
    public decimal Iva => Subtotal * 0.21m;
    public decimal Total => Subtotal + Iva;

    public bool EsGrande() => Total > 1000m;
}
```

> ⚠️ **Cuidado — la igualdad por valor de un record es *superficial*.** Si un record contiene una `List<T>`, dos records con listas de contenido idéntico pero instancias distintas **no** serán iguales, porque `List<T>` compara por referencia. Es un fallo sorprendente si esperabas comparación profunda.

---

## 9.2 Pattern matching de verdad

La Lección 1 mostró `is` y `switch` expression básicos. El pattern matching de C# llega mucho más lejos y sustituye cadenas enteras de `if` anidados.

### Patrones de propiedad

```csharp
// En vez de esto...
if (pedido != null && pedido.Cliente != null && pedido.Cliente.Pais == "España" && pedido.Total > 100)

// ...esto: una sola expresión, con comprobación de null incluida
if (pedido is { Cliente.Pais: "España", Total: > 100 })
{
    AplicarEnvioGratis(pedido);
}
```

`is { ... }` ya comprueba que el objeto no es null, y `Cliente.Pais` navega en profundidad sin riesgo de `NullReferenceException`. Es más corto **y** más seguro que la versión con `&&`.

> 💡 **Tip:** el patrón vacío `is { }` significa "no es null". `if (libro is { })` es equivalente a `if (libro is not null)` — este último es más legible y es el que se prefiere en code review.

### `switch` con patrones sobre tipos y propiedades

```csharp
public decimal CalcularTarifa(Vehiculo v) => v switch
{
    Moto                              => 2.50m,
    Coche { Plazas: > 5 }             => 8.00m,      // tipo + condición sobre propiedad
    Coche                             => 5.00m,
    Camion { Toneladas: var t } when t > 20 => 30.00m, // "when" para lógica más compleja
    Camion                            => 20.00m,
    null                              => throw new ArgumentNullException(nameof(v)),
    _                                 => throw new NotSupportedException($"Tipo no soportado: {v.GetType().Name}")
};
```

**El orden importa:** se evalúa de arriba abajo y gana el primer patrón que encaja. Por eso `Coche { Plazas: > 5 }` debe ir **antes** que `Coche` a secas; al revés, nunca se alcanzaría. El compilador te avisa de patrones inalcanzables — hazle caso.

> 🧠 **Mentalidad Java → C#:** esto sustituye a la cascada de `instanceof` + cast de Java, y también al polimorfismo en los casos donde no puedes (o no quieres) tocar las clases. Ojo: si el comportamiento *pertenece* al objeto, un método virtual sigue siendo mejor diseño que un switch sobre tipos. Pattern matching brilla cuando la lógica es **externa** a las clases (un cálculo de tarifas, una traducción a DTO, un formateo).

### Patrones combinados: `and`, `or`, `not`

```csharp
string Clasificar(int edad) => edad switch
{
    < 0            => "Inválida",
    >= 0 and < 18  => "Menor",
    >= 18 and < 65 => "Adulto",
    _              => "Senior"
};

if (respuesta is not null and not "")  { /* ... */ }
if (codigo is 404 or 410)              { /* recurso desaparecido */ }
```

### Desestructuración y tuplas con nombre

```csharp
// Tupla con nombres: para devolver varios valores sin crear una clase
public (bool Exito, string Mensaje) Validar(Libro libro)
{
    if (string.IsNullOrWhiteSpace(libro.Titulo))
        return (false, "El título es obligatorio");
    return (true, "OK");
}

var (exito, mensaje) = Validar(miLibro);   // desestructuración
if (!exito) Console.WriteLine(mensaje);

// Switch sobre una tupla: matriz de decisión legible de un vistazo
string Resultado(bool pagado, bool enviado) => (pagado, enviado) switch
{
    (true,  true)  => "Completado",
    (true,  false) => "Pendiente de envío",
    (false, true)  => "¡Revisar! Enviado sin pagar",
    (false, false) => "Nuevo"
};
```

> ⚠️ **Cuidado:** las tuplas son estupendas **dentro** de un método o entre dos métodos privados muy próximos. No las uses como tipo de retorno de una API pública ni las serialices a JSON: los nombres de campo de una tupla no sobreviven bien a la serialización ni a la reflexión, y un `(bool, string)` no documenta nada a quien llame a tu método desde otro archivo. Para eso, un `record` con nombres claros.

---

## 9.3 Otros azucarillos que verás a diario

```csharp
// Interpolación con formato: alineación y formato numérico dentro de la cadena
Console.WriteLine($"{producto.Nombre,-20} {producto.Precio,10:C2}");
// -20 = alineado a la izquierda en 20 caracteres; C2 = moneda con 2 decimales

// nameof: obtiene el nombre de una variable/propiedad como string, y SOBREVIVE a los refactors
throw new ArgumentException("Valor inválido", nameof(cantidad));
// si renombras "cantidad", el string se actualiza solo — a diferencia de escribir "cantidad" a mano

// Colecciones con sintaxis literal (C# 12)
int[] numeros = [1, 2, 3, 4];
List<string> nombres = ["Ana", "Luis"];
int[] combinados = [..numeros, 5, 6];     // spread, como en JavaScript

// Métodos de extensión: añadir métodos a tipos que no controlas (¡ni siquiera a string!)
public static class StringExtensiones
{
    public static bool EsEmailValido(this string texto)   // "this" en el primer parámetro
        => texto.Contains('@') && texto.Contains('.');
}

bool valido = "ana@example.com".EsEmailValido();  // se usa como si fuera un método de string
```

> 💡 **Tip:** los métodos de extensión son la razón por la que LINQ existe. `Where`, `Select` y compañía no están dentro de `IEnumerable<T>`: son métodos de extensión definidos en `System.Linq.Enumerable`. Por eso, si se te olvida el `using System.Linq`, LINQ "desaparece" sin más y el error del compilador no menciona LINQ para nada. En Espiral MS los verás sobre todo para los *mappers* de DTOs: `libro.ToDto()`.

> ⚠️ **Cuidado:** no abuses de los métodos de extensión sobre tipos del framework (`string`, `int`, `DateTime`). Contaminan el autocompletado de **todo el proyecto** y confunden a quien lee el código porque parecen parte del lenguaje. Regla: extensiones sobre tus propios tipos, o sobre tipos del framework solo cuando la utilidad sea evidente y esté en un namespace bien acotado.

---

## 9.4 Ejercicios Lección 9

1. Convierte la clase `Libro` del mini-proyecto en un `record LibroDto(int Id, string Titulo, string? Autor, int Anio)`.
2. Crea dos instancias con los mismos valores y comprueba que `==` devuelve `true`. Hazlo también con la clase original y compara.
3. Usa una expresión `with` para crear una copia de un libro cambiando solo el año, y demuestra que el original no se ha modificado.
4. Escribe un `readonly record struct Dinero(decimal Cantidad, string Divisa)` con un método `Sumar` que lance excepción si las divisas no coinciden.
5. Reescribe con patrón de propiedad (`is { ... }`) una condición anidada con tres `&&` y comprobaciones de null.
6. Escribe un `switch` expression sobre tipos que calcule el precio de préstamo según sea `Libro`, `Revista` o `Dvd`, con una condición extra usando `when`.
7. Escribe un método que devuelva una tupla con nombres `(bool Valido, string? Error)` y consúmelo con desestructuración.
8. Crea un método de extensión `ToDto()` sobre tu entidad `Libro` que devuelva el `LibroDto` del ejercicio 1.
9. Escribe un `switch` sobre una tupla `(bool, bool)` que cubra los cuatro casos posibles, y comprueba que el compilador te avisa si eliminas uno.

---

# Lección 10: ASP.NET Core Web API

[[#Índice|↑ Volver al índice]]

Aquí es donde todo lo anterior se junta. Una API web es, literalmente: recibir una petición HTTP → validar → llamar a un servicio → que use Entity Framework → devolver JSON. Cada pieza ya la conoces por separado.

> 🧠 **Mentalidad Java → C#:** si has visto Spring Boot, el mapa es directo: `@RestController` → `[ApiController]`, `@RequestMapping` → `[Route]`, `@Autowired` → inyección por constructor, `ResponseEntity<T>` → `ActionResult<T>`, `application.properties` → `appsettings.json`. La versión completa de esta equivalencia, aplicada al proyecto real MarinaApi, está en `MIGRACION_JAVA_A_CSHARP.md` (capítulos 9 a 14).

---

## 10.1 Anatomía de un proyecto Web API

```bash
dotnet new webapi -n MiApi      # crea el proyecto
cd MiApi
dotnet run                       # arranca en https://localhost:7xxx
```

```
MiApi/
├── Program.cs              ← configuración + arranque (el "main" de todo)
├── appsettings.json        ← configuración (cadenas de conexión, niveles de log)
├── Controllers/
│   └── LibrosController.cs ← endpoints HTTP
├── Services/
│   └── LibroServicio.cs    ← lógica de negocio
├── Repositories/
│   └── LibroRepositorio.cs ← acceso a datos (EF Core)
├── Models/                 ← entidades de base de datos
└── Dtos/                   ← lo que entra y sale por HTTP
```

Esta separación en capas no es burocracia: **el controlador no sabe de base de datos, y el repositorio no sabe de HTTP**. Cada capa se puede testear sola.

---

## 10.2 `Program.cs` — el punto donde se ensambla la aplicación

```csharp
var builder = WebApplication.CreateBuilder(args);

// ── 1. Registro de servicios (el contenedor de DI de la Lección 6) ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILibroRepositorio, LibroRepositorio>();
builder.Services.AddScoped<ILibroServicio, LibroServicio>();

var app = builder.Build();

// ── 2. Pipeline de middleware (el ORDEN es significativo) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();      // interfaz web para probar la API a mano
}

app.UseHttpsRedirection();
app.UseAuthentication();     // ¿quién eres?   → siempre ANTES de Authorization
app.UseAuthorization();      // ¿puedes hacer esto?
app.MapControllers();

app.Run();
```

**Las dos mitades del archivo son distintas y conviene no confundirlas:** antes de `builder.Build()` se **registra** qué servicios existen; después se **configura** por dónde pasa cada petición HTTP.

> ⚠️ **Cuidado — el orden del middleware es una fuente de bugs desconcertantes.** El pipeline es una cebolla: cada middleware envuelve al siguiente (el patrón Decorator de la Lección 6). Si pones `UseAuthorization()` antes de `UseAuthentication()`, el sistema intentará decidir permisos sobre un usuario que todavía no se ha identificado, y obtendrás 401 en endpoints que deberían funcionar. Regla: autenticar, luego autorizar, y `MapControllers()` al final.

---

## 10.3 Un controlador real, endpoint a endpoint

```csharp
[ApiController]
[Route("api/[controller]")]          // → api/libros  (toma el nombre de la clase sin "Controller")
public class LibrosController : ControllerBase
{
    private readonly ILibroServicio _servicio;

    public LibrosController(ILibroServicio servicio)   // DI por constructor, como en la Lección 6
        => _servicio = servicio;

    // GET api/libros?disponibles=true&pagina=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibroDto>>> ObtenerTodos(
        [FromQuery] bool? disponibles = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        var libros = await _servicio.ObtenerAsync(disponibles, pagina, tamano);
        return Ok(libros);                               // 200 OK + JSON
    }

    // GET api/libros/5
    [HttpGet("{id:int}")]                                // ":int" restringe la ruta a números
    public async Task<ActionResult<LibroDto>> ObtenerPorId(int id)
    {
        var libro = await _servicio.ObtenerPorIdAsync(id);
        return libro is null
            ? NotFound()                                 // 404
            : Ok(libro);                                 // 200
    }

    // POST api/libros
    [HttpPost]
    public async Task<ActionResult<LibroDto>> Crear([FromBody] CrearLibroDto dto)
    {
        var creado = await _servicio.CrearAsync(dto);
        // 201 Created + cabecera Location apuntando al recurso nuevo
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    // PUT api/libros/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarLibroDto dto)
    {
        var actualizado = await _servicio.ActualizarAsync(id, dto);
        return actualizado ? NoContent() : NotFound();   // 204 si fue bien
    }

    // DELETE api/libros/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _servicio.EliminarAsync(id);
        return eliminado ? NoContent() : NotFound();
    }
}
```

### Los códigos de estado que debes conocer

| Código | Cuándo devolverlo | Helper de `ControllerBase` |
|---|---|---|
| 200 OK | Petición correcta con contenido | `Ok(objeto)` |
| 201 Created | Recurso creado (POST) | `CreatedAtAction(...)` |
| 204 No Content | Correcto, sin nada que devolver (PUT/DELETE) | `NoContent()` |
| 400 Bad Request | Los datos que envió el cliente son inválidos | `BadRequest(detalle)` |
| 401 Unauthorized | No estás identificado | `Unauthorized()` |
| 403 Forbidden | Estás identificado, pero no tienes permiso | `Forbid()` |
| 404 Not Found | El recurso no existe | `NotFound()` |
| 409 Conflict | Choca con el estado actual (duplicado, ya prestado) | `Conflict(detalle)` |
| 500 Internal Server Error | Algo falló en el servidor | (no lo devuelvas a mano) |

> ⚠️ **Cuidado — el error conceptual más repetido:** devolver siempre `200 OK` con un cuerpo tipo `{ "exito": false, "mensaje": "no encontrado" }`. El código HTTP **es** parte de la respuesta: los clientes, las herramientas de monitorización y las cachés lo usan. Un 404 dice "no existe" a todo el ecosistema; un 200 con un texto dentro no se lo dice a nadie.

> 💡 **Tip:** 500 no se devuelve a mano, se *provoca* al lanzar una excepción no controlada. Tu trabajo es que solo ocurra cuando de verdad hay un fallo del servidor — si el cliente mandó datos inválidos, eso es un 400, no un 500.

---

## 10.4 DTOs — por qué nunca se devuelve la entidad directamente

```csharp
// Entidad (base de datos): tiene navegaciones, campos internos, quizá datos sensibles
public class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal CosteAdquisicion { get; set; }        // ¡interno! no debe salir por la API
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}

// DTO de salida: exactamente lo que el cliente debe ver
public record LibroDto(int Id, string Titulo, string? Autor, bool Disponible);

// DTO de entrada: exactamente lo que el cliente puede enviar (sin Id: lo asigna el servidor)
public record CrearLibroDto(string Titulo, string? Autor, int Anio);
```

**Cuatro razones, todas prácticas:**
1. **Seguridad:** la entidad puede tener campos que no deben salir (costes, notas internas, hashes).
2. ***Over-posting*:** sin DTO de entrada, un cliente malicioso puede enviar `{"id": 1, "esAdmin": true}` y el model binding lo asignará alegremente.
3. **Ciclos de serialización:** `Libro → Prestamos → Libro → ...` hace que el serializador JSON entre en bucle o lance una excepción.
4. **Desacoplamiento:** renombrar una columna en la base de datos no debería romper a todos los clientes de tu API.

> 💡 **Tip:** un DTO es el caso de uso perfecto para un `record` (Lección 9): son datos puros, inmutables y se comparan por valor, lo que facilita enormemente los tests. Y la conversión queda muy limpia como método de extensión:
> ```csharp
> public static LibroDto ToDto(this Libro l) => new(l.Id, l.Titulo, l.Autor, !l.Prestado);
> ```

> ⚠️ **Cuidado:** no proyectes a DTO *después* de traerte todo a memoria. `ToListAsync().Select(x => x.ToDto())` trae todas las columnas de todas las filas y luego descarta. `Select(...).ToListAsync()` traduce la proyección a SQL y trae solo lo necesario. El orden de esas dos llamadas cambia por completo el rendimiento (es el punto de la Lección 8.7 aplicado aquí).

---

## 10.5 Validación con Data Annotations

```csharp
public class CrearLibroDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(200, MinimumLength = 1)]
    public string Titulo { get; set; } = string.Empty;

    [Range(-3000, 2100, ErrorMessage = "Año fuera de rango")]
    public int Anio { get; set; }

    [EmailAddress]
    public string? EmailContacto { get; set; }
}
```

Con `[ApiController]` en el controlador, la validación es **automática**: si el DTO no cumple las reglas, ASP.NET responde un `400 Bad Request` con el detalle de los errores **antes de entrar en tu método**. No tienes que escribir ni un `if`.

```json
// Respuesta automática ante datos inválidos (formato ProblemDetails, estándar RFC 7807)
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Titulo": ["El título es obligatorio"],
    "Anio": ["Año fuera de rango"]
  }
}
```

> 💡 **Tip:** `ProblemDetails` es el formato estándar de errores de las APIs .NET modernas. Si devuelves tus propios errores con ese mismo formato (`Problem(...)`, `ValidationProblem(...)`), todos los errores de tu API serán consistentes y cualquier cliente sabrá interpretarlos.

---

## 10.6 Middleware: tratamiento global de errores

Tus servicios lanzan excepciones de negocio (Lección 3). Traducirlas a códigos HTTP en cada `catch` de cada controlador sería repetitivo e inconsistente. Se hace **una vez**, en un middleware:

```csharp
public class MiddlewareDeErrores
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<MiddlewareDeErrores> _logger;

    public MiddlewareDeErrores(RequestDelegate siguiente, ILogger<MiddlewareDeErrores> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);          // deja pasar la petición al resto del pipeline
        }
        catch (LibroNoEncontradoException ex)
        {
            await Responder(contexto, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (LibroYaPrestadoException ex)
        {
            await Responder(contexto, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            // Aquí SÍ es correcto un catch genérico: es la frontera de la aplicación
            _logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
            await Responder(contexto, StatusCodes.Status500InternalServerError,
                            "Ha ocurrido un error inesperado.");   // nunca expongas el stack trace
        }
    }

    private static async Task Responder(HttpContext ctx, int codigo, string detalle)
    {
        ctx.Response.StatusCode = codigo;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails { Status = codigo, Detail = detalle });
    }
}

// En Program.cs, lo más ARRIBA posible para que envuelva a todo lo demás
app.UseMiddleware<MiddlewareDeErrores>();
```

> ⚠️ **Cuidado:** nunca devuelvas `ex.ToString()` ni el stack trace en la respuesta HTTP de producción. Revela rutas de archivos, nombres de clases internas y versiones de librerías: es información de oro para quien quiera atacar el sistema. El detalle va **al log**; al cliente solo un mensaje genérico y, como mucho, un identificador de correlación para poder buscarlo.

---

## 10.7 Minimal APIs — la alternativa ligera

Desde .NET 6 se puede definir una API entera sin controladores:

```csharp
var app = builder.Build();

app.MapGet("/api/libros", async (ILibroServicio svc) => Results.Ok(await svc.ObtenerAsync()));

app.MapGet("/api/libros/{id:int}", async (int id, ILibroServicio svc) =>
    await svc.ObtenerPorIdAsync(id) is { } libro
        ? Results.Ok(libro)
        : Results.NotFound());

app.MapPost("/api/libros", async (CrearLibroDto dto, ILibroServicio svc) =>
{
    var creado = await svc.CrearAsync(dto);
    return Results.Created($"/api/libros/{creado.Id}", creado);
});

app.Run();
```

Las dependencias se inyectan **como parámetros del método**, en vez de por constructor. Es más conciso para microservicios y APIs pequeñas; para una API grande con muchos endpoints, filtros y convenciones compartidas, los controladores siguen organizando mejor. **En la empresa te encontrarás las dos** — y en un proyecto heredado, casi seguro controladores.

---

## 10.8 Swagger / OpenAPI

Con `AddSwaggerGen()` + `UseSwaggerUI()`, al arrancar en desarrollo tienes en `/swagger` una página web que documenta todos tus endpoints y **permite probarlos sin Postman**: rellenar el cuerpo, ejecutar y ver la respuesta real.

```csharp
/// <summary>Obtiene un libro por su identificador.</summary>
/// <response code="200">El libro solicitado</response>
/// <response code="404">No existe un libro con ese Id</response>
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(LibroDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<LibroDto>> ObtenerPorId(int id) { /* ... */ }
```

Esos atributos hacen que Swagger muestre exactamente qué devuelve cada endpoint y con qué código.

> 💡 **Tip:** la primera tarea de muchas prácticas es "añade un endpoint". Abrir Swagger y probar el endpoint tú mismo **antes** de pedir revisión evita el 90% de los comentarios de "esto devuelve 500". Y si activas la generación del XML de documentación en el `.csproj`, tus comentarios `///` aparecen directamente en Swagger.

---

## 10.9 Ejercicios Lección 10

1. Crea un proyecto con `dotnet new webapi` y arráncalo; abre `/swagger` en el navegador.
2. Crea `LibrosController` con los cinco endpoints CRUD, usando los helpers correctos (`Ok`, `CreatedAtAction`, `NoContent`, `NotFound`).
3. Define `LibroDto`, `CrearLibroDto` y `ActualizarLibroDto` como `record`, y un método de extensión `ToDto()`.
4. Añade Data Annotations a `CrearLibroDto` y comprueba en Swagger que un título vacío devuelve un 400 con `ProblemDetails` sin que escribas ningún `if`.
5. Registra el servicio y el repositorio en `Program.cs` con el lifetime adecuado y justifica por escrito por qué elegiste ese.
6. Implementa `MiddlewareDeErrores` y comprueba que una `LibroNoEncontradoException` lanzada en el servicio llega al cliente como un 404 limpio.
7. Añade paginación (`?pagina=2&tamano=10`) al endpoint de listado usando `Skip`/`Take` de la Lección 2.
8. Reescribe dos de los endpoints como Minimal API y compara legibilidad y cantidad de código.
9. Documenta un endpoint con `///` y `[ProducesResponseType]`, y verifica el resultado en Swagger.

---

# Lección 11: Testing con xUnit y Moq

[[#Índice|↑ Volver al índice]]

En la Lección 6 viste *por qué* Repository + DI hacen el código testeable. Esta lección es el *cómo*: el marco de tests completo que sostiene aquello.

> 🧠 **Mentalidad Java → C#:** JUnit → **xUnit**, Mockito → **Moq**, AssertJ → **FluentAssertions**. Los conceptos son idénticos; cambia la sintaxis y un par de detalles de ciclo de vida.

---

## 11.1 Estructura: un proyecto de tests aparte

```bash
dotnet new xunit -n MiApi.Tests            # proyecto de tests
dotnet add MiApi.Tests reference MiApi     # referencia al proyecto a testear
dotnet add MiApi.Tests package Moq
dotnet test                                # ejecuta todos los tests
```

La convención es `<Proyecto>.Tests`, exactamente como el `MarinaApi.Tests` que ya existe en este repositorio.

---

## 11.2 Arrange - Act - Assert

Todo test tiene tres partes, y conviene separarlas visualmente:

```csharp
public class LibroServicioTests
{
    [Fact]                                   // [Fact] = un test sin parámetros
    public async Task PrestarAsync_CuandoLibroDisponible_MarcaComoPrestado()
    {
        // Arrange — preparar el escenario
        var repo = new Mock<ILibroRepositorio>();
        var libro = new Libro(1, "1984", "Orwell", 1949, prestado: false);
        repo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(libro);
        var servicio = new LibroServicio(repo.Object);

        // Act — ejecutar EXACTAMENTE una cosa: lo que se está probando
        await servicio.PrestarAsync(1);

        // Assert — comprobar el resultado
        Assert.True(libro.Prestado);
        repo.Verify(r => r.GuardarAsync(It.IsAny<Libro>()), Times.Once);
    }
}
```

> 💡 **Tip — el nombre del test es documentación.** La convención más usada en .NET es `Metodo_Escenario_ResultadoEsperado`. Cuando un test falla en el servidor de integración continua, lo único que lees es su nombre: `PrestarAsync_CuandoLibroYaPrestado_LanzaExcepcion` te dice qué se rompió sin abrir el código. `Test1` no te dice nada.

> ⚠️ **Cuidado:** un test, una cosa. Si tu test tiene dos bloques "Act", en realidad son dos tests. Cuando falle, no sabrás cuál de los dos comportamientos se ha roto, y ese es precisamente el valor que un test debe darte.

---

## 11.3 `[Theory]` — el mismo test con muchos datos

```csharp
[Theory]
[InlineData(1949, false)]
[InlineData(1900, true)]
[InlineData(1499, true)]
public void EsAntiguo_SegunElAnio_DevuelveLoEsperado(int anio, bool esperado)
{
    var libro = new Libro { Anio = anio };
    Assert.Equal(esperado, libro.EsAntiguo);
}
```

Cada `[InlineData]` es un caso de prueba **independiente**: si uno falla, ves exactamente cuál, y los demás siguen ejecutándose.

> 💡 **Tip:** cuando estimes tareas (Lección 14), la pregunta "¿y los casos límite?" se responde sola con un `[Theory]`: cero, negativo, null, cadena vacía, el valor justo en la frontera. Es donde viven la mayoría de los bugs reales.

---

## 11.4 Asserts habituales

```csharp
Assert.Equal(esperado, obtenido);          // ¡esperado PRIMERO! (al revés que en algunos frameworks)
Assert.NotEqual(a, b);
Assert.True(condicion);
Assert.Null(objeto);  Assert.NotNull(objeto);
Assert.Contains("texto", cadena);
Assert.Empty(lista);  Assert.Single(lista);
Assert.Equal(3, lista.Count);
Assert.IsType<LibroDto>(resultado);

// Comprobar que algo LANZA una excepción
var ex = await Assert.ThrowsAsync<LibroYaPrestadoException>(
    () => servicio.PrestarAsync(1));
Assert.Contains("ya está prestado", ex.Message);
```

> 💡 **Tip:** si el proyecto usa **FluentAssertions**, los asserts se leen casi como inglés y los mensajes de error son mucho más claros: `resultado.Should().NotBeNull();`, `libros.Should().HaveCount(3).And.OnlyContain(l => l.Anio > 1900);`. Mira qué usa el equipo antes de escribir tu primer test y sé consistente con ello.

---

## 11.5 Moq en profundidad

```csharp
var repo = new Mock<ILibroRepositorio>();

// Devolver un valor
repo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(unLibro);

// Cualquier argumento
repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync(unLibro);

// Argumento que cumpla una condición
repo.Setup(r => r.ObtenerPorIdAsync(It.Is<int>(id => id > 100))).ReturnsAsync((Libro?)null);

// Lanzar una excepción para probar el camino de error
repo.Setup(r => r.GuardarAsync(It.IsAny<Libro>()))
    .ThrowsAsync(new DbUpdateException("BD caída", (Exception?)null));

// Verificar interacciones (¿se llamó? ¿cuántas veces?)
repo.Verify(r => r.GuardarAsync(It.IsAny<Libro>()), Times.Once);
repo.Verify(r => r.EliminarAsync(It.IsAny<int>()), Times.Never);
```

> ⚠️ **Cuidado:** Moq solo puede simular lo que es **virtual**: interfaces, clases abstractas o métodos `virtual`. No puedes mockear una clase sellada ni un método normal. Esto no es una limitación arbitraria: es la razón técnica por la que se programa contra interfaces (Lección 6). Si algo no se puede mockear, normalmente es que está demasiado acoplado.

> ⚠️ **Cuidado — no mockees lo que estás probando.** El sujeto del test es una clase real; los mocks son sus *dependencias*. Un test que mockea el propio servicio que quiere probar no prueba nada, solo comprueba que Moq funciona.

> 💡 **Tip:** ¿verificar con `Verify` o comprobar el resultado con `Assert`? Prefiere siempre `Assert` sobre el resultado observable. Usa `Verify` cuando el efecto **es** la interacción y no se ve de otra forma: que se envió el email, que se guardó, que **no** se llamó al servicio de pagos. Un test lleno de `Verify` se rompe en cuanto refactorizas por dentro, aunque el comportamiento siga siendo correcto — eso es un test frágil.

---

## 11.6 Fixtures: compartir preparación entre tests

```csharp
// Se crea UNA vez y se comparte entre todos los tests de la clase
public class BaseDatosFixture : IDisposable
{
    public AppDbContext Contexto { get; }

    public BaseDatosFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")   // nombre único: aísla cada ejecución
            .Options;
        Contexto = new AppDbContext(options);
        Contexto.Libros.AddRange(DatosDePrueba());
        Contexto.SaveChanges();
    }

    public void Dispose() => Contexto.Dispose();
}

public class LibroRepositorioTests : IClassFixture<BaseDatosFixture>
{
    private readonly BaseDatosFixture _fixture;
    public LibroRepositorioTests(BaseDatosFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ObtenerDisponibles_DevuelveSoloNoPrestados() { /* ... */ }
}
```

> 💡 **Tip sobre xUnit que sorprende a quien viene de JUnit:** xUnit **crea una instancia nueva de la clase de test para cada `[Fact]`**. No hay `@Before`/`@After`: el constructor es el setup y `Dispose()` es el teardown, y no hay estado compartido accidental entre tests. Es una decisión de diseño deliberada y muy sana.

> ⚠️ **Cuidado:** los tests deben ser **independientes y poder ejecutarse en cualquier orden**. xUnit ejecuta clases de test en paralelo por defecto. Si dos tests comparten una base de datos con el mismo nombre, fallarán de forma intermitente — el peor tipo de fallo, porque a veces pasa y a veces no. De ahí el `Guid.NewGuid()` del ejemplo.

---

## 11.7 Qué testear (y qué no)

| Testea | No pierdas el tiempo testeando |
|---|---|
| Lógica de negocio: cálculos, validaciones, reglas | Getters y setters automáticos |
| Casos límite: null, cero, vacío, valores frontera | Que Entity Framework sepa hacer un SELECT |
| Caminos de error: qué excepción se lanza y cuándo | Código de terceros o del framework |
| Bugs que ya has arreglado (test de regresión) | Detalles internos de implementación privada |

> 💡 **Tip:** cuando te toque arreglar un bug, **escribe primero un test que falle reproduciéndolo**, y luego arréglalo. Tienes la garantía de que lo has entendido y de que nadie lo reintroducirá. Además, es de las cosas que mejor impresión causan en un equipo: demuestra que entiendes el bug, no solo que lo has parcheado.

> ⚠️ **Cuidado con la cobertura como métrica:** un 90% de cobertura con tests que no comprueban nada útil vale menos que un 50% bien elegido. La cobertura dice qué líneas se ejecutaron, no si el comportamiento es correcto.

**Por qué importa en Espiral MS:** en muchos equipos, un pull request sin tests no se aprueba. Y cuando entres a un proyecto que no conoces, **leer sus tests es la forma más rápida de entender qué hace el sistema**: son documentación ejecutable que, a diferencia de la escrita, no puede quedarse obsoleta sin que nadie se entere.

---

## 11.8 Ejercicios Lección 11

1. Crea un proyecto `MiApi.Tests` con xUnit y añádele la referencia al proyecto principal y el paquete Moq.
2. Escribe tres tests de `GestorPrestamos` usando `Mock<ILibroRepositorio>`: caso correcto, libro no encontrado y libro ya prestado.
3. Nombra los tres siguiendo la convención `Metodo_Escenario_ResultadoEsperado`.
4. Convierte a `[Theory]` con `[InlineData]` un test de clasificación de libros por siglo, cubriendo al menos cinco años distintos incluyendo los límites.
5. Usa `Assert.ThrowsAsync` para comprobar que se lanza la excepción correcta y que su mensaje contiene el texto esperado.
6. Usa `Verify(..., Times.Once)` para comprobar que `PrestarAsync` guarda exactamente una vez, y `Times.Never` para comprobar que no guarda si el libro ya estaba prestado.
7. Crea un `IClassFixture` con una base de datos en memoria y tres libros de prueba, y escribe dos tests que la usen.
8. Ejecuta `dotnet test` y provoca un fallo a propósito para ver cómo se lee la salida de error.
9. Busca un método de tu proyecto que **no** puedas testear fácilmente y explica por escrito qué dependencia habría que extraer a una interfaz para conseguirlo.

---

# Lección 12: Logging y Configuración en .NET

[[#Índice|↑ Volver al índice]]

Dos temas que nunca aparecen en un tutorial y que son de las primeras cosas que tocarás en un proyecto real. Cuando algo falla en producción, **los logs son lo único que tienes**.

---

## 12.1 `ILogger<T>` — logging integrado

No hace falta instalar nada: el logging viene en el framework y se inyecta como cualquier otra dependencia.

```csharp
public class LibroServicio : ILibroServicio
{
    private readonly ILogger<LibroServicio> _logger;   // el <T> identifica el origen del log
    private readonly ILibroRepositorio _repo;

    public LibroServicio(ILogger<LibroServicio> logger, ILibroRepositorio repo)
    {
        _logger = logger;
        _repo = repo;
    }

    public async Task PrestarAsync(int id)
    {
        _logger.LogInformation("Iniciando préstamo del libro {IdLibro}", id);

        var libro = await _repo.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            _logger.LogWarning("Libro {IdLibro} no encontrado", id);
            throw new LibroNoEncontradoException($"Libro {id} no encontrado");
        }

        try
        {
            libro.Prestado = true;
            await _repo.GuardarAsync(libro);
            _logger.LogInformation("Libro {IdLibro} prestado correctamente", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al guardar el préstamo del libro {IdLibro}", id);
            throw;
        }
    }
}
```

### Logging estructurado — la parte que casi nadie entiende al principio

Fíjate en que el mensaje usa `{IdLibro}` **y el valor va como argumento aparte**. No es interpolación de cadenas, y la diferencia es enorme:

```csharp
_logger.LogInformation($"Préstamo del libro {id}");      // ❌ interpolación
_logger.LogInformation("Préstamo del libro {IdLibro}", id); // ✅ plantilla estructurada
```

Con la segunda forma, el sistema de logs guarda **el mensaje y el dato por separado**. Eso permite, en herramientas como Seq, Elastic o Application Insights, buscar literalmente "todos los logs donde `IdLibro` = 42" o "agrupar por `IdLibro`". Con la primera forma tienes una cadena de texto plano, y solo puedes hacer búsquedas de texto.

> ⚠️ **Cuidado:** un analizador de código te marcará la versión con `$` como advertencia. No es una manía de estilo: además de perder la estructura, la interpolación **construye la cadena aunque ese nivel de log esté desactivado**, lo que cuesta rendimiento para nada. La plantilla solo se compone si de verdad se va a escribir.

### Niveles de log y cuándo usar cada uno

| Nivel | Cuándo | Ejemplo |
|---|---|---|
| `LogTrace` | Detalle extremo de depuración | Valor de cada variable en un bucle |
| `LogDebug` | Información útil mientras desarrollas | "Consulta devolvió 42 filas" |
| `LogInformation` | Hitos normales del flujo de negocio | "Pedido 123 creado" |
| `LogWarning` | Algo raro, pero la aplicación sigue | "Reintentando conexión (2/3)" |
| `LogError` | Una operación ha fallado | Excepción capturada al guardar |
| `LogCritical` | La aplicación no puede continuar | No hay conexión a la base de datos al arrancar |

> 💡 **Tip:** el error de principiante es loguear **todo** como `Information`. Si todo es importante, nada lo es: cuando haya un incidente, tendrás que buscar la aguja en un pajar que has construido tú. Regla práctica: `Information` para hitos de negocio que le importarían a una persona de negocio, `Debug` para lo que te interesa a ti mientras programas.

> 💡 **Tip:** pasa la excepción como **primer argumento** — `_logger.LogError(ex, "mensaje {Dato}", dato)` — nunca dentro del texto. Solo así el sistema de logs captura el stack trace y las excepciones internas como datos consultables.

> ⚠️ **Cuidado — nunca loguees datos personales ni secretos.** Contraseñas, tokens, números de tarjeta, DNI, direcciones completas. Los logs se replican, se archivan y los ve mucha más gente de la que imaginas; además, el RGPD aplica a los logs igual que a la base de datos. Si necesitas rastrear a un usuario, usa su Id, no su email.

---

## 12.2 `appsettings.json` e `IConfiguration`

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Biblioteca;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "Biblioteca": {
    "DiasPrestamo": 15,
    "MaxLibrosPorUsuario": 3,
    "EmailNotificaciones": "biblioteca@espiralms.com"
  }
}
```

> 💡 **Tip:** ese `"Microsoft.EntityFrameworkCore.Database.Command": "Information"` es el interruptor para **ver todo el SQL que genera EF Core** en la consola, sin tocar una línea de código. Es la forma más rápida de diagnosticar un N+1 (Lección 8.7) en un proyecto que no conoces.

### Configuración por entorno

.NET carga los archivos **en cascada**: primero `appsettings.json` y encima `appsettings.{Entorno}.json`, que sobrescribe solo las claves que redefine.

```
appsettings.json                  ← base, común a todos los entornos
appsettings.Development.json      ← sobrescribe en tu máquina
appsettings.Production.json       ← sobrescribe en el servidor
```

El entorno lo determina la variable `ASPNETCORE_ENVIRONMENT` (`Development`, `Staging`, `Production`).

**Orden de prioridad (de menor a mayor):** appsettings.json → appsettings.{Entorno}.json → *user secrets* (solo en desarrollo) → variables de entorno → argumentos de línea de comandos. **Gana siempre el último.**

> ⚠️ **Cuidado — regla absoluta: ningún secreto en `appsettings.json`.** Contraseñas de base de datos, claves de API, cadenas de conexión con credenciales. Ese archivo va a Git, y lo que entra en el historial de Git **no se borra fácilmente**. En desarrollo se usan *user secrets*; en producción, variables de entorno o un gestor de secretos (Azure Key Vault y similares).
> ```bash
> dotnet user-secrets init
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=..."
> ```
> Los secretos se guardan **fuera** de la carpeta del proyecto y se cargan automáticamente en desarrollo. Este es, literalmente, uno de los errores que más aparecen en las auditorías de seguridad de proyectos reales.

---

## 12.3 Options pattern — configuración tipada

Leer la configuración con cadenas mágicas (`_config["Biblioteca:DiasPrestamo"]`) funciona, pero es frágil: sin tipos, sin autocompletado, y un error tipográfico no se detecta hasta que falla en tiempo de ejecución. La forma profesional es mapear la sección a una clase:

```csharp
// 1. Una clase que refleja la sección del JSON
public class BibliotecaOptions
{
    public const string SeccionNombre = "Biblioteca";

    public int DiasPrestamo { get; set; } = 15;
    public int MaxLibrosPorUsuario { get; set; } = 3;
    public string EmailNotificaciones { get; set; } = string.Empty;
}

// 2. Registrarla en Program.cs
builder.Services.Configure<BibliotecaOptions>(
    builder.Configuration.GetSection(BibliotecaOptions.SeccionNombre));

// 3. Inyectarla donde haga falta
public class LibroServicio
{
    private readonly BibliotecaOptions _opciones;

    public LibroServicio(IOptions<BibliotecaOptions> opciones)
        => _opciones = opciones.Value;

    public DateTime CalcularFechaDevolucion()
        => DateTime.Today.AddDays(_opciones.DiasPrestamo);   // tipado, con autocompletado
}
```

> 💡 **Tip:** las tres variantes que verás y su diferencia real:
> - `IOptions<T>`: se lee una vez al arrancar. **Es el que quieres el 95% de las veces.**
> - `IOptionsSnapshot<T>`: se relee en cada petición (scoped). Útil si la configuración cambia en caliente.
> - `IOptionsMonitor<T>`: notifica cambios; el único que funciona dentro de un singleton.

> 💡 **Tip de nivel senior:** puedes **validar la configuración al arrancar**, de modo que la aplicación falle inmediatamente y con un mensaje claro si falta un valor, en vez de reventar tres horas después en la primera petición que lo use:
> ```csharp
> builder.Services.AddOptions<BibliotecaOptions>()
>     .Bind(builder.Configuration.GetSection(BibliotecaOptions.SeccionNombre))
>     .ValidateDataAnnotations()
>     .ValidateOnStart();
> ```
> Con Data Annotations (`[Required]`, `[Range]`) en la clase de opciones, tienes validación de configuración prácticamente gratis. *Fail fast*: es muchísimo mejor que arranque mal a las 9:00 con un error claro que a las 17:00 con un `NullReferenceException` sin contexto.

---

## 12.4 Cómo se ve todo junto

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuración tipada y validada
builder.Services.AddOptions<BibliotecaOptions>()
    .Bind(builder.Configuration.GetSection(BibliotecaOptions.SeccionNombre))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Cadena de conexión desde configuración, NUNCA hardcodeada
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// El logger también está disponible aquí
app.Logger.LogInformation("Aplicación iniciada en entorno {Entorno}", app.Environment.EnvironmentName);

app.Run();
```

**Por qué importa en Espiral MS:** el primer día, cuando clones el repositorio y la aplicación no arranque, el 90% de las veces será por configuración — una cadena de conexión que apunta a una base de datos que no tienes, o un secreto que debes pedir a un compañero porque (correctamente) no está en Git. Saber dónde mirar (`appsettings.json`, `appsettings.Development.json`, variables de entorno, user secrets, y en ese orden de prioridad) te ahorra la primera mañana entera.

---

## 12.5 Ejercicios Lección 12

1. Inyecta `ILogger<T>` en tu `LibroServicio` y añade logs de `Information`, `Warning` y `Error` en los puntos adecuados.
2. Reescribe un log con interpolación `$"..."` a plantilla estructurada `"... {Dato}", dato` y explica por escrito qué se gana.
3. Cambia el nivel mínimo en `appsettings.json` a `Warning` y comprueba qué logs desaparecen de la consola.
4. Activa `"Microsoft.EntityFrameworkCore.Database.Command": "Information"` y observa el SQL real que genera una de tus consultas LINQ.
5. Crea `appsettings.Development.json` con un valor distinto para `DiasPrestamo` y comprueba cuál gana al arrancar.
6. Crea la clase `BibliotecaOptions`, regístrala con `Configure<T>` e inyéctala con `IOptions<T>` en un servicio.
7. Añade `[Required]` y `[Range(1, 90)]` a las propiedades de `BibliotecaOptions`, activa `ValidateOnStart()` y borra el valor del JSON para ver el error de arranque.
8. Mueve la cadena de conexión a *user secrets* con `dotnet user-secrets` y verifica que la aplicación sigue funcionando sin que el secreto esté en ningún archivo del repositorio.
9. Revisa tu propio código y busca cualquier valor "mágico" escrito a fuego (un email, un número de días, una URL). Muévelo a configuración.

---

# Lección 13: Git Avanzado

[[#Índice|↑ Volver al índice]]

## 13.1 Git Flow — el modelo de ramas estándar en empresa

```
main (producción)
├── release/v1.0
└── develop (integración)
    ├── feature/login
    ├── feature/reportes
    └── hotfix/bug-critico
```

**Ramas principales:**
- `main`: código en producción, solo recibe merges desde `release` o `hotfix`
- `develop`: rama de integración, donde confluyen todas las features terminadas
- `feature/*`: una rama por cada funcionalidad nueva, nace y muere en `develop`
- `hotfix/*`: corrección urgente directamente sobre `main` (bug crítico en producción)
- `release/*`: preparación de una versión antes de pasarla a `main` (últimos ajustes, testing)

---

## 13.2 Comandos básicos de repaso

```bash
git init                          # crear un repo nuevo
git clone <url>                   # clonar un repo remoto
git status                        # ver el estado actual (cambios, staging)
git add archivo.cs                # añadir un archivo al staging
git add .                         # añadir todos los cambios
git commit -m "mensaje"           # crear un commit
git push origin nombre-rama       # subir cambios al remoto
git pull origin nombre-rama       # traer y fusionar cambios del remoto
git log --oneline                 # historial compacto de commits
```

> 💡 **Tip — `git stash`, el botón de pausa:** te piden mirar un bug urgente y tienes cambios a medias sin terminar. `git stash` los guarda y deja el directorio limpio; `git stash pop` los recupera cuando vuelves. Ponle nombre (`git stash push -m "wip validación"`) porque a las tres semanas tendrás cuatro stashes y ni idea de qué es cada uno.

> 💡 **Tip — `git reflog`, la red de seguridad:** ¿has hecho un `reset --hard` y crees que has perdido un commit? `git reflog` lista **todo** lo que ha estado apuntando tu HEAD en las últimas semanas, incluyendo commits ya "borrados". `git checkout <hash>` los recupera. En Git es asombrosamente difícil perder de verdad algo que ya habías *commiteado*; lo que sí se pierde para siempre es lo que nunca llegaste a confirmar.

> ⚠️ **Cuidado:** `git add .` añade todo lo que haya cambiado, incluyendo archivos que no querías subir (un `appsettings.json` con una contraseña, un volcado de base de datos, una carpeta `bin/`). Acostúmbrate a mirar `git status` antes y, mejor aún, a usar `git add -p` para revisar cambio por cambio lo que estás confirmando. Es más lento y detecta la mitad de los descuidos.

---

## 13.3 Crear y trabajar con ramas

```bash
git branch -a                                    # ver todas las ramas (locales y remotas)
git checkout develop
git pull origin develop
git checkout -b feature/autenticacion-usuarios    # crear y cambiar a una rama nueva

# Hacer cambios y commits
git add .
git commit -m "feat: implementar login con JWT"
git commit -m "fix: validar email en registro"

# Enviar la rama al servidor
git push origin feature/autenticacion-usuarios

# Sintaxis moderna equivalente a checkout -b
git switch -c feature/autenticacion-usuarios
```

### Convención de mensajes de commit (Conventional Commits)

Un estándar muy usado en empresas — cada mensaje empieza con un prefijo que indica el tipo de cambio:

| Prefijo | Significado |
|---|---|
| `feat:` | Nueva funcionalidad |
| `fix:` | Corrección de un bug |
| `refactor:` | Cambio interno sin alterar comportamiento |
| `docs:` | Solo documentación |
| `test:` | Añadir o modificar tests |
| `chore:` | Tareas de mantenimiento (dependencias, config) |

```bash
git commit -m "feat: añadir endpoint de exportación a PDF"
git commit -m "fix: corregir cálculo de IVA en el total del pedido"
```

---

## 13.4 Pull Requests (PRs) — el flujo real en equipo

Un pull request es una solicitud para fusionar tu rama en otra (normalmente `develop`).

**Proceso típico en empresa:**
1. Terminas tu feature en `feature/autenticacion`
2. `git push origin feature/autenticacion`
3. En GitHub/GitLab/Azure DevOps, creas un PR describiendo qué cambia y por qué
4. Asignas un revisor (code review)
5. El revisor comenta, aprueba, o pide cambios
6. Una vez aprobado, se mergea a `develop` (normalmente con "squash and merge" para mantener el historial limpio)
7. La rama feature se elimina

**Buenas prácticas al abrir un PR:**
- Título claro y descriptivo, siguiendo la misma convención que los commits
- Descripción de qué se hizo y por qué (no solo "arreglé un bug")
- PRs pequeños y enfocados — más fáciles de revisar que uno gigante con 20 cambios distintos
- Enlazar el ticket/issue relacionado si el equipo usa Jira o similar

> 💡 **Tip — revísate el PR a ti mismo antes de pedirlo.** Abre la pestaña de cambios en GitHub y léelo como si fuera de otra persona. Encontrarás código comentado, `Console.WriteLine` de depuración, archivos que no tocan nada y nombres a medio renombrar. Cinco minutos tuyos ahorran una ronda entera de revisión, y la impresión que causa es muy distinta.

> 💡 **Tip:** deja tú mismo comentarios en tu PR explicando las decisiones no obvias ("uso `AsNoTracking` aquí porque el resultado solo se serializa"). Es la diferencia entre que el revisor pregunte y que apruebe directamente.

> ⚠️ **Cuidado con los comentarios de code review:** no son ataques personales, por directos que suenen. La respuesta correcta a "esto tendría que ser Scoped, no Singleton" no es defenderse, es preguntar por qué si no lo entiendes, y cambiarlo si lo entiendes. **Recibir bien las críticas técnicas es, con diferencia, lo que más rápido te hace crecer en unas prácticas** — y lo que más se nota desde fuera.

> 💡 **Tip:** un PR que lleva dos días parado no se desbloquea solo. Un mensaje corto y educado por el canal del equipo ("¿puede alguien echarle un ojo al PR #23 cuando tenga un rato?") es lo esperado, no una molestia.

---

## 13.5 Mergear ramas localmente

```bash
git checkout develop
git pull origin develop
git merge feature/autenticacion

# Si hay conflictos, Git te lo indica:
# CONFLICT (content): Merge conflict in Libro.cs

git status   # ver qué archivos tienen conflicto

# Abrir el archivo y buscar los marcadores de conflicto:
# <<<<<<< HEAD
# tu código actual en develop
# =======
# el código de la rama que estás mergeando
# >>>>>>> feature/autenticacion

# Tras resolver manualmente y elegir qué código conservar:
git add Libro.cs
git commit -m "merge: resolver conflictos en Libro.cs"
git push origin develop
```

---

## 13.6 Rebase — alternativa más limpia a merge

```bash
git checkout feature/autenticacion
git rebase develop            # reaplica tus commits encima de los últimos cambios de develop

# Si hay conflictos durante el rebase:
git add archivo-resuelto.cs
git rebase --continue

# Si te arrepientes a mitad del rebase:
git rebase --abort

# Como el rebase reescribe el historial, el push normal falla; hace falta forzar:
git push origin feature/autenticacion --force-with-lease
```

**`--force-with-lease` en vez de `--force`:** es más seguro porque falla si alguien más subió cambios a esa rama remota que tú no tienes localmente, evitando sobrescribir el trabajo de otra persona por accidente.

**Merge vs Rebase — cuándo usar cada uno:**
- **Merge**: conserva el historial exacto de cómo ocurrieron las cosas, incluye un commit de merge. Más seguro para ramas compartidas por varias personas.
- **Rebase**: historial más limpio y lineal, sin commits de merge extra. Ideal para tu propia rama feature antes de abrir el PR, nunca sobre una rama que otros ya están usando.

---

## 13.7 Deshacer cambios

```bash
git restore archivo.cs                    # descartar cambios no confirmados en un archivo
git restore --staged archivo.cs           # sacar un archivo del staging (sin perder el cambio)
git reset --soft HEAD~1                   # deshacer el último commit, conservando los cambios
git reset --hard HEAD~1                   # deshacer el último commit, DESCARTANDO los cambios (¡cuidado!)
git revert <hash-del-commit>              # crear un commit nuevo que deshace uno anterior (seguro para ramas compartidas)
```

**Regla de oro:** en una rama compartida con el equipo, usa siempre `revert` (crea un commit nuevo) en vez de `reset` (reescribe el historial) — así nadie más se rompe al hacer `pull`.

> 💡 **Tip:** `git commit --amend` corrige el último commit (mensaje o contenido) sin crear uno nuevo. Perfecto para el típico "se me olvidó un archivo" o una errata en el mensaje... **pero solo si aún no lo has subido**. Si ya hiciste `push`, amend reescribe el historial y tendrás que forzar el push, con los mismos riesgos que un rebase.

> ⚠️ **Cuidado:** `git reset --hard` y `git checkout -- archivo` **borran cambios no confirmados de forma irrecuperable**. `reflog` no te salva de esto, porque nunca llegaron a ser un commit. Si dudas, haz `git stash` en vez de `reset --hard`: consigues el mismo directorio limpio y conservas la posibilidad de arrepentirte.

---

## 13.8 Etiquetas (tags) para versiones

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
git tag                        # listar todas las etiquetas
git checkout v1.0.0            # ver el código exacto de esa versión
```

---

## 13.9 Buscando en el historial

```bash
git log archivo.cs                              # commits que tocaron un archivo concreto
git log --author="tu-nombre"                     # commits de una persona
git log --since="2026-01-01" --until="2026-02-01" # commits en un rango de fechas
git blame archivo.cs                             # quién cambió cada línea y en qué commit
git diff develop feature/autenticacion            # diferencias entre dos ramas
```

---

## 13.10 .gitignore — qué no subir al repositorio

```
# .gitignore típico para un proyecto .NET
bin/
obj/
*.user
.vs/
appsettings.Development.json
```

Nunca se sube al repositorio: binarios compilados (`bin/`, `obj/`), configuración local con credenciales, ni archivos temporales del IDE.

---

## 13.11 Ejercicios Lección 13

1. Crea una rama `feature/nueva-funcionalidad` desde `develop`
2. Haz 3 commits con mensajes siguiendo Conventional Commits
3. Cambia a `develop` y haz un commit ahí también (para forzar un conflicto)
4. Mergea `feature/nueva-funcionalidad` a `develop` y resuelve el conflicto
5. Crea una etiqueta `v0.1.0` en develop
6. Practica `git rebase` en una rama feature nueva antes de mergearla
7. Simula un error: haz un commit, luego deshazlo con `git revert`
8. Crea un `.gitignore` apropiado para un proyecto C#/.NET

---

# Lección 14: Scrum y Agile

[[#Índice|↑ Volver al índice]]

## 14.1 ¿Qué es Scrum?

**Scrum** es un marco de trabajo ágil para gestionar proyectos complejos mediante ciclos cortos e iterativos llamados **sprints**. En vez de planificar todo el proyecto de golpe (modelo "cascada"), el equipo entrega software funcionando cada 1-2 semanas y ajusta el rumbo según feedback real.

**Por qué se usa en empresas de software:** los requisitos cambian, y Scrum asume eso desde el diseño en vez de luchar contra ello.

---

## 14.2 Roles

| Rol | Responsabilidad |
|---|---|
| **Product Owner (PO)** | Define qué construir, prioriza el backlog, acepta o rechaza el trabajo terminado |
| **Scrum Master** | Facilita el proceso, elimina obstáculos, protege al equipo de interrupciones externas |
| **Development Team** | Implementa las features, se auto-organiza, típicamente 5-8 personas |

**Importante:** el Scrum Master no es un jefe de proyecto tradicional — no asigna tareas, facilita que el equipo se organice solo.

---

## 14.3 Artefactos (Artifacts)

| Artefacto | Qué es | Ejemplo |
|---|---|---|
| **Product Backlog** | Lista completa de todo lo pendiente, ordenada por prioridad | Login, reportes, API de pagos, exportar a PDF... |
| **Sprint Backlog** | Subconjunto del backlog que el equipo se compromete a hacer en este sprint | 5-6 items para las próximas 2 semanas |
| **Increment** | El resultado tangible: software funcionando y probado al final del sprint | Versión desplegable con las nuevas features |

---

## 14.4 El ciclo de un Sprint

Un sprint dura típicamente **1-2 semanas** (2 semanas es lo más común en entornos empresariales, incluido probablemente Espiral MS).

### Sprint Planning (inicio del sprint)

Duración: 2-4 horas. El equipo y el PO seleccionan y desglosan los items del backlog.

```
PO: "Necesitamos poder exportar el catálogo de libros a PDF"

Team desglosa:
  - Investigar librería de generación de PDF (2 pts)
  - Crear endpoint /api/libros/exportar (3 pts)
  - Diseñar plantilla del PDF (2 pts)
  - Tests de integración (1 pt)

Total: 8 puntos. El equipo se compromete a esto para el sprint.
```

### Daily Standup (cada día, 15 minutos máximo)

Cada persona responde brevemente:
- ¿Qué hice ayer?
- ¿Qué haré hoy?
- ¿Tengo algún obstáculo (blocker)?

```
Dev: "Ayer terminé el endpoint de exportación.
      Hoy empiezo la plantilla del PDF.
      Obstáculo: necesito acceso a la librería iTextSharp, aún no la tengo instalada."

Scrum Master: "Anotado, te ayudo con el acceso después de la reunión."
```

**Regla de oro:** el standup no es para resolver problemas en el momento, sino para detectarlos. La resolución ocurre después, fuera de la reunión.

> 💡 **Tip para tu primer standup:** prepara tus tres frases **antes** de la reunión, aunque sean dos líneas en un papel. Los standups improvisados se alargan y se convierten en un relato confuso. Treinta segundos de preparación te hacen parecer (y ser) mucho más organizado.

> ⚠️ **Cuidado — el error clásico del becario:** callarse que estás bloqueado por vergüenza o por "no molestar", y pasar dos días atascado en algo que un compañero resuelve en diez minutos. **En un equipo ágil, decir "estoy bloqueado con X" es exactamente lo que se espera de ti**; no decirlo es el problema real. Una norma sensata: si llevas más de media hora atascado sin ningún avance, pregunta.

> 💡 **Tip:** "Ayer hice cosas de la tarea del login" no informa a nadie. "Ayer terminé el endpoint de login, hoy empiezo la validación del token, sin bloqueos" sí. Concreto, en términos de trabajo terminado, no de tiempo invertido.

### Sprint Review (final del sprint, 1-2 horas)

El equipo hace una **demo en vivo** de lo que terminó, ante el PO y stakeholders. No son diapositivas — es software funcionando.

### Sprint Retrospective (final del sprint, 1 hora)

El equipo reflexiona sobre el propio proceso de trabajo (no sobre el producto):

```
Qué salió bien:
- La comunicación entre backend y frontend fue fluida
- Detectamos un bug crítico gracias a los tests automatizados

Qué no salió bien:
- Subestimamos la complejidad de la plantilla PDF (2 pts se convirtieron en 5)
- El PO no estuvo disponible dos días para resolver dudas

Acciones para el próximo sprint:
- Reservar 30 min diarios de disponibilidad del PO
- Añadir un margen de contingencia al estimar tareas con librerías nuevas
```

---

## 14.5 Story Points y estimación

Scrum no estima en horas exactas, sino en **complejidad relativa** usando Story Points. Escala típica (Fibonacci): **1, 2, 3, 5, 8, 13, 21**

| Puntos | Complejidad aproximada |
|---|---|
| 1 | Trivial — menos de 1 hora (cambiar un texto, un color) |
| 2 | Pequeño — 2-4 horas (endpoint GET simple sin lógica) |
| 3 | Pequeño-medio — 4-8 horas (endpoint con validaciones) |
| 5 | Medio — 1-2 días (feature con lógica de negocio moderada) |
| 8 | Grande — 2-3 días (feature con BD, validaciones y tests) |
| 13+ | Muy grande — debe desglosarse en tareas más pequeñas |

### Planning Poker (técnica de estimación en grupo)

1. El PO explica la feature
2. Cada desarrollador estima en secreto (con cartas físicas o una app)
3. Todos revelan su estimación a la vez
4. Si hay diferencias grandes, se discute el porqué y se re-estima

```
Feature: "Dashboard de estadísticas de préstamos"

Dev A: 5 puntos → "Pensé que era solo una tabla con datos"
Dev B: 8 puntos → "Pero hay que agregar por fechas, cachear resultados y hacer gráficos"
Dev A: "Ah, no había pensado en el caching. Cambio a 8."

Consenso final: 8 puntos
```

Este proceso es valioso no tanto por el número final, sino porque **obliga a discutir la complejidad real antes de empezar a programar**.

> ⚠️ **Cuidado — los story points NO son horas.** Miden complejidad e incertidumbre, no tiempo. Una tarea de 2 puntos que ya has hecho diez veces puede llevarte más horas que una de 5 que es rutinaria pero larga. Intentar convertirlos a horas ("aquí 1 punto = 4 horas") vacía el concepto y convierte la estimación en un compromiso de plazos — que es justo lo que Scrum intenta evitar.

> 💡 **Tip:** si no sabes estimar algo porque desconoces la tecnología, **eso es información valiosa, no una debilidad**. Lo que se hace en ese caso es proponer un *spike*: una tarea acotada en tiempo ("4 horas para investigar la librería de PDF") cuyo entregable es *saber*, no código. Proponer un spike en tu primer Planning demuestra criterio.

> 💡 **Tip:** no estimes a la baja para quedar bien. Todo el mundo se da cuenta al final del sprint, y el daño (a la planificación del equipo y a tu credibilidad) es mucho mayor que el de haber dicho un número alto al principio.

---

## 14.6 Velocidad del equipo (Velocity)

La **velocidad** es la cantidad de story points que el equipo completa, de media, por sprint. Se usa para planificar sprints futuros con datos reales en vez de suposiciones.

```
Sprint 1: comprometidos 20 pts, completados 18 pts
Sprint 2: comprometidos 22 pts, completados 21 pts
Sprint 3: comprometidos 20 pts, completados 19 pts

Velocidad media: ~19-20 puntos por sprint

→ Para el Sprint 4, el equipo no debería comprometerse a más de ~20 puntos,
  aunque el backlog tenga 35 puntos de features "deseables".
```

---

## 14.7 Tablero Kanban (herramienta de seguimiento visual)

Muchos equipos Scrum usan un tablero Kanban (en Jira, Azure DevOps o Trello) para visualizar el flujo del sprint:

```
TO DO              IN PROGRESS         REVIEW              DONE
─────────────      ─────────────       ─────────────       ─────────────
Exportar PDF        Endpoint login      Validar JWT          Crear BD usuarios
(5 pts)              (Dev1, en curso)    (Dev2, esperando      (- completado)
                                          aprobación)
Plantilla PDF                                                Generar JWT
(2 pts)                                                       (- completado)
```

El equipo mueve las tarjetas de izquierda a derecha durante el sprint. Un vistazo al tablero dice inmediatamente dónde está el cuello de botella.

---

## 14.8 Qué esperar en Espiral MS

Con alta probabilidad, en las prácticas:
- Sprints de 2 semanas, con Jira o Azure DevOps para el backlog
- Daily standup a primera hora de la mañana
- Sprint Planning el primer día del sprint, Review/Retro el último
- Se te pedirá estimar tus propias tareas en story points

**Consejo práctico:** al principio, tus estimaciones probablemente sean optimistas (es normal, le pasa a todo el mundo). Lo importante no es acertar siempre, sino comunicar pronto cuando algo se complica más de lo esperado — eso es lo que un equipo Scrum realmente valora.

---

## 14.9 Ejercicio práctico

Simula un mini-sprint sobre tu proyecto de biblioteca:

1. Escribe 5 features nuevas para el Gestor de Biblioteca (ej: "renovar préstamo", "historial de un usuario", "notificar por email al devolver")
2. Estima cada una en story points usando la tabla de arriba
3. Selecciona 3-4 que sumen entre 10-13 puntos (tu "sprint")
4. Implementa una de ellas
5. Escribe 2 líneas de "retrospectiva": qué salió bien, qué mejorarías

---

# Lección 15: Frontend

[[#Índice|↑ Volver al índice]]

## 15.1 HTML5 Semántico

Tener nociones básicas de HTML no es lo mismo que escribirlo con **semántica** — usar la etiqueta que realmente describe el contenido, no solo `<div>` para todo.

```html
<!-- MAL: sin semántica, todo son divs -->
<div>
  <div>Mi Blog</div>
  <div>
    <div><a href="/">Home</a></div>
  </div>
</div>

<!-- BIEN: cada etiqueta describe su función -->
<header>
  <h1>Mi Blog</h1>
  <nav>
    <ul>
      <li><a href="/">Home</a></li>
      <li><a href="/about">About</a></li>
    </ul>
  </nav>
</header>

<main>
  <article>
    <h2>Primer post</h2>
    <p>Contenido del artículo...</p>
    <footer>
      <time datetime="2026-07-12">12 de julio de 2026</time>
    </footer>
  </article>
</main>

<aside>
  <h3>Sidebar</h3>
  <p>Contenido secundario...</p>
</aside>

<footer>
  <p>&copy; 2026 Mi Blog</p>
</footer>
```

### Tags semánticos clave

| Tag | Uso |
|---|---|
| `<header>` | Cabecera de página o de una sección |
| `<nav>` | Bloque de navegación |
| `<main>` | Contenido principal (uno solo por página) |
| `<article>` | Contenido independiente y reutilizable |
| `<section>` | Agrupación de contenido relacionado |
| `<aside>` | Contenido lateral o secundario |
| `<figure>` + `<figcaption>` | Imagen con leyenda |
| `<time>` | Fecha u hora, en formato legible por máquina |

**Por qué importa:** los lectores de pantalla (accesibilidad) y los motores de búsqueda entienden mucho mejor la estructura de la página con etiquetas semánticas que con `<div>` genéricos.

---

## 15.2 Formularios HTML5

```html
<form>
  <label for="email">Correo electrónico:</label>
  <input type="email" id="email" name="email" required>

  <label for="edad">Edad:</label>
  <input type="number" id="edad" name="edad" min="18" max="120">

  <label for="fecha">Fecha de nacimiento:</label>
  <input type="date" id="fecha" name="fecha">

  <button type="submit">Enviar</button>
</form>
```

Los tipos `email`, `number`, `date` activan validación nativa del navegador sin escribir JavaScript.

> ⚠️ **Cuidado — la validación del navegador es comodidad, no seguridad.** Cualquiera puede desactivar JavaScript, editar el HTML desde las herramientas de desarrollo o enviar la petición directamente con Postman. **Todo dato que llegue al servidor debe validarse otra vez en el servidor**, siempre, sin excepción (Lección 10.5). La validación del cliente es para que el usuario no pierda tiempo; la del servidor es para que tu sistema no se rompa.

> 💡 **Tip:** el `<label for="...">` no es decoración: hace que al pulsar sobre el texto se enfoque el campo, y es lo que permite a un lector de pantalla anunciar para qué sirve cada input. Un formulario sin labels es inaccesible, y en muchos contratos públicos la accesibilidad es un requisito legal, no una preferencia.

---

## 15.3 CSS3 — Flexbox y Grid

### Flexbox (alineación en una dimensión)

```css
.contenedor {
  display: flex;
  justify-content: space-between;  /* alineación horizontal */
  align-items: center;              /* alineación vertical */
  gap: 20px;                        /* espacio entre elementos */
  flex-wrap: wrap;                  /* permite que los items salten de línea */
}

.item {
  flex: 1;  /* crece proporcionalmente para llenar el espacio disponible */
}
```

### Grid (alineación en dos dimensiones)

```css
.grid-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr);  /* 3 columnas de igual ancho */
  grid-template-rows: auto;
  gap: 20px;
}

.item-destacado {
  grid-column: span 2;  /* ocupa 2 columnas */
  grid-row: span 2;     /* ocupa 2 filas */
}
```

**Cuándo usar cada uno:** Flexbox para alinear elementos en una fila o columna (una navbar, una lista de tarjetas en línea). Grid para layouts de página completos con filas y columnas a la vez.

### Responsive design con media queries

```css
/* Móvil primero (mobile-first) */
.header {
  font-size: 18px;
  padding: 10px;
}

/* Tablets */
@media (min-width: 768px) {
  .header {
    font-size: 24px;
    padding: 20px;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .header {
    font-size: 32px;
  }
}
```

---

## 15.4 JavaScript Moderno (ES6+)

### Arrow functions

```javascript
// Sintaxis clásica
function sumar(a, b) {
  return a + b;
}

// Arrow function equivalente
const sumar = (a, b) => a + b;

// Con cuerpo de varias líneas
const procesar = (dato) => {
  console.log("Procesando...");
  return dato * 2;
};
```

### Destructuring

```javascript
// De arrays
const [nombre, edad, ciudad] = ["Ana", 30, "Madrid"];

// De objetos
const usuario = { id: 1, email: "ana@example.com" };
const { id, email } = usuario;

// En parámetros de función directamente
const mostrar = ({ id, email }) => console.log(`${id}: ${email}`);
```

### Template literals

```javascript
const nombre = "Juan";
const edad = 25;

// Antes (concatenación manual)
console.log("Hola, " + nombre + ". Tienes " + edad + " años.");

// Ahora (template literal)
console.log(`Hola, ${nombre}. Tienes ${edad} años.`);
```

### Spread y Rest operators

```javascript
// Spread: expandir un array/objeto
const numeros = [1, 2, 3];
const masNumeros = [...numeros, 4, 5]; // [1, 2, 3, 4, 5]

const persona = { nombre: "Ana" };
const personaCompleta = { ...persona, edad: 30 }; // añade edad sin mutar el original

// Rest: agrupar argumentos sobrantes
function sumarTodos(...numeros) {
  return numeros.reduce((acc, n) => acc + n, 0);
}
sumarTodos(1, 2, 3, 4); // 10
```

### async/await en JavaScript

```javascript
// Antes (promesas encadenadas)
fetch("/api/usuarios")
  .then(response => response.json())
  .then(usuarios => console.log(usuarios))
  .catch(error => console.error(error));

// Ahora (async/await, igual de concepto que en C#)
async function obtenerUsuarios() {
  try {
    const response = await fetch("/api/usuarios");
    const usuarios = await response.json();
    console.log(usuarios);
  } catch (error) {
    console.error(error);
  }
}
```

### Array methods (el equivalente JS de LINQ)

```javascript
const numeros = [1, 2, 3, 4, 5];

const pares = numeros.filter(n => n % 2 === 0);           // como .Where()
const cuadrados = numeros.map(n => n * n);                 // como .Select()
const primero = numeros.find(n => n > 2);                  // como .FirstOrDefault()
const suma = numeros.reduce((acc, n) => acc + n, 0);        // agregación
const hayAlguno = numeros.some(n => n > 4);                 // como .Any()
const todosPositivos = numeros.every(n => n > 0);           // como .All()
```

> 🧠 **Mentalidad C# → JavaScript:** a diferencia de LINQ, estos métodos **no son perezosos**: cada uno crea un array nuevo inmediatamente. Encadenar `.filter().map().filter()` sobre 100.000 elementos recorre la colección tres veces y crea tres arrays. Con listas normales da igual; con datos grandes, importa.

> ⚠️ **Cuidado con `fetch`:** no lanza error ante un 404 o un 500. La promesa se resuelve **correctamente** con una respuesta cuyo `ok` es `false`. Solo falla si hay un error de red. Hay que comprobarlo a mano:
> ```javascript
> const response = await fetch("/api/libros");
> if (!response.ok) throw new Error(`Error ${response.status}`);
> const datos = await response.json();
> ```
> Es exactamente el caso contrario a `HttpClient` de C#, donde `EnsureSuccessStatusCode()` lo hace por ti. Mucha gente pierde horas aquí.

> 💡 **Tip:** cuando pintes HTML a partir de datos, **nunca** metas texto del usuario directamente con `innerHTML`: es una vulnerabilidad XSS de manual (alguien registra un usuario llamado `<script>...</script>` y su código se ejecuta en el navegador de todos). Usa `textContent` para texto, o construye los nodos con `document.createElement`.

---

## 15.5 Ejercicio práctico

1. Crea una página HTML con estructura semántica completa (header, nav, main, article, aside, footer)
2. Usa Flexbox para una barra de navegación y Grid para una galería de tarjetas
3. Añade media queries para que se vea bien en móvil y en desktop
4. Escribe JavaScript que obtenga datos de `https://jsonplaceholder.typicode.com/users` con `fetch` + `async/await`
5. Muestra los usuarios dinámicamente en la página usando `.map()` para generar el HTML
6. Añade un campo de texto que filtre los usuarios por nombre en tiempo real usando `.filter()`

---

# Lección 16: TypeScript

[[#Índice|↑ Volver al índice]]

## 16.1 ¿Qué es TypeScript y por qué usarlo?

TypeScript es JavaScript con **tipos estáticos**, desarrollado por Microsoft. Compila a JavaScript normal — el navegador nunca ejecuta TypeScript directamente.

```typescript
// JavaScript: sin tipos, el error solo aparece en tiempo de ejecución
function sumar(a, b) {
  return a + b;
}
sumar("5", 3); // "53" — concatenó en vez de sumar, bug silencioso

// TypeScript: el compilador detecta el error ANTES de ejecutar
function sumar(a: number, b: number): number {
  return a + b;
}
sumar("5", 3); // Error de compilación: string no asignable a number
```

**Por qué importa en Espiral MS:** si el frontend usa Angular o un stack TypeScript, esta detección temprana de errores ahorra muchísimo tiempo de debugging comparado con JavaScript puro.

---

## 16.2 Tipos básicos

```typescript
const nombre: string = "Ana";
const edad: number = 30;
const activo: boolean = true;

// any: evita el tipado, úsalo solo como último recurso
let cualquiera: any = "hola";
cualquiera = 123; // válido, pero pierdes toda la protección de tipos

// union types: la variable puede ser uno de varios tipos
let id: number | string = 1;
id = "ABC123"; // también válido

// type alias: definir una "forma" reutilizable de objeto
type Usuario = {
  id: number;
  nombre: string;
  email?: string; // el "?" indica propiedad opcional
};

const user: Usuario = { id: 1, nombre: "Ana" }; // email es opcional, se puede omitir

// arrays tipados
const numeros: number[] = [1, 2, 3];
const nombres: Array<string> = ["Ana", "Luis"];
```

> ⚠️ **Cuidado — `any` es contagioso.** Un solo `any` desactiva la comprobación de tipos en toda la cadena de valores que salen de él, y como los errores desaparecen en silencio, es fácil creer que el código está bien tipado cuando no lo está. Si de verdad no sabes el tipo, usa `unknown`: te obliga a comprobarlo antes de usarlo, que es justo lo que quieres.

> 💡 **Tip:** los tipos de TypeScript **se borran al compilar**. No existen en tiempo de ejecución, igual que los generics de Java con *type erasure* (y al revés que en C#, ver Lección 4). Por eso, si recibes un JSON de tu API .NET y lo tipas como `Usuario`, TypeScript **te cree sin comprobar nada**: si la API cambió y ya no manda `email`, no habrá error de compilación y fallará en el navegador. Para validar de verdad la forma de un JSON en tiempo de ejecución hace falta una librería como Zod.

> 💡 **Tip:** `type` e `interface` son casi intercambiables para describir objetos. La convención práctica: `interface` para la forma de objetos y contratos (se puede extender y reabrir), `type` para uniones, alias y tipos calculados (`type Estado = "activo" | "inactivo"`). Ese tipo de unión de literales es el equivalente ligero de un `enum` de C#, y es el que verás más en código TypeScript moderno.

---

## 16.3 Interfaces (paralelo directo con C#)

```typescript
interface IRepositorio<T> {
  obtenerTodos(): Promise<T[]>;
  obtenerPorId(id: number): Promise<T>;
  agregar(item: T): Promise<number>;
}

class RepositorioUsuarios implements IRepositorio<Usuario> {
  async obtenerTodos(): Promise<Usuario[]> {
    const response = await fetch("/api/usuarios");
    return response.json();
  }

  async obtenerPorId(id: number): Promise<Usuario> {
    const response = await fetch(`/api/usuarios/${id}`);
    return response.json();
  }

  async agregar(usuario: Usuario): Promise<number> {
    const response = await fetch("/api/usuarios", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(usuario)
    });
    const resultado = await response.json();
    return resultado.id;
  }
}
```

Esta estructura es literalmente el mismo patrón Repository que viste en C# (Lección 6), solo que en TypeScript.

---

## 16.4 Generics en TypeScript

```typescript
// Función genérica
function obtenerPrimero<T>(lista: T[]): T {
  if (lista.length === 0) throw new Error("Lista vacía");
  return lista[0];
}

const numeros = [1, 2, 3];
const primero = obtenerPrimero(numeros); // tipo inferido: number

// Clase genérica
class Contenedor<T> {
  constructor(private valor: T) {}

  obtener(): T {
    return this.valor;
  }
}

const contenedor = new Contenedor<string>("Hola");
```

---

## 16.5 Enums

```typescript
enum EstadoPedido {
  Pendiente,
  Procesando,
  Enviado,
  Entregado
}

let estado: EstadoPedido = EstadoPedido.Procesando;

// Enum con valores string (más legible en debugging/logs)
enum Rol {
  Admin = "ADMIN",
  Usuario = "USUARIO",
  Invitado = "INVITADO"
}
```

---

## 16.6 Configuración básica de un proyecto TypeScript

```bash
npm init -y
npm install typescript --save-dev
npx tsc --init          # genera tsconfig.json
npx tsc archivo.ts       # compila a archivo.js
npx tsc --watch          # recompila automáticamente al guardar
```

```json
// tsconfig.json — opciones más relevantes
{
  "compilerOptions": {
    "target": "ES2020",
    "strict": true,          // activa todas las comprobaciones estrictas de tipos
    "outDir": "./dist",
    "module": "commonjs"
  }
}
```

> 💡 **Tip:** activa `"strict": true` **desde el primer día** de un proyecto nuevo. Activarlo más tarde, sobre miles de líneas ya escritas, genera cientos de errores de golpe y acaba desactivándose "temporalmente" para siempre. `strict` incluye `strictNullChecks`, que es el equivalente TypeScript de los *nullable reference types* de C# (Lección 1.4) — exactamente la misma idea, el mismo beneficio.

> 💡 **Tip:** que TypeScript compile **no** significa que el programa funcione: el compilador genera el `.js` incluso habiendo errores de tipo, salvo que actives `"noEmitOnError": true`. No confundas "compiló" con "está bien".

---

## 16.7 Ejercicio práctico

1. Reescribe tu código JavaScript de la Lección 15 en TypeScript, añadiendo tipos explícitos
2. Define una interfaz `Usuario` con `id`, `nombre`, `email` (opcional)
3. Crea una interfaz genérica `IRepositorio<T>` y una clase `RepositorioUsuarios` que la implemente
4. Añade un `enum EstadoUsuario` con valores `Activo`, `Inactivo`, `Pendiente`
5. Configura un `tsconfig.json` con modo `strict` activado y compila tu código

---

# Resumen Final

[[#Índice|↑ Volver al índice]]

## Lo que has aprendido

**Fundamentos del lenguaje**
- **C# moderno**: sintaxis, propiedades, `init`/`required`, nulabilidad y el operador `!`
- **Colecciones y LINQ**: el equivalente y mejora de los Streams de Java, ejecución diferida y el coste de re-enumerar
- **async/await**: la base del código moderno en .NET, y los tres pecados capitales (`.Result`, `async void`, `async` sin `await`)
- **Generics**: constraints, clases genéricas, reificación y varianza
- **Delegates y eventos**: patrón publicador/suscriptor nativo, `Func` vs `Expression`, fugas de memoria por suscripción
- **Records y pattern matching**: inmutabilidad, igualdad por valor, `with`, patrones de propiedad y tuplas

**Arquitectura y datos**
- **Patrones de diseño**: Repository, Dependency Injection (y sus lifetimes), Factory, Singleton, Strategy, Decorator
- **SQL**: Oracle y T-SQL, joins, agregaciones, subconsultas, índices y planes de ejecución
- **Entity Framework Core**: LINQ → SQL automático, change tracking, `AsNoTracking()`, el problema N+1, transacciones explícitas y concurrencia optimista
- **ASP.NET Core Web API**: controladores, minimal APIs, DTOs, validación, códigos HTTP, middleware, `ProblemDetails` y Swagger

**Calidad y entorno profesional**
- **Testing**: xUnit (`[Fact]`, `[Theory]`, fixtures), Moq, y el criterio de qué merece la pena testear
- **Logging y configuración**: `ILogger<T>`, logging estructurado, `appsettings.json`, Options pattern y gestión de secretos
- **Git Flow**: ramas, merge, rebase, pull requests y code review
- **Scrum**: sprints, story points, ceremonias y cómo comunicar bloqueos
- **Frontend**: HTML5 semántico, CSS3, JavaScript y TypeScript moderno

---

## Los 10 errores que más vas a cometer (y cómo evitarlos)

Si tuvieras que llevarte una sola página de toda la guía, que sea esta.

| # | Error | Solución |
|---|---|---|
| 1 | `Dictionary[clave]` sobre una clave inexistente | `TryGetValue` — en C# **lanza excepción**, no devuelve null como en Java |
| 2 | Re-enumerar una consulta LINQ perezosa varias veces | `.ToList()` en cuanto vayas a usar el resultado más de una vez |
| 3 | `.Result` o `.Wait()` sobre una `Task` | `await` — *async all the way*, o arriesgas un deadlock |
| 4 | `async void` fuera de un manejador de eventos | `async Task`, siempre |
| 5 | El problema N+1 en Entity Framework | `Include`, o mejor: proyectar con `Select` a un DTO |
| 6 | Listados de solo lectura con tracking activado | `AsNoTracking()` |
| 7 | Devolver la entidad de base de datos por la API | DTOs de entrada y de salida separados |
| 8 | Secretos y cadenas de conexión en `appsettings.json` | *User secrets* en desarrollo, variables de entorno en producción |
| 9 | `catch (Exception) { }` que se traga el error | Resolver, o registrar y relanzar con `throw;` |
| 10 | Devolver `200 OK` para todo, incluidos los errores | Usa el código HTTP correcto: 400, 404, 409, 500 |

---

## Cómo seguir a partir de aquí

1. **Antes de empezar las prácticas:** haz los ejercicios de las lecciones 1 a 6 y monta el mini-proyecto. Es el cimiento; sin eso, el resto se queda en teoría.
2. **La primera semana:** lecciones 8, 10 y 12 (Entity Framework, Web API, configuración). Es exactamente lo que tocarás el primer día en un proyecto real.
3. **Cuando te asignen tu primera tarea:** vuelve a la Lección 11 (testing) y a la 13 (Git). Tu primer pull request se juzgará por esas dos cosas tanto como por el código.
4. **En paralelo:** `MIGRACION_JAVA_A_CSHARP.md` recorre el proyecto real MarinaApi aplicando todo esto, con la comparación explícita frente a Spring Boot e Hibernate.

Y una última cosa que no cabe en ninguna lección: **en unas prácticas no se espera que lo sepas todo, se espera que preguntes bien, escuches las revisiones y no repitas dos veces el mismo error.** Eso pesa más que cualquier lista de tecnologías.

---

**Fin de la Guía Definitiva**

¡Mucho éxito en Espiral MS! 
