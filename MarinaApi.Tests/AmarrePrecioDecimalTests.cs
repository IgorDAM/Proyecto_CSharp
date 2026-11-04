using FluentAssertions;
using MarinaApi.Data;
using MarinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarinaApi.Tests;

public class AmarrePrecioDecimalTests
{
    [Fact]
    public async Task SumarPreciosDe0_1Y0_2_ConDecimal_DaExactamente0_3()
    {
        // Arrange: dos amarres con precios que en double no suman exacto
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var siembra = new MarinaDbContext(options))
        {
            siembra.Amarres.AddRange(
                new Amarre { Id = 1, Ubicacion = "A-1", Precio = 0.1m, Profundidad = 3, Longitud = 8, Electricidad = true },
                new Amarre { Id = 2, Ubicacion = "B-2", Precio = 0.2m, Profundidad = 4, Longitud = 10, Electricidad = true });
            await siembra.SaveChangesAsync();
        }

        // Act
        using var context = new MarinaDbContext(options);
        var total = (await context.Amarres.ToListAsync()).Sum(a => a.Precio);

        // Assert
        total.Should().Be(0.3m);
    }
}