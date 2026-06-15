using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.Commands.EliminarHorario;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class EliminarHorarioHandlerTests
{
    private readonly IHorarioCursadaRepository _horarioRepository = Substitute.For<IHorarioCursadaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly EliminarHorarioHandler _sut;

    public EliminarHorarioHandlerTests()
    {
        _sut = new EliminarHorarioHandler(_horarioRepository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_HorarioNoExiste_LanzaKeyNotFoundException()
    {
        var horarioId = Guid.NewGuid();
        _horarioRepository.GetByIdAsync(horarioId, Arg.Any<CancellationToken>()).Returns((HorarioCursada?)null);

        var act = async () => await _sut.Handle(new EliminarHorarioCommand(Guid.NewGuid(), horarioId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{horarioId}*");
    }

    [Fact]
    public async Task Handle_HorarioNoPertenecAMateria_LanzaKeyNotFoundException()
    {
        var otraMateriaId = Guid.NewGuid();
        var horario = HorarioCursada.Crear(otraMateriaId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        _horarioRepository.GetByIdAsync(horario.Id, Arg.Any<CancellationToken>()).Returns(horario);

        var act = async () => await _sut.Handle(new EliminarHorarioCommand(Guid.NewGuid(), horario.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_HorarioEliminable_InvocaDeleteYSaveChanges()
    {
        var materiaId = Guid.NewGuid();
        var horario = HorarioCursada.Crear(materiaId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        _horarioRepository.GetByIdAsync(horario.Id, Arg.Any<CancellationToken>()).Returns(horario);

        await _sut.Handle(new EliminarHorarioCommand(materiaId, horario.Id), CancellationToken.None);

        _horarioRepository.Received(1).Delete(horario);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HorarioNoExiste_NoInvocaDeleteNiSaveChanges()
    {
        _horarioRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HorarioCursada?)null);

        var act = async () => await _sut.Handle(new EliminarHorarioCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _horarioRepository.DidNotReceive().Delete(Arg.Any<HorarioCursada>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
