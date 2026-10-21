using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class TripulanteRepositoryPaginacionTests
{
    [Fact]
    public async Task FindPageWithBarcoAsync_PrimeraPagina_DevuelveLosPrimerosPorIdConElBarcoCargado()
    {
        // Arrange: la misma base InMemory se usa en dos contextos distintos
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Contexto 1: solo siembra los datos (los tripulantes, insertados en desorden)
        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Barcos.Add(new Barco { Id = 1, Nombre = "La Bestia", Tipo = "Velero" });
            siembra.Tripulantes.AddRange(
                new Tripulante { Id = 3, Nombre = "Carla", Rol = "Patrón", BarcoId = 1 },
                new Tripulante { Id = 1, Nombre = "Ana", Rol = "Timonel", BarcoId = 1 },
                new Tripulante { Id = 2, Nombre = "Bruno", Rol = "Grumete", BarcoId = 1 });
            await siembra.SaveChangesAsync();
        }

        // Contexto 2: nuevo y vacío, sin nada rastreado. Si Barco llega cargado,
        // es porque el repositorio hizo el Include.
        using var context = new MarinaDbContext(options);
        var repository = new TripulanteRepository(context);

        // Act: página 1, de 2 en 2
        var (elementos, total) = await repository.FindPageWithBarcoAsync(1, 2);

        // Assert
        total.Should().Be(3);
        elementos.Select(t => t.Id).Should().Equal(1, 2);
        elementos.Should().OnlyContain(t => t.Barco != null && t.Barco.Nombre == "La Bestia");
    }
}