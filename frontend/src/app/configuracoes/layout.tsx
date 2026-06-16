'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { ArrowLeft, Users, Mail, User } from 'lucide-react';

const NAV = [
  { href: '/configuracoes/perfil', label: 'Meu Perfil', icon: User },
  { href: '/configuracoes/remetentes', label: 'Remetentes', icon: Mail },
  { href: '/configuracoes/usuarios', label: 'Usuários', icon: Users },
];

export default function ConfiguracoesLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();

  return (
    <div className="flex h-screen bg-gray-50">
      <aside className="w-56 bg-white border-r flex flex-col shrink-0">
        <div className="px-4 py-4 border-b">
          <Link
            href="/kanban"
            className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-800 transition-colors"
          >
            <ArrowLeft size={14} />
            Voltar ao Kanban
          </Link>
          <h2 className="mt-3 text-sm font-semibold text-gray-700">Configurações</h2>
        </div>
        <nav className="flex-1 p-3 flex flex-col gap-1">
          {NAV.map(({ href, label, icon: Icon }) => {
            const ativo = pathname.startsWith(href);
            return (
              <Link
                key={href}
                href={href}
                className={`flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-colors ${
                  ativo
                    ? 'bg-blue-50 text-blue-700 font-medium'
                    : 'text-gray-600 hover:bg-gray-100'
                }`}
              >
                <Icon size={15} />
                {label}
              </Link>
            );
          })}
        </nav>
      </aside>

      <main className="flex-1 overflow-y-auto p-6">{children}</main>
    </div>
  );
}
