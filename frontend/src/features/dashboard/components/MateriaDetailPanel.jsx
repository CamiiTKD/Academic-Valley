import { useEffect, useState } from 'react';
import { getEvaluaciones, getHorarios, agregarHorario, agregarEvaluacion } from '../../../services/api';
import './MateriaDetailPanel.css';

const DIAS_SEMANA = [
  { value: 'Monday',    label: 'Lunes',     order: 1 },
  { value: 'Tuesday',   label: 'Martes',    order: 2 },
  { value: 'Wednesday', label: 'Miércoles', order: 3 },
  { value: 'Thursday',  label: 'Jueves',    order: 4 },
  { value: 'Friday',    label: 'Viernes',   order: 5 },
  { value: 'Saturday',  label: 'Sábado',    order: 6 },
  { value: 'Sunday',    label: 'Domingo',   order: 7 },
];

const DIA_LABEL = Object.fromEntries(DIAS_SEMANA.map(d => [d.value, d.label]));
const DIA_ORDER = Object.fromEntries(DIAS_SEMANA.map(d => [d.value, d.order]));

const TIPOS_EVAL = ['Parcial', 'Teorico', 'TrabajoPractico', 'Coloquio', 'Final'];
const TIPOS_EVAL_LABEL = { Parcial: 'Parcial', Teorico: 'Teórico', TrabajoPractico: 'TP', Coloquio: 'Coloquio', Final: 'Final' };

const HORARIO_INIT = { diaSemana: 'Monday', horaInicio: '', horaFin: '', aula: '', esVirtual: false };
const EVAL_INIT    = { tipo: 'Parcial', fecha: '', descripcion: '' };

function formatTime(timeOnly) {
  return timeOnly?.slice(0, 5) ?? '';
}

function formatFecha(isoDate) {
  if (!isoDate) return '—';
  const [y, m, d] = String(isoDate).split('-');
  return `${d}/${m}/${y}`;
}

