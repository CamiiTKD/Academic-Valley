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
        _repository.GetCursandoWithHorariosAndEvaluacionesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia>().AsReadOnly() as IReadOnlyList<Materia>);
    }

    [Fact]
    public async Task Handle_Siempre_Retorna7DiasEnOrdenLunesDomingo()
    {
        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(7);
        result[0].DiaSemana.Should().Be(DayOfWeek.Monday);
        result[1].DiaSemana.Should().Be(DayOfWeek.Tuesday);
        result[2].DiaSemana.Should().Be(DayOfWeek.Wednesday);
        result[3].DiaSemana.Should().Be(DayOfWeek.Thursday);
        result[4].DiaSemana.Should().Be(DayOfWeek.Friday);
        result[5].DiaSemana.Should().Be(DayOfWeek.Saturday);
        result[6].DiaSemana.Should().Be(DayOfWeek.Sunday);
    }

    [Fact]
    public async Task Handle_SinMateriasEnCursado_Retorna7DiasConHorariosYAlertasVacios()
    {
        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().HaveCount(7);
        result.Should().AllSatisfy(d =>
        {
            d.Horarios.Should().BeEmpty();
            d.Alertas.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Handle_UnaMateriaConHorarioElLunes_ApareceEnDiaLunes()
    {
        var materia = CrearMateriaCursando("Álgebra Lineal", "ALG1");
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Monday, new TimeOnly(18, 30), new TimeOnly(21, 30), "Aula 5"));
        SetupMock([materia]);

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        var lunes = result.Single(d => d.DiaSemana == DayOfWeek.Monday);
        lunes.Horarios.Should().HaveCount(1);

        var dto = lunes.Horarios[0];
        dto.MateriaId.Should().Be(materia.Id);
        dto.MateriaNombre.Should().Be("Álgebra Lineal");
        dto.MateriaCodigo.Should().Be("ALG1");
        dto.HoraInicio.Should().Be(new TimeOnly(18, 30));
        dto.HoraFin.Should().Be(new TimeOnly(21, 30));
        dto.Aula.Should().Be("Aula 5");
        dto.EsVirtual.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_HorarioEnLunes_OtrosDiasQuedanVacios()
    {
        var materia = CrearMateriaCursando("Física", "FIS1");
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        SetupMock([materia]);

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Where(d => d.DiaSemana != DayOfWeek.Monday)
              .Should().AllSatisfy(d => d.Horarios.Should().BeEmpty());
    }

    [Fact]
    public async Task Handle_MultipleHorariosMismoDia_OrdenaPorHoraInicio()
    {
        var materia = CrearMateriaCursando("Física", "FIS1");
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(18, 0), new TimeOnly(20, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        materia.Horarios.Add(HorarioCursada.Crear(materia.Id, DayOfWeek.Wednesday, new TimeOnly(14, 0), new TimeOnly(16, 0)));
        SetupMock([materia]);

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        var miercoles = result.Single(d => d.DiaSemana == DayOfWeek.Wednesday);
        miercoles.Horarios.Should().HaveCount(3);
        miercoles.Horarios[0].HoraInicio.Should().Be(new TimeOnly(9, 0));
        miercoles.Horarios[1].HoraInicio.Should().Be(new TimeOnly(14, 0));
        miercoles.Horarios[2].HoraInicio.Should().Be(new TimeOnly(18, 0));
    }

    [Fact]
    public async Task Handle_MultiplesMaterias_AgrupaPorDiaYOrdenaPorHoraInicio()
    {
        var mat1 = CrearMateriaCursando("Álgebra", "ALG1");
        var mat2 = CrearMateriaCursando("Física", "FIS1");
        mat1.Horarios.Add(HorarioCursada.Crear(mat1.Id, DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(21, 0)));
        mat1.Horarios.Add(HorarioCursada.Crear(mat1.Id, DayOfWeek.Thursday, new TimeOnly(14, 0), new TimeOnly(16, 0)));
        mat2.Horarios.Add(HorarioCursada.Crear(mat2.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0)));
        SetupMock([mat1, mat2]);

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

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

    [Fact]
    public async Task Handle_EvaluacionDentroDeUnaSemanaDesdeHoy_ApareceComoAlerta()
    {
        var materia = CrearMateriaCursando("Química", "QUI1");
        var mañana = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        materia.Evaluaciones.Add(Evaluacion.Crear(materia.Id, TipoEvaluacion.Parcial, mañana, "Primer parcial"));
        SetupMock([materia]);

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        var diaConAlerta = result.Single(d => d.DiaSemana == mañana.DayOfWeek);
        diaConAlerta.Alertas.Should().HaveCount(1);
        diaConAlerta.Alertas[0].MateriaId.Should().Be(materia.Id);
        diaConAlerta.Alertas[0].Tipo.Should().Be(TipoEvaluacion.Parcial);
        diaConAlerta.Alertas[0].Descripcion.Should().Be("Primer parcial");
    }

    [Fact]
    public async Task Handle_EvaluacionMasDeUnaSemanaDesdeHoy_NoApareceComoAlerta()
    {
        var materia = CrearMateriaCursando("Historia", "HIS1");
        var lejano = DateOnly.FromDateTime(DateTime.Today.AddDays(8));
        materia.Evaluaciones.Add(Evaluacion.Crear(materia.Id, TipoEvaluacion.Final, lejano));
        SetupMock([materia]);

        var result = await _sut.Handle(new GetAgendaSemanalQuery(), CancellationToken.None);

        result.Should().AllSatisfy(d => d.Alertas.Should().BeEmpty());
    }

    private void SetupMock(IReadOnlyList<Materia> materias)
    {
        _repository.GetCursandoWithHorariosAndEvaluacionesAsync(Arg.Any<CancellationToken>())
            .Returns(materias);
    }

    private static Materia CrearMateriaCursando(string nombre, string codigo)
    {
        var m = Materia.Crear(nombre, codigo, cuatrimestre: 1);
        m.ActualizarEstado(EstadoMateria.Cursando);
        return m;
    }
}
