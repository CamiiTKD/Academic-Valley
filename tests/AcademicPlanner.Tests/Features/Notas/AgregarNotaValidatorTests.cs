using AcademicPlanner.Application.Features.Notas.Commands.AgregarNota;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AcademicPlanner.Tests.Features.Notas;

public sealed class AgregarNotaValidatorTests
{
    private readonly AgregarNotaValidator _sut = new();

    [Fact]
    public void Validar_MateriaIdVacio_RetornaError()
    {
        var command = Valido() with { MateriaId = Guid.Empty };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MateriaId));
    }

    [Fact]
    public void Validar_ValorNotaCero_RetornaError()
    {
        var command = Valido() with { ValorNota = 0 };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ValorNota));
    }

    [Fact]
    public void Validar_ValorNotaOnce_RetornaError()
    {
        var command = Valido() with { ValorNota = 11 };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ValorNota));
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
    public void Validar_FechaFutura_RetornaError()
    {
        var command = Valido() with { Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Fecha));
    }

    [Fact]
    public void Validar_FechaHoy_SinErrores()
    {
        var command = Valido() with { Fecha = DateOnly.FromDateTime(DateTime.Today) };
        _sut.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_NotaMinima_SinErrores()
    {
        var command = Valido() with { ValorNota = 1 };
        _sut.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_NotaMaxima_SinErrores()
    {
        var command = Valido() with { ValorNota = 10 };
        _sut.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_DatosValidos_SinErrores()
    {
        _sut.Validate(Valido()).IsValid.Should().BeTrue();
    }

    // ─── Reglas de dominio de RegistroNota ──────────────────────────────────

    [Fact]
    public void Crear_ValorNotaCero_LanzaArgumentOutOfRangeException()
    {
        var act = () => RegistroNota.Crear(Guid.NewGuid(), 0, new DateOnly(2026, 6, 15), TipoNota.ExamenFinal);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*valorNota*");
    }

    [Fact]
    public void Crear_ValorNotaOnce_LanzaArgumentOutOfRangeException()
    {
        var act = () => RegistroNota.Crear(Guid.NewGuid(), 11, new DateOnly(2026, 6, 15), TipoNota.ExamenFinal);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*valorNota*");
    }

    [Fact]
    public void Crear_MateriaIdVacio_LanzaArgumentException()
    {
        var act = () => RegistroNota.Crear(Guid.Empty, 7, new DateOnly(2026, 6, 15), TipoNota.ExamenFinal);
        act.Should().Throw<ArgumentException>().WithMessage("*materiaId*");
    }

    private static AgregarNotaCommand Valido() =>
        new(Guid.NewGuid(), 7, DateOnly.FromDateTime(DateTime.Today), TipoNota.ExamenFinal);
}
