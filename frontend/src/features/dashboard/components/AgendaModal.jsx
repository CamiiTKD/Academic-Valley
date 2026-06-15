import { useEffect, useState } from 'react';
import { getAgendaSemanal } from '../../../services/api';
import WoodPanel from '../../../components/ui/WoodPanel';
import './AgendaModal.css';

const DIAS_CONFIG = [
  { value: 'Monday',    label: 'Lunes',     short: 'LUN' },
  { value: 'Tuesday',   label: 'Martes',    short: 'MAR' },
  { value: 'Wednesday', label: 'Miércoles', short: 'MIÉ' },
  { value: 'Thursday',  label: 'Jueves',    short: 'JUE' },
  { value: 'Friday',    label: 'Viernes',   short: 'VIE' },
  { value: 'Saturday',  label: 'Sábado',    short: 'SÁB' },
  { value: 'Sunday',    label: 'Domingo',   short: 'DOM' },
];

const TIPO_LABEL = {
  Parcial: 'Parcial', Teorico: 'Teórico', TrabajoPractico: 'TP',
  Coloquio: 'Coloquio', Final: 'Final',
};

const DOW_NAMES = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
const TODAY_DOW = DOW_NAMES[new Date().getDay()];

function formatTime(t) { return t?.slice(0, 5) ?? ''; }

function timeToMin(t) {
  const [h, m] = (t ?? '00:00').split(':').map(Number);
  return h * 60 + m;
}

function formatFechaCorta(iso) {
  if (!iso) return '';
  const [, m, d] = String(iso).split('-');
  return `${d}/${m}`;
}

function getWeekRange() {
  const now = new Date();
  const day = now.getDay();
  const lunes = new Date(now);
  lunes.setDate(now.getDate() - (day === 0 ? 6 : day - 1));
  const domingo = new Date(lunes);
  domingo.setDate(lunes.getDate() + 6);
  const fmt = (d) => d.toLocaleDateString('es-AR', { day: '2-digit', month: 'short' });
  return `${fmt(lunes)} — ${fmt(domingo)}`;
}

