'use client';

import { AlertTriangle } from 'lucide-react';

interface ConfirmDialogProps {
  mensagem: string;
  onConfirmar: () => void;
  onCancelar: () => void;
  carregando?: boolean;
}

export default function ConfirmDialog({ mensagem, onConfirmar, onCancelar, carregando }: ConfirmDialogProps) {
  return (
    <div className="fixed inset-0 z-60 flex items-center justify-center bg-black/50 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6 flex flex-col gap-4">
        <div className="flex items-start gap-3">
          <div className="p-2 bg-red-100 rounded-full shrink-0">
            <AlertTriangle size={18} className="text-red-600" />
          </div>
          <p className="text-sm text-gray-700 pt-1">{mensagem}</p>
        </div>
        <div className="flex justify-end gap-2">
          <button
            onClick={onCancelar}
            disabled={carregando}
            className="px-4 py-2 text-sm text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            onClick={onConfirmar}
            disabled={carregando}
            className="px-4 py-2 text-sm text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors disabled:opacity-50"
          >
            {carregando ? 'Excluindo...' : 'Confirmar'}
          </button>
        </div>
      </div>
    </div>
  );
}
