'use client';

import { useEffect, useState } from 'react';
import { Plus, Pencil, Trash2, CheckCircle, XCircle, Mail } from 'lucide-react';
import { RemetenteMonitorado, CriarRemetenteDto, AtualizarRemetenteDto } from '@/types';
import { api } from '@/services/api';
import Modal from '@/components/Modal';
import ConfirmDialog from '@/components/ConfirmDialog';

interface FormState {
  emailOuDominio: string;
  nome: string;
  ativo: boolean;
}

const FORM_VAZIO: FormState = { emailOuDominio: '', nome: '', ativo: true };

export default function RemetentesPage() {
  const [remetentes, setRemetentes] = useState<RemetenteMonitorado[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);
  const [excluindo, setExcluindo] = useState(false);

  const [modalAberto, setModalAberto] = useState(false);
  const [editando, setEditando] = useState<RemetenteMonitorado | null>(null);
  const [form, setForm] = useState<FormState>(FORM_VAZIO);
  const [erroForm, setErroForm] = useState<string | null>(null);

  const [confirmarExclusao, setConfirmarExclusao] = useState<RemetenteMonitorado | null>(null);

  const carregar = async () => {
    try {
      const dados = await api.listarRemetentes();
      setRemetentes(dados);
    } catch (e: unknown) {
      setErro(e instanceof Error ? e.message : 'Erro ao carregar remetentes.');
    } finally {
      setCarregando(false);
    }
  };

  useEffect(() => { carregar(); }, []);

  const abrirCriar = () => {
    setEditando(null);
    setForm(FORM_VAZIO);
    setErroForm(null);
    setModalAberto(true);
  };

  const abrirEditar = (r: RemetenteMonitorado) => {
    setEditando(r);
    setForm({ emailOuDominio: r.emailOuDominio, nome: r.nome ?? '', ativo: r.ativo });
    setErroForm(null);
    setModalAberto(true);
  };

  const fecharModal = () => {
    setModalAberto(false);
    setEditando(null);
    setForm(FORM_VAZIO);
    setErroForm(null);
  };

  const salvar = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.emailOuDominio.trim()) {
      setErroForm('E-mail ou domínio é obrigatório.');
      return;
    }
    setSalvando(true);
    setErroForm(null);
    try {
      if (editando) {
        const dto: AtualizarRemetenteDto = {
          emailOuDominio: form.emailOuDominio.trim(),
          nome: form.nome.trim() || undefined,
          ativo: form.ativo,
        };
        await api.atualizarRemetente(editando.id, dto);
      } else {
        const dto: CriarRemetenteDto = {
          emailOuDominio: form.emailOuDominio.trim(),
          nome: form.nome.trim() || undefined,
        };
        await api.criarRemetente(dto);
      }
      fecharModal();
      await carregar();
    } catch (e: unknown) {
      setErroForm(e instanceof Error ? e.message : 'Erro ao salvar.');
    } finally {
      setSalvando(false);
    }
  };

  const excluir = async () => {
    if (!confirmarExclusao) return;
    setExcluindo(true);
    try {
      await api.excluirRemetente(confirmarExclusao.id);
      setConfirmarExclusao(null);
      await carregar();
    } catch (e: unknown) {
      setErro(e instanceof Error ? e.message : 'Erro ao excluir.');
      setConfirmarExclusao(null);
    } finally {
      setExcluindo(false);
    }
  };

  return (
    <>
      <div className="max-w-3xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-xl font-bold text-gray-800">Remetentes Monitorados</h1>
            <p className="text-sm text-gray-500 mt-0.5">E-mails ou domínios que serão capturados pelo sistema.</p>
          </div>
          <button
            onClick={abrirCriar}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Plus size={15} /> Adicionar
          </button>
        </div>

        {erro && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{erro}</div>
        )}

        {carregando ? (
          <div className="text-center py-12 text-gray-400">Carregando...</div>
        ) : remetentes.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <Mail size={36} className="mx-auto mb-3 opacity-30" />
            <p>Nenhum remetente cadastrado.</p>
          </div>
        ) : (
          <div className="bg-white rounded-xl border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-500">E-mail / Domínio</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-500">Nome</th>
                  <th className="text-center px-4 py-3 font-medium text-gray-500">Status</th>
                  <th className="text-right px-4 py-3 font-medium text-gray-500">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {remetentes.map((r) => (
                  <tr key={r.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-mono text-xs text-gray-700">{r.emailOuDominio}</td>
                    <td className="px-4 py-3 text-gray-600">{r.nome ?? <span className="text-gray-300">—</span>}</td>
                    <td className="px-4 py-3 text-center">
                      {r.ativo ? (
                        <span className="inline-flex items-center gap-1 text-green-700 bg-green-50 px-2 py-0.5 rounded-full text-xs">
                          <CheckCircle size={11} /> Ativo
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 text-gray-400 bg-gray-100 px-2 py-0.5 rounded-full text-xs">
                          <XCircle size={11} /> Inativo
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => abrirEditar(r)}
                          className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="Editar"
                        >
                          <Pencil size={14} />
                        </button>
                        <button
                          onClick={() => setConfirmarExclusao(r)}
                          className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          title="Excluir"
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {modalAberto && (
        <Modal titulo={editando ? 'Editar Remetente' : 'Novo Remetente'} onClose={fecharModal}>
          <form onSubmit={salvar} className="flex flex-col gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">
                E-mail ou Domínio <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={form.emailOuDominio}
                onChange={(e) => setForm((f) => ({ ...f, emailOuDominio: e.target.value }))}
                placeholder="exemplo@empresa.com ou @empresa.com"
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-300"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Nome (opcional)</label>
              <input
                type="text"
                value={form.nome}
                onChange={(e) => setForm((f) => ({ ...f, nome: e.target.value }))}
                placeholder="Identificação amigável"
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-300"
              />
            </div>
            {editando && (
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="ativo"
                  checked={form.ativo}
                  onChange={(e) => setForm((f) => ({ ...f, ativo: e.target.checked }))}
                  className="w-4 h-4 accent-blue-600"
                />
                <label htmlFor="ativo" className="text-sm text-gray-600">Ativo</label>
              </div>
            )}
            {erroForm && (
              <p className="text-sm text-red-600 bg-red-50 px-3 py-2 rounded-lg">{erroForm}</p>
            )}
            <div className="flex justify-end gap-2 pt-1">
              <button
                type="button"
                onClick={fecharModal}
                className="px-4 py-2 text-sm text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={salvando}
                className="px-4 py-2 text-sm text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors disabled:opacity-50"
              >
                {salvando ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {confirmarExclusao && (
        <ConfirmDialog
          mensagem={`Deseja excluir o remetente "${confirmarExclusao.emailOuDominio}"? Esta ação não pode ser desfeita.`}
          onConfirmar={excluir}
          onCancelar={() => setConfirmarExclusao(null)}
          carregando={excluindo}
        />
      )}
    </>
  );
}