export default function AgendaModal({ onClose }) {
  const [dias, setDias]     = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError]   = useState(null);

  useEffect(() => {
    getAgendaSemanal()
      .then(setDias)
      .catch(() => setError('No se pudo cargar la agenda.'))
      .finally(() => setLoading(false));
  }, []);

  function handleOverlayClick(e) {
    if (e.target === e.currentTarget) onClose();
  }

  const diaMap = Object.fromEntries((dias ?? []).map(d => [d.diaSemana, d]));

  // Siempre mostrar Lun-Vie; Sáb/Dom solo si tienen contenido
  const diasToShow = DIAS_CONFIG.filter(({ value }) => {
    if (value !== 'Saturday' && value !== 'Sunday') return true;
    const d = diaMap[value];
    return d && (d.horarios.length > 0 || d.alertas.length > 0);
  });

  // Estamina: total de minutos de cursada en la semana
  const totalMin = (dias ?? [])
    .flatMap(d => d.horarios)
    .reduce((acc, h) => acc + timeToMin(h.horaFin) - timeToMin(h.horaInicio), 0);
  const totalHoras = totalMin / 60;
  const staminaPct = Math.min(100, (totalHoras / 20) * 100);

  const staminaLevel = totalHoras > 15 ? 'heavy' : totalHoras > 10 ? 'medium' : 'light';
  const staminaMsg = {
    heavy:  '⚠ ¡Semana Pesada! Estamina al límite',
    medium: '⚡ Semana intensa — ¡Descansá entre clases!',
    light:  '✓ Semana manejable — ¡A estudiar!',
  }[staminaLevel];

  return (
    <div className="agenda-overlay" onClick={handleOverlayClick}>
      <div className="agenda-modal">
        <WoodPanel variant="heavy" style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>

          {/* ── Header ── */}
          <div className="agenda-header">
            <div className="agenda-header-left">
              <span className="agenda-title">📅 Agenda Semanal</span>
              <span className="agenda-semana">{getWeekRange()}</span>
            </div>
            <button className="agenda-close-btn" onClick={onClose} title="Cerrar">✕</button>
          </div>

          {/* ── Cuerpo ── */}
          <div className="agenda-body">
            {loading && <p className="agenda-msg agenda-msg--loading">Cargando agenda...</p>}
            {error   && <p className="agenda-msg agenda-msg--error">{error}</p>}

            {!loading && !error && (
              <div
                className="agenda-grid"
                style={{ gridTemplateColumns: `repeat(${diasToShow.length}, minmax(130px, 1fr))` }}
              >
                {diasToShow.map(({ value, label }) => {
                  const dia   = diaMap[value] ?? { horarios: [], alertas: [] };
                  const esHoy = value === TODAY_DOW;

                  return (
                    <div key={value} className={`agenda-column${esHoy ? ' agenda-column--today' : ''}`}>
                      {/* Encabezado del día */}
                      <div className="agenda-col-header">
                        <span className="agenda-dia-nombre">{label}</span>
                        {esHoy && <span className="agenda-hoy-badge">★ HOY</span>}
                      </div>

                      {/* Bloques de contenido */}
                      <div className="agenda-col-body">
                        {dia.horarios.map((h, i) => (
                          <div key={i} className="agenda-bloque">
                            <div className="agenda-bloque-time">
                              {formatTime(h.horaInicio)} — {formatTime(h.horaFin)}
                            </div>
                            <div className="agenda-bloque-nombre">{h.materiaNombre}</div>
                            {h.esVirtual && (
                              <div className="agenda-bloque-tag agenda-bloque-tag--virtual">Virtual</div>
                            )}
                            {h.aula && !h.esVirtual && (
                              <div className="agenda-bloque-tag">🏫 {h.aula}</div>
                            )}
                          </div>
                        ))}

                        {dia.alertas.map((a, i) => (
                          <div key={i} className="agenda-alerta">
                            <span className="agenda-alerta-icon">!</span>
                            <div className="agenda-alerta-info">
                              <span className="agenda-alerta-tipo">{TIPO_LABEL[a.tipo] ?? a.tipo}</span>
                              <span className="agenda-alerta-nombre">{a.materiaNombre}</span>
                              {a.descripcion && (
                                <span className="agenda-alerta-desc">{a.descripcion}</span>
                              )}
                              <span className="agenda-alerta-fecha">{formatFechaCorta(a.fecha)}</span>
                            </div>
                            <div className="agenda-alerta-tooltip">
                              ¡Alerta!: {TIPO_LABEL[a.tipo] ?? a.tipo} de {a.materiaNombre}
                              {a.descripcion ? ` — ${a.descripcion}` : ''}
                            </div>
                          </div>
                        ))}

                        {dia.horarios.length === 0 && dia.alertas.length === 0 && (
                          <p className="agenda-col-vacio">— Sin clases —</p>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* ── Footer: barra de estamina ── */}
          {!loading && !error && (
            <div className="agenda-footer">
              <div className="agenda-stamina-label-row">
                <span className="agenda-stamina-title">⚡ Estamina Semanal</span>
                <span className={`agenda-stamina-msg agenda-stamina-msg--${staminaLevel}`}>
                  {staminaMsg}
                </span>
                <span className="agenda-stamina-horas">{totalHoras.toFixed(1)}h / 20h</span>
              </div>
              <div className="agenda-stamina-track">
                <div
                  className={`agenda-stamina-fill agenda-stamina-fill--${staminaLevel}`}
                  style={{ width: `${staminaPct}%` }}
                />
                <span className="agenda-stamina-text">{totalHoras.toFixed(1)}h</span>
              </div>
            </div>
          )}

        </WoodPanel>
      </div>
    </div>
  );
}
