# Kanban de E-mails

Sistema para monitorar uma caixa de e-mail via IMAP e transformar e-mails em cards de um quadro Kanban.

## Pré-requisitos

- .NET 10 SDK
- SQL Server 2022 (ou Express)
- Node.js 18+

## Configuração

### 1. Banco de dados

Edite a connection string em `src/KanbanEmails.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=KanbanEmails;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Execute a migration:

```bash
dotnet ef database update \
  --project src/KanbanEmails.Infrastructure \
  --startup-project src/KanbanEmails.Api
```

### 2. Credenciais IMAP

Use variáveis de ambiente (recomendado) ou edite o `appsettings.json`:

```bash
# Variáveis de ambiente (Windows)
set EmailImap__Username=seu@email.com
set EmailImap__Password=sua_senha
set EmailImap__Host=imap.hostinger.com
```

Provedores suportados:

| Provedor   | Host                        | Porta |
|------------|-----------------------------|-------|
| Hostinger  | `imap.hostinger.com`        | 993   |
| Gmail      | `imap.gmail.com`            | 993   |
| Outlook    | `outlook.office365.com`     | 993   |

> **Gmail**: use uma [Senha de App](https://support.google.com/accounts/answer/185833), não a senha da conta.

### 3. Remetentes monitorados

Após subir a API, cadastre os e-mails/domínios via Swagger ou API:

```bash
POST /api/remetentes-monitorados
{ "emailOuDominio": "cliente@empresa.com.br", "nome": "Empresa XPTO" }

# Ou por domínio inteiro:
{ "emailOuDominio": "empresa.com.br", "nome": "Empresa XPTO" }
```

## Executando

### Backend (API)

```bash
cd src/KanbanEmails.Api
dotnet run
# API disponível em http://localhost:5000
# Swagger em http://localhost:5000/swagger
```

### Worker (leitura IMAP automática)

```bash
cd src/KanbanEmails.Worker
dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# Disponível em http://localhost:3000
```

## Sincronização manual

Sem precisar do Worker, dispare a leitura pelo botão **Sincronizar** no Kanban ou via API:

```bash
POST http://localhost:5000/api/emails-kanban/sync
```

## Estrutura

```
src/
├── KanbanEmails.Domain/          # Entidades, Enums, Interfaces de repositório
├── KanbanEmails.Application/     # DTOs, Serviços, Interfaces de serviço
├── KanbanEmails.Infrastructure/  # EF Core, MailKit, Storage, Repositórios
├── KanbanEmails.Api/             # Controllers, Swagger
└── KanbanEmails.Worker/          # Worker IMAP em background
frontend/                         # Next.js + TypeScript + TailwindCSS
storage/
└── email-anexos/{id}/            # Anexos salvos em disco
```

## Fases implementadas

- [x] Fase 1 — Base: entidades, migrations EF Core, API com Swagger
- [x] Fase 2 — Worker IMAP com MailKit, deduplicação por MessageId
- [x] Fase 3 — Anexos em disco, download via API
- [x] Fase 4 — Frontend Kanban com drag-and-drop, filtros, modal de detalhes
- [ ] Fase 5 — Histórico detalhado, tela de remetentes monitorados
- [ ] Fase 6 — IA para resumo e classificação (opcional)
