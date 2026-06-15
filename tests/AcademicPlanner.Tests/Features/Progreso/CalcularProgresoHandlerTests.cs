using Xunit;
using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Progreso.Queries.CalcularProgreso;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace AcademicPlanner.Tests.Features.Progreso;

public sealed class CalcularProgresoHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly CalcularProgresoHandler _sut;

    public CalcularProgresoHandlerTests()
    {
        _sut = new CalcularProgresoHandler(_repository);
    }

    [Fact]
    public async Task Handle_ConMateriasAprobadasYPendientes_CalculaProgresoCorrectamente()
    {
        // Arrange: 3 aprobadas (notas 4, 7, 8) + 1 pendiente + 1 cursando
        var materias = new List<Materia>
        {
            CrearMateriaAprobada("Matematicas I",  "MAT1", nota: 4m),
            CrearMateriaAprobada("Fisica I",       "FIS1", nota: 7m),
            CrearMateriaAprobada("Programacion I", "PRG1", nota: 8m),
            CrearMateria("Matematicas II", "MAT2", EstadoMateria.Pendiente),
            CrearMateria("Fisica II",      "FIS2", EstadoMateria.Cursando),
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
                   .Returns(materias.AsReadOnly());

        // Act
        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        // Assert — totales
        result.TotalMaterias.Should().Be(5);
        result.MateriasAprobadas.Should().Be(3);
        result.PorcentajeProgreso.Should().Be(60.00m);

        // Assert — promedios: (4+7+8)/3 = 6.3333 → round(2) = 6.33
        result.PromedioSinAplazos.Should().Be(6.33m);
        result.PromedioConAplazos.Should().Be(6.33m); // Sin notas registradas en no-aprobadas
    }

    [Fact]
    public async Task Handle_SinMaterias_RetornaTodoEnCero()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
                   .Returns(Array.Empty<Materia>().ToList().AsReadOnly());

        // Act
        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        // Assert
        result.TotalMaterias.Should().Be(0);
        result.MateriasAprobadas.Should().Be(0);
        result.PorcentajeProgreso.Should().Be(0m);
        result.PromedioConAplazos.Should().Be(0m);
        result.PromedioSinAplazos.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_ConAplazoRegistrado_PromedioConAplazosMenorQueSinAplazos()
    {
        // Arrange: nota 2 en una materia Regular (aplazo registrado) + nota 8 en una aprobada
        var materias = new List<Materia>
        {
            CrearMateriaConNota("Quimica I", "QUI1", EstadoMateria.Regular, nota: 2m),
            CrearMateriaAprobada("Programacion I", "PRG1", nota: 8m),
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
                   .Returns(materias.AsReadOnly());

        // Act
        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        // Assert
        result.MateriasAprobadas.Should().Be(1);
        result.PromedioSinAplazos.Should().Be(8.00m);  // Solo nota de aprobada: 8/1
        result.PromedioConAplazos.Should().Be(5.00m);  // Todas las notas: (2+8)/2

        result.PromedioConAplazos.Should().BeLessThan(result.PromedioSinAplazos);
    }

    [Theory]
    [InlineData(0, 10, 0.00)]
    [InlineData(5, 10, 50.00)]
    [InlineData(10, 10, 100.00)]
    [InlineData(1, 3, 33.33)]
    public async Task Handle_PorcentajeProgreso_CalculaCorrectamente(
        int cantAprobadas, int cantTotal, decimal porcentajeEsperado)
    {
        // Arrange
        var materias = Enumerable.Range(0, cantTotal)
            .Select((i) => i < cantAprobadas
                ? CrearMateriaAprobada($"Materia {i}", $"M{i:D3}", nota: 6m)
                : CrearMateria($"Materia {i}", $"M{i:D3}", EstadoMateria.Pendiente))
            .ToList();

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
                   .Returns(materias.AsReadOnly());

        // Act
        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        // Assert
        result.PorcentajeProgreso.Should().Be(porcentajeEsperado);
    }

    #region Helpers

    private static Materia CrearMateriaAprobada(string nombre, string codigo, decimal nota)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        m.ActualizarEstado(EstadoMateria.Aprobada, nota);
        return m;
    }

    private static Materia CrearMateria(string nombre, string codigo, EstadoMateria estado)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        if (estado != EstadoMateria.Pendiente)
            m.ActualizarEstado(estado);
        return m;
    }

    private static Materia CrearMateriaConNota(string nombre, string codigo, EstadoMateria estado, decimal nota)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        m.ActualizarEstado(estado, nota);
        return m;
    }

    #endregion
}
