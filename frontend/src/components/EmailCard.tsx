'use client';

import { useState, useRef, useEffect } from 'react';
import { Draggable } from '@hello-pangea/dnd';
import { Paperclip, User, Calendar, ArrowRightLeft } from 'lucide-react';
import { EmailKanban, StatusKanban, STATUS_LABELS, COLUNAS } from '@/types';

interface Props {
  email: EmailKanban;
  index: number;
  onClick: (email: EmailKanban) => void;
  onStatusChange: (email: EmailKanban, newStatus: StatusKanban) => void;
}

function formatarData(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function EmailCard({ email, index, onClick, onStatusChange }: Props) {
  const [menuAberto, setMenuAberto] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!menuAberto) return;
    function fechar(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuAberto(false);
      }
    }
    document.addEventListener('mousedown', fechar);
    return () => document.removeEventListener('mousedown', fechar);
  }, [menuAberto]);

  const outrosColunas = COLUNAS.filter((s) => s !== email.status);

  return (
    <Draggable draggableId={String(email.id)} index={index}>
      {(provided, snapshot) => (
        <div
          ref={provided.innerRef}
          {...provided.draggableProps}
          {...provided.dragHandleProps}
          onClick={() => onClick(email)}
          className={`
            bg-white rounded-lg border p-3 shadow-sm cursor-pointer
            hover:shadow-md transition-shadow select-none
            ${snapshot.isDragging ? 'shadow-lg ring-2 ring-blue-400 rotate-1' : ''}
          `}
        >
          {/* Assunto */}
          <h3 className="text-sm font-semibold text-gray-800 line-clamp-2 mb-2">
            {email.assunto ?? '(sem assunto)'}
          </h3>

          {/* Resumo */}
          {email.resumo && (
            <p className="text-xs text-gray-500 line-clamp-2 mb-2">
              {email.resumo}
            </p>
          )}

          {/* Remetente */}
          <div className="flex items-center gap-1 text-xs text-gray-500 mb-1">
            <User size={11} className="shrink-0" />
            <span className="truncate">{email.remetente}</span>
          </div>

          {/* Data */}
          <div className="flex items-center gap-1 text-xs text-gray-400 mb-2">
            <Calendar size={11} className="shrink-0" />
            <span>{formatarData(email.dataRecebimento)}</span>
          </div>

          {/* Rodapé */}
          <div className="flex items-center justify-between mt-1">
            {email.categoria && (
              <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                {email.categoria}
              </span>
            )}
            <div className="flex items-center gap-2 ml-auto">
              {email.quantidadeAnexos > 0 && (
                <div className="flex items-center gap-1 text-xs text-gray-500">
                  <Paperclip size={11} />
                  <span>{email.quantidadeAnexos}</span>
                </div>
              )}

              {/* Botão mover para */}
              <div ref={menuRef} className="relative" onClick={(e) => e.stopPropagation()}>
                <button
                  type="button"
                  title="Mover para..."
                  onClick={() => setMenuAberto((v) => !v)}
                  className="flex items-center gap-0.5 text-xs text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded px-1 py-0.5 transition-colors"
                >
                  <ArrowRightLeft size={11} />
                </button>

                {menuAberto && (
                  <div className="absolute right-0 bottom-6 z-50 bg-white border border-gray-200 rounded-lg shadow-lg py-1 min-w-36">
                    <p className="text-xs text-gray-400 px-3 py-1 font-medium border-b border-gray-100">
                      Mover para...
                    </p>
                    {outrosColunas.map((s) => (
                      <button
                        key={s}
                        type="button"
                        onClick={() => {
                          setMenuAberto(false);
                          onStatusChange(email, s);
                        }}
                        className="w-full text-left text-xs px-3 py-1.5 hover:bg-blue-50 hover:text-blue-700 transition-colors"
                      >
                        {STATUS_LABELS[s]}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </Draggable>
  );
}
