# Guía de Git

Guía práctica de Git aplicada a MarinaApi. Git es independiente del lenguaje:
todo lo de aquí vale igual para Java y para C#. La única parte que cambia
según el stack es el `.gitignore` (sección 8).

## Índice

1. Modelo mental
2. Commits y mensajes
3. Ramas
4. Merge vs rebase
5. Flujo con Pull Request
6. Deshacer cosas
7. Conflictos
8. .gitignore para .NET

---

## 1. Modelo mental

Git trabaja con cuatro zonas:

| Zona | Qué es | Cómo llegas a ella |
|---|---|---|
| Working directory | Tus archivos tal cual los editas | Editas en el IDE |
| Staging (index) | Cambios que has marcado para el próximo commit | `git add` |
| Repositorio local | Historial de commits en tu máquina | `git commit` |
| Remoto (GitHub) | Copia compartida del repo | `git push` |

Idea clave: un commit no guarda "todo lo que has cambiado", guarda solo lo
que pusiste en staging. Por eso puedes hacer commits pequeños y separados
aunque hayas tocado muchos archivos.

Casi todos los comandos de Git mueven cambios de una zona a otra. Cuando algo
sale raro, la pregunta es: ¿en qué zona está este cambio ahora mismo?
`git status` te lo dice.

---

## 2. Commits y mensajes

### Comandos del día a día

```bash
git status                 # qué hay modificado y en qué zona
git add Services/AmarreService.cs          # a staging un archivo concreto
git add -p                 # a staging por trozos (te pregunta hunk a hunk)
git commit -m "feat(#151): asignar amarre a barco existente"
git log --oneline          # historial resumido
git diff                   # working directory vs staging
git diff --staged          # staging vs último commit
```

`git add -p` es la herramienta para separar en dos commits cambios que
tocaron el mismo archivo.

### Mensajes: Conventional Commits

Formato: `tipo(alcance): descripción en imperativo`.

| Tipo | Cuándo |
|---|---|
| `feat` | Funcionalidad nueva |
| `fix` | Corrección de un bug |
| `test` | Añadir o cambiar tests |
| `refactor` | Cambiar estructura sin cambiar comportamiento |
| `docs` | Solo documentación |
| `chore` | Mantenimiento (dependencias, configuración) |

Ejemplos con los tickets del sprint:

```text
feat(#150): CRUD de Tripulante
test(#151): tests de AssignBarcoService
fix(#153): corregir consulta de amarres libres
```

### Commits atómicos

Un commit = un cambio con sentido propio, que compila y pasa los tests.
Razones: el historial se lee como una historia, `git revert` deshace solo
una cosa, y la revisión del PR es más fácil.

---

## 3. Ramas

Una rama es solo un puntero móvil a un commit. Crearla es casi gratis, por
eso se crea una por ticket.

```bash
git switch master                                    # cambiar de rama
git switch -c feature/154-asignar-amarre              # crear y cambiar
git branch                                            # listar locales
git branch -d feature/154-asignar-amarre              # borrar (solo si ya está mergeada)
```

`HEAD` es el puntero a "dónde estás ahora" (la rama actual).

Convención de nombres: `feature/<ticket>-descripción` para funcionalidad,
`fix/<ticket>-descripción` para bugs, `chore/...` para mantenimiento, con el
número de ticket del backlog delante. Ejemplo real de este proyecto:
`feature/152-total-tripulantes-regata`.

> `git switch` es la forma moderna. Verás mucho `git checkout` en tutoriales:
> hace lo mismo y más cosas (por eso se separó en `switch` y `restore`).

**Equivalente Java:** ninguna diferencia. Git no sabe si el repo es Maven o
.NET.

---

## 4. Merge vs rebase

Ambos integran cambios de una rama en otra. Cambia cómo queda el historial.

### Merge

Crea (o no) un commit de unión y conserva el historial tal como ocurrió.

```bash
git switch master
git merge feature/154-asignar-amarre
```

- **Fast-forward:** si `master` no avanzó desde que creaste la rama, Git solo
  mueve el puntero. No hay commit de unión.
- **Merge commit:** si `master` sí avanzó, Git crea un commit con dos padres.

### Rebase

Reescribe tus commits para que cuelguen del último commit de la rama destino.

```bash
git switch feature/154-asignar-amarre
git rebase master
```

Resultado: historial lineal, pero **los commits cambian de hash** (son
commits nuevos).

### Regla de oro

**No hagas rebase de ramas que otros ya tienen descargadas.** Reescribir
historial compartido rompe el repo de los demás. Rebase sobre tu propia rama
de trabajo aún no compartida está bien.

Si haces rebase de una rama ya subida y necesitas forzar el push, usa:

```bash
git push --force-with-lease
```

Es más seguro que `--force`: se niega si alguien subió algo que tú no has
visto.

### Squash merge (GitHub)

Al hacer merge de un PR puedes elegir "Squash and merge": todos los commits
de la rama se aplastan en uno solo en `master`. Deja un `master` limpio, un
commit por ticket. Es habitual en equipos con muchas ramas pequeñas.

---

