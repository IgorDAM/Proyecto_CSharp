using FluentAssertions;
using Moq;
using MarinaApi.Data;
using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Models;
using MarinaApi.Repositories;
using MarinaApi.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MarinaApi.Tests;

/// <summary>
/// Equivalente a BarcoDAOImplTest.java (Capítulo 9), pero testeando la capa de
/// Servicio en vez del DAO — porque en esta arquitectura EF Core ya resuelve
/// el acceso a datos, así que la lógica que de verdad merece test unitario
/// vive en el Service. Moq sustituye a Mockito: mismo concepto (@Mock →
/// new Mock&lt;T&gt;(), when().thenReturn() → .Setup().ReturnsAsync(),
/// verify() → .Verify()), sintaxis distinta.
/// </summary>
public class BarcoServiceTests
{
    private readonly Mock<IBarcoRepository> _repositoryMock;
    private readonly BarcoService _service;

    public BarcoServiceTests()
    {
        _repositoryMock = new Mock<IBarcoRepository>();
        var loggerMock = new Mock<ILogger<BarcoService>>();
        // BarcoService ahora tiene un constructor sin context, perfecto para tests
        _service = new BarcoService(_repositoryMock.Object, loggerMock.Object);
    }

    // ── Patrón AAA (Arrange-Act-Assert), igual que en el Capítulo 9 de Java ──

    [Fact]
    public async Task FindByIdAsync_CuandoExiste_DevuelveDto()
    {
        // Arrange
        var barco = new Barco { Id = 1, Nombre = "Estrella del Mar", Tipo = "Velero", Eslora = 12 };
        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barco);

        // Act
        var resultado = await _service.FindByIdAsync(1);

        // Assert
        resultado.Nombre.Should().Be("Estrella del Mar");
        resultado.Id.Should().Be(1);
    }

    [Fact]
    public async Task FindByIdAsync_CuandoNoExiste_LanzaNotFoundException()
    {
        // Arrange: el mock devuelve null, como session.find() en Java cuando no hay resultado
        _repositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barco?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.FindByIdAsync(999))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_LlamaAlRepositorioConLaEntidadCorrecta()
    {
        // Arrange
        var dto = new BarcoRequestDto("Rayo Azul", "Motor", 15, 5, 10);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Barco>(), It.IsAny<CancellationToken>()))
            .Returns<Barco, CancellationToken>((b, _) =>
            {
                b.Id = 42;
                return Task.FromResult(b);
            });

        // Act
        var resultado = await _service.CreateAsync(dto);

        // Assert
        resultado.Id.Should().Be(42);
        resultado.Nombre.Should().Be("Rayo Azul");
        _repositoryMock.Verify(r => r.AddAsync(
            It.Is<Barco>(b => b.Nombre == "Rayo Azul" && b.Tipo == "Motor"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CuandoExiste_LlamaADeleteUnaVez()
    {
        // Arrange
        var barco = new Barco { Id = 5, Nombre = "Corsario Negro" };
        _repositoryMock.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barco);

        // Act
        await _service.DeleteAsync(5);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(barco, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FindAllAsync_MapeaTodasLasEntidadesADto()
    {
        // Arrange
        var barcos = new List<Barco>
        {
            new() { Id = 1, Nombre = "Barco A", Tipo = "Velero" },
            new() { Id = 2, Nombre = "Barco B", Tipo = "Motor" }
        };
        _repositoryMock.Setup(r => r.FindAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(barcos);

        // Act
        var resultado = await _service.FindAllAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Select(b => b.Nombre).Should().Contain(new[] { "Barco A", "Barco B" });
    }

    [Fact]
    public async Task UpdateAsync_CuandoNoExiste_LanzaNotFoundException()
    {
        // Arrange: el mock devuelve null cuando intenta encontrar un barco inexistente
        var dtoEntrante = new BarcoRequestDto("Nuevo Nombre", "Velero", 10, 5, 20);
        _repositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barco?)null);

        // Act + Assert: intenta actualizar y espera excepción
        await FluentActions.Awaiting(() => _service.UpdateAsync(999, dtoEntrante))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_CuandoExiste_ActualizaYDevuelveDto()
    {
        // Arrange
        var barcoExistente = new Barco
        {
            Id = 1,
            Nombre = "Nombre Original",
            Tipo = "Velero",
            Eslora = 10,
            Manga = 5,
            Capacidad = 15
        };
        var dtoEntrante = new BarcoRequestDto("Nombre Actualizado", "Motor", 12, 6, 20);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barcoExistente);

        // Act
        var resultado = await _service.UpdateAsync(1, dtoEntrante);

        // Assert
        resultado.Nombre.Should().Be("Nombre Actualizado");
        resultado.Tipo.Should().Be("Motor");
        resultado.Eslora.Should().Be(12);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Barco>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
