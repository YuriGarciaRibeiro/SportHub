---
name: sporthub-frontend-feature
description: >
  Scaffolds the complete frontend integration for a SportHub feature: shared DTO types, API client methods, TanStack Query hooks (queries + mutations) — following all storefront conventions.
  Use this skill whenever the user wants to consume a new or existing backend endpoint in the frontend — even if they say "integrar no front", "criar hook para X", "adicionar no frontend", "consumir endpoint de Y", "tipagem do Z no shared", "criar a api de W". Trigger any time the storefront needs to talk to a new backend resource, not just when the user explicitly says "scaffold frontend".
---

# SportHub Frontend Feature

Generates all frontend plumbing to consume a SportHub API endpoint: DTOs in `packages/shared`, API client methods in `api.ts`, and TanStack Query hooks.

## Repo layout

- **Frontend root:** `/Users/yurigarciaribeiro/Documents/GitHub/sporthub-front-end`
- **Shared types:** `packages/shared/src/types/` — exported from `packages/shared/src/index.ts`
- **API client:** `apps/storefront/src/lib/api.ts` — one `xxxApi` object per resource
- **Query hooks:** `apps/storefront/src/hooks/use-xxx.ts` — read-only queries
- **Mutation hooks:** `apps/storefront/src/hooks/use-xxx-mutations.ts` — writes (create/update/delete)

## Conventions

**Shared types (`packages/shared/src/types/{entity}.ts`):**
- One file per domain (e.g. `court.ts`, `reservation.ts`)
- DTOs use `interface`, not `class`; IDs are `string` (not `Guid`)
- Dates are `string` (ISO format); nullable fields are `Type | null`
- Request types end in `Request` or `Dto`; response types end in `Dto` or `Response`
- Export every new type; add `export * from './types/{entity}'` to `index.ts` if new file

**API client (`apps/storefront/src/lib/api.ts`):**
- One exported const per resource: `export const {entity}Api = { ... }`
- All methods use `api.get/post/put/delete(...).then(res => res.data)`
- Import request/response types from `@workspace/shared`
- Append to the end of the file; never modify the axios instance setup

**Query hooks (`apps/storefront/src/hooks/use-{entity}.ts`):**
```typescript
import { useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import { XxxDto } from '@workspace/shared';
import { xxxApi } from '@/lib/api';

export function useXxx() {
  return useQuery<XxxDto[]>({
    queryKey: ['xxx'],
    queryFn: xxxApi.list,
    staleTime: 5 * 60 * 1000,
    retry: 1,
    throwOnError: false,
    meta: {
      onError: () => toast.error('Erro ao carregar xxx'),
    },
  });
}
```

**Mutation hooks (`apps/storefront/src/hooks/use-{entity}-mutations.ts`):**
```typescript
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { XxxRequest } from '@workspace/shared';
import { xxxApi } from '@/lib/api';

export function useCreateXxx() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: XxxRequest) => xxxApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['xxx'] });
      toast.success('Xxx criado com sucesso');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Erro ao criar xxx');
    },
  });
}
```

**Query key conventions:**
- List: `['entity']`
- By ID: `['entity', id]`
- Filtered/parameterized: `['entity', 'filter-name', ...params]`
- When a mutation succeeds, invalidate the affected query keys

## Scaffold process

### 1. Clarify intent (ask if not clear)
- Which entity/resource? (e.g. `Tournament`, `Promotion`)
- Which operations? list, getById, create, update, delete, or custom actions
- What does the backend return? (check the Response class in `SportHub/src/SportHub.Application/UseCases/`)
- Is the endpoint authenticated? tenant-scoped?

### 2. Generate shared types

Create or update `packages/shared/src/types/{entity}.ts`:
```typescript
export interface {Entity}Dto {
  id: string;
  // mirror the backend Response class fields, converting:
  // Guid → string
  // DateTime → string
  // decimal → number
  // ? → | null
}

export interface {Entity}Request {
  // mirror the backend Command class fields (input)
}
```

If creating a new file, add to `packages/shared/src/index.ts`:
```typescript
export * from './types/{entity}';
```

### 3. Generate API client methods

Append to `apps/storefront/src/lib/api.ts`:
```typescript
export const {entity}Api = {
  list: () =>
    api.get<{Entity}Dto[]>('/api/{entities}').then(res => res.data),

  getById: (id: string) =>
    api.get<{Entity}Dto>(`/api/{entities}/${id}`).then(res => res.data),

  create: (data: {Entity}Request) =>
    api.post<{Entity}Dto>('/api/{entities}', data).then(res => res.data),

  update: (id: string, data: {Entity}Request) =>
    api.put<{Entity}Dto>(`/api/{entities}/${id}`, data).then(res => res.data),

  delete: (id: string) =>
    api.delete(`/api/{entities}/${id}`).then(res => res.data),
};
```

Only include methods that have corresponding backend endpoints.

### 4. Generate query hooks

Create `apps/storefront/src/hooks/use-{entity}.ts` with one `useXxx` function per read operation.

For list queries use `staleTime: 5 * 60 * 1000` (5 min).
For detail queries add `enabled: !!id` to skip when ID is absent.
For parameterized queries (filters, pagination) include params in `queryKey` and `queryFn`.

### 5. Generate mutation hooks

Create `apps/storefront/src/hooks/use-{entity}-mutations.ts` with one hook per write operation.

Each mutation must:
- Call `queryClient.invalidateQueries({ queryKey: ['{entity}'] })` on success
- Show `toast.success(...)` on success in Portuguese
- Show `toast.error(error.message || 'Erro ao ...')` on error

### 6. Print summary

```
✓ Arquivos criados/editados:
  packages/shared/src/types/{entity}.ts          ← DTOs
  packages/shared/src/index.ts                   ← export adicionado (se novo arquivo)
  apps/storefront/src/lib/api.ts                 ← {entity}Api adicionado
  apps/storefront/src/hooks/use-{entity}.ts      ← queries
  apps/storefront/src/hooks/use-{entity}-mutations.ts  ← mutations

⚠ Próximos passos:
  1. Importar os hooks onde precisar no componente
  2. Se a rota é nova, verificar se o X-Tenant-Slug é enviado corretamente (automático via interceptor)
  3. Rodar: pnpm --filter storefront build  (para checar tipos)
```

## Key things to get right

- All IDs from the backend are `Guid` but must be `string` in TypeScript
- `null` in C# becomes `| null` in TypeScript, not `| undefined` (keep consistent)
- The `api` axios instance already injects the auth token and `X-Tenant-Slug` header automatically — never add them manually
- `toast` messages must be in Portuguese
- Never modify the axios interceptors or the `resolveSlug` / `setTenantSlug` functions
- Imports from shared always use `@workspace/shared`, never relative paths to packages/
