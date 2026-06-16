'use client';

import { useEffect, useState } from 'react';
import { X, Download, Paperclip, Clock, User, Tag } from 'lucide-react';
import { EmailKanbanDetalhe, STATUS_LABELS } from '@/types';
import { api } from '@/services/api';

interface Props {
  emailId: number;
  onClose: () => void;
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

function formatarTamanho(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function EmailDetailModal({ emailId, onClose }: Props) {
  const [detalhe, setDetalhe] = useState<EmailKanbanDetalhe | null>(null);
  const [loading, setLoading] = useState(true);
  const [abaAtiva, setAbaAtiva] = useState<'corpo' | 'historico'>('corpo');

  useEffect(() => {
    api
      .obterDetalhe(emailId)
      .then(setDetalhe)
      .finally(() => setLoading(false));
  }, [emailId]);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onClose]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-start justify-between p-4 border-b">
          <div className="flex-1 min-w-0">
            {loading ? (
              <div className="h-5 bg-gray-200 rounded animate-pulse w-3/4" />
            ) : (
              <h2 className="text-base font-semibold text-gray-800 truncate">
                {detalhe?.assunto ?? '(sem assunto)'}
              </h2>
            )}
          </div>
          <button
            onClick={onClose}
            className="ml-3 p-1 rounded-full hover:bg-gray-100 text-gray-500 shrink-0"
          >
            <X size={18} />
          </button>
        </div>

        {loading ? (
          <div className="flex-1 flex items-center justify-center p-8">
            <div className="text-gray-400 text-sm">Carregando...</div>
          </div>
        ) : detalhe ? (
          <>
            {/* Metadados */}
            <div className="px-4 py-3 bg-gray-50 border-b flex flex-wrap gap-4 text-xs text-gray-600">
              <div className="flex items-center gap-1">
                <User size={12} />
                <span>{detalhe.remetente}</span>
              </div>
              <div className="flex items-center gap-1">
                <Clock size={12} />
                <span>{formatarData(detalhe.dataRecebimento)}</span>
              </div>
              <div className="flex items-center gap-1">
                <Tag size={12} />
                <span className="font-medium">
                  {STATUS_LABELS[detalhe.status]}
                </span>
              </div>
              {detalhe.categoria && (
                <span className="bg-gray-200 px-2 py-0.5 rounded-full">
                  {detalhe.categoria}
                </span>
              )}
            </div>

            {/* Abas */}
            <div className="flex border-b px-4">
              {(['corpo', 'historico'] as const).map((aba) => (
                <button
                  key={aba}
                  onClick={() => setAbaAtiva(aba)}
                  className={`px-3 py-2 text-sm font-medium border-b-2 transition-colors ${
                    abaAtiva === aba
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {aba === 'corpo' ? 'Conteúdo' : 'Histórico'}
                </button>
              ))}
            </div>

            {/* Conteúdo das abas */}
            <div className="flex-1 overflow-y-auto">
              {abaAtiva === 'corpo' && (
                <div className="p-4">
                  {/* Resumo */}
                  {detalhe.resumo && (
                    <div className="mb-4 p-3 bg-blue-50 rounded-lg text-sm text-gray-700 border border-blue-100">
                      <p className="font-medium text-blue-700 text-xs mb-1">Resumo</p>
                      {detalhe.resumo}
                    </div>
                  )}

                  {/* Corpo HTML ou texto */}
                  {detalhe.corpoHtml ? (
                    <div
                      className="prose prose-sm max-w-none text-gray-700 text-sm"
                      dangerouslySetInnerHTML={{ __html: detalhe.corpoHtml }}
                    />
                  ) : detalhe.corpoTexto ? (
                    <pre className="whitespace-pre-wrap text-sm text-gray-700 font-sans">
                      {detalhe.corpoTexto}
                    </pre>
                  ) : (
                    <p className="text-gray-400 text-sm italic">Sem conteúdo.</p>
                  )}
                </div>
              )}

              {abaAtiva === 'historico' && (
                <div className="p-4">
                  {detalhe.historico.length === 0 ? (
                    <p className="text-gray-400 text-sm italic">Sem movimentações registradas.</p>
                  ) : (
                    <ol className="relative border-l border-gray-200 ml-3 space-y-4">
                      {detalhe.historico.map((h) => (
                        <li key={h.id} className="ml-4">
                          <div className="absolute -left-1.5 w-3 h-3 rounded-full bg-blue-400 border-2 border-white" />
                          <p className="text-xs text-gray-500">{formatarData(h.dataMovimento)}</p>
                          <p className="text-sm text-gray-700 font-medium">
                            {h.statusAnterior
                              ? `${STATUS_LABELS[h.statusAnterior as keyof typeof STATUS_LABELS] ?? h.statusAnterior} → ${STATUS_LABELS[h.statusNovo as keyof typeof STATUS_LABELS] ?? h.statusNovo}`
                              : `Criado como ${STATUS_LABELS[h.statusNovo as keyof typeof STATUS_LABELS] ?? h.statusNovo}`}
                          </p>
                          {h.observacao && (
                            <p className="text-xs text-gray-500 mt-0.5">{h.observacao}</p>
                          )}
                        </li>
                      ))}
                    </ol>
                  )}
                </div>
              )}
            </div>

            {/* Anexos */}
            {detalhe.anexos.length > 0 && (
              <div className="border-t p-4">
                <p className="text-xs font-semibold text-gray-500 uppercase mb-2 flex items-center gap-1">
                  <Paperclip size={12} /> Anexos ({detalhe.anexos.length})
                </p>
                <div className="flex flex-wrap gap-2">
                  {detalhe.anexos.map((a) => (
                    <a
                      key={a.id}
                      href={api.downloadAnexoUrl(a.id)}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex items-center gap-2 bg-gray-100 hover:bg-gray-200 rounded-lg px-3 py-2 text-xs text-gray-700 transition-colors"
                    >
                      <Download size={12} />
                      <span className="max-w-[160px] truncate">{a.nomeArquivo}</span>
                      <span className="text-gray-400 shrink-0">
                        {formatarTamanho(a.tamanhoBytes)}
                      </span>
                    </a>
                  ))}
                </div>
              </div>
            )}
          </>
        ) : (
          <div className="p-8 text-center text-gray-400 text-sm">
            E-mail não encontrado.
          </div>
        )}
      </div>
    </div>
  );
}
