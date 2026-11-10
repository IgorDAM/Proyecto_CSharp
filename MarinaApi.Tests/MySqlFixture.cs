using Testcontainers.MySql;
using Xunit;
using MarinaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MarinaApi.Tests;

public class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _contenedor = new MySqlBuilder("mysql:8.0")
        .Build();

    public string CadenaConexion => _contenedor.GetConnectionString();

    public MarinaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<MarinaDbContext>()
            .UseMySql(CadenaConexion, ServerVersion.AutoDetect(CadenaConexion))
            .Options;

        return new MarinaDbContext(options);
    }
    /// <summary>
    /// Arranca el contenedor MySQL y le aplica todas las migraciones del proyecto,
    /// de modo que cada test encuentra el esquema real (no uno deducido del modelo).
    /// xUnit espera a que termine antes de ejecutar ningún test de la clase.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _contenedor.StartAsync();

        // Contexto de usar y tirar: solo sirve para a
        // plicar las migraciones.
        using var contexto = CrearContexto();
        await contexto.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _contenedor.DisposeAsync().AsTask();
}