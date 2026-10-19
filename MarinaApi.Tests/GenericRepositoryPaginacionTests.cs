using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class GenericRepositoryPaginacionTests
{
    [Fact]
    public async Task FindPageAsync_PrimeraPagina_DevuelveLosPrimerosElementosOrdenadosYElTotal()
    {
        // Arrange: 5 barcos en una base InMemory propia de este test
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MarinaDbContext(options);
        for (var i = 1; i <= 5; i++)
            context.Barcos.Add(new Barco { Id = i, Nombre = $"Barco {i}", Tipo = "Velero" });
        await context.SaveChangesAsync();
        var repository = new GenericRepository<Barco>(context);

        // Act: página 1, de 2 en 2
        var (elementos, total) = await repository.FindPageAsync(b => b.Id, 1, 2);

        // Assert
        total.Should().Be(5);
        elementos.Select(b => b.Id).Should().Equal(1, 2);
    }

    [Fact]
    public async Task FindPageAsync_UltimaPaginaIncompleta_DevuelveSoloLosQueQuedan()
    {
        // Arrange: 5 barcos; con tamanio 2 hay 3 páginas y la última solo tiene 1
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MarinaDbContext(options);
        for (var i = 1; i <= 5; i++)
            context.Barcos.Add(new Barco { Id = i, Nombre = $"Barco {i}", Tipo = "Velero" });
        await context.SaveChangesAsync();
        var repository = new GenericRepository<Barco>(context);

        // Act: página 3, de 2 en 2 -> salta (3 - 1) * 2 = 4 filas
        var (elementos, total) = await repository.FindPageAsync(b => b.Id, 3, 2);

        // Assert: el total sigue siendo 5, pero solo queda el barco 5
        total.Should().Be(5);
        elementos.Select(b => b.Id).Should().Equal(5);
    }

    [Fact]
    public async Task FindPageAsync_PaginaFueraDeRango_DevuelveListaVaciaPeroConElTotal()
    {
        // Arrange: 5 barcos, y pediremos la página 10
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MarinaDbContext(options);
        for (var i = 1; i <= 5; i++)
            context.Barcos.Add(new Barco { Id = i, Nombre = $"Barco {i}", Tipo = "Velero" });
        await context.SaveChangesAsync();
        var repository = new GenericRepository<Barco>(context);

        // Act: salta (10 - 1) * 2 = 18 filas, muy por encima de las 5 que hay
        var (elementos, total) = await repository.FindPageAsync(b => b.Id, 10, 2);

        // Assert: no hay excepción, la lista está vacía y el total sigue siendo real
        elementos.Should().BeEmpty();
        total.Should().Be(5);
    }
}