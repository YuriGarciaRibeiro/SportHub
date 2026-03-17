# Tech Spec — Painel SuperAdmin: Dashboard e Gestão de Tenants

## Resumo Executivo

Implementação das primeiras telas funcionais do `apps/admin` (Next.js 16, App Router), aproveitando toda a infraestrutura já existente: auth BFF, Zustand, TanStack Query, Axios com interceptores e `@workspace/ui`. O trabalho se divide em quatro frentes paralelas: (1) adicionar componentes shadcn ao `@workspace/ui`, (2) corrigir e expandir a camada de API/tipos, (3) construir o layout base com sidebar, e (4) implementar as páginas de Dashboard e Tenants com seus modais/drawers. Nenhuma mudança no backend é necessária — todos os endpoints estão disponíveis.

---

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Novos arquivos e componentes a criar/modificar:**

| Arquivo | Ação | Responsabilidade |
|---|---|---|
| `packages/ui/src/components/` — Table, Dialog, Drawer, Select, Skeleton, Badge, AlertDialog, Separator, ScrollArea | **Criar** (shadcn CLI) | Componentes compartilhados necessários para as novas telas |
| `packages/shared/src/types/tenant.ts` | **Modificar** | Adicionar `ownerFirstName: string \| null` e `ownerLastName: string \| null` ao `TenantResponse`; adicionar `TenantUserResponse` e `ProvisionTenantResponse` |
| `packages/shared/src/types/enums.ts` | **Modificar** | Alinhar `TenantStatus` com o backend: adicionar `Canceled = 'Canceled'`, remover `Provisioning` (não existe no backend — enum do backend tem Active=0, Suspended=1, Canceled=2) |
| `packages/shared/src/types/common.ts` | **Modificar** | Confirmar que `PaginatedResponse<T>` tem `hasPreviousPage` e `hasNextPage` |
| `apps/admin/src/lib/api.ts` | **Modificar** | Corrigir `tenantsApi.list()` para `PaginatedResponse<TenantResponse>`, adicionar `tenantsApi.getUsers()` e query params tipados |
| `apps/admin/src/app/page.tsx` | **Deletar / mover** | Substituído pelo dashboard no route group |
| `apps/admin/src/app/(dashboard)/layout.tsx` | **Criar** | Layout protegido com sidebar + header, chama `useRequireAuth` |
| `apps/admin/src/app/(dashboard)/page.tsx` | **Criar** | Página do dashboard (`/`) |
| `apps/admin/src/app/(dashboard)/tenants/page.tsx` | **Criar** | Página de gestão de tenants (`/tenants`) |
| `apps/admin/src/components/layout/sidebar.tsx` | **Criar** | Sidebar com links de navegação |
| `apps/admin/src/components/layout/header.tsx` | **Criar** | Header com usuário logado e logout |
| `apps/admin/src/components/dashboard/stats-cards.tsx` | **Criar** | 3 cards de métricas com React Query |
| `apps/admin/src/components/dashboard/recent-tenants-table.tsx` | **Criar** | Tabela dos 5 tenants mais recentes |
| `apps/admin/src/components/tenants/tenants-table.tsx` | **Criar** | Tabela paginada com filtros |
| `apps/admin/src/components/tenants/tenants-filters.tsx` | **Criar** | Barra de filtros: searchTerm + status |
| `apps/admin/src/components/tenants/provision-modal.tsx` | **Criar** | Modal de criação de tenant |
| `apps/admin/src/components/tenants/tenant-detail-drawer.tsx` | **Criar** | Drawer lateral com detalhe e usuários |
| `apps/admin/src/components/tenants/tenant-status-badge.tsx` | **Criar** | Badge colorido de status |
| `apps/admin/src/components/tenants/suspend-confirm-dialog.tsx` | **Criar** | AlertDialog de confirmação para suspender |
| `apps/admin/src/hooks/use-tenants.ts` | **Criar** | React Query hooks: `useTenants`, `useTenantStats`, `useTenantUsers` |
| `apps/admin/src/hooks/use-tenant-mutations.ts` | **Criar** | Mutations: `useProvisionTenant`, `useSuspendTenant`, `useActivateTenant` |

**Fluxo de dados geral:**

