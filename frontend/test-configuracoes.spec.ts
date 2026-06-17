import { test, expect } from '@playwright/test';

const BASE = 'http://localhost:3000';
const API = 'http://localhost:5142';
const EMAIL = 'admin@teste.com';
const SENHA = 'Admin@123';

// Shared auth state — set once in beforeAll
let authToken = '';
let authUser: Record<string, unknown> = {};

test.beforeAll(async ({ request }) => {
  const res = await request.post(`${API}/api/auth/login`, {
    data: { email: EMAIL, senha: SENHA },
  });
  if (!res.ok()) throw new Error(`Login falhou: ${res.status()} ${await res.text()}`);
  const data = await res.json();
  authToken = data.token;
  authUser = data.usuario;
});

// Inject auth into every test's page
test.beforeEach(async ({ page }) => {
  if (!authToken) return;
  // Set cookie first (middleware reads it)
  await page.context().addCookies([{
    name: 'auth-token',
    value: authToken,
    domain: 'localhost',
    path: '/',
    httpOnly: false,
    secure: false,
  }]);
  // Set localStorage (AuthContext reads it)
  await page.goto(`${BASE}/login`, { waitUntil: 'domcontentloaded' });
  await page.evaluate(([token, user]) => {
    localStorage.setItem('auth-token', token as string);
    localStorage.setItem('auth-user', user as string);
  }, [authToken, JSON.stringify(authUser)]);
});

// ─── Testes ──────────────────────────────────────────────────────────────────

test('kanban carrega após autenticação', async ({ page }) => {
  await page.goto(`${BASE}/kanban`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('header', { timeout: 20000 });
  await expect(page.locator('h1')).toContainText('Kanban');
  await page.screenshot({ path: 'ss-01-kanban.png' });
});

test('header tem ícone de configurações', async ({ page }) => {
  await page.goto(`${BASE}/kanban`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('header', { timeout: 20000 });
  const gear = page.locator('a[href="/configuracoes/perfil"]');
  await expect(gear).toBeVisible();
  await page.screenshot({ path: 'ss-02-header-gear.png' });
});

test('perfil - aba dados pessoais', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/perfil`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('input[type="text"]', { timeout: 20000 });
  await expect(page.locator('h1')).toContainText('Meu Perfil');
  await expect(page.locator('input[type="text"]').first()).not.toBeEmpty();
  await page.screenshot({ path: 'ss-03-perfil-dados.png' });
});

test('perfil - aba alterar senha', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/perfil`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('text=Alterar Senha');
  await page.click('text=Alterar Senha');
  await expect(page.locator('input[type="password"]').first()).toBeVisible();
  await page.screenshot({ path: 'ss-04-perfil-senha.png' });
});

test('perfil - aba IMAP com botões de provedor', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/perfil`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('text=Configuração IMAP');
  await page.click('text=Configuração IMAP');
  await expect(page.locator('text=Gmail')).toBeVisible();
  await expect(page.locator('text=Hostinger')).toBeVisible();
  await page.click('button:has-text("Gmail")');
  const hostInput = page.locator('input[placeholder="imap.gmail.com"]');
  await expect(hostInput).toHaveValue('imap.gmail.com');
  await page.screenshot({ path: 'ss-05-perfil-imap.png' });
});

test('remetentes - página carrega', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/remetentes`, { waitUntil: 'domcontentloaded' });
  await expect(page.locator('h1')).toContainText('Remetentes Monitorados');
  await page.screenshot({ path: 'ss-06-remetentes-lista.png' });
});

