import {
  EmailKanban,
  EmailKanbanDetalhe,
  FiltroEmails,
  StatusKanban,
} from '@/types';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

async function fetchJson<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  });
  if (!res.ok) throw new Error(`API error ${res.status}: ${path}`);
  return res.json() as Promise<T>;
}

export const api = {
  listarEmails(filtro?: FiltroEmails): Promise<EmailKanban[]> {
    const params = new URLSearchParams();
    if (filtro?.remetente) params.set('remetente', filtro.remetente);
    if (filtro?.assunto) params.set('assunto', filtro.assunto);
    if (filtro?.status) params.set('status', filtro.status);
    if (filtro?.dataInicio) params.set('dataInicio', filtro.dataInicio);
    if (filtro?.dataFim) params.set('dataFim', filtro.dataFim);
    params.set('tamanhoPagina', '200');
    const qs = params.toString();
    return fetchJson<EmailKanban[]>(`/api/emails-kanban${qs ? `?${qs}` : ''}`);
  },

  obterDetalhe(id: number): Promise<EmailKanbanDetalhe> {
    return fetchJson<EmailKanbanDetalhe>(`/api/emails-kanban/${id}`);
  },

  alterarStatus(
    id: number,
    status: StatusKanban,
    observacao?: string
  ): Promise<void> {
    return fetchJson(`/api/emails-kanban/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status, observacao }),
    });
  },

  sincronizar(): Promise<{ mensagem: string; processados: number }> {
    return fetchJson(`/api/emails-kanban/sync`, { method: 'POST' });
  },

  downloadAnexoUrl(id: number): string {
    return `${BASE_URL}/api/anexos/${id}/download`;
  },
};
