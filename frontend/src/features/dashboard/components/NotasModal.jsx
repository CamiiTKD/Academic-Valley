import { useState } from 'react';
import { agregarRegistroNota } from '../../../services/api';
import WoodPanel from '../../../components/ui/WoodPanel';
import './NotasModal.css';

const TIPO_LABEL = { Promocion: 'Promoción', ExamenFinal: 'Examen Final' };

function formatFecha(iso) {
  if (!iso) return '';
  const [y, m, d] = String(iso).split('-');
  return `${d}/${m}/${y}`;
}

const HOY = new Date().toISOString().split('T')[0];

export default function NotasModal({ materia, onClose, onNotaAdded }) {
  const [valor, setValor] = useState('');
  const [tipo, setTipo] = useState('ExamenFinal');
  const [fecha, setFecha] = useState(HOY);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  function handleOverlayClick(e) {
    if (e.target === e.currentTarget) onClose();
  }

  async function handleAgregar(e) {
    e.preventDefault();
    const v = Number(valor);
    if (!v || v < 1 || v > 10) { setError('La nota debe estar entre 1 y 10.'); return; }
    if (!fecha) { setError('La fecha es requerida.'); return; }

    setLoading(true);
    setError(null);
    try {
      const [y, m, d] = fecha.split('-');
      await agregarRegistroNota(materia.id, {
        materiaId: materia.id,
        valorNota: v,
        tipo,
        fecha: `${y}-${m}-${d}`,
      });
      onNotaAdded();
    } catch (err) {
      setError(err.message ?? 'Error al agregar la nota.');
      setLoading(false);
    }
  }

  const notas = [...(materia.registroNotas ?? [])].sort((a, b) =>
    String(b.fecha).localeCompare(String(a.fecha))
  );

  const yaAprobada = (materia.registroNotas ?? []).some(
    n => n.tipo === 'ExamenFinal' && n.valorNota >= 4
  );

  return (
    <div className="notas-overlay" onClick={handleOverlayClick}>
      <div className="notas-modal">
        <WoodPanel variant="heavy" style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>

          {/* ── Header ── */}
          <div className="notas-header">
            <div className="notas-header-left">
              <span className="notas-title">📜 Historial de Notas</span>
              <span className="notas-materia-nombre">{materia.nombre}</span>
            </div>
            <button className="notas-close-btn" onClick={onClose}>✕</button>
          </div>

          {/* ── Bitácora ── */}
          <div className="notas-body">
            {notas.length === 0 ? (
              <p className="notas-vacio">Sin notas registradas aún.</p>
            ) : (
              <div className="notas-lista">
                <div className="notas-cabecera">
                  <span>Fecha</span>
                  <span>Tipo</span>
                  <span>Nota</span>
                </div>
                {notas.map(n => (
                  <div
                    key={n.id}
                    className={`nota-row${n.esAplazo ? ' nota-row--aplazo' : ' nota-row--aprobada'}`}
                  >
                    <span className="nota-fecha">{formatFecha(n.fecha)}</span>
                    <span className="nota-tipo">{TIPO_LABEL[n.tipo] ?? n.tipo}</span>
                    <span className="nota-valor">
                      {n.esAplazo && <span className="nota-aplazo-mark">✗</span>}
                      {n.valorNota}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* ── Formulario agregar nota ── */}
          <div className="notas-footer">
            {yaAprobada ? (
              <div className="notas-aprobada-aviso">
                ✓ Materia aprobada. No se admiten más registros.
              </div>
            ) : (
              <>
                <span className="notas-form-title">+ Agregar nota</span>
                <form className="notas-form" onSubmit={handleAgregar}>
                  <div className="notas-form-row">
                    <div className="notas-form-field">
                      <label className="notas-form-label">Nota (1–10)</label>
                      <input
                        className="notas-form-input notas-form-input--num"
                        type="number"
                        min={1} max={10} step={1}
                        value={valor}
                        onChange={e => { setValor(e.target.value); setError(null); }}
                        placeholder="Ej: 7"
                        disabled={loading}
                      />
                    </div>
                    <div className="notas-form-field">
                      <label className="notas-form-label">Tipo</label>
                      <select
                        className="notas-form-select"
                        value={tipo}
                        onChange={e => setTipo(e.target.value)}
                        disabled={loading}
                      >
                        <option value="ExamenFinal">Examen Final</option>
                        <option value="Promocion">Promoción</option>
                      </select>
                    </div>
                    <div className="notas-form-field">
                      <label className="notas-form-label">Fecha</label>
                      <input
                        className="notas-form-input"
                        type="date"
                        max={HOY}
                        value={fecha}
                        onChange={e => { setFecha(e.target.value); setError(null); }}
                        disabled={loading}
                      />
                    </div>
                    <button
                      type="submit"
                      className={`notas-form-btn${loading ? ' notas-form-btn--loading' : ''}`}
                      disabled={loading || !valor}
                    >
                      {loading ? '...' : '✓ Guardar'}
                    </button>
                  </div>
                  {error && <p className="notas-form-error">⚠ {error}</p>}
                </form>
              </>
            )}
          </div>

        </WoodPanel>
      </div>
    </div>
  );
}