export default function MateriaDetailPanel({ materia, onClose }) {
  const [horarios, setHorarios]           = useState([]);
  const [evaluaciones, setEvaluaciones]   = useState([]);
  const [loading, setLoading]             = useState(true);

  // Horario inline form
  const [showHorarioForm, setShowHorarioForm] = useState(false);
  const [horarioFields, setHorarioFields]     = useState(HORARIO_INIT);
  const [savingHorario, setSavingHorario]     = useState(false);
  const [horarioError, setHorarioError]       = useState(null);

  // Evaluación inline form
  const [showEvalForm, setShowEvalForm] = useState(false);
  const [evalFields, setEvalFields]     = useState(EVAL_INIT);
  const [savingEval, setSavingEval]     = useState(false);
  const [evalError, setEvalError]       = useState(null);

  useEffect(() => {
    setLoading(true);
    Promise.all([
      getHorarios(materia.id).catch(() => []),
      getEvaluaciones(materia.id).catch(() => []),
    ]).then(([h, e]) => {
      setHorarios(h);
      setEvaluaciones(e);
      setLoading(false);
    });
  }, [materia.id]);

  const horariosOrdenados = [...horarios].sort((a, b) => {
    const oa = DIA_ORDER[a.diaSemana] ?? 99;
    const ob = DIA_ORDER[b.diaSemana] ?? 99;
    if (oa !== ob) return oa - ob;
    return a.horaInicio.localeCompare(b.horaInicio);
  });

  function handleOverlayClick(e) {
    if (e.target === e.currentTarget) onClose();
  }

  // ── Horario form ──
  function handleHorarioChange(e) {
    const { name, value, type, checked } = e.target;
    setHorarioFields(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    setHorarioError(null);
  }

  async function handleAddHorario(e) {
    e.preventDefault();
    setSavingHorario(true);
    setHorarioError(null);
    try {
      const nuevo = await agregarHorario(materia.id, {
        materiaId: materia.id,
        diaSemana: horarioFields.diaSemana,
        horaInicio: horarioFields.horaInicio + ':00',
        horaFin:    horarioFields.horaFin    + ':00',
        aula:       horarioFields.aula || null,
        esVirtual:  horarioFields.esVirtual,
      });
      setHorarios(prev => [...prev, nuevo]);
      setShowHorarioForm(false);
      setHorarioFields(HORARIO_INIT);
    } catch (err) {
      setHorarioError(err.message ?? 'Error al agregar el horario.');
    } finally {
      setSavingHorario(false);
    }
  }

  // ── Evaluación form ──
  function handleEvalChange(e) {
    const { name, value } = e.target;
    setEvalFields(prev => ({ ...prev, [name]: value }));
    setEvalError(null);
  }

  async function handleAddEval(e) {
    e.preventDefault();
    setSavingEval(true);
    setEvalError(null);
    try {
      const nueva = await agregarEvaluacion(materia.id, {
        materiaId:   materia.id,
        tipo:        evalFields.tipo,
        fecha:       evalFields.fecha,
        descripcion: evalFields.descripcion || null,
      });
      setEvaluaciones(prev => [...prev, nueva]);
      setShowEvalForm(false);
      setEvalFields(EVAL_INIT);
    } catch (err) {
      setEvalError(err.message ?? 'Error al agregar la evaluación.');
    } finally {
      setSavingEval(false);
    }
  }

  const canAddHorario = horarioFields.horaInicio && horarioFields.horaFin &&
    horarioFields.horaFin > horarioFields.horaInicio;

  const canAddEval = evalFields.fecha.length > 0;

  return (
    <div className="detail-overlay" onClick={handleOverlayClick}>
      <div className="detail-panel">
        {/* Top scroll cylinder */}
        <div className="scroll-end">
          <div className="scroll-knob" />
          <div className="scroll-knob" />
        </div>

        {/* Parchment body */}
        <div className="detail-scroll-body">
          <div className="detail-title-bar">
            <div>
              <span className="detail-title">{materia.nombre}</span>
              <span className="detail-code">[{materia.codigo}]</span>
            </div>
            <button className="detail-close-btn" onClick={onClose} title="Cerrar">✕</button>
          </div>

          {loading ? (
            <p className="loading-text">Cargando datos...</p>
          ) : (
            <>
              {/* ── Horarios ── */}
              <div className="detail-section">
                <div className="detail-section-header">
                  <p className="detail-section-title">📅 Horarios de cursada</p>
                  <button
                    className="section-add-btn"
                    onClick={() => { setShowHorarioForm(v => !v); setHorarioError(null); }}
                  >
                    {showHorarioForm ? '✕ Cancelar' : '＋ Agregar'}
                  </button>
                </div>

                {horariosOrdenados.length === 0 && !showHorarioForm && (
                  <p className="empty-state">Sin horarios registrados.</p>
                )}

                {horariosOrdenados.length > 0 && (
                  <div className="horarios-grid">
                    {horariosOrdenados.map((h) => (
                      <div key={h.id} className="horario-block">
                        <span className="horario-dia">{DIA_LABEL[h.diaSemana] ?? h.diaSemana}</span>
                        <span className="horario-horas">
                          {formatTime(h.horaInicio)} → {formatTime(h.horaFin)}
                        </span>
                        {h.esVirtual && <span className="horario-virtual-badge">Virtual</span>}
                        {h.aula && !h.esVirtual && (
                          <span className="horario-aula">🏫 {h.aula}</span>
                        )}
                      </div>
                    ))}
                  </div>
                )}

                {showHorarioForm && (
                  <form className="inline-form" onSubmit={handleAddHorario} noValidate>
                    <div className="inline-form-row">
                      <div className="inline-form-field">
                        <label className="inline-form-label">Día</label>
                        <select
                          className="inline-form-select"
                          name="diaSemana"
                          value={horarioFields.diaSemana}
                          onChange={handleHorarioChange}
                          disabled={savingHorario}
                        >
                          {DIAS_SEMANA.map(d => (
                            <option key={d.value} value={d.value}>{d.label}</option>
                          ))}
                        </select>
                      </div>
                      <div className="inline-form-field">
                        <label className="inline-form-label">Inicio</label>
                        <input
                          className="inline-form-input"
                          type="time"
                          name="horaInicio"
                          value={horarioFields.horaInicio}
                          onChange={handleHorarioChange}
                          disabled={savingHorario}
                        />
                      </div>
                      <div className="inline-form-field">
                        <label className="inline-form-label">Fin</label>
                        <input
                          className="inline-form-input"
                          type="time"
                          name="horaFin"
                          value={horarioFields.horaFin}
                          onChange={handleHorarioChange}
                          disabled={savingHorario}
                        />
                      </div>
                    </div>
                    <div className="inline-form-row">
                      <div className="inline-form-field inline-form-field--grow">
                        <label className="inline-form-label">Aula (opcional)</label>
                        <input
                          className="inline-form-input"
                          type="text"
                          name="aula"
                          placeholder="Ej: Aula 3B"
                          maxLength={100}
                          value={horarioFields.aula}
                          onChange={handleHorarioChange}
                          disabled={savingHorario}
                        />
                      </div>
                      <label className="inline-form-checkbox-label">
                        <input
                          type="checkbox"
                          name="esVirtual"
                          checked={horarioFields.esVirtual}
                          onChange={handleHorarioChange}
                          disabled={savingHorario}
                        />
                        Virtual
                      </label>
                    </div>
                    {horarioError && <p className="inline-form-error">⚠ {horarioError}</p>}
                    <div className="inline-form-actions">
                      <button
                        type="submit"
                        className="inline-form-submit-btn"
                        disabled={!canAddHorario || savingHorario}
                      >
                        {savingHorario ? 'Guardando...' : '✓ Guardar Horario'}
                      </button>
                    </div>
                  </form>
                )}
              </div>

              {/* ── Evaluaciones ── */}
              <div className="detail-section">
                <div className="detail-section-header">
                  <p className="detail-section-title">📝 Evaluaciones</p>
                  <button
                    className="section-add-btn"
                    onClick={() => { setShowEvalForm(v => !v); setEvalError(null); }}
                  >
                    {showEvalForm ? '✕ Cancelar' : '＋ Agregar'}
                  </button>
                </div>

                {evaluaciones.length === 0 && !showEvalForm && (
                  <p className="empty-state">Sin evaluaciones registradas.</p>
                )}

                {evaluaciones.length > 0 && (
                  <div className="evaluaciones-list">
                    {evaluaciones.map((ev) => (
                      <div key={ev.id} className="evaluacion-row">
                        <span className="evaluacion-tipo">
                          {TIPOS_EVAL_LABEL[ev.tipo] ?? ev.tipo}
                        </span>
                        <span className="evaluacion-nombre">{ev.descripcion ?? '—'}</span>
                        <span className="evaluacion-fecha">{formatFecha(ev.fecha)}</span>
                        {ev.nota != null ? (
                          <span className="evaluacion-nota">{ev.nota}/10</span>
                        ) : (
                          <span className="evaluacion-nota evaluacion-nota--pendiente">—</span>
                        )}
                      </div>
                    ))}
                  </div>
                )}

                {showEvalForm && (
                  <form className="inline-form" onSubmit={handleAddEval} noValidate>
                    <div className="inline-form-row">
                      <div className="inline-form-field">
                        <label className="inline-form-label">Tipo</label>
                        <select
                          className="inline-form-select"
                          name="tipo"
                          value={evalFields.tipo}
                          onChange={handleEvalChange}
                          disabled={savingEval}
                        >
                          {TIPOS_EVAL.map(t => (
                            <option key={t} value={t}>{TIPOS_EVAL_LABEL[t]}</option>
                          ))}
                        </select>
                      </div>
                      <div className="inline-form-field">
                        <label className="inline-form-label">Fecha</label>
                        <input
                          className="inline-form-input"
                          type="date"
                          name="fecha"
                          value={evalFields.fecha}
                          onChange={handleEvalChange}
                          disabled={savingEval}
                        />
                      </div>
                    </div>
                    <div className="inline-form-row">
                      <div className="inline-form-field inline-form-field--grow">
                        <label className="inline-form-label">Descripción (opcional)</label>
                        <input
                          className="inline-form-input"
                          type="text"
                          name="descripcion"
                          placeholder="Ej: Parcial 1er módulo"
                          maxLength={500}
                          value={evalFields.descripcion}
                          onChange={handleEvalChange}
                          disabled={savingEval}
                        />
                      </div>
                    </div>
                    {evalError && <p className="inline-form-error">⚠ {evalError}</p>}
                    <div className="inline-form-actions">
                      <button
                        type="submit"
                        className="inline-form-submit-btn"
                        disabled={!canAddEval || savingEval}
                      >
                        {savingEval ? 'Guardando...' : '✓ Guardar Evaluación'}
                      </button>
                    </div>
                  </form>
                )}
              </div>
            </>
          )}
        </div>

        {/* Bottom scroll cylinder */}
        <div className="scroll-end scroll-end--bottom">
          <div className="scroll-knob" />
          <div className="scroll-knob" />
        </div>
      </div>
    </div>
  );
}
