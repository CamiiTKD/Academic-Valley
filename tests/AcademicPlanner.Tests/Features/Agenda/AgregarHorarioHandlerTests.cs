using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.Commands.AgregarHorario;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class AgregarHorarioHandlerTests
{
    private readonly IMateriaRepository _materiaRepository = Substitute.For<IMateriaRepository>();
    private readonly IHorarioCursadaRepository _horarioRepository = Substitute.For<IHorarioCursadaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AgregarHorarioHandler _sut;

    public AgregarHorarioHandlerTests()
    {
        _sut = new AgregarHorarioHandler(_materiaRepository, _horarioRepository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _materiaRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaExiste_RetornaDtoConDatosCorrectos()
    {
        var materia = Materia.Crear("Álgebra Lineal", "ALG1", 1);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = ComandoValido(materia.Id);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Id.Should().NotBe(Guid.Empty);
        result.MateriaId.Should().Be(materia.Id);
        result.DiaSemana.Should().Be(command.DiaSemana);
        result.HoraInicio.Should().Be(command.HoraInicio);
        result.HoraFin.Should().Be(command.HoraFin);
        result.Aula.Should().Be(command.Aula);
        result.EsVirtual.Should().Be(command.EsVirtual);
    }

    [Fact]
    public async Task Handle_MateriaExiste_InvocaAddYSaveChanges()
    {
        var materia = Materia.Crear("Álgebra Lineal", "ALG1", 1);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        await _horarioRepository.Received(1).AddAsync(Arg.Any<HorarioCursada>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_NoInvocaAddNiSaveChanges()
    {
        _materiaRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        await _horarioRepository.DidNotReceive().AddAsync(Arg.Any<HorarioCursada>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static AgregarHorarioCommand ComandoValido(Guid materiaId) => new(
        materiaId, DayOfWeek.Monday, new TimeOnly(18, 30), new TimeOnly(21, 30), "Aula 5", false);
}
