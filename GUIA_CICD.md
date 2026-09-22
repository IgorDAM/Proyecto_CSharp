# Guía de CI/CD

Guía práctica de integración y despliegue continuos aplicada a MarinaApi
(ASP.NET Core 8) con GitHub Actions. Se apoya en `GUIA_GIT.md`.

> Las rutas de proyecto y nombres de este documento (`MarinaApi/MarinaApi.csproj`,
> nombre de la solución) ya están adaptados a la estructura real del repo. La
> rama principal de MarinaApi es `master` (no `main`); si tu repo usa otro
> nombre, ajústalo.

## Índice

1. CI vs CD
2. Anatomía de un pipeline
3. GitHub Actions
4. Workflow de MarinaApi
5. El artifact
6. Entornos y secretos
7. Tests con base de datos en CI
8. Docker, Azure y AWS
9. Glosario rápido

---

## 1. CI vs CD

| Sigla | Significa | Qué garantiza |
|---|---|---|
| CI | Continuous Integration | Cada cambio se compila y se prueba automáticamente |
| CD | Continuous Delivery | Cada cambio que pasa CI queda listo para desplegar, con aprobación manual |
| CD | Continuous Deployment | Igual, pero se despliega a producción sin aprobación manual |

La sigla CD es ambigua: casi siempre se refiere a *Delivery*. La diferencia
es si hay una persona que pulsa el botón antes de producción.

Cadena completa:

```text
commit → push → CI (build + test) → merge → artifact → staging → aprobación → producción
```

Idea central: el artifact se construye **una sola vez** y ese mismo paquete
se promueve de un entorno al siguiente.

---

## 2. Anatomía de un pipeline

Un pipeline es una receta automática con estas piezas:

- **Trigger:** qué lo dispara (un push, un PR, una hora, un botón).
- **Job:** un bloque de trabajo que corre en una máquina limpia.
- **Step:** cada paso dentro de un job (un comando o una acción reutilizable).
- **Runner:** la máquina que ejecuta el job (en GitHub, `ubuntu-latest`).
- **Artifact:** archivos que un job produce y otros jobs reutilizan.
- **Environment:** el destino (staging, producción) con sus reglas y secretos.

Los jobs corren en paralelo salvo que uno declare que depende de otro.
Cada job arranca en una máquina limpia: lo que no guardes como artifact, se
pierde al acabar.

---

## 3. GitHub Actions

Los workflows son archivos YAML en `.github/workflows/`. Cada archivo es un
pipeline independiente.

### Equivalencia con Java

| Concepto | Jenkins (habitual con Java) | GitHub Actions |
|---|---|---|
| Definición | `Jenkinsfile` | `.github/workflows/*.yml` |
| Comando de build | `mvn package` | `dotnet build` / `dotnet publish` |
| Comando de test | `mvn test` | `dotnet test` |
| Resultado | `.jar` | carpeta de `dotnet publish` (o imagen Docker) |

El concepto es el mismo; cambia la herramienta y los comandos.

### Estructura mínima

```yaml
name: CI                      # nombre visible en GitHub

on:                           # triggers
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-test:                 # nombre del job
    runs-on: ubuntu-latest    # runner
    steps:
      - uses: actions/checkout@v4     # reutiliza una acción publicada
      - run: echo "hola"              # ejecuta un comando
```

`uses:` invoca una acción ya hecha; `run:` ejecuta un comando de shell.

---

## 4. Workflow de MarinaApi

Archivo: `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [master]
  pull_request:
    branches: [master]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restaurar dependencias
        run: dotnet restore

      - name: Compilar
        run: dotnet build --configuration Release --no-restore

      - name: Ejecutar tests
        run: dotnet test --configuration Release --no-build --verbosity normal
```

### Por qué cada paso

- `checkout`: el runner arranca vacío; esto descarga tu código.
- `setup-dotnet`: instala el SDK que pides. Fijar `8.0.x` hace el build
  reproducible.
- `restore`: descarga los paquetes NuGet (equivale a la resolución de
  dependencias de Maven).
- `build --no-restore`: no repite el restore que acabas de hacer.
- `test --no-build`: no recompila lo que ya compiló el paso anterior.

Si algún paso falla (un test en rojo, un error de compilación), el job se
marca como fallido y el PR queda bloqueado si tienes activada la protección
de rama.

### Proteger `master`

En GitHub: Settings → Branches → regla para `master` que exija que el check
`build-test` pase antes de hacer merge. Sin esto, el CI avisa pero no impide.

---

## 5. El artifact

Un artifact es el resultado empaquetado del build: lo que realmente se
despliega. Se genera con `dotnet publish`, que produce una carpeta con la
dll de la API y sus dependencias.

Se añaden dos jobs al mismo `ci.yml` (dentro de `jobs:`, al nivel de
`build-test`):

```yaml
  publish:
    needs: build-test         # solo si build-test pasó
    if: github.ref == 'refs/heads/master' && github.event_name == 'push'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Publicar
        run: dotnet publish MarinaApi/MarinaApi.csproj --configuration Release --output ./publish

      - name: Guardar artifact
        uses: actions/upload-artifact@v4
        with:
          name: marinaapi
          path: ./publish

  deploy-staging:
    needs: publish
    runs-on: ubuntu-latest
    environment: staging      # entorno con sus reglas y secretos
    steps:
      - name: Descargar artifact
        uses: actions/download-artifact@v4
        with:
          name: marinaapi
          path: ./publish

      - name: Desplegar
        run: echo "Aquí iría el despliegue real (ver sección 8)"
```

