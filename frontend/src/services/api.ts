import {
  AlterarSenhaDto,
  AtualizarRemetenteDto,
  AtualizarUsuarioDto,
  CriarRemetenteDto,
  EmailKanban,
  EmailKanbanDetalhe,
  FiltroEmails,
  LoginResponse,
  RemetenteMonitorado,
  SalvarConfiguracaoEmailDto,
  StatusKanban,
  Usuario,
} from '@/types';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

function getToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('auth-token');
}

async function fetchJson<T>(path: string, init?: RequestInit, skipAuth = false): Promise<T> {
  const token = skipAuth ? null : getToken();

  const res = await fetch(`${BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    },
    ...init,
  });

  if (!skipAuth && res.status === 401 && typeof window !== 'undefined') {
    localStorage.removeItem('auth-token');
    localStorage.removeItem('auth-user');
    document.cookie = 'auth-token=; path=/; max-age=0';
    window.location.href = '/login';
    throw new Error('Sessão expirada.');
  }

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error((body as { mensagem?: string }).mensagem ?? `Erro ${res.status}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

export const api = {
  login(email: string, senha: string): Promise<LoginResponse> {
    return fetchJson<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, senha }),
    }, true);
  },

  cadastrar(nome: string, email: string, senha: string): Promise<void> {
    return fetchJson<void>('/api/usuarios', {
      method: 'POST',
      body: JSON.stringify({ nome, email, senha }),
    }, true);
  },

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

  alterarStatus(id: number, status: StatusKanban, observacao?: string): Promise<void> {
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

  // Remetentes Monitorados
  listarRemetentes(): Promise<RemetenteMonitorado[]> {
    return fetchJson<RemetenteMonitorado[]>('/api/remetentes-monitorados');
  },

  criarRemetente(dto: CriarRemetenteDto): Promise<RemetenteMonitorado> {
    return fetchJson<RemetenteMonitorado>('/api/remetentes-monitorados', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  },

  atualizarRemetente(id: number, dto: AtualizarRemetenteDto): Promise<void> {
    return fetchJson<void>(`/api/remetentes-monitorados/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    });
  },

  excluirRemetente(id: number): Promise<void> {
    return fetchJson<void>(`/api/remetentes-monitorados/${id}`, {
      method: 'DELETE',
    });
  },

  // Usuários
  listarUsuarios(): Promise<Usuario[]> {
    return fetchJson<Usuario[]>('/api/usuarios');
  },

  obterPerfil(id: number): Promise<Usuario> {
    return fetchJson<Usuario>(`/api/usuarios/${id}`);
  },

  atualizarUsuario(id: number, dto: AtualizarUsuarioDto): Promise<void> {
    return fetchJson<void>(`/api/usuarios/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    });
  },

  alterarSenha(id: number, dto: AlterarSenhaDto): Promise<void> {
    return fetchJson<void>(`/api/usuarios/${id}/senha`, {
      method: 'PATCH',
      body: JSON.stringify(dto),
    });
  },

  salvarConfiguracaoEmail(id: number, dto: SalvarConfiguracaoEmailDto): Promise<void> {
    return fetchJson<void>(`/api/usuarios/${id}/configuracao-email`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    });
  },

  toggleAtivo(id: number, ativo: boolean): Promise<void> {
    return fetchJson<void>(`/api/usuarios/${id}/ativo?ativo=${ativo}`, {
      method: 'PATCH',
    });
  },
};
