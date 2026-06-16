'use client';

import { useState, useCallback } from 'react';
import { DragDropContext, DropResult } from '@hello-pangea/dnd';
import { RefreshCw, Search, X } from 'lucide-react';
import {
  EmailKanban,
  FiltroEmails,
  StatusKanban,
  COLUNAS,
  STATUS_LABELS,
} from '@/types';
import { api } from '@/services/api';
import KanbanColumn from './KanbanColumn';
import EmailDetailModal from './EmailDetailModal';

interface Props {
  emailsIniciais: EmailKanban[];
}

export default function KanbanBoard({ emailsIniciais }: Props) {
  const [emails, setEmails] = useState<EmailKanban[]>(emailsIniciais);
  const [emailSelecionado, setEmailSelecionado] = useState<number | null>(null);
  const [sincronizando, setSincronizando] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [filtro, setFiltro] = useState<FiltroEmails>({});
  const [filtroVisivel, setFiltroVisivel] = useState(false);
  const [mensagem, setMensagem] = useState<string | null>(null);

  const emailsPorColuna = useCallback(
    (status: StatusKanban) => emails.filter((e) => e.status === status),
    [emails]
  );

  const recarregar = useCallback(async (f?: FiltroEmails) => {
    setCarregando(true);
    try {
      const dados = await api.listarEmails(f ?? filtro);
      setEmails(dados);
    } finally {
      setCarregando(false);
    }
  }, [filtro]);

  const sincronizar = async () => {
    setSincronizando(true);
    try {
      const res = await api.sincronizar();
      setMensagem(res.mensagem);
      await recarregar();
      setTimeout(() => setMensagem(null), 4000);
    } catch {
      setMensagem('Erro ao sincronizar e-mails.');
      setTimeout(() => setMensagem(null), 4000);
    } finally {
      setSincronizando(false);
    }
  };

  const onDragEnd = async (result: DropResult) => {
    const { draggableId, destination } = result;
    if (!destination) return;

    const novoStatus = destination.droppableId as StatusKanban;
    const id = parseInt(draggableId, 10);
    const email = emails.find((e) => e.id === id);
    if (!email || email.status === novoStatus) return;

    // Atualização otimista
    setEmails((prev) =>
      prev.map((e) => (e.id === id ? { ...e, status: novoStatus } : e))
    );

    try {
      await api.alterarStatus(id, novoStatus);
    } catch {
      // Reverte em caso de erro
      setEmails((prev) =>
        prev.map((e) => (e.id === id ? { ...e, status: email.status } : e))
      );
    }
  };

  const aplicarFiltro = async (e: React.FormEvent) => {
    e.preventDefault();
    await recarregar(filtro);
    setFiltroVisivel(false);
  };

  const limparFiltro = async () => {
    const novoFiltro: FiltroEmails = {};
    setFiltro(novoFiltro);
    await recarregar(novoFiltro);
  };

  const temFiltroAtivo = Object.values(filtro).some(Boolean);

  return (
    <div className="flex flex-col h-screen bg-gray-50">
      {/* Topbar */}
      <header className="bg-white border-b shadow-sm px-4 py-3 flex items-center gap-3">
        <h1 className="text-lg font-bold text-gray-800 shrink-0">
          📧 Kanban de E-mails
        </h1>

        <div className="flex-1" />

        {/* Toast de mensagem */}
        {mensagem && (
          <span className="text-sm text-green-700 bg-green-50 border border-green-200 rounded-lg px-3 py-1">
            {mensagem}
          </span>
        )}

        <button
          onClick={() => setFiltroVisivel((v) => !v)}
          className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
            temFiltroAtivo
              ? 'bg-blue-100 text-blue-700 hover:bg-blue-200'
              : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
          }`}
        >
          <Search size={14} />
          Filtrar
          {temFiltroAtivo && (
            <span className="bg-blue-500 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center">
              •
            </span>
          )}
        </button>

        <button
          onClick={sincronizar}
          disabled={sincronizando}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white rounded-lg text-sm font-medium transition-colors"
        >
          <RefreshCw size={14} className={sincronizando ? 'animate-spin' : ''} />
          {sincronizando ? 'Sincronizando...' : 'Sincronizar'}
        </button>
      </header>

      {/* Painel de filtros */}
      {filtroVisivel && (
        <div className="bg-white border-b px-4 py-3 shadow-sm">
          <form onSubmit={aplicarFiltro} className="flex flex-wrap items-end gap-3">
            <div className="flex flex-col gap-1">
              <label className="text-xs text-gray-500">Remetente</label>
              <input
                type="text"
                value={filtro.remetente ?? ''}
                onChange={(e) => setFiltro((f) => ({ ...f, remetente: e.target.value }))}
                className="border rounded-lg px-2 py-1.5 text-sm w-48 focus:outline-none focus:ring-2 focus:ring-blue-300"
                placeholder="email@exemplo.com"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs text-gray-500">Assunto</label>
              <input
                type="text"
                value={filtro.assunto ?? ''}
                onChange={(e) => setFiltro((f) => ({ ...f, assunto: e.target.value }))}
                className="border rounded-lg px-2 py-1.5 text-sm w-48 focus:outline-none focus:ring-2 focus:ring-blue-300"
                placeholder="Palavra-chave"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs text-gray-500">Status</label>
              <select
                value={filtro.status ?? ''}
                onChange={(e) =>
                  setFiltro((f) => ({
                    ...f,
                    status: e.target.value as StatusKanban | '',
                  }))
                }
                className="border rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-300"
              >
                <option value="">Todos</option>
                {COLUNAS.map((s) => (
                  <option key={s} value={s}>
                    {STATUS_LABELS[s]}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs text-gray-500">De</label>
              <input
                type="date"
                value={filtro.dataInicio ?? ''}
                onChange={(e) => setFiltro((f) => ({ ...f, dataInicio: e.target.value }))}
                className="border rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-300"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs text-gray-500">Até</label>
              <input
                type="date"
                value={filtro.dataFim ?? ''}
                onChange={(e) => setFiltro((f) => ({ ...f, dataFim: e.target.value }))}
                className="border rounded-lg px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-300"
              />
            </div>
            <div className="flex gap-2">
              <button
                type="submit"
                className="px-3 py-1.5 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700"
              >
                Aplicar
              </button>
              {temFiltroAtivo && (
                <button
                  type="button"
                  onClick={limparFiltro}
                  className="flex items-center gap-1 px-3 py-1.5 bg-gray-100 text-gray-600 rounded-lg text-sm hover:bg-gray-200"
                >
                  <X size={12} /> Limpar
                </button>
              )}
            </div>
          </form>
        </div>
      )}

      {/* Board */}
      <div className="flex-1 overflow-x-auto p-4">
        {carregando ? (
          <div className="flex items-center justify-center h-full text-gray-400">
            Carregando...
          </div>
        ) : (
          <DragDropContext onDragEnd={onDragEnd}>
            <div className="flex gap-4 h-full">
              {COLUNAS.map((status) => (
                <KanbanColumn
                  key={status}
                  status={status}
                  emails={emailsPorColuna(status)}
                  onCardClick={(e) => setEmailSelecionado(e.id)}
                />
              ))}
            </div>
          </DragDropContext>
        )}
      </div>

      {/* Modal de detalhe */}
      {emailSelecionado !== null && (
        <EmailDetailModal
          emailId={emailSelecionado}
          onClose={() => setEmailSelecionado(null)}
        />
      )}
    </div>
  );
}
