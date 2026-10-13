using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using MarinaApi.Data;
using Microsoft.Extensions.DependencyInjection;

namespace MarinaApi.Tests;

/// <summary>
/// Comprueba que el contenedor de DI de la API se puede construir entero y que
/// no hay dependencias cautivas (un Singleton que recibe un Scoped).
/// </summary>
public class ContenedorDiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ContenedorDiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public void Contenedor_NoTieneDependenciasCautivas()
    {
        // Acceder a Services fuerza el arranque de la app y la construcción del
        // contenedor: si ValidateOnBuild está activo, lanza ante un Singleton → Scoped.
        var act = () => _factory.Services;

        act.Should().NotThrow();
    }

    [Fact]
    public void Contenedor_ConUnSingletonQueRecibeUnScoped_FallaAlConstruirse()
    {
        // Fábrica derivada: la API real + un Singleton cautivo registrado solo para este test.
        var fabricaConCautiva = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddSingleton<ICacheCautiva, CacheCautiva>()));

        var act = () => fabricaConCautiva.Services;

        act.Should().Throw<Exception>()
            .WithMessage("*Cannot consume scoped service*");
    }

    internal interface ICacheCautiva { }

    /// <summary>
    /// Servicio de pruebas que, registrado como Singleton, recibe un MarinaDbContext
    /// (Scoped): es una dependencia cautiva de manual.
    /// </summary>
    internal class CacheCautiva : ICacheCautiva
    {
        public CacheCautiva(MarinaDbContext db) { }
    }
}