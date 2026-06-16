'use client';

import { useEffect, useState } from 'react';
import { Users, CheckCircle, XCircle } from 'lucide-react';
import { Usuario } from '@/types';
import { api } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

export default function UsuariosPage() {
  const { user: authUser } = useAuth();
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [toggling, setToggling] = useState<number | null>(null);

  const carregar = async () => {
    try {
      const dados = await api.listarUsuarios();
      setUsuarios(dados);
    } catch (e: unknown) {
      setErro(e instanceof Error ? e.message : 'Erro ao carregar usuários.');
    } finally {
      setCarregando(false);
    }
  };

  useEffect(() => { carregar(); }, []);

  const toggleAtivo = async (u: Usuario) => {
    setToggling(u.id);
    try {
      await api.toggleAtivo(u.id, !u.ativo);
      setUsuarios((prev) => prev.map((x) => x.id === u.id ? { ...x, ativo: !u.ativo } : x));
    } catch (e: unknown) {
      setErro(e instanceof Error ? e.message : 'Erro ao alterar status.');
    } finally {
      setToggling(null);
    }
  };

  const formatarData = (iso: string) =>
    new Date(iso).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });

  return (
    <div className="max-w-3xl mx-auto">
      <div className="mb-6">
        <h1 className="text-xl font-bold text-gray-800">Usuários</h1>
        <p className="text-sm text-gray-500 mt-0.5">Gerenciar usuários do sistema.</p>
      </div>

      {erro && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{erro}</div>
      )}

      {carregando ? (
        <div className="text-center py-12 text-gray-400">Carregando...</div>
      ) : usuarios.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          <Users size={36} className="mx-auto mb-3 opacity-30" />
          <p>Nenhum usuário cadastrado.</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Nome</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">E-mail</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Cadastro</th>
                <th className="text-center px-4 py-3 font-medium text-gray-500">IMAP</th>
                <th className="text-center px-4 py-3 font-medium text-gray-500">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {usuarios.map((u) => {
                const isSelf = authUser?.id === u.id;
                return (
                  <tr key={u.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-800">
                      {u.nome}
                      {isSelf && (
                        <span className="ml-2 text-xs text-blue-500 bg-blue-50 px-1.5 py-0.5 rounded-full">Você</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-600">{u.email}</td>
                    <td className="px-4 py-3 text-gray-400 text-xs">{formatarData(u.dataCriacao)}</td>
                    <td className="px-4 py-3 text-center">
                      {u.configuracaoEmail ? (
                        <span className="text-xs text-green-600" title={u.configuracaoEmail.emailUsuario}>✓ Configurado</span>
                      ) : (
                        <span className="text-xs text-gray-300">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-center">
                      <button
                        onClick={() => !isSelf && toggleAtivo(u)}
                        disabled={isSelf || toggling === u.id}
                        title={isSelf ? 'Não é possível inativar a própria conta' : u.ativo ? 'Desativar' : 'Ativar'}
                        className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                          isSelf
                            ? 'cursor-default'
                            : 'cursor-pointer hover:opacity-80'
                        } ${
                          u.ativo
                            ? 'bg-green-50 text-green-700'
                            : 'bg-gray-100 text-gray-400'
                        } disabled:opacity-40`}
                      >
                        {toggling === u.id ? (
                          <span className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin" />
                        ) : u.ativo ? (
                          <CheckCircle size={11} />
                        ) : (
                          <XCircle size={11} />
                        )}
                        {u.ativo ? 'Ativo' : 'Inativo'}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
