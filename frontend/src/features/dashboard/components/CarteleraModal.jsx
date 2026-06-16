import { useState } from 'react';
import WoodPanel from '../../../components/ui/WoodPanel';
import NotasModal from './NotasModal';
import './CarteleraModal.css';

const FILTROS = ['Todas', 'Pendiente', 'Cursando', 'Regular', 'Aprobada'];

export default function CarteleraModal({ materias, loading, onClose, onEdit, onNotaAdded }) {
  const [filtro, setFiltro] = useState('Todas');
  const [notasMateria, setNotasMateria] = useState(null);

  function handleOverlayClick(e) {
    if (e.target === e.currentTarget) onClose();
  }

  const lista = filtro === 'Todas'
    ? materias
    : materias.filter(m => m.estado === filtro);

  function countEstado(e) {
    return materias.filter(m => m.estado === e).length;
  }

  return (
    <>
      <div className="cartelera-overlay" onClick={handleOverlayClick}>
        <div className="cartelera-modal">
          <WoodPanel
            variant="heavy"
            style={{ display: 'flex', flexDirection: 'column', height: '100%' }}
          >
            {/* ─── Header ─── */}
            <div className="cartelera-header">
              <span className="cartelera-title">📋 Historial Académico</span>
              <button className="cartelera-close-btn" onClick={onClose}>✕</button>
            </div>

            {/* ─── Filter tabs ─── */}
            <div className="cartelera-filtros">
              {FILTROS.map(f => (
                <button
                  key={f}
                  className={`filtro-btn${filtro === f ? ' filtro-btn--active' : ''}`}
                  onClick={() => setFiltro(f)}
                >
                  {f}
                  {f !== 'Todas' && (
                    <span className="filtro-badge">{countEstado(f)}</span>
                  )}
                  {f === 'Todas' && (
                    <span className="filtro-badge">{materias.length}</span>
                  )}
                </button>
              ))}
            </div>

            {/* ─── List body ─── */}
            <div className="cartelera-body">
              {loading ? (
                <p className="cartelera-msg">Cargando el tablón...</p>
              ) : lista.length === 0 ? (
                <p className="cartelera-msg">No hay materias con ese estado.</p>
              ) : (
                <div className="cartelera-lista">
                  <div className="lista-cabecera">
                    <span className="col-nombre">Materia</span>
                    <span className="col-codigo">Código</span>
                    <span className="col-estado">Estado</span>
                    <span className="col-notas">Notas</span>
                    <span className="col-acciones" />
                  </div>

                  {lista.map((m, i) => {
                    const cantNotas = m.registroNotas?.length ?? 0;
                    const tieneAplazo = m.registroNotas?.some(n => n.esAplazo);
                    return (
                      <div
                        key={m.id}
                        className={`materia-row${i % 2 === 0 ? ' materia-row--alt' : ''}`}
                      >
                        <span className="col-nombre materia-nombre">{m.nombre}</span>
                        <span className="col-codigo materia-codigo">{m.codigo}</span>
                        <span className="col-estado">
                          <span className={`estado-badge estado-badge--${m.estado.toLowerCase()}`}>
                            {m.estado}
                          </span>
                        </span>
                        <span className="col-notas">
                          <button
                            className={`notas-row-btn${tieneAplazo ? ' notas-row-btn--aplazo' : ''}`}
                            title="Ver historial de notas"
                            onClick={() => setNotasMateria(m)}
                          >
                            {cantNotas > 0 ? `📖 ${cantNotas}` : '📖 —'}
                          </button>
                        </span>
                        <span className="col-acciones">
                          <button
                            className="edit-row-btn"
                            title="Editar materia"
                            onClick={() => onEdit(m)}
                          >
                            ✏
                          </button>
                        </span>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            {/* ─── Footer count ─── */}
            {!loading && (
              <div className="cartelera-footer">
                {lista.length} {lista.length === 1 ? 'materia' : 'materias'} mostradas
              </div>
            )}
          </WoodPanel>
        </div>
      </div>

      {notasMateria && (
        <NotasModal
          materia={notasMateria}
          onClose={() => setNotasMateria(null)}
          onNotaAdded={() => {
            setNotasMateria(null);
            onNotaAdded?.();
          }}
        />
      )}
    </>
  );
}
