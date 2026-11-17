using FluentAssertions;
using MarinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MySqlConnector;

namespace MarinaApi.Tests;

[Collection("MySql")]
public class MySqlIntegracionTests
{
    private readonly MySqlFixture _mysql;

    public MySqlIntegracionTests(MySqlFixture mysql)
    {
        _mysql = mysql;
    }

    [Fact]
    public async Task SumaDePrecios_ConDecimalEnMySql_DaExactamente0_3()
    {
        // Arrange: ubicaciones propias ("SUMA-") para no mezclarse con los
        // datos de otros tests, que comparten el mismo contenedor
        using (var siembra = _mysql.CrearContexto())
        {
            siembra.Amarres.AddRange(
                new Amarre { Ubicacion = "SUMA-1", Precio = 0.1m, Profundidad = 3, Longitud = 8, Electricidad = true },
                new Amarre { Ubicacion = "SUMA-2", Precio = 0.2m, Profundidad = 4, Longitud = 10, Electricidad = true });
            await siembra.SaveChangesAsync();
        }

        // Act: la suma la hace MySQL, no C#
        using var contexto = _mysql.CrearContexto();
        var total = await contexto.Amarres
            .Where(a => a.Ubicacion.StartsWith("SUMA-"))
            .SumAsync(a => a.Precio);

        // Assert
        total.Should().Be(0.3m);
    }

    [Fact]
    public async Task BorrarBarco_ConAmarreAsignado_DejaElAmarreLibreEnMySql()
    {
        // Arrange: un barco con su amarre, enlazados por la navegación Barco
        long barcoId;
        long amarreId;

        using (var siembra = _mysql.CrearContexto())
        {
            var barco = new Barco { Nombre = "La Gaviota", Tipo = "Velero", Eslora = 10, Manga = 3, Capacidad = 6 };
            var amarre = new Amarre { Ubicacion = "FK-1", Precio = 100m, Profundidad = 3, Longitud = 12, Electricidad = true, Barco = barco };

            siembra.Amarres.Add(amarre);
            await siembra.SaveChangesAsync();

            barcoId = barco.Id;
            amarreId = amarre.Id;
        }

        // Act: borrar el barco con un contexto nuevo.
        // Sin Include a propósito: si el Amarre estuviera cargado, EF Core pondría
        // BarcoId = null por su cuenta y el test no probaría el ON DELETE SET NULL
        // de la base de datos, que es lo que queremos comprobar.
        using (var contexto = _mysql.CrearContexto())
        {
            var barco = await contexto.Barcos
                .SingleAsync(b => b.Id == barcoId);

            contexto.Barcos.Remove(barco);
            await contexto.SaveChangesAsync();
        }

        // Assert: el amarre sigue existiendo y ya no tiene barco
        using var lectura = _mysql.CrearContexto();
        var amarreLibre = await lectura.Amarres.SingleAsync(a => a.Id == amarreId);

        amarreLibre.BarcoId.Should().BeNull();
    }

    [Fact]
    public async Task AsignarElMismoBarcoADosAmarres_ViolaElIndiceUnicoEnMySql()
    {
        // Arrange: un barco ya asignado al amarre UQ-1, y un segundo amarre
        // UQ-2 libre. Ubicaciones con prefijo propio ("UQ-") por el contenedor compartido.
        long amarreLibreId;
        long barcoId;

        using (var siembra = _mysql.CrearContexto())
        {
            var barco = new Barco { Nombre = "El Albatros", Tipo = "Velero", Eslora = 9, Manga = 3, Capacidad = 5 };
            var ocupado = new Amarre { Ubicacion = "UQ-1", Precio = 100m, Profundidad = 3, Longitud = 12, Electricidad = true, Barco = barco };
            var libre = new Amarre { Ubicacion = "UQ-2", Precio = 100m, Profundidad = 3, Longitud = 12, Electricidad = true };

            siembra.Amarres.AddRange(ocupado, libre);
            await siembra.SaveChangesAsync();

            barcoId = barco.Id;
            amarreLibreId = libre.Id;
        }
        // Act: con un contexto nuevo, asignar al amarre libre el barco que ya
        // tiene el UQ-1. Nada se ejecuta todavía: guardamos la acción en una variable.
        using var contexto = _mysql.CrearContexto();
        var amarre = await contexto.Amarres.SingleAsync(a => a.Id == amarreLibreId);
        amarre.BarcoId = barcoId;

        var guardar = () => contexto.SaveChangesAsync();

        // Assert: la excepción de EF envuelve la de MySQL, y el 1062 es "Duplicate entry"
        var excepcion = await guardar.Should().ThrowAsync<DbUpdateException>();
        excepcion.Which.InnerException.Should().BeOfType<MySqlException>()
            .Which.Number.Should().Be(1062);
    }
}