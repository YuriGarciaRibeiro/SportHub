# Tarefa 4.0: Layout base do painel — sidebar, header e route group protegido

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>MEDIUM</complexity>

Construir o shell do painel SuperAdmin: route group `(dashboard)` com layout protegido, sidebar de navegação e header com dados do usuário e logout. Toda página futura do painel herda este layout automaticamente.

A `app/page.tsx` atual (placeholder) é substituída pelo novo `(dashboard)/page.tsx` (temporariamente vazio, preenchido na tarefa 6.0).

Depende das tarefas **2.0** (componentes shadcn) e **3.0** (api.ts corrigido).

<requirements>
- Criar `apps/admin/src/app/(dashboard)/layout.tsx` com proteção de autenticação e verificação de role SuperAdmin
- Criar `apps/admin/src/components/layout/sidebar.tsx` com links para Dashboard (`/`) e Tenants (`/tenants`)
- Criar `apps/admin/src/components/layout/header.tsx` com nome do usuário, badge "SuperAdmin" e botão de logout
- Criar `apps/admin/src/app/(dashboard)/page.tsx` com placeholder (será preenchido na tarefa 6.0)
- Criar `apps/admin/src/app/(dashboard)/tenants/page.tsx` com placeholder (será preenchido na tarefa 7.0)
- Remover ou esvaziar `apps/admin/src/app/page.tsx` original
- Logout deve chamar `authApi.logout()` e redirecionar para `/login`
- Redirecionar para `/login` se não autenticado OU se role !== SuperAdmin
</requirements>

## Subtarefas

- [ ] 4.1 Criar `(dashboard)/layout.tsx` — usar `useRequireAuth()` + verificar `user.role === UserRole.SuperAdmin`; renderizar `<Sidebar>` e `<Header>` ao redor do `{children}`
- [ ] 4.2 Criar `components/layout/sidebar.tsx` — links de navegação com destaque do item ativo via `usePathname()`
- [ ] 4.3 Criar `components/layout/header.tsx` — exibir `user.fullName`, badge "SuperAdmin" e botão de logout
- [ ] 4.4 Implementar logout no header: chamar `authApi.logout()` → `router.push('/login')`
- [ ] 4.5 Criar `(dashboard)/page.tsx` com conteúdo placeholder (`<p>Dashboard em construção</p>`)
- [ ] 4.6 Criar `(dashboard)/tenants/page.tsx` com conteúdo placeholder
- [ ] 4.7 Remover conteúdo de `app/page.tsx` (o route group assume a rota `/`)

## Detalhes de Implementação

Ver seção **"Arquitetura do Sistema — Visão Geral dos Componentes"** e **"Pontos de Integração — Autorização no layout"** da `techspec.md`.

O `useRequireAuth()` já existe em `hooks/use-require-auth.ts`. A verificação de role é adicional:
```ts
if (!isLoading && (!isAuthenticated || user?.role !== UserRole.SuperAdmin)) {
  router.replace('/login');
}
```

O layout deve exibir um spinner/skeleton enquanto `isLoading === true` para evitar flash de conteúdo não autorizado.

## Critérios de Sucesso

- Acessar `/` sem login redireciona para `/login`
- Acessar `/` com login de role diferente de SuperAdmin redireciona para `/login`
- Acessar `/` com SuperAdmin exibe sidebar + header + placeholder
- Navegar entre `/` e `/tenants` não recarrega a sidebar (layout persistente)
- Logout limpa a sessão e redireciona para `/login`

## Testes da Tarefa

- [ ] Teste de unidade: `Sidebar` renderiza links para `/` e `/tenants`
- [ ] Teste de unidade: `Header` exibe `fullName` e badge "SuperAdmin" com dados mockados do auth store
- [ ] Teste de integração: acessar `(dashboard)/layout.tsx` sem token → redirect para `/login`
- [ ] Teste de integração: acessar com role `Admin` (não SuperAdmin) → redirect para `/login`

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `apps/admin/src/app/(dashboard)/layout.tsx` ← criar
- `apps/admin/src/app/(dashboard)/page.tsx` ← criar
- `apps/admin/src/app/(dashboard)/tenants/page.tsx` ← criar
- `apps/admin/src/app/page.tsx` ← esvaziar/remover
- `apps/admin/src/components/layout/sidebar.tsx` ← criar
- `apps/admin/src/components/layout/header.tsx` ← criar
- `apps/admin/src/hooks/use-require-auth.ts` (referência — não modificar)
- `apps/admin/src/stores/auth-store.ts` (referência — não modificar)
- `apps/admin/src/lib/api.ts` — usar `authApi.logout()`
- `packages/shared/src/types/enums.ts` — usar `UserRole.SuperAdmin`
