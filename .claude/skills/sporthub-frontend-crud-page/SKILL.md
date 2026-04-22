---
name: sporthub-frontend-crud-page
description: Scaffolds a complete CRUD admin page for SportHub storefront: DataTable listing, create/edit Dialog, delete AlertDialog, MetricCards, PageHeader, and sidebar nav entry — following all existing admin conventions. Use this skill whenever the user wants a full admin CRUD page for a resource — even if they say "criar página admin de X", "gestão de Y no painel", "CRUD admin de Z", "página para gerenciar X", "tela de administração de Y", or "adicionar seção de X no admin". Generates the complete page file at apps/storefront/src/app/[tenantSlug]/admin/<resource>/page.tsx plus a sidebar entry.
---

# sporthub-frontend-crud-page

Gera uma página de CRUD completa no painel admin do storefront, seguindo o padrão estabelecido pelas páginas existentes (ex: `admin/sports/page.tsx`).

## Estrutura Gerada

```
apps/storefront/src/app/[tenantSlug]/admin/<resource>/
└── page.tsx   (página completa com form dialog + delete dialog + tabela)
```

Além disso, adicione a entrada na sidebar em `admin/_components/admin-sidebar.tsx`.

## Anatomia de uma Página Admin CRUD

A página segue sempre esta estrutura:

```tsx
'use client';

// 1. Imports (lucide, shadcn/ui, hooks, tipos)

// 2. XxxFormDialog — Dialog para criar/editar
function XxxFormDialog({ open, onClose, initial, id }) {
  // useForm simples com useState (não React Hook Form)
  // ou React Hook Form + Zod se houver validação mais complexa
  // Chama useCreateXxx / useUpdateXxx
}

// 3. Componentes auxiliares (ex: ícone, badge de status)

// 4. Page default export
export default function AdminXxxPage() {
  // useXxxs() para listar
  // useDeleteXxx() para deletar
  // useState para controle de dialogs: createOpen, editTarget, deleteTarget

  // columns: DataTableColumn<XxxDto>[] — define as colunas da tabela
  // Cada coluna tem: key, label, render

  // Retorna:
  // - PageHeader com title, description, action (botão "Novo X")
  // - Grid de MetricCards
  // - DataTable dentro de um container com borda
  // - SportFormDialog (create) + SportFormDialog (edit)
  // - AlertDialog para confirmar deleção
}
```

## Componentes Disponíveis (de `@workspace/ui`)

| Componente | Import | Uso |
|---|---|---|
| `PageHeader` | `@workspace/ui/components/page-header` | Título + descrição + action button |
| `MetricCard` | `@workspace/ui/components/metric-card` | Card de métricas (label, value, icon, isLoading) |
| `DataTable` | `@workspace/ui/components/data-table` | Tabela de dados com colunas configuráveis |
| `EmptyState` | `@workspace/ui/components/empty-state` | Estado vazio da tabela (icon, title, action) |
| `FormField` | `@workspace/ui/components/form-field` | Label + campo + erro |
| `Button` | `@workspace/ui/components/button` | Botão (variant: outline, ghost, brand) |
| `Input` | `@workspace/ui/components/input` | Campo de texto |
| `Dialog` / `DialogContent` / `DialogHeader` / `DialogTitle` / `DialogFooter` | `@workspace/ui/components/dialog` | Modal de criação/edição |
| `AlertDialog` + filhos | `@workspace/ui/components/alert-dialog` | Modal de confirmação de deleção |

## Padrão de Colunas DataTable

```tsx
const columns: DataTableColumn<XxxDto>[] = [
  {
    key: 'name',
    label: 'Nome',
    headerClassName: 'w-[40%]',
    render: (item) => (
      <div className="flex items-center gap-3 min-w-0">
        <div className="min-w-0">
          <p className="font-bold text-sm truncate">{item.name}</p>
          <p className="text-[10px] uppercase tracking-wider text-brand font-semibold mt-0.5">Xxx</p>
        </div>
      </div>
    ),
  },
  {
    key: 'actions',
    label: 'Ações',
    headerClassName: 'w-20',
    render: (item) => (
      <div className="flex items-center gap-1 shrink-0">
        <Button size="sm" variant="ghost" className="h-7 w-7 p-0" onClick={(e) => { e.stopPropagation(); setEditTarget(item); }}>
          <Pencil className="size-3.5" />
        </Button>
        <Button size="sm" variant="ghost" className="h-7 w-7 p-0 text-destructive hover:text-destructive" onClick={(e) => { e.stopPropagation(); setDeleteTarget(item); }}>
          <Trash2 className="size-3.5" />
        </Button>
      </div>
    ),
  },
];
```

## Padrão do AlertDialog de Deleção

```tsx
<AlertDialog open={!!deleteTarget} onOpenChange={(v) => !v && setDeleteTarget(null)}>
  <AlertDialogContent>
    <AlertDialogHeader>
      <AlertDialogTitle>Remover xxx?</AlertDialogTitle>
    </AlertDialogHeader>
    <p className="text-sm text-muted-foreground px-1">
      O item <span className="font-medium text-foreground">{deleteTarget?.name}</span> será removido permanentemente.
    </p>
    <AlertDialogFooter>
      <AlertDialogCancel>Cancelar</AlertDialogCancel>
      <AlertDialogAction
        className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
        onClick={() => {
          if (deleteTarget) deleteXxx.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
        }}
      >
        {deleteXxx.isPending ? 'Removendo...' : 'Remover'}
      </AlertDialogAction>
    </AlertDialogFooter>
  </AlertDialogContent>
</AlertDialog>
```

## Entrada na Sidebar

Em `apps/storefront/src/app/[tenantSlug]/admin/_components/admin-sidebar.tsx`, adicione no array `NAV_ITEMS`:

```ts
{ href: 'admin/xxxs', label: 'Xxxs', icon: SomeIcon, minRole: UserRole.Owner },
```

Escolha o `minRole` apropriado:
- `UserRole.Staff` — visível para todos os admins
- `UserRole.Manager` — apenas manager e owner
- `UserRole.Owner` — apenas owner

## Passos

1. Confirme: qual é o recurso? Quais campos ele tem? Qual o minRole para acesso?
2. Verifique se os hooks já existem em `apps/storefront/src/hooks/use-<resource>.ts` — se não, use `sporthub-frontend-hook`
3. Verifique os tipos de DTO disponíveis em `packages/shared/src/types/`
4. Gere a página completa em `apps/storefront/src/app/[tenantSlug]/admin/<resource>/page.tsx`
5. Adicione a entrada na sidebar
6. Avise se há dependências faltando (hooks, api service, tipos)

## Observações

- Se o recurso tiver imagem de capa, inclua o padrão de upload com `<input type="file" className="sr-only">` + label estilizado
- Se tiver campos complexos (select de outros recursos, horários, endereço), gere um `_components/xxx-form-dialog.tsx` separado para manter a página enxuta
- Não use React Hook Form na página diretamente — use `useState` para o form state simples (como no padrão `sports/page.tsx`) a menos que haja validação complexa
