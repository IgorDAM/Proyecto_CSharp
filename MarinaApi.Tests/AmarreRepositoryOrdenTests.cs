using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class AmarreRepositoryOrdenTests
{
    [Fact]
    public async Task LosFindersQueDevuelvenListas_ConAmarresInsertadosEnDesorden_LosDevuelvenOrdenadosPorId()
    {
        // Arrange: tres amarres que cumplen TODOS los criterios de los finders
        // (precio bajo, con electricidad, sin barco), insertados como 3, 1, 2
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Amarres.AddRange(
                new Amarre { Id = 3, Ubicacion = "C-3", Precio = 30, Profundidad = 5, Longitud = 12, Electricidad = true },
                new Amarre { Id = 1, Ubicacion = "A-1", Precio = 10, Profundidad = 3, Longitud = 8, Electricidad = true },
                new Amarre { Id = 2, Ubicacion = "B-2", Precio = 20, Profundidad = 4, Longitud = 10, Electricidad = true });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new AmarreRepository(context);

        // Act + Assert: 1, 2, 3 en cada finder
        (await repository.FindByPrecioLessThanAsync(100))
            .Select(a => a.Id).Should().Equal(1, 2, 3);
        (await repository.FindByElectricidadAsync(true))
            .Select(a => a.Id).Should().Equal(1, 2, 3);
        (await repository.FindLibresAsync())
            .Select(a => a.Id).Should().Equal(1, 2, 3);
    }
}