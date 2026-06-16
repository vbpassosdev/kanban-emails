'use client';

import { useEffect, useState } from 'react';
import { User, Lock, Mail } from 'lucide-react';
import { api } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import { AtualizarUsuarioDto, AlterarSenhaDto, SalvarConfiguracaoEmailDto, Usuario } from '@/types';

type Aba = 'dados' | 'senha' | 'imap';

const PROVEDORES = [
  { label: 'Gmail', host: 'imap.gmail.com', porta: 993, ssl: true },
  { label: 'Outlook / Hotmail', host: 'outlook.office365.com', porta: 993, ssl: true },
  { label: 'Hostinger', host: 'imap.hostinger.com', porta: 993, ssl: true },
  { label: 'Personalizado', host: '', porta: 993, ssl: true },
];

export default function PerfilPage() {
  const { user: authUser } = useAuth();
  const [perfil, setPerfil] = useState<Usuario | null>(null);
  const [aba, setAba] = useState<Aba>('dados');
  const [carregando, setCarregando] = useState(true);

  // Dados pessoais
  const [dados, setDados] = useState({ nome: '', email: '' });
  const [salvandoDados, setSalvandoDados] = useState(false);
  const [msgDados, setMsgDados] = useState<{ tipo: 'ok' | 'err'; texto: string } | null>(null);

  // Senha
  const [senha, setSenha] = useState({ senhaAtual: '', novaSenha: '', confirmar: '' });
  const [salvandoSenha, setSalvandoSenha] = useState(false);
  const [msgSenha, setMsgSenha] = useState<{ tipo: 'ok' | 'err'; texto: string } | null>(null);

  // IMAP
  const [imap, setImap] = useState<SalvarConfiguracaoEmailDto & { confirmarSenha: string }>({
    host: '', porta: 993, usarSsl: true, emailUsuario: '', senha: '', confirmarSenha: '', pasta: 'INBOX', intervaloMinutos: 5,
  });
  const [salvandoImap, setSalvandoImap] = useState(false);
  const [msgImap, setMsgImap] = useState<{ tipo: 'ok' | 'err'; texto: string } | null>(null);

  useEffect(() => {
    if (!authUser) return;
    api.obterPerfil(authUser.id).then((p) => {
      setPerfil(p);
      setDados({ nome: p.nome, email: p.email });
      if (p.configuracaoEmail) {
        const cfg = p.configuracaoEmail;
        setImap((prev) => ({
          ...prev,
          host: cfg.host,
          porta: cfg.porta,
          usarSsl: cfg.usarSsl,
          emailUsuario: cfg.emailUsuario,
          pasta: cfg.pasta,
          intervaloMinutos: cfg.intervaloMinutos,
        }));
      }
    }).catch(() => {}).finally(() => setCarregando(false));
  }, [authUser]);

  const salvarDados = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!authUser || !perfil) return;
    setSalvandoDados(true);
    setMsgDados(null);
    try {
      const dto: AtualizarUsuarioDto = { nome: dados.nome.trim(), email: dados.email.trim() };
      await api.atualizarUsuario(authUser.id, dto);
      setMsgDados({ tipo: 'ok', texto: 'Dados atualizados com sucesso.' });
    } catch (e: unknown) {
      setMsgDados({ tipo: 'err', texto: e instanceof Error ? e.message : 'Erro ao salvar.' });
    } finally {
      setSalvandoDados(false);
    }
  };

  const salvarSenha = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!authUser) return;
    if (senha.novaSenha !== senha.confirmar) {
      setMsgSenha({ tipo: 'err', texto: 'A nova senha e a confirmação não coincidem.' });
      return;
    }
    if (senha.novaSenha.length < 6) {
      setMsgSenha({ tipo: 'err', texto: 'A nova senha deve ter no mínimo 6 caracteres.' });
      return;
    }
    setSalvandoSenha(true);
    setMsgSenha(null);
    try {
      const dto: AlterarSenhaDto = { senhaAtual: senha.senhaAtual, novaSenha: senha.novaSenha };
      await api.alterarSenha(authUser.id, dto);
      setSenha({ senhaAtual: '', novaSenha: '', confirmar: '' });
      setMsgSenha({ tipo: 'ok', texto: 'Senha alterada com sucesso.' });
    } catch (e: unknown) {
      setMsgSenha({ tipo: 'err', texto: e instanceof Error ? e.message : 'Erro ao alterar senha.' });
    } finally {
      setSalvandoSenha(false);
    }
  };

  const salvarImap = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!authUser) return;
    if (imap.senha !== imap.confirmarSenha) {
      setMsgImap({ tipo: 'err', texto: 'As senhas não coincidem.' });
      return;
    }
    setSalvandoImap(true);
    setMsgImap(null);
    try {
      const dto: SalvarConfiguracaoEmailDto = {
        host: imap.host.trim(),
        porta: imap.porta,
        usarSsl: imap.usarSsl,
        emailUsuario: imap.emailUsuario.trim(),
        senha: imap.senha,
        pasta: imap.pasta.trim() || 'INBOX',
        intervaloMinutos: imap.intervaloMinutos,
      };
      await api.salvarConfiguracaoEmail(authUser.id, dto);
      setImap((prev) => ({ ...prev, senha: '', confirmarSenha: '' }));
      setMsgImap({ tipo: 'ok', texto: 'Configuração IMAP salva com sucesso.' });
    } catch (e: unknown) {
      setMsgImap({ tipo: 'err', texto: e instanceof Error ? e.message : 'Erro ao salvar configuração.' });
    } finally {
      setSalvandoImap(false);
    }
  };

  const aplicarProvedor = (idx: number) => {
    const p = PROVEDORES[idx];
    setImap((prev) => ({ ...prev, host: p.host, porta: p.porta, usarSsl: p.ssl }));
  };

  const ABAS: { id: Aba; label: string; icon: React.ElementType }[] = [
    { id: 'dados', label: 'Dados Pessoais', icon: User },
    { id: 'senha', label: 'Alterar Senha', icon: Lock },
    { id: 'imap', label: 'Configuração IMAP', icon: Mail },
  ];

  const inputCls = 'w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-300';
  const labelCls = 'block text-xs font-medium text-gray-600 mb-1';

  const Msg = ({ m }: { m: { tipo: 'ok' | 'err'; texto: string } | null }) =>
    m ? (
      <p className={`text-sm px-3 py-2 rounded-lg ${m.tipo === 'ok' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}>
        {m.texto}
      </p>
    ) : null;

  return (
    <div className="max-w-xl mx-auto">
      <h1 className="text-xl font-bold text-gray-800 mb-6">Meu Perfil</h1>

      {carregando ? (
        <div className="text-center py-12 text-gray-400">Carregando...</div>
      ) : (
        <>
          {/* Abas */}
          <div className="flex border-b mb-6">
            {ABAS.map(({ id, label, icon: Icon }) => (
              <button
                key={id}
                onClick={() => setAba(id)}
                className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors ${
                  aba === id
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                <Icon size={14} /> {label}
              </button>
            ))}
          </div>

          {/* Dados Pessoais */}
          {aba === 'dados' && (
            <form onSubmit={salvarDados} className="flex flex-col gap-4">
              <div>
                <label className={labelCls}>Nome</label>
                <input
                  type="text"
                  value={dados.nome}
                  onChange={(e) => setDados((d) => ({ ...d, nome: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>E-mail</label>
                <input
                  type="email"
                  value={dados.email}
                  onChange={(e) => setDados((d) => ({ ...d, email: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <Msg m={msgDados} />
              <div className="flex justify-end">
                <button
                  type="submit"
                  disabled={salvandoDados}
                  className="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
                >
                  {salvandoDados ? 'Salvando...' : 'Salvar Dados'}
                </button>
              </div>
            </form>
          )}

          {/* Alterar Senha */}
          {aba === 'senha' && (
            <form onSubmit={salvarSenha} className="flex flex-col gap-4">
              <div>
                <label className={labelCls}>Senha Atual</label>
                <input
                  type="password"
                  value={senha.senhaAtual}
                  onChange={(e) => setSenha((s) => ({ ...s, senhaAtual: e.target.value }))}
                  autoComplete="current-password"
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>Nova Senha</label>
                <input
                  type="password"
                  value={senha.novaSenha}
                  onChange={(e) => setSenha((s) => ({ ...s, novaSenha: e.target.value }))}
                  autoComplete="new-password"
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>Confirmar Nova Senha</label>
                <input
                  type="password"
                  value={senha.confirmar}
                  onChange={(e) => setSenha((s) => ({ ...s, confirmar: e.target.value }))}
                  autoComplete="new-password"
                  className={inputCls}
                />
              </div>
              <Msg m={msgSenha} />
              <div className="flex justify-end">
                <button
                  type="submit"
                  disabled={salvandoSenha}
                  className="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
                >
                  {salvandoSenha ? 'Alterando...' : 'Alterar Senha'}
                </button>
              </div>
            </form>
          )}

          {/* Configuração IMAP */}
          {aba === 'imap' && (
            <form onSubmit={salvarImap} className="flex flex-col gap-4">
              <div>
                <label className={labelCls}>Provedor</label>
                <div className="flex flex-wrap gap-2">
                  {PROVEDORES.map((p, i) => (
                    <button
                      key={i}
                      type="button"
                      onClick={() => aplicarProvedor(i)}
                      className={`px-3 py-1.5 text-xs rounded-lg border transition-colors ${
                        imap.host === p.host && p.host !== ''
                          ? 'bg-blue-50 border-blue-400 text-blue-700'
                          : 'bg-white border-gray-200 text-gray-600 hover:border-gray-400'
                      }`}
                    >
                      {p.label}
                    </button>
                  ))}
                </div>
              </div>
              <div className="grid grid-cols-3 gap-3">
                <div className="col-span-2">
                  <label className={labelCls}>Servidor IMAP</label>
                  <input
                    type="text"
                    value={imap.host}
                    onChange={(e) => setImap((i) => ({ ...i, host: e.target.value }))}
                    placeholder="imap.gmail.com"
                    className={inputCls}
                  />
                </div>
                <div>
                  <label className={labelCls}>Porta</label>
                  <input
                    type="number"
                    value={imap.porta}
                    onChange={(e) => setImap((i) => ({ ...i, porta: parseInt(e.target.value) || 993 }))}
                    className={inputCls}
                  />
                </div>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="ssl"
                  checked={imap.usarSsl}
                  onChange={(e) => setImap((i) => ({ ...i, usarSsl: e.target.checked }))}
                  className="w-4 h-4 accent-blue-600"
                />
                <label htmlFor="ssl" className="text-sm text-gray-600">Usar SSL/TLS</label>
              </div>
              <div>
                <label className={labelCls}>E-mail da Conta</label>
                <input
                  type="email"
                  value={imap.emailUsuario}
                  onChange={(e) => setImap((i) => ({ ...i, emailUsuario: e.target.value }))}
                  placeholder="seu@email.com"
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>Senha da Conta</label>
                <input
                  type="password"
                  value={imap.senha}
                  onChange={(e) => setImap((i) => ({ ...i, senha: e.target.value }))}
                  placeholder="Deixe em branco para manter a atual"
                  autoComplete="new-password"
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>Confirmar Senha</label>
                <input
                  type="password"
                  value={imap.confirmarSenha}
                  onChange={(e) => setImap((i) => ({ ...i, confirmarSenha: e.target.value }))}
                  autoComplete="new-password"
                  className={inputCls}
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className={labelCls}>Pasta</label>
                  <input
                    type="text"
                    value={imap.pasta}
                    onChange={(e) => setImap((i) => ({ ...i, pasta: e.target.value }))}
                    placeholder="INBOX"
                    className={inputCls}
                  />
                </div>
                <div>
                  <label className={labelCls}>Intervalo (min)</label>
                  <input
                    type="number"
                    min={1}
                    value={imap.intervaloMinutos}
                    onChange={(e) => setImap((i) => ({ ...i, intervaloMinutos: parseInt(e.target.value) || 5 }))}
                    className={inputCls}
                  />
                </div>
              </div>
              <Msg m={msgImap} />
              <div className="flex justify-end">
                <button
                  type="submit"
                  disabled={salvandoImap}
                  className="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
                >
                  {salvandoImap ? 'Salvando...' : 'Salvar Configuração'}
                </button>
              </div>
            </form>
          )}
        </>
      )}
    </div>
  );
}
