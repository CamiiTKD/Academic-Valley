import { useState } from 'react';
import { crearMateria, actualizarMateria } from '../../../services/api';
import WoodPanel from '../../../components/ui/WoodPanel';
import './NuevaMateriaModal.css';

const ESTADOS = ['Pendiente', 'Cursando', 'Regular', 'Aprobada'];

function buildInitial(materia) {
  if (!materia) return { nombre: '', codigo: '', cuatrimestre: '', estado: 'Pendiente', notaFinal: '' };
  return {
    nombre: materia.nombre,
    codigo: materia.codigo,
    cuatrimestre: String(materia.cuatrimestre),
    estado: materia.estado,
    notaFinal: materia.notaFinal != null ? String(materia.notaFinal) : '',
  };
}

export default function NuevaMateriaModal({ onClose, onCreated, materia }) {
  const isEditing = materia != null;
  const [fields, setFields] = useState(() => buildInitial(materia));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  function handleChange(e) {
    const { name, value } = e.target;
    setFields((prev) => ({ ...prev, [name]: value }));
    setError(null);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);
    setError(null);

    const payload = {
      nombre: fields.nombre.trim(),
      codigo: fields.codigo.trim(),
      cuatrimestre: Number(fields.cuatrimestre),
      notaFinal: fields.estado === 'Aprobada' ? Number(fields.notaFinal) : null,
    };

    try {
      let dto;
      if (isEditing) {
        dto = await actualizarMateria(materia.id, { ...payload, id: materia.id, estado: fields.estado });
      } else {
        dto = await crearMateria({ ...payload, estadoInicial: fields.estado });
      }
      onCreated(dto);
      onClose();
    } catch (err) {
      setError(err.message ?? 'Error al guardar la materia.');
    } finally {
      setLoading(false);
    }
  }

  function handleOverlayClick(e) {
    if (e.target === e.currentTarget) onClose();
  }

  const notaOk =
    fields.estado !== 'Aprobada' ||
    (Number(fields.notaFinal) >= 1 && Number(fields.notaFinal) <= 10);

  const canSubmit =
    fields.nombre.trim() &&
    fields.codigo.trim() &&
    Number(fields.cuatrimestre) >= 1 &&
    notaOk;

  return (
    <div className="nueva-materia-overlay" onClick={handleOverlayClick}>
      <div className="nueva-materia-panel">
        <WoodPanel variant="heavy">
          <div className="modal-title-bar">
            <span className="modal-title">
              {isEditing ? '✏ Editar Materia' : '⛏ Nueva Materia'}
            </span>
            <button className="modal-close-btn" onClick={onClose} disabled={loading}>
              ✕
            </button>
          </div>

          <form className="modal-form" onSubmit={handleSubmit} noValidate>
            <div className="form-field">
              <label className="form-label" htmlFor="nm-nombre">
                Nombre
              </label>
              <input
                id="nm-nombre"
                className="form-input"
                name="nombre"
                type="text"
                placeholder="Ej: Álgebra Lineal"
                maxLength={200}
                value={fields.nombre}
                onChange={handleChange}
                autoFocus
                disabled={loading}
              />
            </div>

            <div className="form-row">
              <div className="form-field">
                <label className="form-label" htmlFor="nm-codigo">
                  Código
                </label>
                <input
                  id="nm-codigo"
                  className="form-input"
                  name="codigo"
                  type="text"
                  placeholder="Ej: I103"
                  maxLength={20}
                  value={fields.codigo}
                  onChange={handleChange}
                  disabled={loading}
                />
              </div>

              <div className="form-field">
                <label className="form-label" htmlFor="nm-cuatrimestre">
                  Cuatrimestre
                </label>
                <input
                  id="nm-cuatrimestre"
                  className="form-input"
                  name="cuatrimestre"
                  type="number"
                  placeholder="Ej: 1"
                  min={1}
                  value={fields.cuatrimestre}
                  onChange={handleChange}
                  disabled={loading}
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-field">
                <label className="form-label" htmlFor="nm-estado">
                  Estado
                </label>
                <select
                  id="nm-estado"
                  className="form-select"
                  name="estado"
                  value={fields.estado}
                  onChange={handleChange}
                  disabled={loading}
                >
                  {ESTADOS.map((e) => (
                    <option key={e} value={e}>{e}</option>
                  ))}
                </select>
              </div>

              {fields.estado === 'Aprobada' && (
                <div className="form-field">
                  <label className="form-label" htmlFor="nm-nota">
                    Nota Final (1–10)
                  </label>
                  <input
                    id="nm-nota"
                    className="form-input"
                    name="notaFinal"
                    type="number"
                    placeholder="Ej: 8"
                    min={1}
                    max={10}
                    step={0.5}
                    value={fields.notaFinal}
                    onChange={handleChange}
                    disabled={loading}
                  />
                </div>
              )}
            </div>

            {error && <p className="form-error">⚠ {error}</p>}

            <button
              type="submit"
              className={`form-submit-btn${loading ? ' form-submit-btn--loading' : ''}`}
              disabled={!canSubmit || loading}
            >
              {loading
                ? (isEditing ? 'Guardando' : 'Plantando')
                : (isEditing ? '💾 Guardar Cambios' : '🌱 Plantar Materia')}
            </button>
          </form>
        </WoodPanel>
      </div>
    </div>
  );
}
