using FluentAssertions;
using MarinaApi.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace MarinaApi.Tests;

[Collection("MySql")]
public class MySqlRestriccionesTests
{
    private readonly MySqlFixture _mysql;

    public MySqlRestriccionesTests(MySqlFixture mysql)
    {
        _mysql = mysql;
    }

    [Fact]
    public async Task GuardarBarcoConNombreDeMasDe100Caracteres_LoRechazaMySql()
    {
        // Arrange: un nombre de 101 caracteres, uno por encima del límite de Barco.Nombre
        using var contexto = _mysql.CrearContexto();
        contexto.Barcos.Add(new Barco
        {
            Nombre = new string('A', 101),
            Tipo = "Velero",
            Eslora = 10,
            Manga = 3,
            Capacidad = 6
        });

        // Act: no se envía nada a MySQL hasta que alguien ejecute esta lambda
        var guardar = () => contexto.SaveChangesAsync();

        // Assert: MySQL rechaza el valor y EF Core lo envuelve en una DbUpdateException
        await guardar.Should().ThrowAsync<DbUpdateException>();
    }
}
