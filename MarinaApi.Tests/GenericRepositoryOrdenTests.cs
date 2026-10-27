using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class GenericRepositoryOrdenTests
{
    [Fact]
    public async Task FindAllAsync_ConBarcosInsertadosEnDesorden_LosDevuelveOrdenadosPorId()
    {
        // Arrange: base InMemory con nombre único, sembrada con un contexto
        // y consultada con otro
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Barcos.AddRange(
                new Barco { Id = 3, Nombre = "La Gaviota", Tipo = "Velero" },
                new Barco { Id = 1, Nombre = "La Bestia", Tipo = "Velero" },
                new Barco { Id = 2, Nombre = "El Cangrejo", Tipo = "Yate" });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new GenericRepository<Barco>(context);

        // Act
        var barcos = await repository.FindAllAsync();

        // Assert
        // 1, 2, 3 aunque se insertaron como 3, 1, 2
        barcos.Select(b => b.Id).Should().Equal(1, 2, 3);
    }

        [Fact]
    public async Task FindAllAsync_ConAmarresInsertadosEnDesorden_LosDevuelveOrdenadosPorId()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Amarres.AddRange(
                new Amarre { Id = 3, Ubicacion = "C-3", Precio = 30, Profundidad = 5, Longitud = 12, Electricidad = true },
                new Amarre { Id = 1, Ubicacion = "A-1", Precio = 10, Profundidad = 3, Longitud = 8, Electricidad = false },
                new Amarre { Id = 2, Ubicacion = "B-2", Precio = 20, Profundidad = 4, Longitud = 10, Electricidad = true });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new GenericRepository<Amarre>(context);

        // Act
        var amarres = await repository.FindAllAsync();

        // Assert: 1, 2, 3 aunque se insertaron como 3, 1, 2
        amarres.Select(a => a.Id).Should().Equal(1, 2, 3);
    }
}