---
name: sporthub-api-client
description: Adds a new API service object to the SportHub storefront api.ts client, following the project's exact service pattern with Axios. Use this skill whenever the user wants to add API endpoints to the frontend client, connect a new backend resource, add methods to api.ts, or create the API layer for a new feature — even if they say "adicionar endpoint de X", "conectar API de Y", "criar serviço de Z no frontend", "adicionar chamada para X", or "expor endpoint Y no cliente".
---

# sporthub-api-client

Adiciona um novo objeto de serviço em `apps/storefront/src/lib/api.ts`, seguindo o padrão exato do projeto.

## Localização e Padrão

O arquivo está em `apps/storefront/src/lib/api.ts`. Todos os serviços seguem este padrão:

```ts
export const xxxApi = {
  list: () =>
    api.get<XxxDto[]>('/api/xxxs').then((res) => res.data),

  getById: (id: string) =>
    api.get<XxxDto>(`/api/xxxs/${id}`).then((res) => res.data),

  create: (data: CreateXxxRequest) =>
    api.post<XxxDto>('/api/xxxs', data).then((res) => res.data),

  update: (id: string, data: UpdateXxxRequest) =>
    api.put<XxxDto>(`/api/xxxs/${id}`, data).then((res) => res.data),

  delete: (id: string) =>
    api.delete(`/api/xxxs/${id}`),
};
```

## Pontos importantes do padrão

- Sempre use a instância `api` (axios com interceptors de auth + X-Tenant-Slug já configurados)
- Retorne sempre `res.data` — nunca retorne a response inteira
- Para listas que podem vir paginadas: `Array.isArray(res.data) ? res.data : (res.data.items ?? [])`
- Para upload de arquivo (multipart): use `new FormData()` e `headers: { 'Content-Type': 'multipart/form-data' }`
- Imports de tipos ficam no bloco de imports existente no topo do arquivo (junto com os outros)
- O objeto novo vai no final do arquivo (antes da última linha)

## Tipos de Request locais

Se o backend não tiver um DTO de request separado (ex: `LocationRequest`), declare a interface diretamente em `api.ts` antes do objeto de serviço — conforme o padrão de `LocationRequest` já existente no arquivo.

## Passos

1. Leia `apps/storefront/src/lib/api.ts` para entender o estado atual
2. Leia `packages/shared/src/types/` para identificar os tipos já disponíveis
3. Pergunte ao usuário quais endpoints do backend precisam ser expostos (se não estiver claro)
4. Adicione os imports necessários ao bloco de imports existente
5. Adicione o objeto de serviço no final do arquivo
6. Avise se os tipos de DTO (`XxxDto`, `CreateXxxRequest`, etc.) precisam ser criados em `packages/shared` primeiro — use a skill `sporthub-frontend-feature` para isso
