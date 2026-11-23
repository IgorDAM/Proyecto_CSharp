using FluentAssertions;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Xunit;

namespace MarinaApi.Tests;

[Collection("MySql")]
public class MySqlRegataPaginacionTests
{
    private readonly MySqlFixture _mysql;

    public MySqlRegataPaginacionTests(MySqlFixture mysql)
    {
        _mysql = mysql;
    }

    [Fact]
    public async Task FindPageConContador_ConTresRegatas_PaginaEnOrdenYCuentaLosBarcos()
    {
        // Arrange: tres regatas creadas en este orden, con 2, 0 y 1 barcos inscritos
        using (var siembra = _mysql.CrearContexto())
        {
            var barcoA = new Barco { Nombre = "PAG-A", Tipo = "Velero", Eslora = 10, Manga = 3, Capacidad = 6 };
            var barcoB = new Barco { Nombre = "PAG-B", Tipo = "Velero", Eslora = 11, Manga = 3, Capacidad = 6 };
            var barcoC = new Barco { Nombre = "PAG-C", Tipo = "Velero", Eslora = 12, Manga = 4, Capacidad = 8 };

            siembra.Regatas.AddRange(
                new Regata { Nombre = "PAG-1", Lugar = "PAG-LUGAR", Fecha = new DateOnly(2027, 5, 1), Distancia = 20, Barcos = { barcoA, barcoB } },
                new Regata { Nombre = "PAG-2", Lugar = "PAG-LUGAR", Fecha = new DateOnly(2027, 5, 2), Distancia = 30 },
                new Regata { Nombre = "PAG-3", Lugar = "PAG-LUGAR", Fecha = new DateOnly(2027, 5, 3), Distancia = 40, Barcos = { barcoC } });
            await siembra.SaveChangesAsync();
        }

        // Act: dos páginas de tamaño 2 con la consulta real contra MySQL
        using var contexto = _mysql.CrearContexto();
        var repositorio = new RegataRepository(contexto);

        var primera = await repositorio.FindPageConContadorAsync(1, 2);
        var segunda = await repositorio.FindPageConContadorAsync(2, 2);

        // Assert: el orden por Id, el contador de barcos y el total los resuelve MySQL
        primera.Total.Should().Be(3);
        primera.Elementos.Select(r => r.Nombre).Should().Equal("PAG-1", "PAG-2");
        primera.Elementos.Select(r => r.TotalBarcosInscritos).Should().Equal(2, 0);
        segunda.Elementos.Select(r => r.Nombre).Should().Equal("PAG-3");
        segunda.Elementos.Select(r => r.TotalBarcosInscritos).Should().Equal(1);
    }
}
