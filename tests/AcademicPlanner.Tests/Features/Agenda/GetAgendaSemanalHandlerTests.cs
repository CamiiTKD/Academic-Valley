using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.Queries.GetAgendaSemanal;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class GetAgendaSemanalHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly GetAgendaSemanalHandler _sut;

    public GetAgendaSemanalHandlerTests()
    {
        _sut = new GetAgendaSemanalHandler(_repository);
    }

    [Fact]
    public async Task Handle_SinMateriasEnCursado_RetornaListaVacia()
    {
        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Materia>().ToList().AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MateriaCursandoSinHorarios_RetornaListaVacia()
    {
        var materia = CrearMateriaCursando("Algebra", "ALG1");
        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia> { materia }.AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UnaMateriaConUnHorario_RetornaDtoConDatosCorrectos()
    {
        var materia = CrearMateriaCursando("Álgebra Lineal", "ALG1");
        var horario = HorarioCursada.Crear(materia.Id, DayOfWeek.Monday, new TimeOnly(18, 30), new TimeOnly(21, 30), "Aula 5");
        materia.Horarios.Add(horario);

        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia> { materia }.AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].DiaSemana.Should().Be(DayOfWeek.Monday);
        result[0].Horarios.Should().HaveCount(1);

        var dto = result[0].Horarios[0];
        dto.MateriaId.Should().Be(materia.Id);
        dto.MateriaNombre.Should().Be("Álgebra Lineal");
        dto.MateriaCodigo.Should().Be("ALG1");
        dto.HoraInicio.Should().Be(new TimeOnly(18, 30));
        dto.HoraFin.Should().Be(new TimeOnly(21, 30));
        dto.Aula.Should().Be("Aula 5");
        dto.EsVirtual.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MultipleHorariosMismoDia_OrdenaPorHoraInicio()
    {
        var materia = CrearMateriaCursando("Física", "FIS1");
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(18, 0), new TimeOnly(20, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(14, 0), new TimeOnly(16, 0)));

        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia> { materia }.AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        var horarios = result[0].Horarios;
        horarios[0].HoraInicio.Should().Be(new TimeOnly(9, 0));
        horarios[1].HoraInicio.Should().Be(new TimeOnly(14, 0));
        horarios[2].HoraInicio.Should().Be(new TimeOnly(18, 0));
    }

    [Fact]
    public async Task Handle_HorariosEnDiferentesDias_OrdenaDeLunesAViernes()
    {
        var materia = CrearMateriaCursando("Química", "QUI1");
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(11, 0)));

        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia> { materia }.AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result[0].DiaSemana.Should().Be(DayOfWeek.Monday);
        result[1].DiaSemana.Should().Be(DayOfWeek.Wednesday);
        result[2].DiaSemana.Should().Be(DayOfWeek.Friday);
    }

    [Fact]
    public async Task Handle_DomingoSeMuestraAlFinal()
    {
        var materia = CrearMateriaCursando("Historia", "HIS1");
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Sunday, new TimeOnly(10, 0), new TimeOnly(12, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Saturday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0)));

        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia> { materia }.AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result[0].DiaSemana.Should().Be(DayOfWeek.Monday);
        result[1].DiaSemana.Should().Be(DayOfWeek.Saturday);
        result[2].DiaSemana.Should().Be(DayOfWeek.Sunday);
    }

    [Fact]
    public async Task Handle_MultiplesMaterias_AgrupaPorDiaYOrdenaPorHoraInicio()
    {
        var mat1 = CrearMateriaCursando("Álgebra", "ALG1");
        var mat2 = CrearMateriaCursando("Física", "FIS1");

        mat1.Horarios.Add(HorarioCursada.Crear(mat1.Id, DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(21, 0)));
        mat1.Horarios.Add(HorarioCursada.Crear(mat1.Id, DayOfWeek.Thursday, new TimeOnly(14, 0), new TimeOnly(16, 0)));
        mat2.Horarios.Add(HorarioCursada.Crear(mat2.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0)));

        _repository.GetCursandoWithHorariosAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia> { mat1, mat2 }.AsReadOnly());

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(2);

        var lunes = result.Single(d => d.DiaSemana == DayOfWeek.Monday);
        lunes.Horarios.Should().HaveCount(2);
        lunes.Horarios[0].HoraInicio.Should().Be(new TimeOnly(9, 0));
        lunes.Horarios[0].MateriaCodigo.Should().Be("FIS1");
        lunes.Horarios[1].HoraInicio.Should().Be(new TimeOnly(18, 0));
        lunes.Horarios[1].MateriaCodigo.Should().Be("ALG1");

        var jueves = result.Single(d => d.DiaSemana == DayOfWeek.Thursday);
        jueves.Horarios.Should().HaveCount(1);
        jueves.Horarios[0].MateriaCodigo.Should().Be("ALG1");
    }

    private static Materia CrearMateriaCursando(string nombre, string codigo)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        m.ActualizarEstado(EstadoMateria.Cursando);
        return m;
    }
}
