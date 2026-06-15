import './CareerProgressBar.css';

export default function CareerProgressBar({ progreso, loading }) {
  if (loading) {
    return (
      <div className="progress-bar-footer">
        <p className="progress-loading">Cargando progreso...</p>
      </div>
    );
  }

  const pct = progreso?.porcentajeProgreso ?? 0;
  const pctClamped = Math.min(100, Math.max(0, pct));
  const isLow = pctClamped < 20;

  const promedioSin = progreso?.promedioSinAplazos?.toFixed(2) ?? '—';
  const promedioCon = progreso?.promedioConAplazos?.toFixed(2) ?? '—';

  return (
    <div className="progress-bar-footer">
      <div className="progress-label-row">
        <span className="progress-title">✦ Progreso de carrera</span>
        <div className="progress-averages">
          <span className="progress-avg">Promedio: {promedioSin}</span>
          <span className="progress-avg">Con aplazos: {promedioCon}</span>
        </div>
      </div>

      <div className="stamina-container">
        <div
          className={`stamina-fill${isLow ? ' stamina-fill--low' : ''}`}
          style={{ width: `${pctClamped}%` }}
        />
        <span className="stamina-text">
          {progreso?.materiasAprobadas ?? 0} / {progreso?.totalMaterias ?? 0} ({pctClamped.toFixed(0)}%)
        </span>
      </div>
    </div>
  );
}
