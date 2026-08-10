**Guía de aprendizaje complementaria al proyecto `MarinaApi`**

---

**Qué es este documento:** una traducción didáctica, capítulo a capítulo, de tu tutorial de acceso a datos en Java (Hibernate → Spring Boot → REST) a su equivalente en C#/.NET. No es solo código: es la explicación del **por qué**, igual que tus capítulos originales.

**Complementa al código:** todo lo que se explica aquí ya está implementado y funcionando en el proyecto `MarinaApi.zip` que te generé antes. Cuando un apartado diga "ver archivo del proyecto", ese archivo ya existe en el zip — este documento es la clase teórica; el zip es la práctica ya resuelta.

**Requisito previo:** este documento asume que ya has hecho las Lecciones 1-8 de tu `GUIA_DEFINITIVA_CSHARP.md` (sintaxis C#, LINQ, async/await, generics, patrones de diseño, EF Core básico). Aquí profundizamos específicamente en la capa de acceso a datos y API REST, con el mismo dominio que tu tutorial Java (Barco, Amarre, Regata).

---

<a id="indice"></a>

# Índice

1. [Introducción: Hibernate vs Entity Framework Core](#introduccion-hibernate-vs-entity-framework-core)
2. [Configuración del proyecto](#configuracion-del-proyecto)
3. [Entidades y Mapeo](#entidades-y-mapeo)
4. [Relaciones entre Entidades](#relaciones-entre-entidades)
5. [Estados y Change Tracking](#estados-y-change-tracking)
6. [Repositorio Genérico (vs DAO)](#repositorio-generico-vs-dao)
7. [Consultas con LINQ (vs HQL/Criteria)](#consultas-con-linq-vs-hql-criteria)
8. [Testing con xUnit y Moq (vs JUnit/Mockito)](#testing-con-xunit-y-moq-vs-junit-mockito)
9. [ASP.NET Core Web API (vs Spring Boot)](#aspnet-core-web-api-vs-spring-boot)
10. [Capa de Servicio](#capa-de-servicio)
11. [API REST con Controllers](#api-rest-con-controllers)
12. [DTOs y Mapper](#dtos-y-mapper)
13. [Swagger / OpenAPI](#swagger-openapi)
14. [Probar la API: Swagger UI y Postman](#probar-la-api-swagger-ui-y-postman)
15. [Resumen: mapa completo del proyecto](#resumen-mapa-completo-del-proyecto)

---

<a id="introduccion-hibernate-vs-entity-framework-core"></a>

# 1. Introducción: Hibernate vs Entity Framework Core

[↑ Volver al índice](#indice)

## 1.1. Mismo problema, dos soluciones

Ya conoces el problema desde tu Capítulo 1 de Java: el **desajuste objeto-relacional**. Tus clases C# (`Barco`, `Amarre`, `Regata`) no encajan de forma natural con las tablas SQL. **Entity Framework Core (EF Core)** resuelve exactamente lo mismo que Hibernate, siendo el ORM oficial de Microsoft para .NET.

| Concepto | Java | C# |
|---|---|---|
| Especificación | JPA | No existe una "especificación" separada — EF Core es a la vez la interfaz y la implementación |
| Implementación ORM | Hibernate | Entity Framework Core |
| Motor de BD (Espiral MS) | MySQL (en tu tutorial) | **SQL Server** |
| Gestor de sesión/contexto | `Session` / `SessionFactory` | `DbContext` |
| Colección de entidades | `session.createQuery("from Barco")` | `DbSet<Barco>` |

> **Vocabulario:** en Java, JPA es la "receta" y Hibernate el "cocinero" (Cap. 1.4-1.5). En C# **no existe esa separación**: EF Core es receta y cocinero a la vez. Esto simplifica las cosas — no hay que elegir "implementación", solo instalar el paquete NuGet del proveedor de tu base de datos (`Microsoft.EntityFrameworkCore.SqlServer`).

## 1.2. El concepto central: `DbContext`

Donde Java tenía `SessionFactory` + `Session`, C# tiene una única clase: el `DbContext`. Es, a la vez:
- El registro de qué entidades existen (como `hibernate.cfg.xml`)
- La fábrica de sesiones (como `SessionFactory`)
- La sesión de trabajo en sí (como `Session`)

```csharp
// Ver archivo del proyecto: Data/MarinaDbContext.cs
public class MarinaDbContext : DbContext
{
    public DbSet<Barco> Barcos => Set<Barco>();
    public DbSet<Amarre> Amarres => Set<Amarre>();
    public DbSet<Regata> Regatas => Set<Regata>();
}
```

Comparado con Java:

```java
// Lo que hacía HibernateUtil + hibernate.cfg.xml + las clases @Entity, todo junto
SessionFactory sf = HibernateUtil.getSessionFactory();
Session session = sf.openSession();
```

> **TIP:** en Java, tenías que abrir y cerrar la `Session` manualmente con `try-with-resources` en cada método DAO (Cap. 7.3.1). En C#, el `DbContext` se inyecta automáticamente por petición HTTP gracias a la Inyección de Dependencias de ASP.NET Core — nunca lo abres/cierras a mano.

## 1.3. Nuestro proyecto: Sistema de Gestión Marítima

Mantenemos exactamente el mismo dominio que tu tutorial Java, para que puedas comparar directamente:

- **Barco**: nombre, eslora, tipo, manga, capacidad. Tiene un amarre (1:1) y participa en regatas (N:M).
- **Amarre**: ubicación, precio, profundidad, electricidad. Pertenece a un barco (1:1).
- **Regata**: nombre, lugar, fecha, distancia. Tiene varios barcos inscritos (N:M).

- **Checkpoint:** antes de continuar, entiende que **EF Core = JPA + Hibernate fusionados en una sola herramienta**, y que `DbContext` sustituye a `Session` + `SessionFactory` + `hibernate.cfg.xml`.

---

<a id="configuracion-del-proyecto"></a>

# 2. Configuración del proyecto

[↑ Volver al índice](#indice)

## 2.1. El equivalente al `pom.xml`: `.csproj`

```xml
<!-- Ver archivo del proyecto: MarinaApi.csproj -->
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
</ItemGroup>
```

| Java (`pom.xml`) | C# (`.csproj`) |
|---|---|
| `<dependency>` | `<PackageReference>` |
| Repositorio Maven Central | Repositorio NuGet |
| `mvn install` | `dotnet restore` |

## 2.2. El equivalente a `application.properties`: `appsettings.json`

```json
// Ver archivo del proyecto: appsettings.json
{
  "ConnectionStrings": {
    "MarinaDb": "Server=localhost,1433;Database=gestion_maritima;User Id=sa;Password=TuPassword123!;TrustServerCertificate=True;"
  }
}
```

Comparado con Java:

```properties
spring.datasource.url=jdbc:mysql://localhost:3306/gestion_maritima?...
spring.datasource.username=root
spring.datasource.password=root
```

> **CUIDADO:** el proyecto usa **SQL Server**, no MySQL como el tutorial Java original — es el motor real de Espiral MS. Si alguna vez ves ejemplos en internet con `UseMySql(...)`, no los copies directamente: nuestro `Program.cs` usa `UseSqlServer(...)`.

## 2.3. `Program.cs`: el equivalente a la autoconfiguración de Spring Boot

Este es probablemente el cambio más grande de mentalidad viniendo de Spring Boot. En Java, `@SpringBootApplication` escaneaba automáticamente tus clases `@Service`, `@Repository`, `@RestController` (Cap. 10.3: *"Spring Boot examina las dependencias del pom.xml y configura los beans automáticamente"*).

**En ASP.NET Core no hay escaneo automático.** Cada servicio se registra explícitamente:

```csharp
// Ver archivo del proyecto: Program.cs
builder.Services.AddScoped<IBarcoRepository, BarcoRepository>();
builder.Services.AddScoped<IBarcoService, BarcoService>();
```

| Java | C# |
|---|---|
| `@Service` detectado automáticamente | `builder.Services.AddScoped<Interfaz, Implementacion>()` explícito |
| `@Autowired` en el campo | Inyección por constructor (obligatoria, no hay alternativa por campo) |
| `spring.jpa.hibernate.ddl-auto=update` | `dotnet ef migrations add` + `dotnet ef database update` |

> **Buena práctica:** el registro explícito de C# es más verboso, pero también más trazable — con un vistazo a `Program.cs` ves *todas* las dependencias de la aplicación en un solo sitio, sin tener que buscar anotaciones desperdigadas por el código.

## 2.4. Docker Compose para SQL Server

```yaml
# Ver archivo del proyecto: docker-compose.yml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "TuPassword123!"
    ports:
      - "1433:1433"
```

Exactamente el mismo concepto que el `docker-compose.yml` de MySQL de tu Capítulo 10, cambiando solo la imagen y el puerto (1433 en vez de 3306).

## 2.5. Ejercicio

### Solución: Configuración del Proyecto y Migraciones

**Paso 1: Levantar SQL Server con Docker**

```powershell
# En la carpeta raíz del proyecto
docker compose up -d

# Verifica que está corriendo
docker ps | findstr sqlserver
```

**Paso 2: Restaurar paquetes e instalar herramientas de EF Core**

```powershell
cd d:\DAM\PROYECTOS\Marina_C#
dotnet restore
dotnet tool install --global dotnet-ef  # Si no está instalado
```

**Paso 3: Crear la migración inicial**

```powershell
dotnet ef migrations add InitialCreate
```

**Observa el archivo generado:**
- Ubicación: `MarinaApi/Migrations/[timestamp]_InitialCreate.cs`
- Comparación con Java (Cap. 4.9):
  - Java mostraba SQL bruto en consola → Hibernate generaba las tablas automáticamente
  - C# genera un archivo `.cs` con métodos `Up()` (crear) y `Down()` (revertir)
  - Esto permite **auditar qué cambios se aplican** — mejor que la auto-creación silenciosa

**SQL generado equivalente:**
```sql
-- En Java, Hibernate mostraba algo como:
-- create table Barcos (id bigint not null primary key, nombre varchar(100), ...)

-- En C#, el archivo de migración contiene llamadas como:
-- migrationBuilder.CreateTable("Barcos", ...);
```

**Paso 4: Aplicar la migración a la BD**

```powershell
dotnet ef database update
```

**Paso 5: Verificar las tablas**

Opción A: **Azure Data Studio** (más moderno)
```powershell
# Instalar si no lo tienes
choco install azure-data-studio
# Conectar a: localhost,1433 con usuario 'sa'
```

Opción B: **sqlcmd** (línea de comandos)
```powershell
sqlcmd -S localhost,1433 -U sa -P TuPassword123! -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;"
```

**Deberías ver:**
```
TABLE_NAME
Barcos
Amarres
Regatas
BarcoRegata  ← Tabla intermedia N:M (generada automáticamente por EF Core)
```

**Comparación con Java (Cap. 4.9):**

| Aspecto | Java (Hibernate) | C# (EF Core) |
|---|---|---|
| **Estrategia** | `spring.jpa.hibernate.ddl-auto=update` (automático) | `dotnet ef migrations add` + `dotnet ef database update` (explícito) |
| **Visibilidad** | Hibernate genera SQL en silencio | Migración es un archivo `.cs` versionable |
| **Control de versión** | ❌ No se guarda, se pierden cambios | ✅ Se guarda en Git, historial completo |
| **Reversibilidad** | ❌ Difícil revertir | ✅ Método `Down()` en migración |
| **SQL custom** | ❌ Casi imposible customizar | ✅ Puedes editar la migración antes de aplicar |

### Ejercicios Adicionales (Opcional)

1. **Edita la migración** antes de aplicarla: abre `Migrations/*_InitialCreate.cs`, añade un comentario y vuelve a ejecutar `dotnet ef database update`
2. **Añade una columna** a la entidad `Barco` (ej: `public string Capitán { get; set; }`), ejecuta `dotnet ef migrations add AnadirCapitan`, observa el archivo generado
3. **Revierte una migración:** `dotnet ef database update InitialCreate` (vuelve a la primera migración) y `dotnet ef database update` (vuelve a la última)

- **Checkpoint:** debes entender que en C# **no hay autoconfiguración mágica** como en Spring Boot — todo se declara explícitamente en `Program.cs`, y eso es una decisión de diseño, no una carencia. Las migraciones son explícitas y versionables, lo que es mejor para equipos.

---

<a id="entidades-y-mapeo"></a>

# 3. Entidades y Mapeo

[↑ Volver al índice](#indice)

## 3.1. `@Entity` no existe en EF Core — y eso es intencional

Este es el cambio conceptual más importante del capítulo. En Java, cada clase persistente **necesita** la anotación `@Entity` (Cap. 4.2.1). En EF Core, **cualquier clase C# que esté declarada como `DbSet<T>` en el `DbContext` ya es una entidad**, sin anotarla:

```csharp
// Ver archivo del proyecto: Models/Barco.cs
public class Barco
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Eslora { get; set; }
    public int Manga { get; set; }
    public int Capacidad { get; set; }
}
```

Comparado con Java:

```java
@Entity                // ← EF Core no necesita esto
@Data                  // ← EF Core no necesita Lombok, las propiedades ya son "auto"
@NoArgsConstructor      // ← EF Core no necesita constructor vacío explícito
public class Barco {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    // ...
}
```

## 3.2. Convención sobre configuración

EF Core usa **convenciones de nombres** para inferir el mapeo sin anotaciones:

| Convención EF Core | Equivalente explícito Java |
|---|---|
| Propiedad llamada `Id` (o `BarcoId`) | `@Id` |
| Tipo numérico (`int`, `long`) en `Id` | `@GeneratedValue(strategy = GenerationType.IDENTITY)` — EF Core lo asume automáticamente en SQL Server |
| Nombre de la clase `Barco` | Nombre de tabla `Barcos` (EF Core pluraliza sola) |
| Nombre de propiedad `Nombre` | Nombre de columna `Nombre` |

> **TIP:** esto es el principio de **"convención sobre configuración"**: si sigues los nombres estándar, no necesitas anotar nada. Solo anotas (o configuras vía Fluent API) cuando te **desvías** de la convención — igual que en Java solo usabas `@Column(name=...)` cuando el nombre por defecto no te servía (Cap. 4.2.4).

## 3.3. Cuándo SÍ necesitas anotar: Data Annotations

Para las validaciones que en Java hacías con `@Column(nullable = false, length = 100)`:

```csharp
using System.ComponentModel.DataAnnotations;

public class Barco
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
}
```

| Java (`@Column`) | C# (`DataAnnotations`) |
|---|---|
| `nullable = false` | `[Required]` |
| `length = 100` | `[MaxLength(100)]` |
| `unique = true` | Se configura en el `DbContext` (`HasIndex().IsUnique()`), no con atributo |

## 3.4. Lombok no existe — y no hace falta

Java necesitaba Lombok (`@Data`, `@NoArgsConstructor`) para no escribir 60 líneas de getters/setters (Cap. 4.5). En C#, las **propiedades automáticas** (`public string Nombre { get; set; }`) ya generan el getter y setter en una sola línea — es sintaxis nativa del lenguaje, no una librería externa.

```csharp
// Esto YA incluye getter y setter, sin Lombok, sin anotación
public string Nombre { get; set; } = string.Empty;
```

> **¿Sabías que?** El `= string.Empty` al final no es capricho: con *nullable reference types* activado (que ya usas desde la Lección 1 de tu guía de C#), el compilador exige que toda propiedad `string` no anulable tenga un valor inicial. Es el equivalente a que Hibernate necesitara un constructor vacío — pero resuelto por el compilador, no por una librería.

## 3.5. `@Temporal` → `DateOnly`

Java necesitaba `@Temporal(TemporalType.DATE)` porque `java.util.Date` incluye hora y zona horaria por defecto. C# moderno (desde .NET 6) tiene un tipo dedicado que resuelve esto de raíz:

```csharp
// Ver archivo del proyecto: Models/Regata.cs
public DateOnly Fecha { get; set; }  // Solo fecha, sin hora — no hace falta anotación
```

```java
@Temporal(TemporalType.DATE)   // Necesario en Java: Date por defecto lleva hora
private Date fecha;
```

## 3.6. `@Enumerated` → `enum` de C#

Si tuvieras un campo de tipo fijo (Java Cap. 4.4.2):

```csharp
public enum TipoBarco { Velero, Motor, Catamaran }

public TipoBarco Tipo { get; set; }
// EF Core lo guarda como int por defecto (como EnumType.ORDINAL)
// Para guardarlo como texto (como EnumType.STRING, más seguro):
// modelBuilder.Entity<Barco>().Property(b => b.Tipo).HasConversion<string>();
```

> **CUIDADO:** igual que en Java con `EnumType.ORDINAL` (Cap. 4.4.2), si guardas el enum como número y luego reordenas sus valores, los datos existentes se corrompen. La recomendación es la misma: convierte a `string` explícitamente en el `DbContext`.

## 3.7. Ejercicio

### Solución: Crear Entidad `Tripulante`

**Paso 1: Análisis de equivalencias (Barco)**

Abre `Models/Barco.cs` y mapea cada propiedad:

```csharp
public class Barco
{
    public long Id { get; set; }                    // ← @Id + @GeneratedValue
    public string Nombre { get; set; }              // ← @Column(nullable=false)
    public string Tipo { get; set; }                // ← @Column(nullable=false)
    public int Eslora { get; set; }                 // ← @Column (primitivo)
    public int Manga { get; set; }
    public int Capacidad { get; set; }
}
```

| Propiedad C# | Anotación Java Equivalente |
|---|---|
| `long Id` | `@Id @GeneratedValue(strategy = GenerationType.IDENTITY)` |
| `string Nombre` | `@Column(nullable = false, length = 100)` → en C# es **convención** |
| `int Eslora` | `@Column(columnDefinition = "int")` → en C# es **automático** |

**Paso 2: Crear la entidad `Tripulante`**

Crea un nuevo archivo `Models/Tripulante.cs`:

```csharp
namespace MarinaApi.Models;

/// <summary>Representa un miembro de la tripulación de un barco.</summary>
public class Tripulante
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;  // ej: Capitán, Marinero, Contramaestre
    
    // Relación: un Tripulante pertenece a un Barco (N:1)
    public long BarcoId { get; set; }
    public Barco? Barco { get; set; }
}
```

**Paso 3: Actualizar `Barco.cs` para la relación inversa**

```csharp
public class Barco
{
    // ... propiedades existentes ...
    
    // Relación: un Barco tiene muchos Tripulantes (1:N)
    public List<Tripulante> Tripulantes { get; set; } = new();
}
```

**Paso 4: Configurar la relación en `MarinaDbContext.cs`**

En el método `OnModelCreating()`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... configuraciones existentes ...
    
    // Relación Barco (1) → Tripulante (N)
    modelBuilder.Entity<Tripulante>()
        .HasOne(t => t.Barco)
        .WithMany(b => b.Tripulantes)
        .HasForeignKey(t => t.BarcoId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

**Paso 5: Crear la migración**

```powershell
dotnet ef migrations add AnadirTripulante
dotnet ef database update
```

### Comparación de Anotaciones Java vs C# (Resumen)

| Función | Java JPA | C# EF Core |
|---|---|---|
| Marcar como entidad | `@Entity` | Convención: `DbSet<T>` en contexto |
| ID / Clave primaria | `@Id` | Convención: propiedad `Id` o `[Nombre]Id` |
| Auto-incremento | `@GeneratedValue(IDENTITY)` | Automático: `int` o `long` en `Id` |
| Longitud texto | `@Column(length = 100)` | Convención: `string` sin límite, o `[MaxLength(100)]` |
| Not null | `@Column(nullable = false)` | Convención: `string` no-nullable con `required` |
| Nombre columna custom | `@Column(name = "custom")` | `[Column("custom")]` o Fluent API |
| Relación (propiedad nav.) | `@OneToMany` | Convención: `List<T>` o Fluent API |
| FK explícita | `@JoinColumn(name = "...")` | Convención: `[Nombre]Id`, o Fluent API |
| Lado inverso | `mappedBy = "..."` | Fluent API: `.WithMany()` / `.WithOne()` |
| Cascade | `cascade = CascadeType.ALL` | Fluent API: `.OnDelete(DeleteBehavior.Cascade)` |

### El Concepto Clave

**Java necesitaba:**
```java
@Entity
@Data
@NoArgsConstructor
public class Barco {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(nullable = false, length = 100)
    private String nombre;
    
    @OneToMany(mappedBy = "barco", cascade = CascadeType.ALL)
    private List<Tripulante> tripulantes;
}
```

**C# necesita:**
```csharp
public class Barco
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<Tripulante> Tripulantes { get; set; } = new();
}
// Punto. La configuración va en MarinaDbContext.cs, no aquí.
```

- **Checkpoint:** debes entender por qué EF Core necesita **menos código visible** que Hibernate para el mismo resultado: las convenciones de nombres + las propiedades automáticas de C# cubren lo que en Java hacían `@Entity`, `@Id`, `@GeneratedValue` y Lombok juntos.

---

<a id="relaciones-entre-entidades"></a>

# 4. Relaciones entre Entidades

[↑ Volver al índice](#indice)

## 4.1. `mappedBy` no existe en EF Core

Este es el cambio más importante de todo el capítulo de relaciones. En Java, marcabas el "lado inverso" de una relación con `mappedBy` **dentro de la propia entidad** (Cap. 5.1.2). En EF Core, **las relaciones se configuran fuera de las entidades**, en el `DbContext`, con Fluent API:

```csharp
// Ver archivo del proyecto: Data/MarinaDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Amarre>()
        .HasOne(a => a.Barco)          // Amarre tiene UN Barco
        .WithOne(b => b.Amarre)        // Barco tiene UN Amarre
        .HasForeignKey<Amarre>(a => a.BarcoId)  // Amarre es el propietario (tiene la FK)
        .OnDelete(DeleteBehavior.Cascade);
}
```

Comparado con Java:

```java
// Lado propietario (Amarre) — DENTRO de la clase
@OneToOne
@JoinColumn(name = "barco_id")
private Barco barco;

// Lado inverso (Barco) — DENTRO de la otra clase
@OneToOne(mappedBy = "barco", cascade = CascadeType.ALL)
private Amarre amarre;
```

| Concepto | Java | C# |
|---|---|---|
| Dónde se declara la relación | Dentro de cada entidad, con anotaciones | Fuera, centralizado en `OnModelCreating` |
| Entidad propietaria (tiene la FK) | Sin `mappedBy` | `.HasForeignKey<T>()` indica cuál |
| Lado inverso | Con `mappedBy = "..."` | `.WithOne()` / `.WithMany()` |
| Cascada | `cascade = CascadeType.ALL` | `.OnDelete(DeleteBehavior.Cascade)` |

> **Buena práctica:** tener toda la configuración de relaciones **en un solo archivo** (`MarinaDbContext.cs`) en vez de repartida entre varias clases con anotaciones es una ventaja real de EF Core: cuando algo no cuadra en las relaciones de tu BD, sabes exactamente dónde mirar.

## 4.2. Relación 1:1 completa

```csharp
// Models/Barco.cs — solo la propiedad de navegación, SIN anotaciones de relación
public Amarre? Amarre { get; set; }

// Models/Amarre.cs — con la FK explícita
public long? BarcoId { get; set; }
public Barco? Barco { get; set; }
```

El `?` en `Amarre? Amarre` es importante: en Java, un objeto puede ser `null` sin que el compilador te avise (Cap. 1, *NullPointerException*). En C# con *nullable reference types*, declaras explícitamente que esa relación **puede no existir** — el compilador te obliga a comprobarlo antes de usarla.

## 4.3. Relación N:M — la mayor simplificación de EF Core

Aquí está la diferencia más grande respecto a Java. En tu Cap. 5.4, para la relación N:M Barco↔Regata necesitabas:

```java
@ManyToMany
@JoinTable(
    name = "barco_regata",
    joinColumns = @JoinColumn(name = "barco_id"),
    inverseJoinColumns = @JoinColumn(name = "regata_id")
)
private List<Regata> regatas;
```

En C# con EF Core 5+ (**skip navigations**), la tabla intermedia se genera sola con una sola línea en el `DbContext`, y las entidades ni siquiera necesitan saber que existe:

```csharp
// Models/Barco.cs
public List<Regata> Regatas { get; set; } = new();

// Models/Regata.cs
public List<Barco> Barcos { get; set; } = new();

// Data/MarinaDbContext.cs — la ÚNICA configuración que hace falta
modelBuilder.Entity<Barco>()
    .HasMany(b => b.Regatas)
    .WithMany(r => r.Barcos)
    .UsingEntity(j => j.ToTable("BarcoRegata"));
```

> **¿Sabías que?** No existe ninguna clase C# `BarcoRegata.cs` en el proyecto — EF Core gestiona la tabla intermedia internamente, sin que tengas que modelarla como entidad, a diferencia de otros ORMs donde sí haría falta una clase explícita para la tabla de unión.

## 4.4. Cascade — misma idea, sintaxis distinta

| `CascadeType` de Java | `DeleteBehavior` de C# |
|---|---|
| `CascadeType.REMOVE` | `DeleteBehavior.Cascade` |
| `CascadeType.ALL` (sin remove) | `DeleteBehavior.ClientCascade` o configurarlo por tipo de operación |
| Sin cascade | `DeleteBehavior.Restrict` (por defecto en EF Core) |

## 4.5. Ejercicio

### Solución: Verificar Relaciones y Agregar `Organizador`

**Paso 1: Localiza las relaciones en `MarinaDbContext.cs`**

Abre `Data/MarinaDbContext.cs` y encuentra:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Relación 1:1 (Barco ← → Amarre)
    modelBuilder.Entity<Amarre>()
        .HasOne(a => a.Barco)
        .WithOne(b => b.Amarre)
        .HasForeignKey<Amarre>(a => a.BarcoId);
    
    // Relación N:M (Barco ← → Regata)
    modelBuilder.Entity<Barco>()
        .HasMany(b => b.Regatas)
        .WithMany(r => r.Barcos)
        .UsingEntity(j => j.ToTable("BarcoRegata"));
}
```

**Paso 2: Verifica la tabla intermedia**

Crea la migración:
```powershell
dotnet ef migrations add VerificarBarcoRegata
```

Abre el archivo generado (`Migrations/*_VerificarBarcoRegata.cs`) y busca `CreateTable("BarcoRegata"...)`:

```csharp
migrationBuilder.CreateTable(
    name: "BarcoRegata",
    columns: table => new
    {
        BarcosId = table.Column<long>(type: "bigint", nullable: false),
        RegatasId = table.Column<long>(type: "bigint", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_BarcoRegata", x => new { x.BarcosId, x.RegatasId });
        table.ForeignKey(
            name: "FK_BarcoRegata_Barcos_BarcosId",
            column: x => x.BarcosId,
            principalTable: "Barcos",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_BarcoRegata_Regatas_RegatasId",
            column: x => x.RegatasId,
            principalTable: "Regatas",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });
```

**Comparación con Java (Cap. 5.4.1):**
- Java: Necesitabas una clase `BarcoRegata` explícita con `@ManyToOne` dos veces
- C#: EF Core crea la tabla automáticamente, sin clase explícita

**Paso 3: Crear entidad `Organizador`**

```csharp
// Models/Organizador.cs
namespace MarinaApi.Models;

public class Organizador
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Relación 1:N (un Organizador dirige muchas Regatas)
    public List<Regata> Regatas { get; set; } = new();
}
```

**Paso 4: Actualizar `Regata.cs`**

```csharp
public class Regata
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    // ... propiedades existentes ...
    
    // Relación: una Regata tiene UN Organizador
    public long? OrganizadorId { get; set; }
    public Organizador? Organizador { get; set; }
}
```

**Paso 5: Configurar en `MarinaDbContext.cs`**

```csharp
// En OnModelCreating():
modelBuilder.Entity<Regata>()
    .HasOne(r => r.Organizador)
    .WithMany(o => o.Regatas)
    .HasForeignKey(r => r.OrganizadorId)
    .OnDelete(DeleteBehavior.SetNull);  // Si se elimina organizador, la regata queda sin él
```

**Paso 6: Migración**

```powershell
dotnet ef migrations add AnadirOrganizador
dotnet ef database update
```

### Comparación: Configuración de Relaciones (Java vs C#)

| Aspecto | Java (Cap. 5) | C# (EF Core) |
|---|---|---|
| **Dónde se configura** | DENTRO de cada entidad (`@OneToMany`, `@ManyToMany`) | FUERA, en `OnModelCreating()` |
| **N:M necesita clase intermedia** | ✅ Sí (`BarcoRegata.java`) | ❌ No (se genera automáticamente) |
| **Configuración centralizada** | ❌ Dispersa entre varias clases | ✅ Un único archivo `MarinaDbContext.cs` |
| **Facilidad para cambiar relaciones** | ⚠️ Difícil (toca varias clases) | ✅ Fácil (solo edita `OnModelCreating`) |

- **Checkpoint:** entiendes que EF Core mueve la configuración de relaciones **fuera** de las entidades (a diferencia de Java, que la pone dentro con anotaciones), y que las relaciones N:M no necesitan una clase Java equivalente a la tabla intermedia.

---

<a id="estados-y-change-tracking"></a>

# 5. Estados y Change Tracking

[↑ Volver al índice](#indice)

## 5.1. Los 4 estados de Hibernate vs el Change Tracker de EF Core

Java (Hibernate) distingue explícitamente 4 estados de una entidad: *Transient*, *Persistent*, *Detached*, *Removed*. EF Core simplifica esto con un **Change Tracker** interno que observa automáticamente los cambios:

| Estado Hibernate | Equivalente en EF Core |
|---|---|
| Transient (recién creado con `new`, sin guardar) | `EntityState.Detached` |
| Persistent (gestionado por la sesión activa) | `EntityState.Unchanged` / `Modified` / `Added` |
| Detached (sesión cerrada) | `EntityState.Detached` |
| Removed | `EntityState.Deleted` |

## 5.2. Cómo funciona en la práctica

```csharp
var barco = await _context.Barcos.FindAsync(1);  // Estado: Unchanged (recién leído)
barco.Nombre = "Nuevo nombre";                     // Estado: Modified (EF Core lo detecta SOLO)
await _context.SaveChangesAsync();                 // Genera el UPDATE, vuelve a Unchanged
```

**No necesitas llamar a `session.update(barco)` como en Java.** EF Core compara automáticamente los valores actuales contra los que leyó de la base de datos (usando una técnica llamada *snapshot tracking*) y genera el `UPDATE` solo con las columnas que realmente cambiaron.

```java
// Java: tenías que llamar update() explícitamente
Transaction tx = session.beginTransaction();
session.update(barco);   // ← paso manual obligatorio
tx.commit();
```

```csharp
// C#: basta con modificar la propiedad y guardar
barco.Nombre = "Nuevo nombre";
await _context.SaveChangesAsync();  // EF Core ya sabía que había cambiado
```

> **TIP:** esto es la razón por la que en `Services/BarcoService.cs` el método `UpdateAsync` no llama a ningún "update" del repositorio en el sentido Java — simplemente modifica las propiedades del objeto ya rastreado y llama a `SaveChangesAsync`.

## 5.3. `AsNoTracking()` — para consultas de solo lectura

Cuando sabes que **no** vas a modificar los datos (por ejemplo, un `GET` que solo lista), puedes decirle a EF Core que no gaste memoria rastreando cambios:

```csharp
var barcos = await _context.Barcos.AsNoTracking().ToListAsync();
```

No existe un equivalente directo en Hibernate — es una optimización propia de EF Core.

## 5.4. Ejercicio

### Solución: Implementar Logging de Change Tracking

**Objetivo:** Observar cómo `EntityState` cambia automáticamente sin llamar a `Update()` explícitamente.

**Paso 1: Modificar `BarcoService.cs`**

Agregar la inyección de `MarinaDbContext` en el constructor:

```csharp
using MarinaApi.Data;

public class BarcoService : IBarcoService
{
    private readonly IBarcoRepository _barcoRepository;
    private readonly MarinaDbContext _context;  // ← Agregar
    private readonly ILogger<BarcoService> _logger;

    public BarcoService(IBarcoRepository barcoRepository, MarinaDbContext context, ILogger<BarcoService> logger)
    {
        _barcoRepository = barcoRepository;
        _context = context;
        _logger = logger;
    }
```

**Paso 2: Actualizar el método `UpdateAsync`**

```csharp
public async Task<BarcoDto> UpdateAsync(long id, BarcoRequestDto dto, CancellationToken ct = default)
{
    var barco = await _barcoRepository.FindByIdAsync(id, ct)
        ?? throw new NotFoundException(nameof(Models.Barco), id);

    // CHECKPOINT 5.4: Inspecciona Change Tracking ANTES
    var stateAntes = _context.Entry(barco).State;
    _logger.LogInformation("🔍 ANTES de UpdateFromDto: EntityState = {State}", stateAntes);

    barco.UpdateFromDto(dto);

    // CHECKPOINT 5.4: Inspecciona Change Tracking DESPUÉS
    var stateDespues = _context.Entry(barco).State;
    _logger.LogInformation("🔍 DESPUÉS de UpdateFromDto: EntityState = {State}", stateDespues);
    _logger.LogInformation("✅ EF Core detectó cambios automáticamente (Change Tracking)");

    await _barcoRepository.UpdateAsync(barco, ct);
    return barco.ToDto();
}
```

### Cómo Verificarlo

1. **Ejecuta en debug:**
   ```powershell
   dotnet run --configuration Debug
   ```

2. **En otra terminal, haz un PUT:**
   ```powershell
   Invoke-WebRequest -Uri "http://localhost:5000/api/barcos/1" `
       -Method PUT `
       -ContentType "application/json" `
       -Body '{"nombre":"TestDebug","tipo":"Velero","eslora":15,"manga":9,"capacidad":25}' `
       -UseBasicParsing
   ```

3. **En la consola del servidor, observa:**
   ```
   🔍 ANTES de UpdateFromDto: EntityState = Unchanged
   🔍 DESPUÉS de UpdateFromDto: EntityState = Modified
   ✅ EF Core detectó cambios automáticamente (Change Tracking)
   ```

### Comparación con Java (Capítulo 7)

**En Java:**
```java
// BarcoDAOImpl.update()
Transaction tx = session.beginTransaction();
try {
    session.merge(barco);  // ← LLAMADA EXPLÍCITA OBLIGATORIA
    tx.commit();
} finally {
    session.close();
}
```

**En C#:**
```csharp
// BarcoService.UpdateAsync()
barco.UpdateFromDto(dto);  // Solo modificar propiedades
// EF Core detecta automáticamente que cambió
await _context.SaveChangesAsync();  // Genera UPDATE solo
```

**Diferencia clave:** En Java necesitabas `session.merge()` explícito. En C#, solo modificas propiedades y EF Core lo detecta automáticamente mediante **snapshot tracking** (guarda una copia de los valores leídos y los compara cuando haces SaveChanges).

- **Checkpoint:** entiendes que EF Core detecta cambios automáticamente (Change Tracking) en vez de exigir una llamada explícita a `update()` como Hibernate.

---

<a id="repositorio-generico-vs-dao"></a>

# 6. Repositorio Genérico (vs DAO)

[↑ Volver al índice](#indice)

## 6.1. El problema que Java resolvía a medias

Tu Cap. 7 terminaba reconociendo el problema: `BarcoDAOImpl`, `AmarreDAOImpl` y `RegataDAOImpl` eran casi idénticos, y proponía un `GenericDAO<T>` — pero **sin implementación completa**, solo la interfaz (Cap. 7.4).

En el proyecto C#, ese `GenericDAO<T>` sí está completamente implementado y en uso real:

```csharp
// Ver archivo del proyecto: Repositories/GenericRepository.cs
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly MarinaDbContext _context;
    private readonly DbSet<T> _dbSet;

    public async Task<T?> FindByIdAsync(long id, CancellationToken ct = default) =>
        await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
    // ... UpdateAsync, DeleteAsync, FindAllAsync
}
```

Una única clase sirve para `Barco`, `Amarre` y `Regata` a la vez — en vez de las ~450 líneas duplicadas entre los 3 `DAOImpl` de Java.

## 6.2. Extender el genérico con consultas propias

Igual que en Java `BarcoDAO` podía tener métodos propios además de los heredados de `GenericDAO<T>`, en C#:

```csharp
// Ver archivo del proyecto: Repositories/BarcoRepository.cs
public interface IBarcoRepository : IGenericRepository<Barco>
{
    Task<Barco?> FindByNombreAsync(string nombre, CancellationToken ct = default);
    Task<List<Barco>> FindByTipoAsync(string tipo, CancellationToken ct = default);
}
```

| Java | C# |
|---|---|
| `interface BarcoDAO extends GenericDAO<Barco>` | `interface IBarcoRepository : IGenericRepository<Barco>` |
| Prefijo `I` no es obligatorio | Prefijo `I` en interfaces **sí es convención obligatoria** (recuerda tu Lección 1) |

## 6.3. `try-with-resources` no hace falta

En Java, cada método DAO abría su propia sesión con `try (Session session = ...)` (Cap. 7.3.1). En C#, el `DbContext` se inyecta una vez por petición HTTP (`AddScoped` en `Program.cs`) y **vive durante toda la petición** — no lo abres/cierras en cada método.

```java
// Java: abrir sesión en CADA método
try (Session session = HibernateUtil.getSessionFactory().openSession()) {
    return session.find(Barco.class, id);
}
```

```csharp
// C#: el DbContext ya está inyectado, listo para usar
public async Task<T?> FindByIdAsync(long id, CancellationToken ct = default) =>
    await _dbSet.FindAsync(new object[] { id }, ct);
```

## 6.4. Transacciones: implícitas vs explícitas

Java exigía el patrón manual `beginTransaction()` → operación → `commit()` en cada método de escritura (Cap. 7.3.2). En EF Core, `SaveChangesAsync()` **ya es una transacción implícita** por sí sola — si falla a mitad, no aplica ningún cambio.

Solo necesitas una transacción **explícita** cuando agrupas varias operaciones que deben tener éxito o fallar todas juntas (ver `RegataService.DeleteAsync` en el proyecto, que agrupa desvincular N:M + borrar en una sola transacción con `BeginTransactionAsync()`).

## 6.5. Ejercicio

### Solución: Agregar `FindByCapacidadGreaterThanAsync`

**Objetivo:** Extender el repositorio específico con una consulta personalizada, siguiendo el patrón genérico.

**Paso 1: Agregar firma en la interfaz `IBarcoRepository`**

```csharp
// Ver archivo del proyecto: Repositories/BarcoRepository.cs
public interface IBarcoRepository : IGenericRepository<Barco>
{
    Task<Barco?> FindByNombreAsync(string nombre, CancellationToken ct = default);
    Task<List<Barco>> FindByTipoAsync(string tipo, CancellationToken ct = default);
    Task<List<Barco>> FindByEsloraGreaterThanAsync(int eslora, CancellationToken ct = default);
    Task<List<Barco>> FindByCapacidadGreaterThanAsync(int capacidad, CancellationToken ct = default);  // ← NUEVA
    Task<long> CountByTipoAsync(string tipo, CancellationToken ct = default);
    Task<Barco?> FindByIdWithRegatasAsync(long id, CancellationToken ct = default);
    Task<List<Barco>> FindSinAmarreAsync(CancellationToken ct = default);
    Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default);
}
```

**Paso 2: Implementar en `BarcoRepository`**

```csharp
public async Task<List<Barco>> FindByCapacidadGreaterThanAsync(int capacidad, CancellationToken ct = default) =>
    await _context.Barcos.Where(b => b.Capacidad > capacidad).ToListAsync(ct);
```

### Traducción Bidireccional: LINQ ↔ HQL

**Cada método como HQL:**

| Método C# (LINQ) | Equivalente HQL (Java Cap. 8) |
|---|---|
| `FindByNombreAsync(nombre)` | `SELECT b FROM Barco b WHERE b.nombre = :nombre` |
| `FindByTipoAsync(tipo)` | `SELECT b FROM Barco b WHERE b.tipo = :tipo` |
| `FindByEsloraGreaterThanAsync(eslora)` | `SELECT b FROM Barco b WHERE b.eslora > :eslora` |
| **`FindByCapacidadGreaterThanAsync(capacidad)`** | **`SELECT b FROM Barco b WHERE b.capacidad > :capacidad`** |
| `CountByTipoAsync(tipo)` | `SELECT COUNT(b) FROM Barco b WHERE b.tipo = :tipo` |
| `FindByIdWithRegatasAsync(id)` | `SELECT b FROM Barco b JOIN FETCH b.regatas WHERE b.id = :id` |
| `FindSinAmarreAsync()` | `SELECT b FROM Barco b WHERE b.amarre IS NULL` |

### Comparación: DAO Java vs GenericRepository C#

**Java (Cap. 7.2.1) — Triplicación:**
```java
// BarcoDAOImpl.java
public class BarcoDAOImpl implements BarcoDAO {
    public List<Barco> findByTipo(String tipo) {
        Session session = sessionFactory.openSession();
        Transaction tx = session.beginTransaction();
        try {
            return session.createQuery("from Barco where tipo = :tipo", Barco.class)
                .setParameter("tipo", tipo)
                .getResultList();
        } finally {
            tx.commit();
            session.close();
        }
    }
    
    public List<Barco> findByEsloraGreaterThan(int eslora) {
        Session session = sessionFactory.openSession();
        Transaction tx = session.beginTransaction();
        try {
            return session.createQuery("from Barco where eslora > :eslora", Barco.class)
                .setParameter("eslora", eslora)
                .getResultList();
        } finally {
            tx.commit();
            session.close();
        }
    }
    // ... y lo mismo repetido para cada método
}
```

**C# (GenericRepository) — Sin triplicación:**
```csharp
// GenericRepository.cs (heredado por BarcoRepository)
public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
    await _dbSet.Where(predicate).ToListAsync(ct);

// BarcoRepository.cs (específico)
public async Task<List<Barco>> FindByTipoAsync(string tipo, CancellationToken ct = default) =>
    await _context.Barcos.Where(b => b.Tipo == tipo).ToListAsync(ct);

public async Task<List<Barco>> FindByCapacidadGreaterThanAsync(int capacidad, CancellationToken ct = default) =>
    await _context.Barcos.Where(b => b.Capacidad > capacidad).ToListAsync(ct);

// Una única clase GenericRepository<T> = BarcoDAOImpl + AmarreDAOImpl + RegataDAOImpl
```

### Por Qué Es Superior

| Aspecto | Java (Cap. 7) | C# (GenericRepository) |
|---|---|---|
| **Sesión/Contexto** | Abierta/cerrada manualmente en cada método | Inyectada una vez por petición |
| **Transacción** | Manual: `beginTransaction()` → `commit()` | Automática: `SaveChangesAsync()` |
| **Consulta** | String HQL repetido en try-catch | Lambda LINQ type-safe |
| **Triplicación** | 3 DAOs casi idénticos (450+ líneas) | 1 GenericRepository (75 líneas) |
| **Cambios** | `merge()` explícito | Change Tracking automático |

- **Checkpoint:** entiendes por qué el repositorio genérico de C# elimina la triplicación de código que Java solo diagnosticaba sin resolver del todo.

---

<a id="consultas-con-linq-vs-hql-criteria"></a>

# 7. Consultas con LINQ (vs HQL/Criteria)

[↑ Volver al índice](#indice)

## 7.1. Una sola herramienta para las tres que usaba Java

Tu Cap. 8 explicaba **tres** formas distintas de consultar en Java: HQL, SQL nativo y Criteria API, cada una con su sintaxis propia. En C#, **LINQ cubre los tres casos** con una única sintaxis (la misma que ya conoces de tu Lección 2):

```csharp
// Equivalente a HQL: "FROM Barco WHERE tipo = :tipo"
await _context.Barcos.Where(b => b.Tipo == tipo).ToListAsync();

// Equivalente a Criteria API (type-safe, sin strings)
await _context.Barcos.Where(b => b.Eslora > 10).ToListAsync();
```

| Java | Equivalente C# |
|---|---|
| HQL (`"FROM Barco WHERE ..."`, strings) | LINQ con lambdas — **type-safe por defecto**, sin strings |
| Criteria API (`CriteriaBuilder`, `Root<T>`, muy verbosa) | LINQ — mismo type-safety, mucho menos código |
| SQL nativo (`createNativeQuery`) | `FromSqlRaw()` — para los casos raros que lo necesiten |

> **Buena práctica:** en Java tenías que **elegir** entre HQL (legible pero no type-safe) y Criteria API (type-safe pero verbosa). En C#, LINQ te da **ambas ventajas a la vez** — no hay ese compromiso.

## 7.2. Parámetros — SQL Injection resuelto por diseño

Java necesitaba disciplina para usar `setParameter()` en vez de concatenar strings (Cap. 8.2.2, con aviso explícito de SQL Injection). En LINQ, **no existe la opción insegura**: como escribes expresiones C# (lambdas), no hay forma de "concatenar" un valor directamente en la consulta como en SQL con strings.

```csharp
// Esto es LINQ compilado a expresión — EF Core lo traduce a parámetro SQL
// automáticamente, sin que puedas hacerlo "mal" ni queriendo
await _context.Barcos.Where(b => b.Nombre == nombreDelUsuario).ToListAsync();
```

## 7.3. `JOIN FETCH` → `Include()`

```java
// Java (Cap. 8.2.4 y Cap. 11.4)
@Query("SELECT b FROM Barco b JOIN FETCH b.regatas WHERE b.id = :id")
Barco findByIdWithRegatas(@Param("id") Long id);
```

```csharp
// Ver archivo del proyecto: Repositories/BarcoRepository.cs
public async Task<Barco?> FindByIdWithRegatasAsync(long id, CancellationToken ct = default) =>
    await _context.Barcos
        .Include(b => b.Regatas)
        .FirstOrDefaultAsync(b => b.Id == id, ct);
```

`Include()` (y `ThenInclude()` para relaciones anidadas) es el equivalente exacto de `JOIN FETCH`: carga la relación en la **misma** consulta SQL, evitando el problema N+1 — el mismo problema de rendimiento que tu Cap. 8 no llegaba a mencionar explícitamente pero que `JOIN FETCH` prevenía.

## 7.4. Consultas derivadas por nombre (Spring Data) → simplemente no hacen falta

Tu Cap. 11 mostraba cómo Spring Data generaba SQL a partir del **nombre del método** (`findByEsloraGreaterThan`). En C#, ese "truco" de nomenclatura no es necesario: **ya escribes directamente la expresión LINQ**, que es igual de concisa y además no depende de que aciertes con las palabras clave exactas (`GreaterThan`, `Containing`...) que Spring Data exige.

```java
// Spring Data infiere el SQL del NOMBRE del método — mágico, pero rígido
List<Barco> findByEsloraGreaterThan(int eslora);
```

```csharp
// C#: escribes la condición directamente, sin depender de convenciones de nombres
public Task<List<Barco>> FindByEsloraGreaterThanAsync(int eslora, CancellationToken ct) =>
    _context.Barcos.Where(b => b.Eslora > eslora).ToListAsync(ct);
```

## 7.5. Agregaciones y proyecciones

```csharp
// COUNT
await _context.Barcos.LongCountAsync(b => b.Tipo == tipo);

// Proyección (como SELECT b.nombre, b.eslora FROM Barco b de HQL)
var resumen = await _context.Barcos
    .Select(b => new { b.Nombre, b.Eslora })
    .ToListAsync();
```

## 7.6. Ejercicio

### Solución: Agregar Consulta de Agregación `GetPromedioEsloraByTipoAsync`

**Objetivo:** Implementar una consulta LINQ que replique `SELECT AVG(b.eslora) FROM Barco b WHERE b.tipo = :tipo` (Cap. 8.2.5 Java).

**Paso 1: Agregar firma en `IBarcoRepository`**

```csharp
// Ver archivo del proyecto: Repositories/BarcoRepository.cs
public interface IBarcoRepository : IGenericRepository<Barco>
{
    // ... métodos existentes ...
    
    // Equivalente a: @Query("SELECT AVG(b.eslora) FROM Barco b WHERE b.tipo = :tipo")
    Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default);
}
```

**Paso 2: Implementar en `BarcoRepository`**

```csharp
public async Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default) =>
    await _context.Barcos
        .Where(b => b.Tipo == tipo)
        .AverageAsync(b => (double)b.Eslora, ct);
```

**Paso 3: Agregar método en `IBarcoService`**

```csharp
public interface IBarcoService
{
    // ... métodos existentes ...
    Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default);
}
```

**Paso 4: Implementar en `BarcoService`**

```csharp
public async Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default)
{
    var promedio = await _barcoRepository.GetPromedioEsloraByTipoAsync(tipo, ct);
    _logger.LogInformation("📊 Promedio de eslora para tipo '{Tipo}': {Promedio:F2} metros", tipo, promedio);
    return promedio;
}
```

**Paso 5: Agregar endpoint en `BarcosController`**

```csharp
/// <summary>Obtiene el promedio de eslora para un tipo de barco (Cap. 8.2.5: Agregación).</summary>
[HttpGet("tipo/{tipo}/promedio-eslora")]
[ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
public async Task<ActionResult<double>> GetPromedioEsloraByTipo(string tipo, CancellationToken ct) =>
    Ok(await _barcoService.GetPromedioEsloraByTipoAsync(tipo, ct));
```

### LINQ Sustituye Tres Sistemas Java Simultáneamente

#### 1. **Sustituye HQL (Strings de Consultas)**

**Java (Cap. 8.2.5):**
```java
String hql = "SELECT AVG(b.eslora) FROM Barco b WHERE b.tipo = :tipo";
Double promedio = session.createQuery(hql, Double.class)
    .setParameter("tipo", tipo)
    .getSingleResult();
```

**C# (LINQ):**
```csharp
double promedio = await _context.Barcos
    .Where(b => b.Tipo == tipo)
    .AverageAsync(b => (double)b.Eslora);
```

**Ventajas:**
- ✅ Type-safe (el compilador verifica `b.Eslora` existe)
- ✅ Sin strings (imposible SQL Injection)
- ✅ IntelliSense (autocomplete al escribir)

---

#### 2. **Sustituye Criteria API (Construcción Programática)**

**Java (Cap. 8):**
```java
CriteriaBuilder cb = session.getCriteriaBuilder();
CriteriaQuery<Double> cq = cb.createQuery(Double.class);
Root<Barco> root = cq.from(Barco.class);
cq.select(cb.avg(root.get("eslora")))
  .where(cb.equal(root.get("tipo"), tipo));
Query<Double> query = session.createQuery(cq);
Double promedio = query.getSingleResult();
```

**C# (LINQ):**
```csharp
double promedio = await _context.Barcos
    .Where(b => b.Tipo == tipo)
    .AverageAsync(b => (double)b.Eslora);
```

**Ventajas:**
- ✅ Mucho menos código (2 líneas vs 7)
- ✅ Legible a primera vista
- ✅ No necesita conocer `CriteriaBuilder`, `Root`, etc.

---

#### 3. **Sustituye Spring Data Derived Methods (Cap. 11)**

**Java:**
```java
@Query("SELECT AVG(b.eslora) FROM Barco b WHERE b.tipo = :tipo")
Double getPromedioEsloraByTipo(@Param("tipo") String tipo);
```

**C# (LINQ):**
```csharp
Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default) =>
    _context.Barcos
        .Where(b => b.Tipo == tipo)
        .AverageAsync(b => (double)b.Eslora, ct);
```

**Ventajas:**
- ✅ No dependes de convenciones de nombres mágicas (`getPromedioEsloraByTipo`)
- ✅ Una única sintaxis: LINQ

---

### Tabla Comparativa: LINQ ↔ HQL ↔ Criteria ↔ Spring Data

| Operación | HQL (Java) | Criteria API | Spring Data | LINQ (C#) |
|---|---|---|---|---|
| **WHERE** | `WHERE b.tipo = :tipo` | `cb.equal(root.get("tipo"), tipo)` | `findByTipo(tipo)` | `.Where(b => b.Tipo == tipo)` |
| **COUNT** | `COUNT(b)` | `cb.count(root)` | `countByTipo(tipo)` | `.LongCountAsync()` |
| **AVG** | `AVG(b.eslora)` | `cb.avg(root.get("eslora"))` | `@Query("AVG...")` | `.AverageAsync(b => (double)b.Eslora)` |
| **>** | `b.eslora > :eslora` | `cb.gt(root.get("eslora"), eslora)` | `findByEsloraGreaterThan(eslora)` | `.Where(b => b.Eslora > eslora)` |
| **ORDER BY** | `ORDER BY b.nombre` | `cq.orderBy(cb.asc(...))` | `findByTipoOrderByNombre()` | `.OrderBy(b => b.Nombre)` |

### Cómo Probarlo

```powershell
# 1. Asegúrate de que existen barcos de tipo "Velero"
Invoke-WebRequest -Uri "http://localhost:5000/api/barcos" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"nombre":"Velero1","tipo":"Velero","eslora":12,"manga":8,"capacidad":20}' `
    -UseBasicParsing

# 2. Consulta el promedio
Invoke-WebRequest -Uri "http://localhost:5000/api/barcos/tipo/Velero/promedio-eslora" `
    -Method GET `
    -UseBasicParsing | Select-Object -ExpandProperty Content

# Respuesta esperada: 12.0 (o similar)
```

### El Concepto Clave

En Java tenías que **elegir**:
- ¿Uso HQL (fácil de leer, pero strings)?
- ¿Uso Criteria API (type-safe, pero verboso)?
- ¿Uso Spring Data Derived (mágico, pero inflexible)?

En C#, LINQ te da todo a la vez:
- ✅ Fácil de leer (como HQL)
- ✅ Type-safe (como Criteria API)
- ✅ Flexible (mejor que Spring Data)

No hay compromiso — simplemente escribes la condición en C# y EF Core la traduce a SQL automáticamente.

- **Checkpoint:** entiendes que LINQ sustituye simultáneamente a HQL, Criteria API y (en gran parte) a las consultas derivadas de Spring Data, con una sola sintaxis type-safe.

---

<a id="testing-con-xunit-y-moq-vs-junit-mockito"></a>

# 8. Testing con xUnit y Moq (vs JUnit/Mockito)

[↑ Volver al índice](#indice)

## 8.1. Correspondencia directa de conceptos

Tu Cap. 9 se traduce casi 1:1 — los conceptos son idénticos, solo cambia la sintaxis:

| Java (JUnit 5 + Mockito) | C# (xUnit + Moq) |
|---|---|
| `@Test` | `[Fact]` |
| `@BeforeEach` | Constructor de la clase de test |
| `@Mock` | `new Mock<T>()` |
| `@InjectMocks` | Pasar `mock.Object` al constructor manualmente |
| `when(x).thenReturn(y)` | `mock.Setup(x => ...).ReturnsAsync(y)` |
| `verify(x).metodo()` | `mock.Verify(x => x.Metodo(), Times.Once)` |
| `assertEquals(a, b)` | `a.Should().Be(b)` (FluentAssertions) |
| `assertThrows(...)` | `await FluentActions.Awaiting(...).Should().ThrowAsync<T>()` |

## 8.2. Ejemplo lado a lado

```java
// Java (Cap. 9.6)
@Test
void testFindById() {
    Barco barcoEsperado = new Barco();
    barcoEsperado.setId(1L);
    when(session.find(Barco.class, 1L)).thenReturn(barcoEsperado);

    Barco resultado = barcoDAO.findById(1L);

    assertNotNull(resultado);
    assertEquals(1L, resultado.getId());
}
```

```csharp
// Ver archivo del proyecto: MarinaApi.Tests/BarcoServiceTests.cs
[Fact]
public async Task FindByIdAsync_CuandoExiste_DevuelveDto()
{
    var barco = new Barco { Id = 1, Nombre = "Estrella del Mar" };
    _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(barco);

    var resultado = await _service.FindByIdAsync(1);

    resultado.Nombre.Should().Be("Estrella del Mar");
}
```

## 8.3. Una diferencia real: qué testeamos

En Java testeabas el **DAO** directamente (mockeando `Session`/`Transaction`). En el proyecto C#, testeamos el **Servicio** (mockeando el Repositorio), porque el repositorio genérico ya es tan delgado que apenas tiene lógica propia que merezca test — toda la lógica de negocio real vive en la capa de Servicio.

> **TIP:** esto es coherente con la arquitectura en capas que ya conoces (Controller → Service → Repository, Cap. 12.1 de Java): testeas la capa que **decide**, no la que solo traduce llamadas a la base de datos.

## 8.4. `It.IsAny<CancellationToken>()`

Una diferencia sin equivalente directo en Java: como todos nuestros métodos son `async` con `CancellationToken`, los mocks necesitan indicar que aceptan **cualquier** token, ya que el valor exacto no importa para el test.

## 8.5. Mock Tests en profundidad

### 8.5.1. Qué es un mock y por qué se usa

Un **mock** es una implementación falsa y controlada de una dependencia: en vez de que `BarcoService` hable con `IBarcoRepository` de verdad (que por debajo abre una conexión a SQL Server vía EF Core), el test le da un objeto que **finge** ser `IBarcoRepository`, pero cuyo comportamiento decide el propio test línea a línea.

La razón para hacerlo es aislar la **unidad bajo test** — en `BarcoServiceTests.cs`, la unidad es `BarcoService` — de todo lo que no sea su propia lógica:

- **Rápido:** no hay round-trip a una base de datos real, ni siquiera a una en memoria. Los tests de `BarcoServiceTests.cs` tardan milisegundos en total.
- **Determinista:** el mock siempre devuelve exactamente lo que el `Setup` le dice, así que el test no depende de qué datos haya en una tabla en un momento dado.
- **No depende de infraestructura real:** los tests pasan sin tener SQL Server corriendo, sin migraciones aplicadas, sin `docker-compose up`. Esto es lo que permite que `dotnet test` funcione en cualquier máquina (o en un pipeline de CI) sin preparar nada más.

Esto es la misma motivación que ya conocías de Mockito en Java (Cap. 9): el DAO/Repositorio no se testea a través del Service, se sustituye por un doble controlado.

### 8.5.2. Cómo se usa Moq en este proyecto — ejemplo real, línea a línea

Todo el testing de Moq del proyecto vive en un único fichero: [`MarinaApi.Tests/BarcoServiceTests.cs`](MarinaApi.Tests/BarcoServiceTests.cs). Vamos a diseccionar el constructor y dos tests.

**El constructor — crear el mock e "inyectarlo" a mano:**

```csharp
private readonly Mock<IBarcoRepository> _repositoryMock;
private readonly BarcoService _service;

public BarcoServiceTests()
{
    _repositoryMock = new Mock<IBarcoRepository>();
    var loggerMock = new Mock<ILogger<BarcoService>>();
    _service = new BarcoService(_repositoryMock.Object, loggerMock.Object);
}
```

- `Mock<IBarcoRepository>` — Moq genera en tiempo de ejecución una clase que implementa `IBarcoRepository` (la interfaz, nunca la clase `BarcoRepository`). Ningún método hace nada todavía: sin `Setup`, cualquier llamada devuelve el valor por defecto del tipo (`null`, `0`, una `Task` vacía...).
- `_repositoryMock.Object` — el objeto `Mock<T>` en sí **no** es un `IBarcoRepository`; `.Object` es la propiedad que expone el doble falso que sí implementa la interfaz. Es lo único que se le pasa al constructor real de `BarcoService`.
- xUnit ejecuta el constructor de la clase de test **antes de cada `[Fact]`**, así que cada test arranca con un mock limpio — es el equivalente a `@BeforeEach` en JUnit.

**`Setup` + `ReturnsAsync` — decidir qué responde el mock:**

```csharp
[Fact]
public async Task FindByIdAsync_CuandoExiste_DevuelveDto()
{
    var barco = new Barco { Id = 1, Nombre = "Estrella del Mar", Tipo = "Velero", Eslora = 12 };
    _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(barco);

    var resultado = await _service.FindByIdAsync(1);

    resultado.Nombre.Should().Be("Estrella del Mar");
    resultado.Id.Should().Be(1);
}
```

- `Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))` — una expresión lambda que describe **qué llamada** interceptar: "cuando alguien llame a `FindByIdAsync` con el argumento `1`". `r` representa al futuro `IBarcoRepository` mockeado.
- `It.IsAny<CancellationToken>()` — comodín de Moq: como todos los métodos async del proyecto reciben un `CancellationToken` (ver 8.4), no queremos que el `Setup` falle solo porque el token exacto no coincide; le decimos "acepta cualquier token, no me importa su valor".
- `.ReturnsAsync(barco)` — versión async de `.Returns(...)`, pensada para métodos que devuelven `Task<T>`. Cuando `BarcoService` haga `await _repository.FindByIdAsync(1, ct)`, recibirá exactamente ese objeto `barco`, sin tocar EF Core ni SQL Server.
- El test nunca llama a `_repositoryMock` directamente — llama a `_service.FindByIdAsync(1)`, y es `BarcoService` quien por dentro invoca al repositorio mockeado. Eso es justo lo que se está testeando: la lógica de `BarcoService`, no el repositorio.

**`Verify` — comprobar que se llamó a algo, no solo qué devolvió:**

```csharp
[Fact]
public async Task DeleteAsync_CuandoExiste_LlamaADeleteUnaVez()
{
    var barco = new Barco { Id = 5, Nombre = "Corsario Negro" };
    _repositoryMock.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>()))
        .ReturnsAsync(barco);

    await _service.DeleteAsync(5);

    _repositoryMock.Verify(r => r.DeleteAsync(barco, It.IsAny<CancellationToken>()), Times.Once);
}
```

- Aquí `DeleteAsync` en `BarcoService` no devuelve nada útil que comprobar con `Should().Be(...)` — lo único observable es que **hizo lo correcto** con su dependencia. `Verify` es para ese caso: en vez de comprobar un resultado, comprueba una interacción.
- `Times.Once` afirma que `DeleteAsync(barco, ...)` se llamó exactamente una vez sobre el mock — ni cero (bug: no se borró nada) ni dos (bug: se borró por duplicado).
- Hay una variante más flexible con `It.Is<T>(...)`, usada en `CreateAsync_LlamaAlRepositorioConLaEntidadCorrecta`:
  ```csharp
  _repositoryMock.Verify(r => r.AddAsync(
      It.Is<Barco>(b => b.Nombre == "Rayo Azul" && b.Tipo == "Motor"),
      It.IsAny<CancellationToken>()), Times.Once);
  ```
  Esto no exige un objeto `Barco` idéntico por referencia, sino cualquier `Barco` que cumpla esa condición — útil porque el objeto que `BarcoService` construye internamente no es el mismo objeto que el test tiene a mano.

### 8.5.3. Mockear una interfaz vs. mockear una clase concreta

En `BarcoServiceTests.cs` solo se mockean interfaces: `IBarcoRepository` e `ILogger<BarcoService>`. Nunca se mockea `BarcoRepository` (la clase concreta que implementa la interfaz y por debajo usa el `DbContext` de EF Core). Esto no es casualidad — es la práctica correcta, y hay una razón concreta:

| | Mockear una **interfaz** (`IBarcoRepository`) | Mockear una **clase concreta** (`BarcoRepository`) |
|---|---|---|
| Qué garantiza Moq | Un doble que cumple el contrato público, sin más | Requiere que los miembros sean `virtual` (si la clase es `sealed` o los métodos no son virtuales, Moq no puede interceptarlos) |
| Acoplamiento | El test depende solo del **contrato** (qué métodos existen), no de cómo se implementan | El test queda acoplado a detalles internos de una implementación concreta |
| Fragilidad | Cambiar la implementación de `BarcoRepository` (p. ej. optimizar una query) no rompe ningún test | Cualquier refactor interno de la clase mockeada puede romper mocks que dependían de su forma exacta |
| Qué demuestra el test | "`BarcoService` funciona bien con **cualquier** cosa que cumpla `IBarcoRepository`" | "`BarcoService` funciona bien con **esta implementación concreta** simulada" — mucho menos útil |

En la práctica: si una dependencia ya tiene interfaz (como todos los repositorios y servicios de este proyecto, gracias al patrón Repository + DI del Cap. 6 de la guía base), mockea siempre la interfaz. Mockear una clase concreta es señal de que falta una abstracción — la solución casi nunca es "forzar el mock", sino extraer una interfaz.

### 8.5.4. Cuándo NO hace falta mock

No todo en el proyecto necesita un mock para testearse. `BarcoMapper` (en `MarinaApi/Mapping/BarcoMapper.cs`) es el ejemplo perfecto:

```csharp
public static BarcoDto ToDto(this Barco barco) =>
    new(barco.Id, barco.Nombre, barco.Tipo, barco.Eslora, barco.Manga, barco.Capacidad);
```

Este método no toca una base de datos, no llama a ningún servicio externo, no tiene estado ni dependencias — solo transforma un objeto en otro. Un test para `ToDto()` no necesita ningún `Mock<T>`:

```csharp
[Fact]
public void ToDto_MapeaTodosLosCampos()
{
    var barco = new Barco { Id = 1, Nombre = "Velero1", Tipo = "Velero", Eslora = 12, Manga = 5, Capacidad = 20 };

    var dto = barco.ToDto();

    dto.Id.Should().Be(1);
    dto.Nombre.Should().Be("Velero1");
    dto.Eslora.Should().Be(12);
}
```

La regla general: **mockea solo lo que cruza un límite que no controlas dentro del test** — base de datos, servicios externos, reloj del sistema, sistema de ficheros, red. Si el código bajo test es **lógica de dominio pura** (mapeos, cálculos, validaciones que no dependen de nada externo), añadir un mock no aporta nada — solo complica el test sin aislar nada real, porque no hay nada de lo que aislarse. Esto también es señal de diseño: cuanta más lógica de negocio viva en clases así de "puras" (sin dependencias), menos mocks hacen falta en general y más fácil es testear el proyecto.

---

## 8.6. Ejercicio

### Solución: Testing con xUnit + Moq

**Paso 1: Ejecutar tests existentes**

```powershell
cd d:\DAM\PROYECTOS\Marina_C#\MarinaApi.Tests
dotnet test
```

**Salida esperada:**
```
Test run for D:\DAM\PROYECTOS\Marina_C#\MarinaApi.Tests\bin\Debug\net8.0\MarinaApi.Tests.dll (.NET 8.0)
Test executor: xUnit.net [2.x]
Running 3 tests (parallel)...
  ✓ BarcoServiceTests.FindByIdAsync_CuandoExiste_DevuelveDto (95ms)
  ✓ BarcoServiceTests.FindByIdAsync_CuandoNoExiste_LanzaNotFoundException (23ms)
  ✓ BarcoServiceTests.FindAllAsync_DevuelveListaCompleta (45ms)

Test run successful.
Total tests: 3
Passed: 3
Failed: 0
```

**Paso 2: Escribir nuevo test**

Abre `MarinaApi.Tests/BarcoServiceTests.cs` y agrega:

```csharp
[Fact]
public async Task UpdateAsync_CuandoNoExiste_LanzaNotFoundException()
{
    // Arrange: configura mock para devolver null
    _repositoryMock
        .Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Barco?)null);
    
    // Act & Assert: intenta actualizar y espera excepción
    await FluentActions
        .Awaiting(() => _service.UpdateAsync(999, new BarcoRequestDto("Nuevo", "Velero", 10, 5, 20)))
        .Should()
        .ThrowAsync<NotFoundException>();
}
```

**Paso 3: Ejecuta el test**

```powershell
dotnet test --filter "UpdateAsync_CuandoNoExiste"
```

### Patrón AAA Explicado

```csharp
[Fact]
public async Task UpdateAsync_CuandoNoExiste_LanzaNotFoundException()
{
    // ARRANGE: preparar datos y mocks
    var dtoEntrante = new BarcoRequestDto("Nuevo", "Velero", 10, 5, 20);
    _repositoryMock
        .Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Barco?)null);
    
    // ACT: ejecutar la acción que queremos probar
    var action = () => _service.UpdateAsync(999, dtoEntrante);
    
    // ASSERT: verificar que lanzó la excepción esperada
    await action.Should().ThrowAsync<NotFoundException>();
}
```

### Comparación Java vs C#

| Concepto | Java (Mockito) | C# (Moq) |
|---|---|---|
| Setup | `when(mock.metodo()).thenReturn()` | `.Setup(m => m.Metodo()).Returns()` |
| Setup async | `when(mock.metodo()).thenReturn(Futures.immediateFuture())` | `.Setup(m => m.Metodo()).ReturnsAsync()` |
| Verify | `verify(mock, times(1)).metodo()` | `.Verify(m => m.Metodo(), Times.Once())` |
| Asserts | `assertEquals()`, `assertTrue()` | `FluentAssertions`: `.Should().Be()`, `.Should().ThrowAsync()` |

- **Checkpoint:** entiendes la correspondencia 1:1 de conceptos entre Mockito y Moq, y por qué en este proyecto se testea el Servicio en vez del Repositorio.

---

<a id="aspnet-core-web-api-vs-spring-boot"></a>

# 9. ASP.NET Core Web API (vs Spring Boot)

[↑ Volver al índice](#indice)

## 9.1. El "gran cambio" de Java, aplicado también en C#

Tu Cap. 10-11 mostraban cómo Spring Boot reducía drásticamente el código de acceso a datos. En C#, ASP.NET Core + EF Core llega aún más lejos gracias a los repositorios genéricos: no hay clase equivalente a `BarcoDAOImpl` en absoluto — solo interfaces pequeñas con las consultas específicas.

## 9.2. Arquitectura en capas — la misma que ya conoces

```
    Petición HTTP
         │
    ┌────▼────┐
    │Controller│  ← [ApiController] — igual que @RestController
    └────┬────┘
         │
    ┌────▼────┐
    │ Service  │  ← Lógica de negocio + DTOs
    └────┬────┘
         │
    ┌────▼────┐
    │Repository│  ← Genérico + específico
    └────┬────┘
         │
    ┌────▼────┐
    │ DbContext│  ← EF Core
    └────┬────┘
         │
    ┌────▼────┐
    │SQL Server│
    └─────────┘
```

Exactamente la misma arquitectura de tu Cap. 12.1 — Controller nunca toca el repositorio directamente, solo el Servicio.

## 9.3. Ejercicio

### Solución: Arquitectura en Capas del Proyecto

**Diagrama con archivos reales:**

```
    Petición HTTP (ej: POST /api/barcos)
         │
    ┌────▼──────────────────────────────┐
    │  Controllers/BarcosController.cs   │ ← [ApiController]
    │  - Mapea HTTP → DTOs               │
    │  - No toca lógica de negocio       │
    └────┬──────────────────────────────┘
         │ inyecta IBarcoService
    ┌────▼──────────────────────────────┐
    │  Services/BarcoService.cs          │ ← Lógica de negocio
    │  - Valida reglas de negocio        │
    │  - Maneja DTOs ← → Entidades       │
    │  - Transacciones (SaveChangesAsync)│
    └────┬──────────────────────────────┘
         │ inyecta IBarcoRepository
    ┌────▼──────────────────────────────┐
    │  Repositories/BarcoRepository.cs   │ ← Acceso a datos
    │  - Hereda de GenericRepository<T>  │
    │  - Consultas específicas (LINQ)    │
    └────┬──────────────────────────────┘
         │ usa
    ┌────▼──────────────────────────────┐
    │  Data/MarinaDbContext.cs           │ ← EF Core
    │  - DbSet<Barco>, etc.              │
    │  - Configuración relaciones        │
    └────┬──────────────────────────────┘
         │
    ┌────▼──────────────────────────────┐
    │  SQL Server (Docker)               │
    │  Tablas: Barcos, Amarres, Regatas  │
    └────────────────────────────────────┘
```

### ¿Por qué Controller NO debería inyectar IBarcoRepository?

**Razón 1: Separación de responsabilidades (Cap. 12.1 Java)**
- El Controller debería solo **transformar HTTP ↔ DTOs**
- La lógica de negocio (validaciones, cálculos, transacciones) pertenece al Servicio
- Si tocas BD directamente, mezclas presentación + lógica

**Razón 2: Testabilidad**
- Testear el Service requiere mockear solo IBarcoRepository (pequeño)
- Testear el Controller requiere mockear solo IBarcoService (también pequeño)
- Si Controller → Repository, tienes que mockear toda la capa de datos

**Ejemplo de MALO (evitar):**

```csharp
// ❌ NO HAGAS ESTO
[ApiController]
public class BarcosController : ControllerBase
{
    private readonly IBarcoRepository _repository;  // ❌ Saltarse el Servicio
    
    [HttpPost]
    public async Task<ActionResult<BarcoDto>> Create([FromBody] BarcoRequestDto dto)
    {
        var barco = dto.ToEntity();
        await _repository.AddAsync(barco);  // ❌ Lógica de negocio en Controller
        return Created(..., barco.ToDto());
    }
}
```

**Ejemplo de BUENO (patrón correcto):**

```csharp
// ✅ CORRECTO
[ApiController]
public class BarcosController : ControllerBase
{
    private readonly IBarcoService _service;  // ✅ Solo el Servicio
    
    [HttpPost]
    public async Task<ActionResult<BarcoDto>> Create([FromBody] BarcoRequestDto dto)
    {
        var creado = await _service.CreateAsync(dto);  // ✅ Delega al Servicio
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }
}
```

- **Checkpoint:** la arquitectura en capas es idéntica conceptualmente; lo que cambia es cuánto código hace falta para llegar a ella.

---

<a id="capa-de-servicio"></a>

# 10. Capa de Servicio

[↑ Volver al índice](#indice)

## 10.1. `@Service` → clase C# + registro explícito

```java
@Service
public class BarcoService {
    @Autowired
    private BarcoRepository barcoRepository;
}
```

```csharp
// Ver archivo del proyecto: Services/BarcoService.cs
public class BarcoService : IBarcoService
{
    private readonly IBarcoRepository _barcoRepository;

    public BarcoService(IBarcoRepository barcoRepository, ILogger<BarcoService> logger)
    {
        _barcoRepository = barcoRepository;
    }
}
```

> **Buena práctica:** en Java, tu Cap. 12.5 ya recomendaba inyección por constructor **en vez de** `@Autowired` sobre campo — en C# esa recomendación no es opcional, es la única forma que existe. Los campos son `readonly`, así que una vez inyectados no pueden cambiar accidentalmente.

## 10.2. `@Transactional` → `SaveChangesAsync()` (o transacción explícita)

```java
@Transactional
public BarcoDTO save(BarcoDTO dto) {
    Barco barco = BarcoMapper.toEntity(dto);
    return BarcoMapper.toDTO(barcoRepository.save(barco));
}
```

```csharp
// Ver archivo del proyecto: Services/BarcoService.cs
public async Task<BarcoDto> CreateAsync(BarcoRequestDto dto, CancellationToken ct = default)
{
    var barco = dto.ToEntity();
    var creado = await _barcoRepository.AddAsync(barco, ct);
    return creado.ToDto();
}
```

No hay un atributo equivalente a `@Transactional` que se ponga "por costumbre" en cada método: en EF Core, `SaveChangesAsync()` ya es transaccional por sí solo. Solo usamos una transacción explícita (`BeginTransactionAsync`) cuando de verdad agrupamos varias operaciones — ver `RegataService.DeleteAsync` en el proyecto, equivalente mejorado del `deleteWithCleanup` de tu Cap. 12.7.

## 10.3. `Optional<T>` → nullable reference types

```java
public BarcoDTO findById(Long id) {
    return barcoRepository.findById(id)
            .map(BarcoMapper::toDTO)
            .orElse(null);
}
```

```csharp
public async Task<BarcoDto> FindByIdAsync(long id, CancellationToken ct = default)
{
    var barco = await _barcoRepository.FindByIdAsync(id, ct)
        ?? throw new NotFoundException(nameof(Models.Barco), id);
    return barco.ToDto();
}
```

> **TIP:** fíjate en la diferencia de filosofía: Java devolvía `null` y dejaba que el Controller comprobara `if (barco != null)` (Cap. 13.3). En C#, el Servicio **lanza una excepción de dominio** (`NotFoundException`) que un middleware global convierte en 404 — nadie más en la aplicación necesita comprobar null manualmente. Ver Capítulo 11 de esta guía.

## 10.4. AOP — `@Transactional`/`@Autowired` en Java vs Middleware en C#

Tu Cap. 12.8 explicaba que `@Transactional` y `@Autowired` funcionan gracias a **AOP** (aspectos que se ejecutan automáticamente alrededor de tus métodos). En C#, el concepto equivalente son los **Middlewares** de ASP.NET Core (ver Capítulo 11) — cada petición HTTP pasa por una cadena de middlewares antes de llegar al Controller, cada uno pudiendo interceptar, modificar o cortocircuitar la petición.

## 10.5. Ejercicio

### Solución: Métodos Avanzados en Servicio

**Paso 1: Analiza `InscribirBarcoAsync` en `Services/RegataService.cs`**

Busca este método y observa por qué se modifica `barco.Regatas` (no `regata.Barcos`):

```csharp
public async Task InscribirBarcoAsync(long regataId, long barcoId, CancellationToken ct = default)
{
    var regata = await _regataRepository.FindByIdAsync(regataId, ct)
        ?? throw new NotFoundException(nameof(Regata), regataId);
    var barco = await _barcoRepository.FindByIdAsync(barcoId, ct)
        ?? throw new NotFoundException(nameof(Barco), barcoId);
    
    // Se modifica BARCO, no REGATA
    barco.Regatas.Add(regata);  // ← Correcto
    
    await _context.SaveChangesAsync(ct);
}
```

**¿Por qué?** Porque en una relación N:M, EF Core configura un lado como "propietario". Aunque ambas direcciones funcionan, es más eficiente hacerlo desde el lado que la configura primero en `OnModelCreating()`. Ver Capítulo 4.3.

**Paso 2: Agregar `FindTop5PorCapacidadAsync` al Servicio**

En `IBarcoRepository`:

```csharp
Task<List<Barco>> FindTop5ByCapacidadDescendingAsync(CancellationToken ct = default);
```

En `BarcoRepository`:

```csharp
public async Task<List<Barco>> FindTop5ByCapacidadDescendingAsync(CancellationToken ct = default) =>
    await _context.Barcos
        .OrderByDescending(b => b.Capacidad)
        .Take(5)
        .ToListAsync(ct);
```

En `IBarcoService`:

```csharp
Task<List<BarcoDto>> FindTop5ByCapacidadAsync(CancellationToken ct = default);
```

En `BarcoService`:

```csharp
public async Task<List<BarcoDto>> FindTop5ByCapacidadAsync(CancellationToken ct = default)
{
    var barcos = await _barcoRepository.FindTop5ByCapacidadDescendingAsync(ct);
    _logger.LogInformation("📊 Top 5 barcos por capacidad obtenidos");
    return barcos.Select(b => b.ToDto()).ToList();
}
```

- **Checkpoint:** entiendes que la capa de Servicio cumple la misma función en ambos lenguajes, y que el manejo de "no encontrado" cambia de un patrón `null`-check a excepciones + middleware.

---

<a id="api-rest-con-controllers"></a>

# 11. API REST con Controllers

[↑ Volver al índice](#indice)

## 11.1. `@RestController` → `[ApiController]`

```java
@RestController
@RequestMapping("/api/barcos")
public class BarcoController {
    @Autowired
    private BarcoService barcoService;
}
```

```csharp
// Ver archivo del proyecto: Controllers/BarcosController.cs
[ApiController]
[Route("api/[controller]")]
public class BarcosController : ControllerBase
{
    private readonly IBarcoService _barcoService;

    public BarcosController(IBarcoService barcoService)
    {
        _barcoService = barcoService;
    }
}
```

`[controller]` en la ruta se sustituye automáticamente por el nombre de la clase sin el sufijo `Controller` — `BarcosController` → `/api/Barcos`.

## 11.2. Los 5 verbos, idénticos

| Verbo | Java | C# |
|---|---|---|
| Listar | `@GetMapping` | `[HttpGet]` |
| Buscar uno | `@GetMapping("/{id}")` + `@PathVariable` | `[HttpGet("{id:long}")]` + parámetro |
| Crear | `@PostMapping` + `@RequestBody` | `[HttpPost]` + `[FromBody]` |
| Actualizar | `@PutMapping("/{id}")` | `[HttpPut("{id:long}")]` |
| Eliminar | `@DeleteMapping("/{id}")` | `[HttpDelete("{id:long}")]` |

```csharp
// Ver archivo del proyecto: Controllers/BarcosController.cs
[HttpGet("{id:long}")]
[ProducesResponseType(typeof(BarcoDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<BarcoDto>> GetById(long id, CancellationToken ct) =>
    Ok(await _barcoService.FindByIdAsync(id, ct));
```

> **¿Sabías que?** `{id:long}` en la ruta es una **restricción de tipo** — si alguien pide `/api/barcos/abc`, ASP.NET Core devuelve 404 automáticamente sin ni siquiera llegar a ejecutar el método, porque `abc` no es un `long`. Java no tiene un equivalente tan directo en `@PathVariable`.

## 11.3. `ResponseEntity<T>` → `ActionResult<T>`

```java
return new ResponseEntity<>(barco, HttpStatus.OK);
return new ResponseEntity<>(HttpStatus.NOT_FOUND);
```

```csharp
return Ok(barco);
return NotFound();
return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado); // 201 + Location header
return NoContent(); // 204
```

## 11.4. La mejora clave: sin `if (x == null)` en el Controller

Tu Cap. 13.3 repetía este patrón en **cada** endpoint:

```java
@GetMapping("/{id}")
public ResponseEntity<BarcoDTO> getBarcoById(@PathVariable Long id) {
    BarcoDTO barco = barcoService.findById(id);
    if (barco != null) {
        return new ResponseEntity<>(barco, HttpStatus.OK);
    }
    return new ResponseEntity<>(HttpStatus.NOT_FOUND);
}
```

En el proyecto C#, ningún Controller tiene ese `if`:

```csharp
// Ver archivo del proyecto: Controllers/BarcosController.cs
[HttpGet("{id:long}")]
public async Task<ActionResult<BarcoDto>> GetById(long id, CancellationToken ct) =>
    Ok(await _barcoService.FindByIdAsync(id, ct));
    // Si no existe, el Servicio lanza NotFoundException.
    // Un middleware global (ver Middleware/ExceptionHandlingMiddleware.cs)
    // la convierte en 404 automáticamente. El Controller no necesita saberlo.
```

Este middleware es una pieza que **no tiene equivalente directo** en tu tutorial Java — es una mejora arquitectónica añadida específicamente en este port.

```csharp
// Ver archivo del proyecto: Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (NotFoundException ex) { /* → 404 uniforme */ }
        catch (Exception ex) { /* → 500 uniforme, sin filtrar detalles internos */ }
    }
}
```

## 11.5. Ejercicio

### Solución: Endpoints REST sin Null Checks

**Paso 1: Análisis - Comparar Java vs C#**

**En Java (Cap. 13 - REST):**
```java
// REPETIDO EN CADA MÉTODO
@GetMapping("/{id}")
public ResponseEntity<BarcoDTO> getById(@PathVariable Long id) {
    BarcoDTO barco = barcoService.findById(id);
    if (barco != null) {                    // ← if null check
        return new ResponseEntity<>(barco, HttpStatus.OK);
    }
    return new ResponseEntity<>(HttpStatus.NOT_FOUND);
}

// Mismo patrón repetido aquí
@PostMapping
public ResponseEntity<BarcoDTO> create(@RequestBody BarcoRequestDTO dto) {
    // ... validaciones ...
    if (/* error */) {
        return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
    }
    // ...
}
```

**En C# (Este proyecto) - Ver `Controllers/BarcosController.cs`:**
```csharp
// ✅ SIN if (... == null) - Cero null checks
[HttpGet("{id:long}")]
public async Task<ActionResult<BarcoDto>> GetById(long id, CancellationToken ct) =>
    Ok(await _barcoService.FindByIdAsync(id, ct));
    // Si no existe, lanza NotFoundException → Middleware lo convierte en 404

[HttpPost]
public async Task<ActionResult<BarcoDto>> Create([FromBody] BarcoRequestDto dto, CancellationToken ct)
{
    var creado = await _barcoService.CreateAsync(dto, ct);
    return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    // ASP.NET Core valida automáticamente el DTO (requeridos, MaxLength, etc.)
}
```

**Paso 2: Agregar endpoint `GET /api/barcos/eslora/{minima}`**

En `Controllers/BarcosController.cs`:

```csharp
/// <summary>Obtiene barcos con eslora mayor o igual a la especificada.</summary>
[HttpGet("eslora/{minima:int}")]
[ProducesResponseType(typeof(List<BarcoDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<BarcoDto>>> GetByEsloraGreaterThan(int minima, CancellationToken ct) =>
    Ok(await _barcoService.FindByEsloraGreaterThanAsync(minima, ct));
```

**Parámetro:** `{minima:int}` asegura que solo acepta números — si envías `/api/barcos/eslora/abc`, ASP.NET Core devuelve 400 automáticamente.

**Cómo probarlo:**

```powershell
# Obtener barcos con eslora >= 12
Invoke-WebRequest -Uri "http://localhost:5000/api/barcos/eslora/12" `
    -Method GET `
    -UseBasicParsing | Select-Object -ExpandProperty Content

# Respuesta: lista de barcos
# Ejemplo: [{"id":1,"nombre":"Velero1","tipo":"Velero","eslora":15,"manga":9,"capacidad":25}]
```

### Por Qué Esto Es Mejor

| Aspecto | Java (Cap. 13) | C# (Middleware) |
|---|---|---|
| **Null checks** | ❌ Repetidos en cada método | ✅ Centralizados en middleware |
| **Código en Controller** | Largo (si + return) | Corto (una línea) |
| **Mantenimiento** | ❌ Cambiar lógica = editar N métodos | ✅ Cambiar lógica = editar 1 middleware |
| **Formato de error** | Cuerpo vacío | ✅ JSON `problem+json` (RFC 7807) |
| **Validación DTO** | Manual en cada método | ✅ Automática (data annotations) |

- **Checkpoint:** entiendes cómo el middleware centraliza el manejo de "no encontrado", eliminando la repetición que existía en cada controlador Java.

---

<a id="dtos-y-mapper"></a>

# 12. DTOs y Mapper

[↑ Volver al índice](#indice)

## 12.1. El mismo problema, la misma solución

Tu Cap. 14 explicaba dos problemas de exponer entidades JPA directamente: bucle infinito por relaciones circulares y exposición de datos internos. **Exactamente el mismo problema existe en C#** con `System.Text.Json` — la solución es idéntica: DTOs.

## 12.2. `record` en vez de clase + Lombok

```java
@Data
@NoArgsConstructor
@AllArgsConstructor
public class BarcoDTO {
    private Long id;
    private String nombre;
    private String tipo;
    private int eslora;
    private int manga;
    private int capacidad;
}
```

```csharp
// Ver archivo del proyecto: Dtos/BarcoDtos.cs
public record BarcoDto(
    long Id,
    string Nombre,
    string Tipo,
    int Eslora,
    int Manga,
    int Capacidad
);
```

Una sola línea (`record`) sustituye a `@Data + @NoArgsConstructor + @AllArgsConstructor` de Lombok: genera constructor, igualdad por valor y `ToString()` automáticamente. Es sintaxis del propio lenguaje C#, no una librería añadida.

## 12.3. Mejora: DTO de entrada separado del de salida

Java reutilizaba el mismo `BarcoDTO` para request y response — lo que técnicamente permitía a un cliente **enviar un `id`** al crear un barco (aunque el servidor lo ignorase). En C# separamos:

```csharp
// Ver archivo del proyecto: Dtos/BarcoDtos.cs
public record BarcoRequestDto(string Nombre, string Tipo, int Eslora, int Manga, int Capacidad);
// ← SIN Id: es físicamente imposible que el cliente lo envíe

public record BarcoDto(long Id, string Nombre, string Tipo, int Eslora, int Manga, int Capacidad);
// ← el de salida, CON Id (lo genera la BD)
```

## 12.4. `BarcoMapper` → extension methods

```java
public class BarcoMapper {
    public static BarcoDTO toDTO(Barco barco) { ... }
    public static Barco toEntity(BarcoDTO dto) { ... }
}

// Uso: BarcoMapper.toDTO(barco)
```

```csharp
// Ver archivo del proyecto: Mapping/BarcoMapper.cs
public static class BarcoMapper
{
    public static BarcoDto ToDto(this Barco barco) => new(...);
    public static Barco ToEntity(this BarcoRequestDto dto) => new() { ... };
}

// Uso: barco.ToDto()  ← se lee como si fuera un método de la propia clase Barco
```

> **TIP:** el `this` antes del primer parámetro convierte el método estático en un **extension method** — puedes escribir `barco.ToDto()` en vez de `BarcoMapper.ToDto(barco)`. Esto se encadena de maravilla con LINQ: `barcos.Select(b => b.ToDto()).ToList()`, exactamente igual que `barcos.stream().map(BarcoMapper::toDTO).collect(toList())` en Java (Cap. 14.6), pero sin necesidad de *method reference* explícito.

## 12.5. Ejercicio

### Solución: DTOs y Mappers Tipados

**Paso 1: Analiza `Mapping/BarcoMapper.cs`**

Abre el archivo y verás que `UpdateFromDto` sustituye a `updateEntityFromDTO` de Java:

```csharp
public static void UpdateFromDto(this Barco barco, BarcoRequestDto dto)
{
    barco.Nombre = dto.Nombre;
    barco.Tipo = dto.Tipo;
    barco.Eslora = dto.Eslora;
    barco.Manga = dto.Manga;
    barco.Capacidad = dto.Capacidad;
}

// Uso: barco.UpdateFromDto(dtoEntrante);
```

**Comparación con Java (Cap. 14.4):**

| Java Lombok | C# |
|---|---|
| `barco.setNombre(dto.getNombre())` | `barco.Nombre = dto.Nombre` |
| Clase con métodos estáticos | Extension method: `barco.UpdateFromDto(dto)` |
| `@Data + @NoArgsConstructor` | `record` nativo |
| `BarcoMapper.toDTO(barco)` | `barco.ToDto()` (extension method) |

**Paso 2: Crear `TripulanteDto` y `TripulanteMapper`**

Crea `Dtos/TripulanteDtos.cs`:

```csharp
namespace MarinaApi.Dtos;

public record TripulanteDto(
    long Id,
    string Nombre,
    string Rol,
    long BarcoId
);

public record TripulanteRequestDto(
    string Nombre,
    string Rol,
    long BarcoId
);
```

Crea `Mapping/TripulanteMapper.cs`:

```csharp
namespace MarinaApi.Mapping;

public static class TripulanteMapper
{
    public static TripulanteDto ToDto(this Tripulante tripulante) => new(
        tripulante.Id,
        tripulante.Nombre,
        tripulante.Rol,
        tripulante.BarcoId
    );

    public static Tripulante ToEntity(this TripulanteRequestDto dto) => new()
    {
        Nombre = dto.Nombre,
        Rol = dto.Rol,
        BarcoId = dto.BarcoId
    };

    public static void UpdateFromDto(this Tripulante tripulante, TripulanteRequestDto dto)
    {
        tripulante.Nombre = dto.Nombre;
        tripulante.Rol = dto.Rol;
    }
}
```

### Ventajas del Patrón DTO Separado

| Aspecto | Un único DTO (Java) | DTOs Separados (C#) |
|---|---|---|
| **Cliente puede enviar ID** | ❌ Teóricamente sí (aunque el servidor lo ignora) | ✅ Imposible (TripulanteRequestDto no tiene Id) |
| **Seguridad** | ⚠️ Confiamos en que ignore el Id | ✅ Garantizado por el tipo |
| **Documentación** | "Espera, ¿puedo mandar Id?" | Claro, RequestDto solo tiene Nombre, Rol, BarcoId |
| **Type-safety** | ❌ Un DTO para dos propósitos | ✅ Cada DTO para su propósito |

- **Checkpoint:** entiendes por qué separar DTO de entrada y de salida es una mejora de seguridad sobre el `BarcoDTO` único de Java, y cómo `record` + extension methods sustituyen a Lombok + una clase Mapper tradicional.

---

<a id="swagger-openapi"></a>

# 13. Swagger / OpenAPI

[↑ Volver al índice](#indice)

## 13.1. Springdoc vs Swashbuckle

| Java | C# |
|---|---|
| Librería | `springdoc-openapi-starter-webmvc-ui` | `Swashbuckle.AspNetCore` |
| URL de la UI | `/swagger-ui.html` | `/swagger` |
| Configuración mínima | `application.properties` (2 líneas) | `Program.cs` (`AddSwaggerGen()`) |

## 13.2. Menos anotaciones necesarias

Java necesitaba anotar cada endpoint manualmente (Cap. 15.3-15.4): `@Tag`, `@Operation`, `@ApiResponse`, `@Parameter`. En C#, Swashbuckle **infiere automáticamente** mucho de esto a partir de los tipos:

```csharp
// Ver archivo del proyecto: Controllers/BarcosController.cs
[HttpGet("{id:long}")]
[ProducesResponseType(typeof(BarcoDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<BarcoDto>> GetById(long id, CancellationToken ct) => ...
```

Con solo `[ProducesResponseType]` (que además sirve para documentar, no solo para Swagger), ya se genera automáticamente en la UI: el tipo de respuesta, los códigos posibles y los parámetros — sin `@Operation` ni `@ApiResponse` explícitos como en Java.

## 13.3. Configuración en `Program.cs`

```csharp
// Ver archivo del proyecto: Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Marina API", Version = "v1" });
});

// más abajo:
app.UseSwagger();
app.UseSwaggerUI();
```

## 13.4. Ejercicio

### Solución: Explora Swagger UI

**Paso 1: Arranca la API y abre Swagger**

```powershell
cd d:\DAM\PROYECTOS\Marina_C#\MarinaApi
dotnet run
```

En el navegador abre: `http://localhost:5000/swagger`

**Comparación Visual con Java (Cap. 15.5):**

| Aspecto | Java (Springdoc) | C# (Swashbuckle) |
|---|---|---|
| **URL** | `/swagger-ui.html` | `/swagger` |
| **Información mostrada** | Manualmente anotada (`@Tag`, `@Operation`) | Inferida automáticamente del código |
| **Response types** | Requiere `@ApiResponse` | Usa `[ProducesResponseType]` |
| **Modelos** | Se generan solos | Se generan solos |
| **Interfaz** | Similar | Similar |

**Paso 2: Prueba el endpoint "Inscribir barco" desde Swagger UI**

1. En Swagger, busca `/api/regatas/{regataId}/barcos/{barcoId}`
2. Expande `POST` (debe estar en `RegatasController`)
3. Haz clic en **"Try it out"**
4. Rellena:
   - `regataId`: `1`
   - `barcoId`: `1`
5. Haz clic en **"Execute"**

**Respuesta esperada:**
```
Status: 204 No Content
```

(204 significa "éxito, sin contenido" — la inscripción se completó)

### Por Qué Swashbuckle Es Mejor

```csharp
// ❌ Java necesitaba anotar CADA endpoint
@Tag(name = "Barcos")
@Operation(summary = "Obtiene todos los barcos")
@ApiResponses(value = {
    @ApiResponse(responseCode = "200", description = "Barcos obtenidos"),
    @ApiResponse(responseCode = "500", description = "Error interno")
})
public List<BarcoDTO> getAll() { ... }

// ✅ C# lo infiere del código
[HttpGet]
[ProducesResponseType(typeof(List<BarcoDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<BarcoDto>>> GetAll(CancellationToken ct) => ...
```

Swashbuckle necesita MENOS anotaciones porque el tipo del return (`ActionResult<List<BarcoDto>>`) ya dice todo lo necesario.

- **Checkpoint:** Swashbuckle cubre el mismo rol que Springdoc, generando más documentación automáticamente a partir del código, con menos anotaciones manuales.

---

<a id="probar-la-api-swagger-ui-y-postman"></a>

# 14. Probar la API: Swagger UI y Postman

[↑ Volver al índice](#indice)

## 14.1. Todo lo de tu Cap. 16 se aplica igual

Postman **no distingue** si el backend es Java o C# — habla HTTP, y tanto Spring Boot como ASP.NET Core exponen el mismo tipo de endpoints REST. Puedes reutilizar exactamente la misma colección de Postman de tu Cap. 16, cambiando solo el puerto (ASP.NET Core suele usar un puerto distinto a 8080, indicado en la consola al arrancar con `dotnet run`).

## 14.2. Diferencias menores a tener en cuenta

| Aspecto | Java (Cap. 16) | C# |
|---|---|---|
| Puerto por defecto | 8080 | Variable (aparece en consola al hacer `dotnet run`, típicamente 5000/5001 o el que asigne `launchSettings.json`) |
| Formato de error | Cuerpo vacío en 404 | JSON `problem+json` con `title`, `status`, `detail` (RFC 7807) — más información útil para depurar desde Postman |
| Scripts de test (`pm.test(...)`) | Igual | Igual — el JavaScript de Postman no cambia |

## 14.3. Ejercicio

### Solución: Probar con Postman y Observar Errores

**Paso 1: Crea colección en Postman**

Si tienes la colección Java del Cap. 16.3, importa y actualiza:
- Host: `localhost`
- Puerto: `5000` (el que aparezca en consola al hacer `dotnet run`)

O crea manualmente:
1. New Collection → `Marina API - C#`
2. Add Request → `GET /api/barcos`
3. URL: `http://localhost:5000/api/barcos`
4. Send

**Paso 2: Prueba un ID inexistente y observa el error**

Crea request:
- Método: **GET**
- URL: `http://localhost:5000/api/barcos/999`

**Respuesta en Java (Cap. 16.3):**
```
Status: 404 Not Found
Body: (vacío)
```

**Respuesta en C# (Este proyecto):**
```
Status: 404 Not Found
Body (JSON - RFC 7807 "problem+json"):
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Barco with id 999 not found",
  "traceId": "0HMJN2F3G9P4K:00000001"
}
```

### Comparación: Formatos de Error

| Aspecto | Java | C# |
|---|---|---|
| **Status** | 404 | 404 |
| **Body** | Vacío ❌ | JSON informativo ✅ |
| **Error message** | Adivina qué salió mal | Explícito: "Barco with id 999 not found" |
| **Estándar** | Personalizado | RFC 7807 `application/problem+json` |
| **Debugging** | Difícil (vacío) | Fácil (mensaje claro) |
| **Trace ID** | No | Sí (para logs) |

### Por Qué Es Mejor

Postman es agnóstico del lenguaje — funciona igual con REST en cualquier tecnología. Pero el **formato de error difiere**:

- **Java**: siguió un patrón minimalista (error vacío)
- **C#**: sigue RFC 7807 (formato estándar de errores HTTP)

```csharp
// En C#, el middleware ExceptionHandlingMiddleware.cs
catch (NotFoundException ex)
{
    context.Response.ContentType = "application/problem+json";
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    
    await context.Response.WriteAsJsonAsync(new
    {
        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        title = "Not Found",
        status = 404,
        detail = ex.Message,
        traceId = context.TraceIdentifier
    });
}
```

### Ejercicio Adicional: Prueba otros errores

1. **Validación fallida (400):**
   ```
   POST /api/barcos
   Body: {"nombre":"","tipo":"Velero"}  ← nombre vacío
   ```
   Respuesta: 400 Bad Request con detalles

2. **Error interno (500) — artificialmente:**
   Lanza una excepción en un método, verá 500 con traceId

- **Checkpoint:** sabes que las herramientas de prueba (Postman, Swagger UI, curl) son agnósticas al lenguaje del backend — lo único que cambia es el formato de los errores, más informativo en este proyecto C#.

---

<a id="resumen-mapa-completo-del-proyecto"></a>

# 15. Resumen: mapa completo del proyecto

[↑ Volver al índice](#indice)

## 15.1. Tabla maestra de equivalencias

| Concepto | Java (tu tutorial) | C# (proyecto `MarinaApi`) |
|---|---|---|
| ORM | Hibernate | Entity Framework Core |
| Especificación | JPA | (no aplica, EF Core es único) |
| Contexto de persistencia | `Session` + `SessionFactory` | `DbContext` |
| Configuración BD | `hibernate.cfg.xml` / `application.properties` | `appsettings.json` |
| Mapeo entidad-tabla | `@Entity`, `@Column` | Convención + Data Annotations |
| Relaciones | `@OneToOne`, `@ManyToMany`, `mappedBy` | Fluent API en `OnModelCreating` |
| Getters/setters | Lombok `@Data` | Propiedades automáticas nativas |
| DAO | `BarcoDAO` + `BarcoDAOImpl` (uno por entidad) | `GenericRepository<T>` único |
| Consultas | HQL / SQL nativo / Criteria API | LINQ (unifica los tres) |
| Framework web | Spring Boot | ASP.NET Core |
| Inyección de dependencias | `@Autowired`, auto-detectado | Constructor + registro explícito en `Program.cs` |
| Capa de negocio | `@Service` | Clase de servicio + interfaz |
| Transacciones | `@Transactional` | `SaveChangesAsync()` implícito / `BeginTransactionAsync()` explícito |
| Controlador REST | `@RestController` | `[ApiController]` |
| DTOs | Clase + Lombok | `record` |
| Mapper | Clase con métodos estáticos | Extension methods |
| Manejo de "no encontrado" | `if (x == null) return 404` en cada método | Excepción + Middleware global |
| Testing | JUnit 5 + Mockito | xUnit + Moq + FluentAssertions |
| Documentación API | Springdoc OpenAPI | Swashbuckle |
| Motor de BD | MySQL (tutorial) | **SQL Server** (real, Espiral MS) |

## 15.2. Cómo sigue encajando con tu plan de verano

- Este proyecto es la aplicación práctica de las **Lecciones 6-8** de tu `GUIA_DEFINITIVA_CSHARP.md` (Patrones de Diseño, SQL, Entity Framework Core)
- Úsalo para practicar la **Lección 9** (Git): crea ramas `feature/tripulantes` siguiendo los ejercicios de esta guía, haz commits con Conventional Commits, simula un PR
- Úsalo para practicar la **Lección 10** (Scrum): trocea los ejercicios de cada capítulo de este documento en historias de usuario con story points, y corre un mini-sprint tú mismo

## 15.3. Qué NO se ha portado (y por qué no hace falta)

- **AOP personalizado** (Cap. 12.8 de Java): no lo necesitas — los middlewares de ASP.NET Core cubren el mismo rol para lo que este proyecto necesita
- **Thymeleaf** (mencionado en el roadmap de tu Cap. 1): si las prácticas requieren interfaz web server-side en vez de solo API REST, el equivalente sería Razor Pages — pídemelo aparte si llega a hacer falta

---

**Fin de la guía.**

Con esto tienes tanto la teoría (este documento) como la práctica ya resuelta (el proyecto `MarinaApi.zip`). Recomendación de uso: lee un capítulo, abre el archivo del proyecto que referencia, y solo después haz el ejercicio propuesto — igual que trabajabas con los capítulos Java originales.
