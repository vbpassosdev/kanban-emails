'use client';

import { Droppable } from '@hello-pangea/dnd';
import { EmailKanban, StatusKanban, STATUS_LABELS, STATUS_HEADER_COLORS } from '@/types';
import EmailCard from './EmailCard';

interface Props {
  status: StatusKanban;
  emails: EmailKanban[];
  onCardClick: (email: EmailKanban) => void;
}

export default function KanbanColumn({ status, emails, onCardClick }: Props) {
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
      </div>

      {/* Área de drop */}
      <Droppable droppableId={status}>
        {(provided, snapshot) => (
          <div
            ref={provided.innerRef}
            {...provided.droppableProps}
            className={`
              flex-1 p-2 flex flex-col gap-2 min-h-24 overflow-y-auto max-h-[calc(100vh-180px)]
              ${snapshot.isDraggingOver ? 'bg-blue-50' : ''}
              transition-colors
            `}
          >
            {emails.map((email, index) => (
              <EmailCard
                key={email.id}
                email={email}
                index={index}
                onClick={onCardClick}
              />
            ))}
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
