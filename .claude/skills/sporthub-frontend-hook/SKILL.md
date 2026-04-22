---
name: sporthub-frontend-hook
description: Scaffolds TanStack Query hooks (queries + mutations) for a SportHub resource in the storefront frontend, following the project's exact hook conventions. Use this skill whenever the user wants to create hooks, queries, or mutations for a resource in the frontend — even if they say "criar hooks de X", "preciso de queries para Y", "adicionar mutations de Z", "hook para buscar X", or "integrar Y no frontend". The output is a complete hook file at apps/storefront/src/hooks/use-<resource>.ts.
---

# sporthub-frontend-hook

Gera o arquivo de hooks TanStack Query para um recurso, seguindo o padrão exato do projeto.

## Padrão do Projeto

Todos os hooks vivem em `apps/storefront/src/hooks/use-<resource>.ts`. O padrão é:

```ts
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { XxxDto, CreateXxxRequest, UpdateXxxRequest } from '@workspace/shared';
import { xxxApi } from '@/lib/api';

export function useXxxs() {
  return useQuery({
    queryKey: ['xxxs'],
    queryFn: xxxApi.list,
    staleTime: 5 * 60 * 1000,
    retry: 1,
    throwOnError: false,
  });
}

export function useCreateXxx() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateXxxRequest) => xxxApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['xxxs'] });
      toast.success('X criado com sucesso');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Erro ao criar X');
    },
  });
}
```

## O que gerar

Com base no recurso pedido, gere as funções necessárias:

- `useXxxs()` — lista todos (useQuery com `xxxApi.list`)
- `useXxx(id)` — busca por id (useQuery com `xxxApi.getById`, só se houver necessidade)
- `useCreateXxx()` — useMutation com `xxxApi.create`
- `useUpdateXxx()` — useMutation com `mutationFn: ({ id, data }) => xxxApi.update(id, data)`
- `useDeleteXxx()` — useMutation com `mutationFn: (id: string) => xxxApi.delete(id)`
- Mutations extras se o usuário mencionar (upload de imagem, etc.)

## Regras

- Toasts em português: "X criado com sucesso", "Erro ao criar X", etc.
- `invalidateQueries` com o queryKey do recurso após cada mutation de sucesso
- Se o update mudar algo que afeta outro recurso (ex: deletar sport invalida courts também), inclua o segundo `invalidateQueries`
- Importe os tipos de `@workspace/shared` e a api de `@/lib/api`
- Se os tipos ainda não existirem em `@workspace/shared`, mencione que precisam ser criados — não invente nomes
- Não adicione `meta.onError` no useQuery (o padrão moderno usa onError no useMutation diretamente)

## Passos

1. Confirme com o usuário quais operações são necessárias (list, getById, create, update, delete, e extras)
2. Verifique se o serviço de API (`xxxApi`) já existe em `apps/storefront/src/lib/api.ts` — se não, avise que a skill `sporthub-api-client` deve ser usada primeiro
3. Verifique os tipos disponíveis em `packages/shared/src/types/`
4. Gere o arquivo completo em `apps/storefront/src/hooks/use-<resource>.ts`