test('remetentes - modal de adicionar', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/remetentes`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('button:has-text("Adicionar")');
  await page.click('button:has-text("Adicionar")');
  await expect(page.locator('h2:has-text("Novo Remetente")')).toBeVisible();
  // Fechar com Esc
  await page.keyboard.press('Escape');
  await expect(page.locator('h2:has-text("Novo Remetente")')).not.toBeVisible();
  await page.screenshot({ path: 'ss-07-remetentes-modal.png' });
});

test('remetentes - CRUD completo', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/remetentes`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('button:has-text("Adicionar")');
  // Aguardar API de listagem concluir antes de abrir modal
  await page.waitForLoadState('networkidle');

  // Criar
  await page.click('button:has-text("Adicionar")');
  await page.waitForSelector('h2:has-text("Novo Remetente")', { timeout: 10000 });
  await page.fill('input[placeholder="exemplo@empresa.com ou @empresa.com"]', 'crud@playwright.com');
  await page.fill('input[placeholder="Identificação amigável"]', 'Teste CRUD');
  await page.click('button:has-text("Salvar")');
  await page.waitForSelector('td:has-text("crud@playwright.com")', { timeout: 10000 });
  await page.screenshot({ path: 'ss-08-remetentes-criado.png' });

  // Editar
  const row = page.locator('tr', { has: page.locator('text=crud@playwright.com') });
  await row.locator('button[title="Editar"]').click();
  await page.waitForSelector('h2:has-text("Editar Remetente")', { timeout: 10000 });
  await expect(page.locator('h2:has-text("Editar Remetente")')).toBeVisible();
  await page.fill('input[placeholder="Identificação amigável"]', 'Editado CRUD');
  await page.click('button:has-text("Salvar")');
  await page.waitForSelector('td:has-text("Editado CRUD")', { timeout: 10000 });
  await page.screenshot({ path: 'ss-09-remetentes-editado.png' });

  // Excluir
  const row2 = page.locator('tr', { has: page.locator('text=crud@playwright.com') });
  await row2.locator('button[title="Excluir"]').click();
  await expect(page.locator('text=Deseja excluir')).toBeVisible();
  await page.click('button:has-text("Confirmar")');
  await page.waitForSelector('td:has-text("crud@playwright.com")', { state: 'hidden', timeout: 10000 });
  await page.screenshot({ path: 'ss-10-remetentes-excluido.png' });
});

test('usuários - lista carregada', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/usuarios`, { waitUntil: 'domcontentloaded' });
  await expect(page.locator('h1')).toContainText('Usuários');
  await page.waitForSelector('table', { timeout: 15000 });
  const rows = page.locator('tbody tr');
  expect(await rows.count()).toBeGreaterThanOrEqual(1);
  await page.screenshot({ path: 'ss-11-usuarios.png' });
});

test('navegação pelo sidebar', async ({ page }) => {
  await page.goto(`${BASE}/configuracoes/perfil`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('text=Remetentes');

  await page.click('a:has-text("Remetentes")');
  await expect(page).toHaveURL(`${BASE}/configuracoes/remetentes`);

  await page.click('a:has-text("Usuários")');
  await expect(page).toHaveURL(`${BASE}/configuracoes/usuarios`);

  await page.click('a:has-text("Voltar ao Kanban")');
  await expect(page).toHaveURL(`${BASE}/kanban`);
  await page.screenshot({ path: 'ss-12-navegacao-sidebar.png' });
});

test('login via UI (formulário)', async ({ browser }) => {
  // Teste isolado, sem storageState — nova sessão limpa
  const ctx = await browser.newContext();
  const page = await ctx.newPage();
  try {
    await page.goto(`${BASE}/login`, { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });

    // pressSequentially dispara keydown/input/keyup por caractere — React processa cada evento
    await page.locator('input[placeholder="seu@email.com"]').click();
    await page.locator('input[placeholder="seu@email.com"]').pressSequentially(EMAIL, { delay: 40 });

    await page.locator('input[type="password"]').first().click();
    await page.locator('input[type="password"]').first().pressSequentially(SENHA, { delay: 40 });

    await page.click('button[type="submit"]');
    // router.push() do Next.js App Router é SPA navigation — usar polling
    await expect(page).toHaveURL(`${BASE}/kanban`, { timeout: 30000 });
    await page.screenshot({ path: 'ss-13-login-ui.png' });
  } finally {
    await ctx.close();
  }
});
