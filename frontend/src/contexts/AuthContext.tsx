'use client';

import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/services/api';
import { LoginUsuario } from '@/types';

interface AuthContextType {
  user: LoginUsuario | null;
  loading: boolean;
  login: (email: string, senha: string) => Promise<void>;
  logout: () => void;
  cadastrar: (nome: string, email: string, senha: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const [user, setUser] = useState<LoginUsuario | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem('auth-user');
    const storedToken = localStorage.getItem('auth-token');
    if (storedUser && storedToken) {
      setUser(JSON.parse(storedUser) as LoginUsuario);
    }
    setLoading(false);
  }, []);

  const login = async (email: string, senha: string) => {
    const data = await api.login(email, senha);
    setUser(data.usuario);
    localStorage.setItem('auth-token', data.token);
    localStorage.setItem('auth-user', JSON.stringify(data.usuario));
    // Cookie lido pelo middleware para proteger rotas server-side
    document.cookie = `auth-token=${data.token}; path=/; max-age=${8 * 60 * 60}`;
    router.push('/kanban');
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('auth-token');
    localStorage.removeItem('auth-user');
    document.cookie = 'auth-token=; path=/; max-age=0';
    router.push('/login');
  };

  const cadastrar = async (nome: string, email: string, senha: string) => {
    await api.cadastrar(nome, email, senha);
    await login(email, senha);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, cadastrar }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth precisa estar dentro de AuthProvider');
  return ctx;
}
