using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class TripulanteRepositoryOrdenTests
{
    [Fact]
    public async Task FindAllWithBarcoAsync_ConFilasInsertadasEnDesorden_LasDevuelveOrdenadasPorId()
    {
        // Arrange: misma base InMemory en dos contextos (siembra y consulta)
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Barcos.Add(new Barco { Id = 1, Nombre = "La Bestia", Tipo = "Velero" });
            siembra.Tripulantes.AddRange(
                new Tripulante { Id = 3, Nombre = "Carla", Rol = "Patrón", BarcoId = 1 },
                new Tripulante { Id = 1, Nombre = "Ana", Rol = "Timonel", BarcoId = 1 },
                new Tripulante { Id = 2, Nombre = "Bruno", Rol = "Grumete", BarcoId = 1 });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new TripulanteRepository(context);

        // Act
        var tripulantes = await repository.FindAllWithBarcoAsync();

        // Assert: 1, 2, 3 aunque se insertaron como 3, 1, 2
        tripulantes.Select(t => t.Id).Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task FindByBarcoIdAsync_ConTripulantesInsertadosEnDesorden_LosDevuelveOrdenadosPorId()
    {
        // Arrange: tres tripulantes del mismo barco, insertados como 3, 1, 2
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Barcos.Add(new Barco { Id = 1, Nombre = "La Bestia", Tipo = "Velero" });
            siembra.Tripulantes.AddRange(
                new Tripulante { Id = 3, Nombre = "Carla", Rol = "Patrón", BarcoId = 1 },
                new Tripulante { Id = 1, Nombre = "Ana", Rol = "Timonel", BarcoId = 1 },
                new Tripulante { Id = 2, Nombre = "Bruno", Rol = "Grumete", BarcoId = 1 });
            await siembra.SaveChangesAsync();
        }

        using var context = new MarinaDbContext(options);
        var repository = new TripulanteRepository(context);

        // Act
        var tripulantes = await repository.FindByBarcoIdAsync(1);

        // Assert: 1, 2, 3 aunque se insertaron como 3, 1, 2
        tripulantes.Select(t => t.Id).Should().Equal(1, 2, 3);
    }
}