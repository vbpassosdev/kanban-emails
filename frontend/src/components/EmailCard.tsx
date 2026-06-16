'use client';

import { Draggable } from '@hello-pangea/dnd';
import { Paperclip, User, Calendar } from 'lucide-react';
import { EmailKanban } from '@/types';

interface Props {
  email: EmailKanban;
  index: number;
  onClick: (email: EmailKanban) => void;
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

export default function EmailCard({ email, index, onClick }: Props) {
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
            {email.quantidadeAnexos > 0 && (
              <div className="flex items-center gap-1 text-xs text-gray-500 ml-auto">
                <Paperclip size={11} />
                <span>{email.quantidadeAnexos}</span>
              </div>
            )}
          </div>
        </div>
      )}
    </Draggable>
  );
}