```
Page → hook (useQuery/useMutation) → api.ts (Axios) → Backend REST
                                          ↓
                                    cache TanStack Query
                                          ↓
                              invalidateQueries → re-fetch
```

---

## Design de Implementação

### Interfaces Principais

**`tenantsApi` corrigido em `apps/admin/src/lib/api.ts`:**

```ts
interface TenantsListParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: 'Active' | 'Suspended' | '';
}

interface TenantUsersParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export const tenantsApi = {
  list: (params?: TenantsListParams) =>
    api.get<PaginatedResponse<TenantResponse>>('/api/tenants', { params }).then(r => r.data),

  getById: (id: string) =>
    api.get<TenantResponse>(`/api/tenants/${id}`).then(r => r.data),

  getUsers: (id: string, params?: TenantUsersParams) =>
    api.get<PaginatedResponse<TenantUserResponse>>(`/api/tenants/${id}/users`, { params }).then(r => r.data),

  provision: (data: ProvisionTenantRequest) =>
    api.post<ProvisionTenantResponse>('/api/tenants', data).then(r => r.data),

  suspend: (id: string) => api.post<void>(`/api/tenants/${id}/suspend`),

  activate: (id: string) => api.post<void>(`/api/tenants/${id}/activate`),
};
```

**Correções em `packages/shared/src/types/tenant.ts`:**

```ts
// Adicionar ao TenantResponse existente:
export interface TenantResponse {
  id: string;
  name: string;
  slug: string;
  status: TenantStatus;
  primaryColor: string | null;
  logoUrl: string | null;
  ownerEmail: string | null;
  ownerFirstName: string | null;  // ← adicionar
  ownerLastName: string | null;   // ← adicionar
  createdAt: string;
  customDomain: string | null;
}

// Novos tipos:
export interface TenantUserResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface ProvisionTenantResponse {
  id: string;
  slug: string;
  name: string;
  schema: string;
}
```

**Correção em `packages/shared/src/types/enums.ts`:**

```ts
// De:
export enum TenantStatus {
  Active = 'Active',
  Suspended = 'Suspended',
  Provisioning = 'Provisioning',  // ← não existe no backend
}

// Para:
export enum TenantStatus {
  Active = 'Active',       // backend: 0
  Suspended = 'Suspended', // backend: 1
  Canceled = 'Canceled',   // backend: 2 — adicionar
  // Provisioning removido: não existe no domínio do backend
}
```

> O backend aceita string ou inteiro no query param `status`. Usar o valor string do enum (`TenantStatus.Active` = `'Active'`) é o correto.

**Zod schema do formulário de provisionar:**

```ts
const provisionSchema = z.object({
  slug: z.string().min(3).regex(/^[a-z0-9-]+$/, 'Apenas letras minúsculas, números e hífens'),
  name: z.string().min(2),
  ownerEmail: z.string().email().optional().or(z.literal('')),
  ownerFirstName: z.string().optional(),
  ownerLastName: z.string().optional(),
});
```

### Modelos de Dados

**Query keys (convenção para invalidação de cache):**

```ts
export const tenantKeys = {
  all: ['tenants'] as const,
  lists: () => [...tenantKeys.all, 'list'] as const,
  list: (params: TenantsListParams) => [...tenantKeys.lists(), params] as const,
  stats: () => [...tenantKeys.all, 'stats'] as const,
  detail: (id: string) => [...tenantKeys.all, id] as const,
  users: (id: string, params?: TenantUsersParams) => [...tenantKeys.detail(id), 'users', params] as const,
};
```

**React Query hooks em `use-tenants.ts`:**

```ts
// Cards de métricas: 3 queries paralelas com pageSize=1 e filtro de status
export function useTenantStats() {
  const total = useQuery({ queryKey: tenantKeys.list({ pageSize: 1 }), queryFn: () => tenantsApi.list({ pageSize: 1 }) });
  const active = useQuery({ queryKey: tenantKeys.list({ pageSize: 1, status: 'Active' }), queryFn: () => tenantsApi.list({ pageSize: 1, status: 'Active' }) });
  const suspended = useQuery({ queryKey: tenantKeys.list({ pageSize: 1, status: 'Suspended' }), queryFn: () => tenantsApi.list({ pageSize: 1, status: 'Suspended' }) });
  return { total: total.data?.totalCount, active: active.data?.totalCount, suspended: suspended.data?.totalCount,
           isLoading: total.isLoading || active.isLoading || suspended.isLoading };
}
```

