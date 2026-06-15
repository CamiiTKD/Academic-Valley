// In development, Vite proxies /api → backend (see vite.config.js).
// In Docker, set VITE_API_BASE_URL to the full origin (e.g. http://localhost:8080).
const BASE = import.meta.env.VITE_API_BASE_URL ?? '';

async function get(path) {
  const res = await fetch(`${BASE}/api${path}`);
  if (!res.ok) throw new Error(`API error ${res.status}: ${path}`);
  return res.json();
}

export async function getProgreso() {
  return get('/Progreso');
}

export async function getMaterias() {
  return get('/Materias');
}

export async function getEvaluaciones(materiaId) {
  return get(`/Materias/${materiaId}/evaluaciones`);
}

export async function getHorarios(materiaId) {
  return get(`/Materias/${materiaId}/horarios`);
}

export async function getAgendaSemanal() {
  return get('/Agenda/semanal');
}
