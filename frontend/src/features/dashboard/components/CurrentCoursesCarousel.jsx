import { useRef } from 'react';
import './CurrentCoursesCarousel.css';

function MateriaCard({ materia, isActive, onClick }) {
  const cuatLabel = materia.cuatrimestre
    ? `${materia.cuatrimestre}° Cuat.`
    : '';

  return (
    <div
      className={`materia-card${isActive ? ' materia-card--active' : ''}`}
      onClick={() => onClick(materia)}
      title={materia.nombre}
    >
      <div className="card-rope" />
      <div className="card-plank">
        <p className="card-nombre">{materia.nombre}</p>
        <p className="card-codigo">{materia.codigo}</p>
        {cuatLabel && <p className="card-cuatrimestre">{cuatLabel}</p>}
      </div>
    </div>
  );
}

export default function CurrentCoursesCarousel({ materias, loading, selectedId, onSelect }) {
  const trackRef = useRef(null);

  const CARD_WIDTH = 180 + 16; // card + gap

  function scrollLeft() {
    trackRef.current?.scrollBy({ left: -CARD_WIDTH * 2, behavior: 'smooth' });
  }
  function scrollRight() {
    trackRef.current?.scrollBy({ left: CARD_WIDTH * 2, behavior: 'smooth' });
  }

  const cursando = materias.filter((m) => m.estado === 'Cursando');

  function handleSelect(materia) {
    onSelect(materia.id === selectedId ? null : materia);
  }

  return (
    <div className="carousel-section">
      <p className="carousel-label">✦ Materias en curso ✦</p>

      <div className="carousel-track-wrapper">
        <button className="carousel-nav-btn" onClick={scrollLeft} disabled={loading || cursando.length === 0}>
          ◀
        </button>

        <div className="carousel-track" ref={trackRef}>
          {loading && <p className="carousel-loading">Cargando materias...</p>}
          {!loading && cursando.length === 0 && (
            <p className="carousel-empty">No hay materias en curso.</p>
          )}
          {!loading &&
            cursando.map((m) => (
              <MateriaCard
                key={m.id}
                materia={m}
                isActive={m.id === selectedId}
                onClick={handleSelect}
              />
            ))}
        </div>

        <button className="carousel-nav-btn" onClick={scrollRight} disabled={loading || cursando.length === 0}>
          ▶
        </button>
      </div>
    </div>
  );
}
