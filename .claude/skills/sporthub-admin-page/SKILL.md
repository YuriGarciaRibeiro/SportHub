---
name: sporthub-admin-page
description: >
  Scaffolds a new admin page in the SportHub storefront: server page component, client page component, and sidebar nav entry — following all existing admin conventions.
  Use this skill whenever the user wants to add a new section to the admin panel — even if they say "criar página admin de X", "adicionar no admin", "nova seção no painel", "tela de gerenciamento de Y", "adicionar no menu admin". Trigger any time a new admin route or panel section is needed.
---

# SportHub Admin Page

Scaffolds a new admin section in `apps/storefront/src/app/[tenantSlug]/admin/`.

## File locations

- **Frontend root:** `/Users/yurigarciaribeiro/Documents/GitHub/sporthub-front-end`
- **Admin pages:** `apps/storefront/src/app/[tenantSlug]/admin/{section}/`
- **Sidebar nav:** `apps/storefront/src/app/[tenantSlug]/admin/_components/admin-sidebar.tsx`

## Architecture pattern (two-file split)

Every admin section uses a **server component → client component** split:

**`page.tsx` (server component):** Fetches initial data via `serverFetch` during SSR, passes it as `initialData` to the client component. No `'use client'`. Minimal — just the fetch + render.

**`_components/{section}-page.tsx` (client component):** Has `'use client'` at the top. Contains all state, hooks, mutations, and JSX. Receives `initialData` as an optional prop to seed TanStack Query's cache.

## Conventions

**Imports:**
- UI components: `@workspace/ui/components/{component-name}` (Button, DataTable, PageHeader, FilterBar, MetricCard, Drawer, etc.)
- Types/DTOs: `@workspace/shared`
- Hooks: `@/hooks/use-{entity}`
- API client: `@/lib/api`
- Server fetch: `@/lib/server-fetch`

**Page header:** Always use `<PageHeader title="..." description="..." />` as the first element inside the client page.

**List pages** typically include:
- `<MetricCard>` grid (2–4 cards with relevant counts/stats)
- `<FilterBar>` with search and optional dropdowns
- `<DataTable>` with typed columns
- `<Drawer direction="right">` for detail/edit panel

**Loading/error states:**
- Use `isLoading` from TanStack Query hooks — pass to `DataTable` and `MetricCard` via `isLoading` prop
- Show `<p className="p-8 text-sm text-destructive text-center">Erro ao carregar ...</p>` on error

**Date formatting:** Use `new Intl.DateTimeFormat('pt-BR', { timeZone: 'America/Sao_Paulo', ... })` — never `toLocaleDateString()`.

**Currency:** `new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)`

## Scaffold process

### 1. Clarify intent (ask if not clear)
- Section name/slug (e.g. `tournaments`, `promotions`) — used as the route segment
- Display name in Portuguese (e.g. "Torneios") — used in sidebar and PageHeader
- Which icon from `lucide-react`? (suggest one if not given)
- Minimum role to see in sidebar: `Staff=1`, `Manager=2`, `Owner=3`
- Does it need SSR initial data fetch? (yes for list pages, no for simple forms)
- What API endpoint does it list? (e.g. `/api/tournaments`)

### 2. Generate server page

`apps/storefront/src/app/[tenantSlug]/admin/{section}/page.tsx`:
```tsx
import { {Entity}Dto } from '@workspace/shared';
import { serverFetch } from '@/lib/server-fetch';
import { {Entity}PageClient } from './_components/{section}-page';

interface {Entity}PageProps {
  params: Promise<{ tenantSlug: string }>;
}

export default async function {Entity}Page({ params }: {Entity}PageProps) {
  const { tenantSlug } = await params;
  const initialData = await serverFetch<{Entity}Dto[]>(
    '/api/{entities}',
    tenantSlug
  ) ?? undefined;

  return <{Entity}PageClient initialData={initialData} />;
}
```

If the page doesn't need SSR data (e.g. it's a settings-style page):
```tsx
import { {Entity}PageClient } from './_components/{section}-page';

export default function {Entity}Page() {
  return <{Entity}PageClient />;
}
```

