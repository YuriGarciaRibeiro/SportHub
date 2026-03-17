# PRD — Painel SuperAdmin: Dashboard Inicial e Gestão de Tenants

## Visão Geral

O SportHub é uma plataforma multi-tenant de gestão de quadras esportivas. Atualmente, o painel administrativo (`apps/admin`) possui apenas a tela de login — não há nenhuma interface operacional para o usuário com perfil SuperAdmin.

Este PRD define os requisitos para as duas primeiras telas do painel SuperAdmin:

1. **Dashboard Inicial** — visão consolidada do estado da plataforma
2. **Gestão de Tenants** — interface completa para criação, monitoramento e controle de tenants

O objetivo é fornecer ao SuperAdmin uma ferramenta funcional para operar a plataforma sem precisar acessar o banco de dados ou APIs diretamente.

---

## Objetivos

- Permitir que o SuperAdmin visualize o estado da plataforma em menos de 5 segundos após o login
- Reduzir a zero a necessidade de acesso direto à API ou banco para operações rotineiras de tenant
- Fornecer ações de gestão de tenant (provisionar, suspender, ativar, ver detalhes) em uma única interface
- Servir como base arquitetural para as demais telas do painel admin

**Critérios de sucesso:**
- Todas as ações de tenant acessíveis em no máximo 2 cliques a partir do dashboard
- Feedback imediato (toast + atualização de estado) após qualquer ação
- Interface funcional sem erros nos cenários principais (lista, provisionar, suspender/ativar, detalhe)

---

## Histórias de Usuário

**Dashboard:**
- Como SuperAdmin, quero ver o total de tenants ativos na plataforma ao abrir o painel, para ter uma noção rápida do estado do sistema
- Como SuperAdmin, quero ver uma lista dos tenants cadastrados mais recentemente, para identificar novos clientes sem precisar navegar para outra tela
- Como SuperAdmin, quero ter atalhos de ação rápida (provisionar tenant), para iniciar operações comuns diretamente do dashboard

**Gestão de Tenants:**
- Como SuperAdmin, quero listar todos os tenants com paginação e filtro por nome, slug e status, para encontrar um tenant específico rapidamente
- Como SuperAdmin, quero provisionar um novo tenant preenchendo um formulário, para cadastrar novos clientes sem acesso à API
- Como SuperAdmin, quero suspender ou ativar um tenant diretamente na listagem, para agir rapidamente em casos de inadimplência ou reativação
- Como SuperAdmin, quero abrir o detalhe de um tenant e ver seus usuários, para entender quem está operando aquele estabelecimento

---

## Funcionalidades Principais

### 1. Layout Base do Painel

O painel precisa de uma estrutura de navegação persistente que sirva a todas as telas futuras.

**Requisitos funcionais:**
- RF-01: Sidebar com navegação para Dashboard e Tenants (expansível para futuras seções)
- RF-02: Header com nome do usuário logado, cargo (SuperAdmin) e opção de logout
- RF-03: Todas as rotas sob `/(dashboard)` devem ser protegidas — redirecionar para `/login` se não autenticado
- RF-04: Suporte a tema claro/escuro (já presente no projeto via `next-themes`)

### 2. Dashboard Inicial (`/`)

Tela de boas-vindas com visão consolidada da plataforma.

**Cards de métricas (dados disponíveis via backend):**
- RF-05: Card com total de tenants cadastrados
- RF-06: Card com total de tenants ativos (status = Active)
- RF-07: Card com total de tenants suspensos (status = Suspended)

> Nota: Os três cards são obtidos via `GET /api/tenants?pageSize=1` com filtros de status diferentes (`totalCount` do `PagedResult` retorna o total sem carregar todos os itens). O endpoint `GET /admin/stats` existe no contexto do storefront (área admin do tenant) — não é aplicável ao painel SuperAdmin.

**Lista de tenants recentes:**
- RF-08: Tabela com os 5 tenants mais recentes (ordenados por `createdAt` desc), exibindo nome, slug e status (badge colorido)
- RF-09: Link "Ver todos" direcionando para `/tenants`

**Ações rápidas:**
- RF-10: Botão "Provisionar Tenant" que abre o formulário de criação em modal

### 3. Gestão de Tenants (`/tenants`)

Tela principal de operação de tenants.

