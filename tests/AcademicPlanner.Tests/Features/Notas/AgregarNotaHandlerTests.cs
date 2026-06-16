using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Notas.Commands.AgregarNota;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Notas;

public sealed class AgregarNotaHandlerTests
{
    private readonly IMateriaRepository _materiaRepository = Substitute.For<IMateriaRepository>();
    private readonly IRegistroNotaRepository _notaRepository = Substitute.For<IRegistroNotaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AgregarNotaHandler _sut;

    public AgregarNotaHandlerTests()
    {
        _sut = new AgregarNotaHandler(_materiaRepository, _notaRepository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _materiaRepository.GetByIdWithNotasAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_NoInvocaAddNiSaveChanges()
    {
        _materiaRepository.GetByIdWithNotasAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        await _notaRepository.DidNotReceive().AddAsync(Arg.Any<RegistroNota>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaYaTieneExamenFinalAprobado_LanzaInvalidOperationException()
    {
        var materia = MateriaConExamenFinalAprobado();
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var act = async () => await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Examen Final aprobado*");
    }

    [Fact]
    public async Task Handle_MateriaYaTieneExamenFinalAprobado_NoInvocaAddNiSaveChanges()
    {
        var materia = MateriaConExamenFinalAprobado();
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var act = async () => await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        await _notaRepository.DidNotReceive().AddAsync(Arg.Any<RegistroNota>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExamenFinalDesaprobado_PermiteAgregarOtraNota()
    {
        var materia = Materia.Crear("Algoritmos", "ALG1", 1);
        materia.AgregarNota(3, new DateOnly(2026, 5, 10), TipoNota.ExamenFinal);
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var result = await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        result.Should().NotBeNull();
        await _notaRepository.Received(1).AddAsync(Arg.Any<RegistroNota>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotaAprobada_RetornaDtoConEsAplazoFalse()
    {
        var materia = Materia.Crear("Seguridad", "SEG1", 4);
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = new AgregarNotaCommand(materia.Id, 7, new DateOnly(2026, 6, 15), TipoNota.ExamenFinal);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.ValorNota.Should().Be(7);
        result.EsAplazo.Should().BeFalse();
        result.Tipo.Should().Be(TipoNota.ExamenFinal);
    }

    [Fact]
    public async Task Handle_NotaDesaprobada_RetornaDtoConEsAplazoTrue()
    {
        var materia = Materia.Crear("Seguridad", "SEG1", 4);
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = new AgregarNotaCommand(materia.Id, 3, new DateOnly(2026, 6, 15), TipoNota.ExamenFinal);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.ValorNota.Should().Be(3);
        result.EsAplazo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotaPromocion_PermiteAgregarAunqueTengaExamenFinalAprobado()
    {
        // Una Promocion no es ExamenFinal, así que la regla no debe bloquearla
        // (la regla solo verifica ExamenFinal >= 4)
        var materia = Materia.Crear("Lógica", "LOG1", 2);
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = new AgregarNotaCommand(materia.Id, 8, new DateOnly(2026, 6, 15), TipoNota.Promocion);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Tipo.Should().Be(TipoNota.Promocion);
        result.ValorNota.Should().Be(8);
    }

    [Fact]
    public async Task Handle_NotaValida_InvocaAddYSaveChanges()
    {
        var materia = Materia.Crear("Sistemas", "SIS1", 3);
        _materiaRepository.GetByIdWithNotasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        await _notaRepository.Received(1).AddAsync(Arg.Any<RegistroNota>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static AgregarNotaCommand ComandoValido(Guid materiaId) =>
        new(materiaId, 7, new DateOnly(2026, 6, 15), TipoNota.ExamenFinal);

    private static Materia MateriaConExamenFinalAprobado()
    {
        var materia = Materia.Crear("Programación", "PRG1", 1);
        materia.AgregarNota(6, new DateOnly(2026, 5, 10), TipoNota.ExamenFinal);
        return materia;
    }
}
