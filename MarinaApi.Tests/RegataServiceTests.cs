using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MarinaApi.Data;
using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Models;
using MarinaApi.Repositories;
using MarinaApi.Services;
using Xunit;

namespace MarinaApi.Tests;

public class RegataServiceTests
{
    private readonly Mock<IRegataRepository> _regataRepositoryMock;
    private readonly Mock<IBarcoRepository> _barcoRepositoryMock;
    private readonly Mock<ITripulanteRepository> _tripulanteRepositoryMock;
    private readonly MarinaDbContext _context;
    private readonly RegataService _service;

    public RegataServiceTests()
    {
        _regataRepositoryMock = new Mock<IRegataRepository>();
        _barcoRepositoryMock = new Mock<IBarcoRepository>();
        _tripulanteRepositoryMock = new Mock<ITripulanteRepository>();

        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MarinaDbContext(options);

        _service = new RegataService(
            _regataRepositoryMock.Object,
            _barcoRepositoryMock.Object,
            _tripulanteRepositoryMock.Object,
            _context
           );
    }


    [Fact]
    public async Task FindAllAsync_MapeaTodasLasEntidadesADto()
    {
        // Arrange
        var regatas = new List<Regata>
        {
            new() { Id = 1, Nombre = "Regata A", Lugar = "Gijón", Distancia = 20 },
            new() { Id = 2, Nombre = "Regata B", Lugar = "Avilés", Distancia = 30 }
        };
        _regataRepositoryMock.Setup(r => r.FindAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(regatas);

        // Act
        var resultado = await _service.FindAllAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Select(r => r.Nombre).Should().Contain(new[] { "Regata A", "Regata B" });
    }

    [Fact]
    public async Task FindByIdAsync_CuandoExiste_DevuelveDto()
    {
        // Arrange
        var regata = new Regata { Id = 1, Nombre = "Regata de Prueba", Lugar = "Gijón", Distancia = 25 };
        _regataRepositoryMock.Setup(r => r.FindByIdWithBarcosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regata);

        // Act
        var resultado = await _service.FindByIdAsync(1);

        // Assert
        resultado.Nombre.Should().Be("Regata de Prueba");
        resultado.TotalBarcosInscritos.Should().Be(0);
    }

    [Fact]
    public async Task FindByIdAsync_CuandoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _regataRepositoryMock.Setup(r => r.FindByIdWithBarcosAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Regata?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.FindByIdAsync(999))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_LlamaAlRepositorioConLaEntidadCorrecta()
    {
        // Arrange
        var dto = new RegataRequestDto("Regata Nueva", "Luarca", new DateOnly(2026, 10, 1), 15);
        _regataRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Regata>(), It.IsAny<CancellationToken>()))
            .Returns<Regata, CancellationToken>((r, _) =>
            {
                r.Id = 7;
                return Task.FromResult(r);
            });

        // Act
        var resultado = await _service.CreateAsync(dto);

        // Assert
        resultado.Id.Should().Be(7);
        resultado.Nombre.Should().Be("Regata Nueva");
        _regataRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Regata>(x => x.Nombre == "Regata Nueva" && x.Lugar == "Luarca"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FindByLugarAsync_DevuelveRegatasDelLugar()
    {
        // Arrange
        var regatas = new List<Regata>
        {
            new() { Id = 1, Nombre = "Regata A", Lugar = "Gijón", Distancia = 20 }
        };
        _regataRepositoryMock.Setup(r => r.FindByLugarAsync("Gijón", It.IsAny<CancellationToken>()))
            .ReturnsAsync(regatas);

        // Act
        var resultado = await _service.FindByLugarAsync("Gijón");

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].Lugar.Should().Be("Gijón");
    }

