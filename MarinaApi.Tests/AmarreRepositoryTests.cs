using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using FluentAssertions;
using MySqlConnector;
using MarinaApi.Exceptions;
using MarinaApi.Models;
using MarinaApi.Repositories;
using Xunit;
using System.Reflection;

namespace MarinaApi.Tests;

/// <summary>
/// DbContext de pruebas: se comporta como el real, pero SaveChangesAsync lanza la
/// excepción que le pasemos. Sirve para simular fallos de la base de datos (como
/// una violación de índice único) que el proveedor InMemory no puede producir.
/// </summary>
internal class DbContextQueFallaAlGuardar : MarinaDbContext
{
    private readonly Exception _excepcion;

    public DbContextQueFallaAlGuardar(DbContextOptions<MarinaDbContext> options, Exception excepcion)
        : base(options)
    {
        _excepcion = excepcion;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw _excepcion;
}

public class AmarreRepositoryTests
{
    // MySqlException no tiene constructores públicos (son internal), así que
    // llamamos al interno (MySqlErrorCode, string) por reflexión.
    private static MySqlException CrearMySqlException(MySqlErrorCode codigo, string mensaje) =>
        (MySqlException)Activator.CreateInstance(
            typeof(MySqlException),
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            args: new object[] { codigo, mensaje },
            culture: null)!;

    [Fact]
    public async Task UpdateAsync_CuandoSeViolaElIndiceUnicoDeBarcoId_LanzaConflictException()
    {
        // Arrange: la misma cadena de excepciones que produce Pomelo con MySQL
        // DbUpdateException -> (InnerException) MySqlException con Number 1062
        var errorMySql = CrearMySqlException(
           MySqlErrorCode.DuplicateKeyEntry,
           "Duplicate entry '5' for key 'IX_Amarres_BarcoId'");
        var errorEf = new DbUpdateException("Error al guardar los cambios.", errorMySql);

        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DbContextQueFallaAlGuardar(options, errorEf);
        var repository = new AmarreRepository(context);
        var amarre = new Amarre { Id = 2, Ubicacion = "B-3", BarcoId = 5 };

        // Act + Assert
        await FluentActions.Awaiting(() => repository.UpdateAsync(amarre))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*Barco*5*");
    }

    [Fact]
    public async Task UpdateAsync_CuandoElErrorNoEsUnDuplicado_NoLoTraduceYLoDejaPasar()
    {
        // Arrange: un DbUpdateException sin MySqlException dentro (p. ej. un fallo genérico)
        var errorEf = new DbUpdateException("Error al guardar los cambios.");

        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DbContextQueFallaAlGuardar(options, errorEf);
        var repository = new AmarreRepository(context);
        var amarre = new Amarre { Id = 2, Ubicacion = "B-3", BarcoId = 5 };

        // Act + Assert: sale tal cual, no como 409
        await FluentActions.Awaiting(() => repository.UpdateAsync(amarre))
            .Should().ThrowAsync<DbUpdateException>();
    }
}