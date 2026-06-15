import { useEffect, useState } from 'react';
import { getEvaluaciones, getHorarios } from '../../../services/api';
import './MateriaDetailPanel.css';

const DIAS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

function formatTime(timeOnly) {
  // API returns "HH:mm:ss" or "HH:mm"
  return timeOnly?.slice(0, 5) ?? '';
}

function formatFecha(isoDate) {
  if (!isoDate) return '—';
  const d = new Date(isoDate);
  return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export default function MateriaDetailPanel({ materia, onClose }) {
  const [horarios, setHorarios] = useState([]);
  const [evaluaciones, setEvaluaciones] = useState([]);
  const [loading, setLoading] = useState(true);

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

  // Sort horarios: Mon-Sun (normalize Sunday to 7)
  const horariosOrdenados = [...horarios].sort((a, b) => {
    const norm = (d) => (d === 0 ? 7 : d);
    if (norm(a.diaSemana) !== norm(b.diaSemana)) return norm(a.diaSemana) - norm(b.diaSemana);
    return a.horaInicio.localeCompare(b.horaInicio);
  });

  function handleOverlayClick(e) {
    if (e.target === e.currentTarget) onClose();
  }

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
              {/* Horarios */}
              <div className="detail-section">
                <p className="detail-section-title">📅 Horarios de cursada</p>
                {horariosOrdenados.length === 0 ? (
                  <p className="empty-state">Sin horarios registrados.</p>
                ) : (
                  <div className="horarios-grid">
                    {horariosOrdenados.map((h) => (
                      <div key={h.id} className="horario-block">
                        <span className="horario-dia">{DIAS[h.diaSemana] ?? h.diaSemana}</span>
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
              </div>

              {/* Evaluaciones */}
              <div className="detail-section">
                <p className="detail-section-title">📝 Evaluaciones</p>
                {evaluaciones.length === 0 ? (
                  <p className="empty-state">Sin evaluaciones registradas.</p>
                ) : (
                  <div className="evaluaciones-list">
                    {evaluaciones.map((ev) => (
                      <div key={ev.id} className="evaluacion-row">
                        <span className="evaluacion-tipo">{ev.tipo}</span>
                        <span className="evaluacion-nombre">{ev.nombre}</span>
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
