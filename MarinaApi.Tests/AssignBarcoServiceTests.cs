using FluentAssertions;
using Moq;
using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Models;
using MarinaApi.Repositories;
using MarinaApi.Services;
using Xunit;

namespace MarinaApi.Tests;

public class AssignBarcoServiceTests
{
    private readonly Mock<IAmarreRepository> _amarreRepositoryMock;
    private readonly Mock<IBarcoRepository> _barcoRepositoryMock;
    private readonly AmarreService _service;

    public AssignBarcoServiceTests()
    {
        _amarreRepositoryMock = new Mock<IAmarreRepository>();
        _barcoRepositoryMock = new Mock<IBarcoRepository>();
        _service = new AmarreService(_amarreRepositoryMock.Object, _barcoRepositoryMock.Object);
    }

    [Fact]
    public async Task AssignBarcoAsync_CuandoTodoEsValido_AsignaYDevuelveDto()
    {
        // Arrange
        var amarre = new Amarre { Id = 1, Ubicacion = "A-12", BarcoId = null };
        var dto = new AsignarBarcoDto(5);

        _amarreRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(amarre);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _amarreRepositoryMock.Setup(r => r.FindByBarcoIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Amarre?)null);

        // Act
        var resultado = await _service.AssignBarcoAsync(1, dto);

        // Assert
        resultado.BarcoId.Should().Be(5);
        _amarreRepositoryMock.Verify(r => r.UpdateAsync(amarre, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignBarcoAsync_CuandoAmarreNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var dto = new AsignarBarcoDto(5);
        _amarreRepositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Amarre?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.AssignBarcoAsync(999, dto))
            .Should().ThrowAsync<NotFoundException>();

        // El Barco nunca debería llegar a comprobarse: el método corta en la primera validación
        _barcoRepositoryMock.Verify(b => b.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

        [Fact]
    public async Task AssignBarcoAsync_CuandoBarcoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var amarre = new Amarre { Id = 1, Ubicacion = "A-12", BarcoId = null };
        var dto = new AsignarBarcoDto(999);

        _amarreRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(amarre);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.AssignBarcoAsync(1, dto))
            .Should().ThrowAsync<NotFoundException>();

        // No debería llegar a comprobar si el Barco ya tiene amarre, ni a actualizar nada
        _amarreRepositoryMock.Verify(r => r.FindByBarcoIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _amarreRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Amarre>(), It.IsAny<CancellationToken>()), Times.Never);
    }

        [Fact]
    public async Task AssignBarcoAsync_CuandoBarcoYaTieneAmarre_LanzaConflictException()
    {
        // Arrange
        var amarre = new Amarre { Id = 2, Ubicacion = "B-03", BarcoId = null };
        var amarreExistente = new Amarre { Id = 1, Ubicacion = "A-12", BarcoId = 5 };
        var dto = new AsignarBarcoDto(5);

        _amarreRepositoryMock.Setup(r => r.FindByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(amarre);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _amarreRepositoryMock.Setup(r => r.FindByBarcoIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(amarreExistente);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.AssignBarcoAsync(2, dto))
            .Should().ThrowAsync<ConflictException>();

        _amarreRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Amarre>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
