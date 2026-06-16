# CLAUDE.md

## Projeto

Sistema de Kanban para gerenciamento de e-mails recebidos.

## Objetivo

Monitorar uma caixa de e-mail via IMAP e transformar automaticamente os e-mails recebidos em cartões Kanban.

## Stack Obrigatória

### Backend

* ASP.NET 10
* Entity Framework Core
* SQL Server 2022
* MailKit
* Swagger

### Frontend

* React
* Next.js
* TypeScript
* TailwindCSS

## Arquitetura

Utilizar DDD:

* Api
* Application
* Domain
* Infrastructure

## Banco de Dados

Tabelas:

* EmailKanban
* EmailAnexo
* EmailHistorico

Não utilizar armazenamento de anexos em banco.

Os anexos devem ser armazenados em disco.

## Integração IMAP

Suportar:

* Gmail
* Hostinger
* Outlook

Utilizar MailKit.

Evitar duplicidade utilizando MessageId.

## Fluxo

1. Ler novos e-mails.
2. Salvar e-mail.
3. Salvar anexos.
4. Criar card Kanban.
5. Atualizar status conforme movimentação.

## Colunas Kanban

* Novo
* Em Análise
* Desenvolvimento
* Aguardando Cliente
* Concluído

## Card

Exibir:

* Assunto
* Remetente
* Data
* Resumo
* Quantidade de anexos

Ao abrir:

* Conteúdo completo do e-mail
* Lista de anexos
* Download dos anexos

## Anexos

Salvar:

* NomeArquivo
* MimeType
* TamanhoBytes
* CaminhoArquivo

Os arquivos devem ser armazenados em:

storage/email-anexos/{EmailId}

## Regras

* Não permitir MessageId duplicado.
* Registrar histórico de mudanças de status.
* Utilizar migrations EF Core.
* Seguir SOLID.
* Seguir Clean Code.
* Gerar código comentado apenas quando necessário.

## Antes de Implementar

Sempre:

1. Analisar requisitos.
2. Identificar riscos.
3. Propor melhorias.
4. Gerar plano de execução.
5. Aguardar aprovação.

Somente após aprovação iniciar a implementação.