## 5. Flujo con Pull Request

El ciclo completo de un ticket:

```bash
git switch master
git pull                                        # 1. master actualizado
git switch -c feature/152-total-tripulantes-regata    # 2. rama por ticket
# ...trabajas, haces commits atómicos...
git push -u origin feature/152-total-tripulantes-regata   # 3. subes (-u la primera vez)
```

4. En GitHub abres el **Pull Request** hacia `master`.
5. El **CI** compila y ejecuta los tests automáticamente (ver `GUIA_CICD.md`).
6. Un compañero revisa (code review) y comentas/corriges.
7. Merge del PR cuando el CI está en verde y hay aprobación.
8. Limpieza local:

```bash
git switch master
git pull
git branch -d feature/152-total-tripulantes-regata
```

`-u` (upstream) enlaza tu rama local con la remota, así luego basta con
`git push` y `git pull` sin argumentos.

**Equivalente Java:** idéntico. Lo que cambia es el CI (`mvn test` frente a
`dotnet test`).

---

## 6. Deshacer cosas

Elegir la herramienta según **dónde está** el cambio que quieres deshacer:

| Situación | Comando |
|---|---|
| Descartar cambios sin commitear de un archivo | `git restore archivo` |
| Sacar un archivo de staging (sin perder cambios) | `git restore --staged archivo` |
| Corregir el último commit (mensaje o añadir algo) | `git commit --amend` |
| Deshacer un commit ya compartido | `git revert <hash>` |
| Mover la rama atrás (solo local) | `git reset` |
| Guardar trabajo a medias para cambiar de rama | `git stash` |
| Recuperar algo "perdido" | `git reflog` |

### reset: tres modos

```bash
git reset --soft HEAD~1   # deshace el commit, deja los cambios en staging
git reset --mixed HEAD~1  # (por defecto) deshace el commit, cambios sin staging
git reset --hard HEAD~1   # deshace el commit Y borra los cambios. Peligroso.
```

`HEAD~1` significa "un commit antes del actual".

### revert vs reset

- `reset` reescribe historial: úsalo solo en commits **no subidos**.
- `revert` crea un commit nuevo que invierte a otro: es seguro en historial
  compartido.

### stash

```bash
git stash            # guarda cambios y deja el directorio limpio
git stash pop        # los recupera
git stash list
```

### reflog: la red de seguridad

`git reflog` lista dónde ha estado `HEAD`. Si hiciste un `reset --hard` y te
arrepientes, el commit "perdido" casi siempre sigue ahí: localizas su hash en
el reflog y haces `git reset --hard <hash>`.

---

## 7. Conflictos

Ocurren cuando dos ramas modifican las mismas líneas y Git no sabe cuál
gana. Git marca el archivo así:

```text
<<<<<<< HEAD
tu versión (la rama en la que estás)
=======
la versión que entra (la otra rama)
>>>>>>> feature/otra-rama
```

Pasos:

1. `git status` muestra los archivos "both modified".
2. Editas el archivo: dejas el resultado final correcto y **borras los
   marcadores** (`<<<<<<<`, `=======`, `>>>>>>>`).
3. `git add archivo` marca el conflicto como resuelto.
4. Terminas la operación:
   - en un merge: `git commit`
   - en un rebase: `git rebase --continue`
5. Compilas y pasas los tests antes de subir.

Para abortar y volver al estado anterior: `git merge --abort` o
`git rebase --abort`.

Consejos: ramas de vida corta y `git pull` a menudo reducen los conflictos.
Resolver bien no es elegir un lado a ciegas: a veces la solución correcta
mezcla los dos.

---

## 8. .gitignore para .NET

Lo que no debe entrar al repo: lo que se genera al compilar, los archivos
del IDE y los secretos.

### Generar uno estándar

```bash
dotnet new gitignore
```

Crea un `.gitignore` completo para .NET en la carpeta actual.

### Lo esencial

```gitignore
# Salida de compilación
bin/
obj/

# IDE
.vs/
.idea/
*.user

# Secretos y configuración local
appsettings.Local.json
*.env
```

### Comparativa con Java

| Java (Maven) | .NET | Qué es |
|---|---|---|
| `target/` | `bin/` y `obj/` | Compilados y artefactos intermedios |
| `.idea/` | `.vs/` (Visual Studio), `.idea/` (Rider) | Configuración del IDE |
| `*.iml` | `*.user` | Preferencias de usuario del IDE |

### Si ya subiste algo que no debías

`.gitignore` solo afecta a archivos **no rastreados**. Si `bin/` ya estaba en
el repo:

```bash
git rm -r --cached bin obj
git commit -m "chore: dejar de rastrear bin y obj"
```

`--cached` lo saca de Git pero no lo borra de tu disco.

### Secretos

Si una contraseña o cadena de conexión llegó a un commit, borrarla en un
commit posterior **no basta**: sigue en el historial. Hay que rotar esa
credencial (cambiarla). En desarrollo local, .NET tiene `dotnet user-secrets`
para no meter secretos en `appsettings.json`; en CI/producción se usan
variables de entorno (ver `GUIA_CICD.md`).