### 3. Generate client page component

`apps/storefront/src/app/[tenantSlug]/admin/{section}/_components/{section}-page.tsx`:
```tsx
'use client';

import { useState } from 'react';
import { {Icon} } from 'lucide-react';
import { PageHeader } from '@workspace/ui/components/page-header';
import { FilterBar } from '@workspace/ui/components/filter-bar';
import { DataTable, type DataTableColumn } from '@workspace/ui/components/data-table';
import { MetricCard } from '@workspace/ui/components/metric-card';
import { {Entity}Dto } from '@workspace/shared';
import { use{Entity} } from '@/hooks/use-{entity}';

interface {Entity}PageClientProps {
  initialData?: {Entity}Dto[];
}

export function {Entity}PageClient({ initialData }: {Entity}PageClientProps) {
  const [search, setSearch] = useState('');
  const { data = initialData ?? [], isLoading, isError } = use{Entity}();

  const filtered = data.filter((item) =>
    !search || item.name.toLowerCase().includes(search.toLowerCase())
  );

  const columns: DataTableColumn<{Entity}Dto>[] = [
    {
      key: 'name',
      label: 'Nome',
      render: (item) => (
        <span className="text-[13px] font-semibold text-foreground">{item.name}</span>
      ),
    },
    // TODO: add more columns
  ];

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="{DisplayName}"
        description="TODO: descrição da seção"
      />

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <MetricCard label="Total" value={filtered.length} isLoading={isLoading} />
      </div>

      <FilterBar
        search={{
          value: search,
          onChange: setSearch,
          placeholder: 'Buscar...',
        }}
      />

      <div className="rounded-sm border border-border bg-card overflow-hidden px-2 pt-1 pb-2">
        {isError ? (
          <p className="p-8 text-sm text-destructive text-center">Erro ao carregar {displayName}.</p>
        ) : (
          <DataTable<{Entity}Dto>
            columns={columns}
            data={filtered}
            isLoading={isLoading}
          />
        )}
      </div>
    </div>
  );
}
```

### 4. Add sidebar nav entry

Edit `apps/storefront/src/app/[tenantSlug]/admin/_components/admin-sidebar.tsx`.

Add the import for the chosen icon (if not already imported):
```tsx
import { ..., {Icon} } from 'lucide-react';
```

Add the nav entry to `NAV_ITEMS` array, in the right position by role order (Staff → Manager → Owner):
```tsx
{ href: 'admin/{section}', label: '{DisplayName}', icon: {Icon}, minRole: UserRole.{Role} },
```

### 5. Print summary

```
✓ Arquivos criados:
  apps/storefront/src/app/[tenantSlug]/admin/{section}/page.tsx
  apps/storefront/src/app/[tenantSlug]/admin/{section}/_components/{section}-page.tsx

✓ Arquivos editados:
  apps/storefront/src/app/[tenantSlug]/admin/_components/admin-sidebar.tsx  ← nav entry adicionado

⚠ Próximos passos:
  1. Completar colunas da DataTable no cliente
  2. Adicionar MetricCards relevantes
  3. Se precisa de mutations (create/edit/delete), usar sporthub-frontend-feature para gerar os hooks
  4. Se o endpoint ainda não existe no backend, usar sporthub-scaffold + sporthub-migration
  5. Rodar: pnpm --filter storefront build  (para checar tipos)
```

## Key things to get right

- `page.tsx` must NOT have `'use client'` — it's a Next.js Server Component
- The client component file must have `'use client'` as the very first line
- `serverFetch` returns `null` on error — always use `?? undefined` so TanStack Query uses its own fetching as fallback
- `params` in Next.js 15 is a `Promise<{...}>` — must `await params` before destructuring
- Nav items are filtered by `userRole >= item.minRole` — choose the minimum role that should see this section
- `href` in `NAV_ITEMS` is relative (e.g. `'admin/tournaments'`), not absolute — `tenantPath()` adds the tenant prefix at render time
