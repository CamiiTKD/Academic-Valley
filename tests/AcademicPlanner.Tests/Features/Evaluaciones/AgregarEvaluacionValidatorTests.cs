using AcademicPlanner.Application.Features.Evaluaciones.Commands.AgregarEvaluacion;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AcademicPlanner.Tests.Features.Evaluaciones;

public sealed class AgregarEvaluacionValidatorTests
{
    private readonly AgregarEvaluacionValidator _sut = new();

    [Fact]
    public void Validar_MateriaIdVacio_RetornaError()
    {
        var command = Valido() with { MateriaId = Guid.Empty };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MateriaId));
    }

    [Fact]
    public void Validar_FechaVacia_RetornaError()
    {
        var command = Valido() with { Fecha = default };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Fecha));
    }

    [Fact]
    public void Validar_DescripcionExcede500Caracteres_RetornaError()
    {
        var command = Valido() with { Descripcion = new string('D', 501) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Descripcion));
    }

    [Fact]
    public void Validar_DescripcionNula_SinErrores()
    {
        var command = Valido() with { Descripcion = null };
        _sut.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_DatosValidos_SinErrores()
    {
        _sut.Validate(Valido()).IsValid.Should().BeTrue();
    }

    // ─── Reglas de dominio de Evaluacion ────────────────────────────────────

    [Fact]
    public void Crear_MateriaIdVacio_LanzaArgumentException()
    {
        var act = () => Evaluacion.Crear(Guid.Empty, TipoEvaluacion.Parcial, new DateOnly(2026, 7, 10));
        act.Should().Throw<ArgumentException>().WithMessage("*materiaId*");
    }

    private static AgregarEvaluacionCommand Valido() =>
        new(Guid.NewGuid(), TipoEvaluacion.Parcial, new DateOnly(2026, 7, 10), "Primer parcial");
}
