using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class BarcoRepositoryOrdenTests
{
    [Fact]
    public async Task LosFindersQueDevuelvenListas_ConBarcosInsertadosEnDesorden_LosDevuelvenOrdenadosPorId()
    {
        // Arrange: tres barcos que cumplen TODOS los criterios de los finders
        // (mismo tipo, eslora y capacidad altas, sin amarre), insertados como 3, 1, 2
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Barcos.AddRange(
                new Barco { Id = 3, Nombre = "La Gaviota", Tipo = "Velero", Eslora = 12, Capacidad = 8 },
                new Barco { Id = 1, Nombre = "La Bestia", Tipo = "Velero", Eslora = 10, Capacidad = 6 },
                new Barco { Id = 2, Nombre = "El Cangrejo", Tipo = "Velero", Eslora = 11, Capacidad = 7 });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new BarcoRepository(context);

        // Act + Assert: 1, 2, 3 en cada finder
        (await repository.FindByTipoAsync("Velero"))
            .Select(b => b.Id).Should().Equal(1, 2, 3);
        (await repository.FindByEsloraGreaterThanAsync(5))
            .Select(b => b.Id).Should().Equal(1, 2, 3);
        (await repository.FindByCapacidadGreaterThanAsync(1))
            .Select(b => b.Id).Should().Equal(1, 2, 3);
        (await repository.FindSinAmarreAsync())
            .Select(b => b.Id).Should().Equal(1, 2, 3);
        (await repository.FindByCapacidadGreaterThanOrEqualAsync(1))
            .Select(b => b.Id).Should().Equal(1, 2, 3);
    }
}