### Endpoints de API Consumidos

| Método | Rota | Usado em |
|---|---|---|
| `GET` | `/api/tenants?pageSize=1` | Card total de tenants |
| `GET` | `/api/tenants?pageSize=1&status=Active` | Card ativos |
| `GET` | `/api/tenants?pageSize=1&status=Suspended` | Card suspensos |
| `GET` | `/api/tenants?page&pageSize=5` | Tabela recentes no dashboard |
| `GET` | `/api/tenants?page&pageSize&searchTerm&status` | Tabela de gestão |
| `GET` | `/api/tenants/{id}` | Drawer de detalhe |
| `GET` | `/api/tenants/{id}/users?page&pageSize&searchTerm` | Lista de usuários no drawer |
| `POST` | `/api/tenants` | Modal de provisionar |
| `POST` | `/api/tenants/{id}/suspend` | Botão inline na tabela |
| `POST` | `/api/tenants/{id}/activate` | Botão inline na tabela |

---

## Pontos de Integração

**`@workspace/ui` — novos componentes via `shadcn` CLI:**

Executar no diretório `packages/ui`:
```bash
npx shadcn@latest add table dialog drawer select skeleton badge alert-dialog separator scroll-area
```

Os componentes ficam em `packages/ui/src/components/` e são exportados via `packages/ui/src/index.ts`.

**Autorização no layout:** O `(dashboard)/layout.tsx` usa `useRequireAuth()` já existente. Adicionar verificação de role:

```ts
const { user, isLoading } = useAuthStore();
// Se !isLoading && user.role !== UserRole.SuperAdmin → redirect('/login')
```

---

## Abordagem de Testes

### Testes Unidade

- **Zod schemas**: testar slug inválido (espaço, maiúscula, especial), email inválido, campos obrigatórios
- **`useTenantStats`**: mockar `tenantsApi.list` com 3 respostas e verificar `totalCount` correto em cada card
- **`tenantsApi`**: verificar que `list()` passa `params` como query string corretamente

### Testes de Integração

- Fluxo de provisionar: formulário válido → mutation → toast de sucesso → query invalidada
- Fluxo de suspender: click → dialog aparece → confirmar → mutation → badge atualizado na tabela

### Testes E2E (Playwright)

- Login como SuperAdmin → dashboard carrega com 3 cards
- Navegar para /tenants → filtrar por "Suspended" → tabela atualiza
- Provisionar tenant → modal abre → preencher → submeter → tenant aparece na lista

---

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **`@workspace/ui` — adicionar componentes shadcn** — desbloqueador de tudo; sem Table, Dialog e Drawer nada funciona
2. **`@workspace/shared` + `api.ts` — corrigir tipos e funções** — contrato correto antes de construir os hooks
3. **`(dashboard)/layout.tsx` + `sidebar.tsx` + `header.tsx`** — shell do painel; qualquer página nova vai herdar isso
4. **Hooks React Query** (`use-tenants.ts`, `use-tenant-mutations.ts`) — lógica de data fetching isolada dos componentes
5. **Dashboard** (`page.tsx` + `stats-cards.tsx` + `recent-tenants-table.tsx`) — tela inicial funcional
6. **Tenants** (`tenants/page.tsx` + tabela + filtros) — listagem paginada com filtros
7. **Modais e Drawers** (`provision-modal.tsx`, `tenant-detail-drawer.tsx`, `suspend-confirm-dialog.tsx`) — ações completas

### Dependências Técnicas

- Backend rodando em `localhost:5001` com usuário SuperAdmin disponível
- `NEXT_PUBLIC_API_URL` configurado no `.env.local` do `apps/admin`

---

## Monitoramento e Observabilidade

- **React Query Devtools** já habilitado em desenvolvimento — cache e queries visíveis no painel
- **Toast (Sonner)** já configurado para feedback de erros e sucesso — usar `toast.success()` / `toast.error()` consistentemente
- **Console.error** nos catch dos mutations para facilitar debugging durante desenvolvimento

