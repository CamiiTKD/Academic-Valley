import { useEffect, useState } from 'react';
import { getMaterias, getProgreso } from '../../services/api';
import WoodPanel from '../../components/ui/WoodPanel';
import CurrentCoursesCarousel from './components/CurrentCoursesCarousel';
import MateriaDetailPanel from './components/MateriaDetailPanel';
import CareerProgressBar from './components/CareerProgressBar';
import './DashboardPage.css';

export default function DashboardPage() {
  const [materias, setMaterias] = useState([]);
  const [progreso, setProgreso] = useState(null);
  const [loadingMaterias, setLoadingMaterias] = useState(true);
  const [loadingProgreso, setLoadingProgreso] = useState(true);
  const [selectedMateria, setSelectedMateria] = useState(null);

  useEffect(() => {
    getMaterias()
      .then(setMaterias)
      .catch(console.error)
      .finally(() => setLoadingMaterias(false));

    getProgreso()
      .then(setProgreso)
      .catch(console.error)
      .finally(() => setLoadingProgreso(false));
  }, []);

  function handleSelectMateria(materia) {
    setSelectedMateria(materia);
  }

  function handleCloseDetail() {
    setSelectedMateria(null);
  }

  const today = new Date().toLocaleDateString('es-AR', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });

  return (
    <div className="dashboard">
      {/* Sky header */}
      <div className="dashboard-sky">
        <div className="cloud cloud-1" />
        <div className="cloud cloud-2" />
        <div className="cloud cloud-3" />
        <div className="cloud cloud-4" />
        <div className="dashboard-title-sign">
          <WoodPanel style={{ padding: '8px 20px' }}>
            <span className="title-sign-text">⭐ Planner Académico ⭐</span>
          </WoodPanel>
        </div>
      </div>

      {/* Grass strip */}
      <div className="dashboard-ground" />

      {/* Main content */}
      <div className="dashboard-content">
        <div className="dashboard-header-sign">
          <span className="header-sign-date">{today}</span>
        </div>

        {/* Carousel inside a wood panel */}
        <WoodPanel style={{ width: '100%', maxWidth: '860px', padding: '16px 12px' }}>
          <CurrentCoursesCarousel
            materias={materias}
            loading={loadingMaterias}
            selectedId={selectedMateria?.id}
            onSelect={handleSelectMateria}
          />
        </WoodPanel>
      </div>

      {/* Materia detail panel (modal) */}
      {selectedMateria && (
        <MateriaDetailPanel
          materia={selectedMateria}
          onClose={handleCloseDetail}
        />
      )}

      {/* Fixed bottom progress bar */}
      <CareerProgressBar progreso={progreso} loading={loadingProgreso} />
    </div>
  );
}
