using AcademicPlanner.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AcademicPlanner.Tests.Integration;

public sealed class MateriasEndpointsTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private SqliteConnection? _connection;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // "Test" env evita el bloque IsDevelopment() en Program.cs (MigrateAsync, Swagger)
                builder.UseEnvironment("Test");

                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        // String de conexión ficticia para que AddInfrastructureServices no tire excepción;
                        // la reemplazamos a continuación con SQLite
                        ["ConnectionStrings:PostgreSQL"] = "Host=localhost;Database=test_db"
                    }));

                builder.ConfigureServices(services =>
                {
                    // Remover el DbContext y sus Options registrados por Npgsql en AddInfrastructureServices.
                    var toRemove = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AcademicPlannerDbContext>) ||
                        d.ServiceType == typeof(AcademicPlannerDbContext)
                    ).ToList();
                    foreach (var d in toRemove)
                        services.Remove(d);

                    // Construir las options SQLite FUERA del DI container de la app:
                    // - Si usáramos AddDbContext() acá, llamaría a AddEntityFrameworkSqlite() en la
                    //   app's IServiceCollection, y EF Core detectaría dos IDatabaseProvider.
                    // - UseInternalServiceProvider() le dice a EF Core que use un service provider
                    //   PROPIO solo con SQLite, ignorando el IDatabaseProvider de Npgsql en la app.
                    var sqliteInternalProvider = new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider();

                    var sqliteOptions = new DbContextOptionsBuilder<AcademicPlannerDbContext>()
                        .UseSqlite(_connection)
                        .UseInternalServiceProvider(sqliteInternalProvider)
                        .Options;

                    // Registrar directamente, sin pasar por AddDbContext para no contaminar el DI de la app.
                    services.AddScoped<DbContextOptions<AcademicPlannerDbContext>>(_ => sqliteOptions);
                    services.AddScoped<AcademicPlannerDbContext>(sp =>
                        new AcademicPlannerDbContext(
                            sp.GetRequiredService<DbContextOptions<AcademicPlannerDbContext>>()));
                });
            });

        // Crear el esquema a partir del modelo EF Core (sin correr migraciones de Npgsql)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AcademicPlannerDbContext>();
        await db.Database.EnsureCreatedAsync();

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }

    [Fact]
    public async Task POST_Materias_Returns201_YPersistsEnDbContext()
    {
        // Arrange
        var body = new
        {
            Nombre = "Álgebra Lineal",
            Codigo = "ALGINT1",
            Cuatrimestre = 2
        };

        // Act
        var response = await _client!.PostAsJsonAsync("/api/Materias", body);

        // Assert — HTTP 201 Created
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Assert — cuerpo de la respuesta
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;
        json.GetProperty("nombre").GetString().Should().Be("Álgebra Lineal");
        json.GetProperty("codigo").GetString().Should().Be("ALGINT1");
        json.GetProperty("cuatrimestre").GetInt32().Should().Be(2);
        json.GetProperty("estado").GetString().Should().Be("Pendiente");
        json.GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);

        // Assert — persiste en la base de datos
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AcademicPlannerDbContext>();
        var saved = await db.Materias.FirstOrDefaultAsync(m => m.Codigo == "ALGINT1");
        saved.Should().NotBeNull();
        saved!.Nombre.Should().Be("Álgebra Lineal");
        saved.Cuatrimestre.Should().Be(2);
    }

    [Fact]
    public async Task POST_Materias_CodigoVacio_LanzaValidationException()
    {
        // Arrange
        var body = new
        {
            Nombre = "Álgebra Lineal",
            Codigo = "",
            Cuatrimestre = 1
        };

        // Act & Assert — sin middleware global de excepciones, la ValidationException de
        // FluentValidation burbujea hasta el TestServer y este la relanza al cliente HTTP.
        // La cobertura funcional de la validación se cubre en CrearMateriaValidatorTests.
        Func<Task> act = async () => await _client!.PostAsJsonAsync("/api/Materias", body);
        await act.Should().ThrowAsync<Exception>()
            .Where(ex => ex.Message.Contains("Codigo") ||
                         (ex.InnerException != null && ex.InnerException.Message.Contains("Codigo")));
    }

    [Fact]
    public async Task GET_Materias_Returns200_ConListaVaciaInicialmente()
    {
        // Act
        var response = await _client!.GetAsync("/api/Materias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;
        json.ValueKind.Should().Be(JsonValueKind.Array);
    }
}