---

## Considerações Técnicas

### Decisões Principais

| Decisão | Escolha | Justificativa |
|---|---|---|
| Estado dos filtros | `useState` local na página | Filtros não precisam de URL params nesta fase; evita complexidade prematura |
| Dados do drawer | Query separada `GET /api/tenants/{id}` | Dados já em cache via TanStack Query; sem prop drilling |
| Cards de métricas | 3 queries paralelas com `pageSize=1` | Não há endpoint de stats para SuperAdmin; `totalCount` já vem no `PagedResult` |
| Componentes shadcn | Instalar em `@workspace/ui` | Reutilizável no storefront; evita duplicação |
| Confirmação de suspender | `AlertDialog` shadcn | Padrão acessível; não usa `window.confirm` |

### Riscos Conhecidos

- **`TenantStatus.Provisioning` no frontend**: qualquer código que referenciasse `TenantStatus.Provisioning` vai quebrar após a remoção do enum. Buscar usos no repositório antes de remover (`grep -r 'Provisioning' packages/shared apps/`).
- **Impacto do `TenantStatus` no storefront**: o `apps/storefront` também importa `TenantStatus` de `@workspace/shared` — validar que nenhuma lógica depende de `Provisioning` antes de remover.

### Conformidade com Padrões

**`.windsurf/rules/techspec-codebase.md`** — regras do backend aplicáveis ao contexto desta tech spec:
- Esta implementação é **exclusivamente frontend** — não altera nenhum endpoint, handler ou domínio do backend
- Os endpoints consumidos já existem e seguem os padrões do projeto (Result Pattern, FluentValidation, JWT)
- Autorização via role `SuperAdmin` já aplicada no backend — frontend replica a verificação para UX (redirect), não para segurança

**Padrões do frontend aplicados:**
- Componentes isolados por domínio (`components/dashboard/`, `components/tenants/`)
- Hooks separados de componentes (`hooks/use-tenants.ts`)
- Formulários com React Hook Form + Zod (mesmo padrão do `login-form.tsx`)
- Mutations invalidam queries relacionadas via `queryClient.invalidateQueries(tenantKeys.lists())`

### Arquivos Relevantes e Dependentes

```
# Modificar
packages/shared/src/types/tenant.ts
packages/shared/src/types/common.ts
apps/admin/src/lib/api.ts
apps/admin/src/app/page.tsx  (deletar ou esvaziar — conteúdo vai para (dashboard)/page.tsx)

# Criar — packages
packages/ui/src/components/table.tsx
packages/ui/src/components/dialog.tsx
packages/ui/src/components/drawer.tsx
packages/ui/src/components/select.tsx
packages/ui/src/components/skeleton.tsx
packages/ui/src/components/badge.tsx
packages/ui/src/components/alert-dialog.tsx
packages/ui/src/components/separator.tsx
packages/ui/src/components/scroll-area.tsx

# Criar — admin app
apps/admin/src/app/(dashboard)/layout.tsx
apps/admin/src/app/(dashboard)/page.tsx
apps/admin/src/app/(dashboard)/tenants/page.tsx
apps/admin/src/components/layout/sidebar.tsx
apps/admin/src/components/layout/header.tsx
apps/admin/src/components/dashboard/stats-cards.tsx
apps/admin/src/components/dashboard/recent-tenants-table.tsx
apps/admin/src/components/tenants/tenants-table.tsx
apps/admin/src/components/tenants/tenants-filters.tsx
apps/admin/src/components/tenants/provision-modal.tsx
apps/admin/src/components/tenants/tenant-detail-drawer.tsx
apps/admin/src/components/tenants/tenant-status-badge.tsx
apps/admin/src/components/tenants/suspend-confirm-dialog.tsx
apps/admin/src/hooks/use-tenants.ts
apps/admin/src/hooks/use-tenant-mutations.ts

# Existentes não modificados (referência)
apps/admin/src/lib/api.ts (base Axios + auth interceptors — manter)
apps/admin/src/stores/auth-store.ts
apps/admin/src/hooks/use-require-auth.ts
apps/admin/src/providers/
apps/admin/src/app/login/
apps/admin/src/app/api/auth/
```
