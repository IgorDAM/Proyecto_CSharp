using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class RegataRepositoryOrdenTests
{
    [Fact]
    public async Task FindAllConContadorAsync_ConRegatasInsertadasEnDesorden_LasDevuelveOrdenadasPorId()
    {
        // Arrange: siembra con un contexto y consulta con otro
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Regatas.AddRange(
                new Regata { Id = 3, Nombre = "Copa Gijón", Lugar = "Gijón", Distancia = 20 },
                new Regata { Id = 1, Nombre = "Trofeo Avilés", Lugar = "Avilés", Distancia = 10 },
                new Regata { Id = 2, Nombre = "Regata Luanco", Lugar = "Luanco", Distancia = 15 });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new RegataRepository(context);

        // Act
        var regatas = await repository.FindAllConContadorAsync();

        // Assert: 1, 2, 3 aunque se insertaron como 3, 1, 2
        regatas.Select(r => r.Id).Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task LosFindersQueDevuelvenListas_ConRegatasInsertadasEnDesorden_LosDevuelvenOrdenadosPorId()
    {
        // Arrange: tres regatas que cumplen TODOS los criterios de los finders
        // (mismo lugar, distancia alta), insertadas como 3, 1, 2
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Regatas.AddRange(
                new Regata { Id = 3, Nombre = "Copa Gijón", Lugar = "Gijón", Distancia = 30 },
                new Regata { Id = 1, Nombre = "Trofeo Gijón", Lugar = "Gijón", Distancia = 10 },
                new Regata { Id = 2, Nombre = "Regata Gijón", Lugar = "Gijón", Distancia = 20 });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new RegataRepository(context);

        // Act + Assert: 1, 2, 3 en cada finder
        (await repository.FindByLugarAsync("Gijón"))
            .Select(r => r.Id).Should().Equal(1, 2, 3);
        (await repository.FindByDistanciaGreaterThanAsync(5))
            .Select(r => r.Id).Should().Equal(1, 2, 3);
        (await repository.FindByLugarConContadorAsync("Gijón"))
            .Select(r => r.Id).Should().Equal(1, 2, 3);
    }
}