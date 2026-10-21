using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class RegataRepositoryPaginacionTests
{
    [Fact]
    public async Task FindPageConContadorAsync_SegundaPagina_DevuelveLasRegatasQueQuedanConSuContador()
    {
        // Arrange: 3 regatas; la 2 tiene 2 barcos inscritos y las demás ninguno
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new MarinaDbContext(options);
        var barco1 = new Barco { Id = 1, Nombre = "Barco 1", Tipo = "Velero" };
        var barco2 = new Barco { Id = 2, Nombre = "Barco 2", Tipo = "Velero" };
        context.Regatas.AddRange(
            new Regata { Id = 1, Nombre = "Regata 1", Lugar = "Gijón", Distancia = 10 },
            new Regata { Id = 2, Nombre = "Regata 2", Lugar = "Gijón", Distancia = 20, Barcos = { barco1, barco2 } },
            new Regata { Id = 3, Nombre = "Regata 3", Lugar = "Avilés", Distancia = 30 });
        await context.SaveChangesAsync();
        var repository = new RegataRepository(context);

        // Act: página 2 de 2 en 2 -> salta 2, queda solo la regata 3
        var (elementos, total) = await repository.FindPageConContadorAsync(2, 2);

        // Assert
        total.Should().Be(3);
        elementos.Select(r => r.Id).Should().Equal(3);

        // Y la primera página trae la 1 y la 2, con el contador calculado
        var (primera, _) = await repository.FindPageConContadorAsync(1, 2);
        primera.Select(r => r.Id).Should().Equal(1, 2);
        primera.Single(r => r.Id == 2).TotalBarcosInscritos.Should().Be(2);
    }
}