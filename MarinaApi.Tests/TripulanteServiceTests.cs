using FluentAssertions;
using Moq;
using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Models;
using MarinaApi.Repositories;
using MarinaApi.Services;
using Xunit;

namespace MarinaApi.Tests;

public class TripulanteServiceTests
{
    private readonly Mock<ITripulanteRepository> _tripulanteRepositoryMock;
    private readonly Mock<IBarcoRepository> _barcoRepositoryMock;
    private readonly TripulanteService _service;

    public TripulanteServiceTests()
    {
        _tripulanteRepositoryMock = new Mock<ITripulanteRepository>();
        _barcoRepositoryMock = new Mock<IBarcoRepository>();
        _service = new TripulanteService(_tripulanteRepositoryMock.Object, _barcoRepositoryMock.Object);
    }

    [Fact]
    public async Task FindByIdAsync_CuandoExiste_DevuelveDto()
    {
        // Arrange
        var tripulante = new Tripulante { Id = 1, Nombre = "Marcos", Rol = "Patrón", BarcoId = 1 };
        _tripulanteRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripulante);

        // Act
        var resultado = await _service.FindByIdAsync(1);

        // Assert
        resultado.Nombre.Should().Be("Marcos");
        resultado.BarcoId.Should().Be(1);
    }

    [Fact]
    public async Task FindByIdAsync_CuandoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _tripulanteRepositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tripulante?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.FindByIdAsync(999))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_CuandoBarcoExiste_LlamaAlRepositorioConLaEntidadCorrecta()
    {
        // Arrange
        var dto = new TripulanteRequestDto("Ana Ruiz", "Cocinera", 1);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _tripulanteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Tripulante>(), It.IsAny<CancellationToken>()))
            .Returns<Tripulante, CancellationToken>((t, _) =>
            {
                t.Id = 10;
                return Task.FromResult(t);
            });

        // Act
        var resultado = await _service.CreateAsync(dto);

        // Assert
        resultado.Id.Should().Be(10);
        resultado.Nombre.Should().Be("Ana Ruiz");
        _tripulanteRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Tripulante>(t => t.Nombre == "Ana Ruiz" && t.BarcoId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CuandoBarcoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var dto = new TripulanteRequestDto("Ana Ruiz", "Cocinera", 999);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>();

        // El repositorio de Tripulante NUNCA debería llegar a llamarse
        _tripulanteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Tripulante>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CuandoTripulanteNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var dto = new TripulanteRequestDto("Nuevo Nombre", "Rol", 1);
        _tripulanteRepositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tripulante?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.UpdateAsync(999, dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_CuandoBarcoNoExiste_LanzaNotFoundException()
    {
        // Arrange: el tripulante existe, pero el barco al que se le quiere mover no
        var tripulanteExistente = new Tripulante { Id = 1, Nombre = "Marcos", Rol = "Patrón", BarcoId = 1 };
        var dto = new TripulanteRequestDto("Marcos", "Patrón", 999);

        _tripulanteRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripulanteExistente);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.UpdateAsync(1, dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_CuandoTodoExiste_ActualizaYDevuelveDto()
    {
        // Arrange
        var tripulanteExistente = new Tripulante { Id = 1, Nombre = "Nombre Original", Rol = "Marinero", BarcoId = 1 };
        var dto = new TripulanteRequestDto("Nombre Actualizado", "Patrón", 1);

        _tripulanteRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripulanteExistente);
        _barcoRepositoryMock.Setup(b => b.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.UpdateAsync(1, dto);

        // Assert
        resultado.Nombre.Should().Be("Nombre Actualizado");
        resultado.Rol.Should().Be("Patrón");
        _tripulanteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tripulante>(), It.IsAny<CancellationToken>()), Times.Once);
    }

        [Fact]
    public async Task DeleteAsync_CuandoExiste_LlamaADeleteUnaVez()
    {
        // Arrange
        var tripulante = new Tripulante { Id = 1, Nombre = "Marcos", Rol = "Patrón", BarcoId = 1 };
        _tripulanteRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripulante);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _tripulanteRepositoryMock.Verify(r => r.DeleteAsync(tripulante, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FindAllAsync_MapeaTodasLasEntidadesADto()
    {
        // Arrange
        var tripulantes = new List<Tripulante>
        {
            new() { Id = 1, Nombre = "Marcos", Rol = "Patrón", BarcoId = 1 },
            new() { Id = 2, Nombre = "Laura", Rol = "Marinero", BarcoId = 1 }
        };
        _tripulanteRepositoryMock.Setup(r => r.FindAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripulantes);

        // Act
        var resultado = await _service.FindAllAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Select(t => t.Nombre).Should().Contain(new[] { "Marcos", "Laura" });
    }

    [Fact]
    public async Task FindByBarcoIdAsync_DevuelveTripulantesDelBarco()
    {
        // Arrange
        var tripulantes = new List<Tripulante>
        {
            new() { Id = 1, Nombre = "Marcos", Rol = "Patrón", BarcoId = 1 }
        };
        _tripulanteRepositoryMock.Setup(r => r.FindByBarcoIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tripulantes);

        // Act
        var resultado = await _service.FindByBarcoIdAsync(1);

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].BarcoId.Should().Be(1);
    }
}