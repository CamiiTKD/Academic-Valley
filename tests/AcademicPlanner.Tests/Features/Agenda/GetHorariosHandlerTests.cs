using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.Queries.GetHorarios;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class GetHorariosHandlerTests
{
    private readonly IMateriaRepository _materiaRepository = Substitute.For<IMateriaRepository>();
    private readonly IHorarioCursadaRepository _horarioRepository = Substitute.For<IHorarioCursadaRepository>();
    private readonly GetHorariosHandler _sut;

    public GetHorariosHandlerTests()
    {
        _sut = new GetHorariosHandler(_materiaRepository, _horarioRepository);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _materiaRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(new GetHorariosQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_NoConsultaRepositorioDeHorarios()
    {
        _materiaRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(new GetHorariosQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        await _horarioRepository.DidNotReceive()
            .GetByMateriaIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaExisteSinHorarios_RetornaListaVacia()
    {
        var materia = Materia.Crear("Compiladores", "COM1", 5);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);
        _horarioRepository.GetByMateriaIdAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(new List<HorarioCursada>().AsReadOnly() as IReadOnlyList<HorarioCursada>);

        var result = await _sut.Handle(new GetHorariosQuery(materia.Id), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MateriaConHorarios_RetornaDtosConDatosCorrectos()
    {
        var materia = Materia.Crear("Arquitectura", "ARQ1", 3);
        var h1 = HorarioCursada.Crear(materia.Id, DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(21, 0), "Aula 5", false);
        var h2 = HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(12, 0), null, true);
        IReadOnlyList<HorarioCursada> lista = [h1, h2];
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);
        _horarioRepository.GetByMateriaIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetHorariosQuery(materia.Id), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].MateriaId.Should().Be(materia.Id);
        result[0].DiaSemana.Should().Be(DayOfWeek.Monday);
        result[0].HoraInicio.Should().Be(new TimeOnly(18, 0));
        result[0].HoraFin.Should().Be(new TimeOnly(21, 0));
        result[0].Aula.Should().Be("Aula 5");
        result[0].EsVirtual.Should().BeFalse();
        result[1].DiaSemana.Should().Be(DayOfWeek.Wednesday);
        result[1].EsVirtual.Should().BeTrue();
    }
}
