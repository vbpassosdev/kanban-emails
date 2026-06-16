export type StatusKanban =
  | 'Novo'
  | 'EmAnalise'
  | 'Desenvolvimento'
  | 'AguardandoCliente'
  | 'Concluido';

export const STATUS_LABELS: Record<StatusKanban, string> = {
  Novo: 'Novo',
  EmAnalise: 'Em Análise',
  Desenvolvimento: 'Desenvolvimento',
  AguardandoCliente: 'Aguardando Cliente',
  Concluido: 'Concluído',
};

export const STATUS_COLORS: Record<StatusKanban, string> = {
  Novo: 'bg-blue-100 border-blue-300',
  EmAnalise: 'bg-yellow-100 border-yellow-300',
  Desenvolvimento: 'bg-purple-100 border-purple-300',
  AguardandoCliente: 'bg-orange-100 border-orange-300',
  Concluido: 'bg-green-100 border-green-300',
};

export const STATUS_HEADER_COLORS: Record<StatusKanban, string> = {
  Novo: 'bg-blue-500',
  EmAnalise: 'bg-yellow-500',
  Desenvolvimento: 'bg-purple-500',
  AguardandoCliente: 'bg-orange-500',
  Concluido: 'bg-green-500',
};

export const COLUNAS: StatusKanban[] = [
  'Novo',
  'EmAnalise',
  'Desenvolvimento',
  'AguardandoCliente',
  'Concluido',
];

export interface EmailAnexoResumo {
  id: number;
  nomeArquivo: string;
  mimeType: string | null;
  tamanhoBytes: number;
}

export interface EmailKanban {
  id: number;
  messageId: string;
  remetente: string;
  assunto: string | null;
  resumo: string | null;
  categoria: string | null;
  status: StatusKanban;
  dataRecebimento: string;
  dataAtualizacao: string | null;
  quantidadeAnexos: number;
  anexos: EmailAnexoResumo[];
}

export interface EmailAnexo {
  id: number;
  emailKanbanId: number;
  nomeArquivo: string;
  mimeType: string | null;
  tamanhoBytes: number;
  dataCriacao: string;
}

export interface EmailKanbanHistorico {
  id: number;
  statusAnterior: string | null;
  statusNovo: string;
  observacao: string | null;
  usuario: string | null;
  dataMovimento: string;
}

export interface EmailKanbanDetalhe {
  id: number;
  messageId: string;
  remetente: string;
  assunto: string | null;
  corpoTexto: string | null;
  corpoHtml: string | null;
  resumo: string | null;
  categoria: string | null;
  status: StatusKanban;
  dataRecebimento: string;
  dataAtualizacao: string | null;
  anexos: EmailAnexo[];
  historico: EmailKanbanHistorico[];
}

export interface FiltroEmails {
  remetente?: string;
  assunto?: string;
  status?: StatusKanban | '';
  dataInicio?: string;
  dataFim?: string;
}

export interface LoginUsuario {
  id: number;
  nome: string;
  email: string;
  ativo: boolean;
}

export interface LoginResponse {
  token: string;
  expiracao: string;
  usuario: LoginUsuario;
}

// Remetentes Monitorados
export interface RemetenteMonitorado {
  id: number;
  nome: string | null;
  emailOuDominio: string;
  ativo: boolean;
  dataCriacao: string;
}

export interface CriarRemetenteDto {
  emailOuDominio: string;
  nome?: string;
}

export interface AtualizarRemetenteDto {
  emailOuDominio: string;
  nome?: string;
  ativo: boolean;
}

// Usuários
export interface ConfiguracaoEmail {
  id: number;
  host: string;
  porta: number;
  usarSsl: boolean;
  emailUsuario: string;
  pasta: string;
  intervaloMinutos: number;
  dataCriacao: string;
  dataAtualizacao: string | null;
}

export interface Usuario {
  id: number;
  nome: string;
  email: string;
  ativo: boolean;
  dataCriacao: string;
  dataAtualizacao: string | null;
  configuracaoEmail: ConfiguracaoEmail | null;
}

export interface AtualizarUsuarioDto {
  nome: string;
  email: string;
}

export interface AlterarSenhaDto {
  senhaAtual: string;
  novaSenha: string;
}

export interface SalvarConfiguracaoEmailDto {
  host: string;
  porta: number;
  usarSsl: boolean;
  emailUsuario: string;
  senha: string;
  pasta: string;
  intervaloMinutos: number;
}
