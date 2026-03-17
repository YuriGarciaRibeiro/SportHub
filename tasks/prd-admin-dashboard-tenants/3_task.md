# Tarefa 3.0: Correção e expansão da camada de API (`api.ts`)

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>LOW</complexity>

Corrigir o `tenantsApi` em `apps/admin/src/lib/api.ts` para refletir os contratos reais do backend: paginação, filtros tipados e o endpoint de usuários de tenant que está faltando. Atualmente `tenantsApi.list()` retorna `TenantResponse[]` quando deveria retornar `PaginatedResponse<TenantResponse>`.

Depende da **tarefa 1.0** (tipos corretos precisam existir antes).

<requirements>
- Corrigir `tenantsApi.list()` para aceitar `TenantsListParams` e retornar `PaginatedResponse<TenantResponse>`
- Adicionar `tenantsApi.getUsers(id, params)` para `GET /api/tenants/{id}/users`
- Corrigir o tipo de retorno de `tenantsApi.provision()` para `ProvisionTenantResponse`
- Definir as interfaces de params (`TenantsListParams`, `TenantUsersParams`) no próprio `api.ts` ou em `@workspace/shared`
- Manter as funções existentes (`suspend`, `activate`, `getById`, `updateBranding`) sem alteração de assinatura
</requirements>

## Subtarefas

- [ ] 3.1 Definir interfaces `TenantsListParams` e `TenantUsersParams`
- [ ] 3.2 Corrigir `tenantsApi.list(params?)` — tipo de retorno `PaginatedResponse<TenantResponse>`, passar `params` como query string
- [ ] 3.3 Corrigir `tenantsApi.provision()` — tipo de retorno `ProvisionTenantResponse`
- [ ] 3.4 Adicionar `tenantsApi.getUsers(id, params?)` — `GET /api/tenants/{id}/users`, retorna `PaginatedResponse<TenantUserResponse>`
- [ ] 3.5 Verificar que `tenantsApi.getById()` retorna `TenantResponse` completo (com `ownerFirstName`/`ownerLastName` agora presentes)

## Detalhes de Implementação

Ver seção **"Interfaces Principais — `tenantsApi` corrigido"** da `techspec.md` para os tipos e assinaturas exatos.

Ponto crítico: o Axios serializa objetos passados como `{ params }` automaticamente como query string — não é necessário construir a URL manualmente.

## Critérios de Sucesso

- `tenantsApi.list({ page: 1, pageSize: 10, status: 'Active' })` compila sem erro de tipo
- `tenantsApi.getUsers('uuid', { searchTerm: 'joao' })` compila sem erro de tipo
- `npm run type-check` em `apps/admin` passa sem erros

## Testes da Tarefa

- [ ] Teste de unidade: mockar `axios` e verificar que `tenantsApi.list({ status: 'Active' })` faz `GET /api/tenants?status=Active`
- [ ] Teste de unidade: verificar que `tenantsApi.getUsers('id', { page: 2 })` faz `GET /api/tenants/id/users?page=2`
- [ ] Teste de compilação: `npm run type-check` em `apps/admin` sem erros

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `apps/admin/src/lib/api.ts`
- `packages/shared/src/types/tenant.ts`
- `packages/shared/src/types/common.ts`
