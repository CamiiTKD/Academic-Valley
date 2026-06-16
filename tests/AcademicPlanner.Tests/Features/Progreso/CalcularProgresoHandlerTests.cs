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
        // 3 aprobadas (notas 4, 7, 8) + 1 pendiente + 1 cursando
        var materias = new List<Materia>
        {
            CrearMateriaAprobada("Matematicas I",  "MAT1", nota: 4),
            CrearMateriaAprobada("Fisica I",       "FIS1", nota: 7),
            CrearMateriaAprobada("Programacion I", "PRG1", nota: 8),
            CrearMateria("Matematicas II", "MAT2", EstadoMateria.Pendiente),
            CrearMateria("Fisica II",      "FIS2", EstadoMateria.Cursando),
        };

        _repository.GetAllWithRegistroNotasAsync(Arg.Any<CancellationToken>())
                   .Returns(materias.AsReadOnly());

        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        result.TotalMaterias.Should().Be(5);
        result.MateriasAprobadas.Should().Be(3);
        result.PorcentajeProgreso.Should().Be(60.00m);

        // (4+7+8)/3 = 6.3333 → round(2) = 6.33
        result.PromedioSinAplazos.Should().Be(6.33m);
        result.PromedioConAplazos.Should().Be(6.33m);
    }

    [Fact]
    public async Task Handle_SinMaterias_RetornaTodoEnCero()
    {
        _repository.GetAllWithRegistroNotasAsync(Arg.Any<CancellationToken>())
                   .Returns(Array.Empty<Materia>().ToList().AsReadOnly());

        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        result.TotalMaterias.Should().Be(0);
        result.MateriasAprobadas.Should().Be(0);
        result.PorcentajeProgreso.Should().Be(0m);
        result.PromedioConAplazos.Should().Be(0m);
        result.PromedioSinAplazos.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_ConAplazoRegistrado_PromedioConAplazosMenorQueSinAplazos()
    {
        // nota 2 en materia Regular (aplazo) + nota 8 en aprobada
        var materias = new List<Materia>
        {
            CrearMateriaConNota("Quimica I",      "QUI1", EstadoMateria.Regular,  nota: 2),
            CrearMateriaAprobada("Programacion I","PRG1", nota: 8),
        };

        _repository.GetAllWithRegistroNotasAsync(Arg.Any<CancellationToken>())
                   .Returns(materias.AsReadOnly());

        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        result.MateriasAprobadas.Should().Be(1);
        result.PromedioSinAplazos.Should().Be(8.00m);  // solo nota aprobada: 8/1
        result.PromedioConAplazos.Should().Be(5.00m);  // todas: (2+8)/2
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
        var materias = Enumerable.Range(0, cantTotal)
            .Select(i => i < cantAprobadas
                ? CrearMateriaAprobada($"Materia {i}", $"M{i:D3}", nota: 6)
                : CrearMateria($"Materia {i}", $"M{i:D3}", EstadoMateria.Pendiente))
            .ToList();

        _repository.GetAllWithRegistroNotasAsync(Arg.Any<CancellationToken>())
                   .Returns(materias.AsReadOnly());

        var result = await _sut.Handle(new CalcularProgresoQuery(), CancellationToken.None);

        result.PorcentajeProgreso.Should().Be(porcentajeEsperado);
    }

    #region Helpers

    private static Materia CrearMateriaAprobada(string nombre, string codigo, int nota)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        m.AgregarNota(nota, new DateOnly(2026, 1, 1), TipoNota.ExamenFinal);
        m.ActualizarEstado(EstadoMateria.Aprobada);
        return m;
    }

    private static Materia CrearMateria(string nombre, string codigo, EstadoMateria estado)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        if (estado != EstadoMateria.Pendiente)
            m.ActualizarEstado(estado);
        return m;
    }

    private static Materia CrearMateriaConNota(string nombre, string codigo, EstadoMateria estado, int nota)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        m.AgregarNota(nota, new DateOnly(2026, 1, 1), TipoNota.ExamenFinal);
        m.ActualizarEstado(estado);
        return m;
    }

    #endregion
}
