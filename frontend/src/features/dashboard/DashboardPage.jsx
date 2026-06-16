import { useEffect, useState, useCallback } from 'react';
import { getMaterias, getProgreso } from '../../services/api';
import WoodPanel from '../../components/ui/WoodPanel';
import FarmElement from '../../components/ui/FarmElement';
import CurrentCoursesCarousel from './components/CurrentCoursesCarousel';
import MateriaDetailPanel from './components/MateriaDetailPanel';
import NuevaMateriaModal from './components/NuevaMateriaModal';
import CarteleraModal from './components/CarteleraModal';
import AgendaModal from './components/AgendaModal';
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
  const [showAgenda, setShowAgenda] = useState(false);
  const [editingMateria, setEditingMateria] = useState(null);

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

  const rawDate = new Date().toLocaleDateString('es-AR', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });
  const today = rawDate
    .replace(/^(.)/, c => c.toUpperCase())
    .replace(/(, \d+ de )([a-záéíóúüñ]+)/, (_, pre, month) =>
      `${pre}${month[0].toUpperCase()}${month.slice(1)}`
    );

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

      {/* Static background decorations — below everything interactive */}
      <div className="deco-layer" aria-hidden="true">
        {/* Left zone — above Log */}
        <span className="deco deco-grass"    style={{ top: '30%', left: '20%' }} />
        <span className="deco deco-flower-y" style={{ top: '32%', left: '23%' }} />
        {/* Left zone — between Log and Bulletin */}
        <span className="deco deco-grass"    style={{ top: '54%', left: '20%' }} />
        <span className="deco deco-flower-r" style={{ top: '57%', left: '23%' }} />
        <span className="deco deco-stone"    style={{ top: '60%', left: '19%' }} />
        {/* Far left — above Bulletin */}
        <span className="deco deco-grass"    style={{ top: '44%', left: '1%'  }} />
        <span className="deco deco-flower-r" style={{ top: '41%', left: '3%'  }} />
        {/* Right zone — flanking Agenda flower */}
        <span className="deco deco-grass"    style={{ top: '33%', left: '90%' }} />
        <span className="deco deco-flower-p" style={{ top: '36%', left: '93%' }} />
        <span className="deco deco-grass"    style={{ top: '58%', left: '91%' }} />
        <span className="deco deco-stone-sm" style={{ top: '60%', left: '94%' }} />
        <span className="deco deco-flower-y" style={{ top: '64%', left: '92%' }} />
        {/* Bottom center — below WoodPanel */}
        <span className="deco deco-grass"    style={{ top: '79%', left: '28%' }} />
        <span className="deco deco-flower-r" style={{ top: '81%', left: '40%' }} />
        <span className="deco deco-stone"    style={{ top: '80%', left: '51%' }} />
        <span className="deco deco-grass"    style={{ top: '79%', left: '62%' }} />
        <span className="deco deco-flower-p" style={{ top: '82%', left: '71%' }} />
        <span className="deco deco-stone-sm" style={{ top: '78%', left: '76%' }} />
      </div>

      {/* Farm elements — absolutely positioned over the grass */}
      <FarmElement
        spriteUrl="/sprites/log.svg"
        label="Nueva Materia"
        top="40%"
        left="13%"
        onClick={() => setShowNuevaMateria(true)}
      />
      <FarmElement
        spriteUrl="/sprites/bulletin.svg"
        label="Historial Académico"
        top="63%"
        left="4%"
        onClick={() => setShowCartelera(true)}
      />
      <FarmElement
        spriteUrl="/sprites/flower.svg"
        label="Revisar Agenda"
        top="44%"
        left="83%"
        onClick={() => setShowAgenda(true)}
      />

      {/* Main content */}
      <div className="dashboard-content">
        <div className="date-plaque-wrap">
          <div className="date-plaque">
            <span className="date-plaque__text">{today}</span>
          </div>
        </div>

        <WoodPanel style={{ width: '100%', maxWidth: '1060px', padding: '20px 16px', position: 'relative' }}>
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

      {/* Agenda modal */}
      {showAgenda && (
        <AgendaModal onClose={() => setShowAgenda(false)} />
      )}

      {/* Cartelera modal */}
      {showCartelera && (
        <CarteleraModal
          materias={materias}
          loading={loadingMaterias}
          onClose={() => setShowCartelera(false)}
          onEdit={(m) => setEditingMateria(m)}
          onNotaAdded={() => { fetchMaterias(); fetchProgreso(); }}
        />
      )}

      {/* Nueva materia / editar materia modal */}
      {(showNuevaMateria || editingMateria) && (
        <NuevaMateriaModal
          materia={editingMateria}
          onClose={() => { setShowNuevaMateria(false); setEditingMateria(null); }}
          onCreated={handleMateriaCreada}
        />
      )}

      {/* Fixed bottom progress bar */}
      <CareerProgressBar progreso={progreso} loading={loadingProgreso} />
    </div>
  );
}