**Listagem com filtros:**
- RF-11: Tabela paginada com colunas: Nome, Slug, Status (badge), Owner (email), Criado em, Ações
- RF-12: Filtros de busca: campo de texto livre (searchTerm) e seletor de status (Todos / Ativo / Suspenso)
- RF-13: Paginação com controle de `page` e `pageSize` (padrão: 10 por página)
- RF-14: Estado de loading (skeleton) e estado vazio ("Nenhum tenant encontrado")

**Provisionar novo tenant:**
- RF-15: Botão "Novo Tenant" abre modal com formulário contendo os campos:
  - Slug (obrigatório, único, kebab-case)
  - Nome do estabelecimento (obrigatório)
  - Email do owner (opcional)
  - Nome e sobrenome do owner (opcionais)
- RF-16: Validação client-side com Zod antes do submit
- RF-17: Após criação bem-sucedida: fechar formulário, exibir toast de sucesso, atualizar listagem

**Ações inline:**
- RF-18: Botão "Suspender" visível apenas para tenants com status Active — confirma ação antes de executar
- RF-19: Botão "Ativar" visível apenas para tenants com status Suspended
- RF-20: Botão "Ver detalhes" abre drawer lateral com o detalhe do tenant

**Detalhe do Tenant (drawer lateral):**
- RF-21: Drawer exibindo: Nome, Slug, Status, Domain customizado, Owner, Data de criação, Logo URL, Cor primária
- RF-22: Lista de usuários do tenant (paginada) com colunas: Nome, Email, Cargo
- RF-23: Filtro de busca por nome/email na lista de usuários

---

## Experiência do Usuário

**Personas:**
- **SuperAdmin**: usuário técnico com alta proficiência, faz operações rápidas, espera feedback imediato e não tolera telas lentas

**Fluxos principais:**

1. Login → Dashboard → cards carregam → lista de recentes carrega → ação rápida "Provisionar"
2. Dashboard → "Ver todos" → Tenants → filtrar por status → suspender tenant → confirmar → toast → lista atualizada
3. Tenants → "Ver detalhes" → painel abre → lista de usuários carrega

**Diretrizes de UX:**
- Ações destrutivas (suspender) exigem confirmação (dialog/alert)
- Estados de loading explícitos: skeleton para tabelas, spinner para botões
- Erros de API devem exibir mensagem via toast (não travar a tela)
- Feedback de sucesso sempre via toast com mensagem clara
- Navegação mantém estado de filtros ao voltar para a lista

---

## Restrições Técnicas de Alto Nível

- **Stack obrigatória**: Next.js App Router, React, TypeScript, TailwindCSS 4, shadcn/ui, TanStack Query, Zustand, Axios — conforme já definido em `apps/admin`
- **Autenticação**: JWT Bearer via Axios interceptor + httpOnly cookie para refresh token — padrão BFF já implementado
- **Autorização**: Apenas usuários com role `SuperAdmin` devem acessar estas telas — verificar role no `useAuthStore` e redirecionar se insuficiente
- **Endpoints disponíveis** (todos sob `/api/tenants`, requerem SuperAdmin):
  - `GET /api/tenants` — listagem paginada com filtros (page, pageSize, name, slug, status, searchTerm)
  - `GET /api/tenants/{id}` — detalhe do tenant
  - `GET /api/tenants/{id}/users` — usuários do tenant (paginado)
  - `POST /api/tenants` — provisionar
  - `POST /api/tenants/{id}/suspend` — suspender
  - `POST /api/tenants/{id}/activate` — ativar
- **Componentes disponíveis** em `@workspace/ui`: Button, Card, Input, Label — usar como base
- **Tipos compartilhados** disponíveis em `@workspace/shared`: `TenantResponse`, `ProvisionTenantRequest`, `PaginatedResponse`, `TenantStatus`, `UserRole`
- **Performance**: TanStack Query com `staleTime: 5min` já configurado — aproveitar cache para evitar re-fetches desnecessários

---

## Fora de Escopo

- Gestão de branding do tenant (PATCH `/api/tenants/{id}/branding`) — futura tela de configuração
- Atribuição de owner (`POST /api/tenants/{id}/owner`) — futura tela de detalhe avançado
- Cancelamento de tenant — não há endpoint disponível atualmente
- Gráficos de crescimento temporal — não há endpoint com dados históricos no backend atual
- Gestão de sports, courts ou reservations no contexto SuperAdmin
- Qualquer funcionalidade do painel de tenant (Owner/Manager) — escopo separado

---

## Questões em Aberto

- Nenhuma questão em aberto — escopo, padrões de UX e endpoints disponíveis estão definidos.
