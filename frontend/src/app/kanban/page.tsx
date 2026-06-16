import { api } from '@/services/api';
import KanbanBoard from '@/components/KanbanBoard';
import { EmailKanban } from '@/types';

export const dynamic = 'force-dynamic';

export default async function KanbanPage() {
  let emails: EmailKanban[] = [];
  try {
    emails = await api.listarEmails();
  } catch {
    // API pode não estar disponível durante build/dev sem backend
  }

  return <KanbanBoard emailsIniciais={emails} />;
}
