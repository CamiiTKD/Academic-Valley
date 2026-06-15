import { useEffect, useState, useCallback } from 'react';
import { getMaterias, getProgreso } from '../../services/api';
import WoodPanel from '../../components/ui/WoodPanel';
import FarmElement from '../../components/ui/FarmElement';
import CurrentCoursesCarousel from './components/CurrentCoursesCarousel';
import MateriaDetailPanel from './components/MateriaDetailPanel';
import NuevaMateriaModal from './components/NuevaMateriaModal';
import CarteleraModal from './components/CarteleraModal';
import CareerProgressBar from './components/CareerProgressBar';
import './DashboardPage.css';

export default function DashboardPage() {
  const [materias, setMaterias] = useState([]);
  const [progreso, setProgreso] = useState(null);
  const [loadingMaterias, setLoadingMaterias] = useState(true);
  const [loadingProgreso, setLoadingProgreso] = useState(true);
  const [selectedMateria, setSelectedMateria] = useState(null);
  const [showNuevaMateria, setShowNuevaMateria] = useState(false);
  const [showCartelera, setShowCartelera] = useState(false);

  const fetchMaterias = useCallback(() => {
    setLoadingMaterias(true);
    getMaterias()
      .then(setMaterias)
      .catch(console.error)
      .finally(() => setLoadingMaterias(false));
  }, []);

  const fetchProgreso = useCallback(() => {
    setLoadingProgreso(true);
    getProgreso()
      .then(setProgreso)
      .catch(console.error)
      .finally(() => setLoadingProgreso(false));
  }, []);

  useEffect(() => {
    fetchMaterias();
    fetchProgreso();
  }, [fetchMaterias, fetchProgreso]);

  function handleMateriaCreada() {
    fetchMaterias();
    fetchProgreso();
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

      {/* Farm elements — absolutely positioned over the grass */}
      <FarmElement
        spriteUrl="/sprites/log.svg"
        label="Nueva Materia"
        top="48%"
        left="15%"
        onClick={() => setShowNuevaMateria(true)}
      />
      <FarmElement
        spriteUrl="/sprites/bulletin.svg"
        label="Historial Académico"
        top="70%"
        left="6%"
        onClick={() => setShowCartelera(true)}
      />
      <FarmElement
        spriteUrl="/sprites/flower.svg"
        label="Revisar Agenda"
        top="52%"
        left="82%"
        onClick={() => {/* TODO: abrir vista de Agenda */}}
      />

      {/* Main content */}
      <div className="dashboard-content">
        <div className="dashboard-header-sign">
          <span className="header-sign-date">{today}</span>
        </div>

        <WoodPanel style={{ width: '100%', maxWidth: '860px', padding: '16px 12px', position: 'relative' }}>
          <CurrentCoursesCarousel
            materias={materias}
            loading={loadingMaterias}
            selectedId={selectedMateria?.id}
            onSelect={(m) => setSelectedMateria(m)}
          />
        </WoodPanel>
      </div>

      {/* Materia detail panel */}
      {selectedMateria && (
        <MateriaDetailPanel
          materia={selectedMateria}
          onClose={() => setSelectedMateria(null)}
        />
      )}

      {/* Cartelera modal */}
      {showCartelera && (
        <CarteleraModal
          materias={materias}
          loading={loadingMaterias}
          onClose={() => setShowCartelera(false)}
        />
      )}

      {/* Nueva materia modal */}
      {showNuevaMateria && (
        <NuevaMateriaModal
          onClose={() => setShowNuevaMateria(false)}
          onCreated={handleMateriaCreada}
        />
      )}

      {/* Fixed bottom progress bar */}
      <CareerProgressBar progreso={progreso} loading={loadingProgreso} />
    </div>
  );
}
