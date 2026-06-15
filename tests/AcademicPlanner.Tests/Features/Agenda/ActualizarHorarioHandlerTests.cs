using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.Commands.ActualizarHorario;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class ActualizarHorarioHandlerTests
{
    private readonly IHorarioCursadaRepository _horarioRepository = Substitute.For<IHorarioCursadaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ActualizarHorarioHandler _sut;

    public ActualizarHorarioHandlerTests()
    {
        _sut = new ActualizarHorarioHandler(_horarioRepository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_HorarioNoExiste_LanzaKeyNotFoundException()
    {
        var horarioId = Guid.NewGuid();
        _horarioRepository.GetByIdAsync(horarioId, Arg.Any<CancellationToken>()).Returns((HorarioCursada?)null);

        var act = async () => await _sut.Handle(ComandoValido(Guid.NewGuid(), horarioId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{horarioId}*");
    }

    [Fact]
    public async Task Handle_HorarioNoPertenecAMateria_LanzaKeyNotFoundException()
    {
        var otraMateriaId = Guid.NewGuid();
        var horario = HorarioCursada.Crear(otraMateriaId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        _horarioRepository.GetByIdAsync(horario.Id, Arg.Any<CancellationToken>()).Returns(horario);

        // Se pide la materia correcta pero el horario pertenece a otra
        var act = async () => await _sut.Handle(ComandoValido(Guid.NewGuid(), horario.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_DatosValidos_ActualizaLaEntidadYPersiste()
    {
        var materiaId = Guid.NewGuid();
        var horario = HorarioCursada.Crear(materiaId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        _horarioRepository.GetByIdAsync(horario.Id, Arg.Any<CancellationToken>()).Returns(horario);

        var command = ComandoValido(materiaId, horario.Id);
        await _sut.Handle(command, CancellationToken.None);

        horario.DiaSemana.Should().Be(command.DiaSemana);
        horario.HoraInicio.Should().Be(command.HoraInicio);
        horario.HoraFin.Should().Be(command.HoraFin);
        horario.Aula.Should().Be(command.Aula);
        horario.EsVirtual.Should().Be(command.EsVirtual);

        _horarioRepository.Received(1).Update(horario);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HorarioNoExiste_NoInvocaUpdateNiSaveChanges()
    {
        _horarioRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HorarioCursada?)null);

        var act = async () => await _sut.Handle(ComandoValido(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _horarioRepository.DidNotReceive().Update(Arg.Any<HorarioCursada>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ActualizarHorarioCommand ComandoValido(Guid materiaId, Guid horarioId) => new(
        materiaId, horarioId, DayOfWeek.Thursday, new TimeOnly(14, 0), new TimeOnly(17, 0), "Virtual", true);
}