### Por qué así

- `needs:` encadena jobs: `publish` no arranca si los tests fallan.
- `if:` limita la publicación a pushes a `master`: los PRs solo compilan y
  prueban, no generan artifact desplegable.
- `upload-artifact` / `download-artifact`: como cada job corre en una
  máquina limpia, el artifact es el puente entre ellos.
- El job de deploy **no recompila**: descarga el mismo paquete. Esa es la
  regla de "construir una vez, desplegar muchas".

---

## 6. Entornos y secretos

### Entornos

Un environment de GitHub (`staging`, `production`) permite:

- exigir **aprobación manual** antes de que el job corra (para producción),
- tener **secretos propios** por entorno.

Se crean en Settings → Environments. El job los usa con `environment: nombre`.

### Secretos

Nunca van en el repo. Se guardan en Settings → Secrets and variables →
Actions (o dentro de un environment) y se leen así:

```yaml
env:
  ConnectionStrings__Default: ${{ secrets.DB_CONNECTION_STRING }}
```

### Configuración por entorno en .NET

.NET lee la configuración por capas; la última que define un valor gana:

1. `appsettings.json`
2. `appsettings.{Environment}.json` (por ejemplo `appsettings.Staging.json`)
3. Variables de entorno
4. Argumentos de línea de comandos

El entorno se elige con la variable `ASPNETCORE_ENVIRONMENT` (`Development`,
`Staging`, `Production`).

En variables de entorno, el separador de secciones JSON es el **doble guion
bajo**: `ConnectionStrings__Default` equivale a
`{ "ConnectionStrings": { "Default": "..." } }`.

### Comparativa con Spring

| Spring Boot | ASP.NET Core |
|---|---|
| `application-staging.yml` + perfil `staging` | `appsettings.Staging.json` + `ASPNETCORE_ENVIRONMENT=Staging` |
| `SPRING_DATASOURCE_URL` como variable de entorno | `ConnectionStrings__Default` como variable de entorno |
| Las variables de entorno pisan al fichero | Igual: las variables de entorno pisan al JSON |

---

## 7. Tests con base de datos en CI

Los tests actuales (xUnit + Moq) no necesitan base de datos: los repositorios
están mockeados. Si más adelante añades tests de integración contra
SQL Server (el proveedor real del proyecto, ver `Microsoft.EntityFrameworkCore.SqlServer`
en `MarinaApi.csproj` y `MarinaApi/docker-compose.yml`), GitHub Actions puede
levantar una base de datos junto al job con `services`:

```yaml
  build-test:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: "Y"
          MSSQL_SA_PASSWORD: "TuPassword123!"
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P TuPassword123! -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 10
    env:
      ConnectionStrings__Default: Server=localhost,1433;Database=marina_test;User Id=sa;Password=TuPassword123!;TrustServerCertificate=True
    steps:
      # ...los mismos pasos de la sección 4...
```

Puntos clave:

- El `health-cmd` hace que el job espere a que SQL Server esté listo antes de
  lanzar los tests.
- La contraseña de este ejemplo es de una base efímera que muere con el job;
  para bases reales siempre `secrets` (ver también el ticket #156 del backlog,
  que señala esta misma contraseña hardcodeada en `appsettings.json` y en
  `docker-compose.yml`).
- Con PostgreSQL o MySQL sería igual cambiando imagen, puerto y variables,
  pero hoy el proyecto usa SQL Server.

---

## 8. Docker, Azure y AWS

Esta sección es orientativa: el despliegue a la nube es la última prioridad
del plan y conviene revisar la documentación vigente cuando se aborde.

### Docker (multi-stage)

`Dockerfile` en la raíz del repo:

```dockerfile
# Etapa 1: compilar con el SDK (imagen grande)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish MarinaApi/MarinaApi.csproj -c Release -o /app

# Etapa 2: ejecutar solo con el runtime (imagen pequeña)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MarinaApi.dll"]
```

Por qué dos etapas: el SDK pesa mucho y solo hace falta para compilar. La
imagen final lleva únicamente el runtime. **Equivalente Java:** una primera
etapa con Maven y una segunda con solo el JRE.

En este modelo el artifact que se promueve es la **imagen Docker**, no la
carpeta.

### Vías habituales de despliegue

| Nube | Opciones típicas para una API .NET |
|---|---|
| Azure | App Service (despliegue directo de la carpeta publicada o de una imagen), Azure Container Apps |
| AWS | Elastic Beanstalk, ECS/Fargate (contenedores) |

Ambas admiten autenticación desde GitHub Actions sin guardar contraseñas de
larga duración (OIDC); es la opción recomendada frente a guardar claves
como secretos.

---

## 9. Glosario rápido

| Término | Significado |
|---|---|
| Pipeline | Secuencia automática de pasos desde el commit hasta el despliegue |
| Runner | Máquina que ejecuta un job |
| Workflow | Un pipeline definido en un YAML de GitHub Actions |
| Artifact | Paquete resultante del build, listo para desplegar |
| Staging | Entorno de pruebas casi idéntico a producción |
| Environment | Destino con reglas y secretos propios en GitHub |
| Smoke test | Prueba rápida de que lo esencial funciona tras desplegar |
| Rollback | Volver a la versión anterior desplegada |
