// In development, Vite proxies /api → backend (see vite.config.js).
// In Docker, set VITE_API_BASE_URL to the full origin (e.g. http://localhost:8080).
const BASE = import.meta.env.VITE_API_BASE_URL ?? '';

async function get(path) {
  const res = await fetch(`${BASE}/api${path}`);
  if (!res.ok) throw new Error(`API error ${res.status}: ${path}`);
  return res.json();
}

async function post(path, body) {
  const res = await fetch(`${BASE}/api${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    // Forward validation errors from the backend when available
    let msg = `Error ${res.status}`;
    try {
      const data = await res.json();
      const details = data?.errors
        ? Object.values(data.errors).flat().join(' ')
        : data?.title ?? data?.message;
      if (details) msg = details;
    } catch { /* non-JSON body */ }
    throw new Error(msg);
  }
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

export async function crearMateria(body) {
  return post('/Materias', body);
}
