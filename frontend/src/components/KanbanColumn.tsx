'use client';

import { useState } from 'react';
import { Droppable } from '@hello-pangea/dnd';
import { Building2, ChevronDown, ChevronRight, Search, X } from 'lucide-react';
import { EmailKanban, StatusKanban, STATUS_LABELS, STATUS_HEADER_COLORS } from '@/types';
import EmailCard from './EmailCard';

interface Props {
  status: StatusKanban;
  emails: EmailKanban[];
  onCardClick: (email: EmailKanban) => void;
  onStatusChange: (email: EmailKanban, newStatus: StatusKanban) => void;
}

function extrairEmpresa(assunto: string | null, remetente: string): string {
  if (assunto) {
    const partes = assunto.split('|');
    if (partes.length > 1) {
      const primeiroToken = partes[0].trim().split(/\s+/)[0];
      if (primeiroToken) return primeiroToken;
    }
  }
  const match = remetente.match(/@([^>]+)$/);
  if (!match) return remetente;
  const dominio = match[1].toLowerCase();
  return dominio.replace(/^(mail|smtp|noreply|no-reply)\./i, '').split('.')[0];
}

function agruparPorEmpresa(emails: EmailKanban[]): [string, EmailKanban[]][] {
  const mapa = new Map<string, EmailKanban[]>();
  for (const email of emails) {
    const empresa = extrairEmpresa(email.assunto, email.remetente);
    const grupo = mapa.get(empresa) ?? [];
    grupo.push(email);
    mapa.set(empresa, grupo);
  }
  return Array.from(mapa.entries()).sort((a, b) => b[1].length - a[1].length);
}

export default function KanbanColumn({ status, emails, onCardClick, onStatusChange }: Props) {
  const agrupar = status === 'Novo' || status === 'EIdSocketError';
  const [busca, setBusca] = useState('');
  const [colapsados, setColapsados] = useState<Set<string>>(new Set());

  const toggleGrupo = (empresa: string) => {
    setColapsados((prev) => {
      const novo = new Set(prev);
      if (novo.has(empresa)) novo.delete(empresa);
      else novo.add(empresa);
      return novo;
    });
  };

  const grupos = agrupar ? agruparPorEmpresa(emails) : null;

  const gruposFiltrados = grupos
    ? grupos.filter(([empresa]) =>
        !busca || empresa.toLowerCase().includes(busca.toLowerCase())
      )
    : null;

  let globalIndex = 0;

  return (
    <div className="flex flex-col w-64 shrink-0 bg-gray-100 rounded-xl overflow-hidden">
      {/* Header da coluna */}
      <div className={`${STATUS_HEADER_COLORS[status]} px-3 py-2`}>
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold text-white">
            {STATUS_LABELS[status]}
          </span>
          <span className="text-xs bg-white bg-opacity-30 text-white px-2 py-0.5 rounded-full font-medium">
            {emails.length}
          </span>
        </div>

        {/* Busca — só na coluna Novo */}
        {agrupar && (
          <div className="mt-2 relative">
            <Search
              size={11}
              className="absolute left-2 top-1/2 -translate-y-1/2 text-white/60 pointer-events-none"
            />
            <input
              type="text"
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="Filtrar empresa..."
              className="w-full pl-6 pr-6 py-1 text-xs rounded-md bg-white/20 text-white placeholder:text-white/60 focus:outline-none focus:bg-white/30"
            />
            {busca && (
              <button
                type="button"
                onClick={() => setBusca('')}
                className="absolute right-2 top-1/2 -translate-y-1/2 text-white/70 hover:text-white"
              >
                <X size={11} />
              </button>
            )}
          </div>
        )}
      </div>

      {/* Área de drop */}
      <Droppable droppableId={status}>
        {(provided, snapshot) => (
          <div
            ref={provided.innerRef}
            {...provided.droppableProps}
            className={`
              flex-1 p-2 flex flex-col gap-2 min-h-24 overflow-y-auto max-h-[calc(100vh-210px)]
              ${snapshot.isDraggingOver ? 'bg-blue-50' : ''}
              transition-colors
            `}
          >
            {agrupar && gruposFiltrados ? (
              <>
                {gruposFiltrados.length === 0 ? (
                  <p className="text-xs text-gray-400 text-center py-4">
                    Nenhuma empresa encontrada.
                  </p>
                ) : (
                  gruposFiltrados.map(([empresa, grupoEmails]) => {
                    const colapsado = colapsados.has(empresa);
                    return (
                      <div key={empresa}>
                        {/* Cabeçalho de empresa clicável */}
                        <button
                          type="button"
                          onClick={() => toggleGrupo(empresa)}
                          className="w-full flex items-center gap-1 px-1 py-1 mb-1 rounded hover:bg-gray-200 transition-colors"
                        >
                          {colapsado ? (
                            <ChevronRight size={11} className="text-gray-400 shrink-0" />
                          ) : (
                            <ChevronDown size={11} className="text-gray-400 shrink-0" />
                          )}
                          <Building2 size={11} className="text-gray-400 shrink-0" />
                          <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide truncate">
                            {empresa}
                          </span>
                          <span className="ml-auto text-xs text-gray-400 shrink-0">
                            {grupoEmails.length}
                          </span>
                        </button>

                        {!colapsado &&
                          grupoEmails.map((email) => {
                            const idx = globalIndex++;
                            return (
                              <EmailCard
                                key={email.id}
                                email={email}
                                index={idx}
                                onClick={onCardClick}
                                onStatusChange={onStatusChange}
                              />
                            );
                          })}
                      </div>
                    );
                  })
                )}
              </>
            ) : (
              emails.map((email, index) => (
                <EmailCard
                  key={email.id}
                  email={email}
                  index={index}
                  onClick={onCardClick}
                  onStatusChange={onStatusChange}
                />
              ))
            )}

            {provided.placeholder}

            {emails.length === 0 && !snapshot.isDraggingOver && (
              <div className="text-xs text-gray-400 text-center py-4">
                Nenhum e-mail
              </div>
            )}
          </div>
        )}
      </Droppable>
    </div>
  );
}