    [Fact]
    public async Task ContarTripulantesTotalesAsync_SumaLosTripulantesDeTodosLosBarcos()
    {
        // Arrange
        var barco1 = new Barco { Id = 1, Nombre = "Test" };
        var barco2 = new Barco { Id = 2, Nombre = "Estrella del Sur" };
        var regata = new Regata
        {
            Id = 1,
            Nombre = "Regata de Prueba",
            Lugar = "Gijón",
            Barcos = new List<Barco> { barco1, barco2 }
        };

        _regataRepositoryMock.Setup(r => r.FindByIdWithBarcosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regata);
        _tripulanteRepositoryMock.Setup(t => t.FindByBarcoIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tripulante> { new() { Id = 1, Nombre = "Marcos", Rol = "Segundo", BarcoId = 1 } });
        _tripulanteRepositoryMock.Setup(t => t.FindByBarcoIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tripulante> { new() { Id = 2, Nombre = "Laura", Rol = "Timonel", BarcoId = 2 } });

        // Act
        var total = await _service.ContarTripulantesTotalesAsync(1);

        // Assert
        total.Should().Be(2);
    }

    [Fact]
    public async Task ContarTripulantesTotalesAsync_CuandoRegataNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _regataRepositoryMock.Setup(r => r.FindByIdWithBarcosAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Regata?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.ContarTripulantesTotalesAsync(999))
            .Should().ThrowAsync<NotFoundException>();

        // No debería llegar a consultar tripulantes de ningún barco
        _tripulanteRepositoryMock.Verify(t => t.FindByBarcoIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InscribirBarcoAsync_CuandoNoEstaInscrito_AnadeLaRegataABarco()
    {
        // Arrange
        var regata = new Regata { Id = 1, Nombre = "Regata de Prueba", Lugar = "Gijón" };
        var barco = new Barco { Id = 1, Nombre = "Test", Regatas = new List<Regata>() };

        _regataRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regata);
        _barcoRepositoryMock.Setup(b => b.FindByIdWithRegatasAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barco);

        // Act
        await _service.InscribirBarcoAsync(1, 1);

        // Assert
        barco.Regatas.Should().ContainSingle(r => r.Id == 1);
    }

    [Fact]
    public async Task InscribirBarcoAsync_CuandoYaEstaInscrito_NoDuplica()
    {
        // Arrange
        var regata = new Regata { Id = 1, Nombre = "Regata de Prueba", Lugar = "Gijón" };
        var barco = new Barco { Id = 1, Nombre = "Test", Regatas = new List<Regata> { regata } };

        _regataRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regata);
        _barcoRepositoryMock.Setup(b => b.FindByIdWithRegatasAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barco);

        // Act
        await _service.InscribirBarcoAsync(1, 1);

        // Assert: sigue habiendo solo una, no se ha duplicado
        barco.Regatas.Should().ContainSingle(r => r.Id == 1);
    }

    [Fact]
    public async Task InscribirBarcoAsync_CuandoRegataNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _regataRepositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Regata?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.InscribirBarcoAsync(999, 1))
            .Should().ThrowAsync<NotFoundException>();

        _barcoRepositoryMock.Verify(b => b.FindByIdWithRegatasAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InscribirBarcoAsync_CuandoBarcoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var regata = new Regata { Id = 1, Nombre = "Regata de Prueba", Lugar = "Gijón" };
        _regataRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regata);
        _barcoRepositoryMock.Setup(b => b.FindByIdWithRegatasAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barco?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.InscribirBarcoAsync(1, 999))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DesinscribirBarcoAsync_EliminaLaRegataDeBarco()
    {
        // Arrange
        var regata = new Regata { Id = 1, Nombre = "Regata de Prueba", Lugar = "Gijón" };
        var barco = new Barco { Id = 1, Nombre = "Test", Regatas = new List<Regata> { regata } };

        _barcoRepositoryMock.Setup(b => b.FindByIdWithRegatasAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barco);

        // Act
        await _service.DesinscribirBarcoAsync(1, 1);

        // Assert
        barco.Regatas.Should().BeEmpty();
    }

    [Fact]
    public async Task DesinscribirBarcoAsync_CuandoBarcoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _barcoRepositoryMock.Setup(b => b.FindByIdWithRegatasAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barco?)null);

        // Act + Assert
        await FluentActions.Awaiting(() => _service.DesinscribirBarcoAsync(1, 999))
            .Should().ThrowAsync<NotFoundException>();
    }
}