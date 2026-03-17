# Tarefa 5.0: Hooks React Query de tenants

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>MEDIUM</complexity>

Criar os hooks React Query que encapsulam toda a lógica de data fetching de tenants. Os componentes de UI (tarefas 6, 7 e 8) vão consumir esses hooks — ter a camada de dados bem definida antes dos componentes evita retrabalho.

Depende da tarefa **3.0** (api.ts corrigido com os tipos e funções corretos).

Pode rodar **em paralelo com a tarefa 4.0**.

<requirements>
- Definir `tenantKeys` como objeto de query keys para cache consistente e invalidação precisa
- Criar `useTenants(params)` — lista paginada com filtros
- Criar `useTenantStats()` — 3 queries paralelas para total, ativos e suspensos
- Criar `useTenant(id)` — detalhe de um tenant
- Criar `useTenantUsers(id, params)` — usuários de um tenant (paginado)
- Criar `useProvisionTenant()` — mutation de criação com invalidação de cache
- Criar `useSuspendTenant()` — mutation de suspensão com invalidação
- Criar `useActivateTenant()` — mutation de ativação com invalidação
- Mutations devem exibir toast de sucesso/erro via `sonner`
</requirements>

## Subtarefas

- [ ] 5.1 Criar `apps/admin/src/hooks/use-tenants.ts` com `tenantKeys` e hooks de query: `useTenants`, `useTenantStats`, `useTenant`, `useTenantUsers`
- [ ] 5.2 Criar `apps/admin/src/hooks/use-tenant-mutations.ts` com: `useProvisionTenant`, `useSuspendTenant`, `useActivateTenant`
- [ ] 5.3 Cada mutation deve chamar `queryClient.invalidateQueries({ queryKey: tenantKeys.lists() })` no `onSuccess`
- [ ] 5.4 Mutations devem chamar `toast.success(mensagem)` no `onSuccess` e `toast.error(err.message)` no `onError`

## Detalhes de Implementação

Ver seção **"Modelos de Dados — Query keys e React Query hooks"** da `techspec.md` para a estrutura exata do `tenantKeys` e o padrão das 3 queries paralelas do `useTenantStats`.

Padrão das mutations:
```ts
export function useProvisionTenant() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ProvisionTenantRequest) => tenantsApi.provision(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
      toast.success('Tenant provisionado com sucesso!');
    },
    onError: (err: ApiError) => toast.error(err.detail ?? 'Erro ao provisionar tenant'),
  });
}
```

## Critérios de Sucesso

- `useTenantStats()` retorna `{ total, active, suspended, isLoading }` corretamente
- `useTenants({ page: 1, status: 'Active' })` faz a query correta e retorna `PaginatedResponse<TenantResponse>`
- Após `useProvisionTenant().mutate(data)`, a lista de tenants é re-buscada automaticamente
- Toast de sucesso/erro aparece após cada mutation

## Testes da Tarefa

- [ ] Teste de unidade (`useTenantStats`): mockar `tenantsApi.list` para retornar `{ totalCount: 5 }` e verificar que `total`, `active` e `suspended` são preenchidos corretamente
- [ ] Teste de unidade (`useProvisionTenant`): verificar que `invalidateQueries` é chamado com `tenantKeys.lists()` após sucesso
- [ ] Teste de unidade (`useSuspendTenant`): verificar que toast de sucesso é exibido e que toast de erro é exibido quando a mutation falha
- [ ] Teste de integração: `useTenants` com `{ searchTerm: 'test' }` gera chamada para `/api/tenants?searchTerm=test`

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `apps/admin/src/hooks/use-tenants.ts` ← criar
- `apps/admin/src/hooks/use-tenant-mutations.ts` ← criar
- `apps/admin/src/lib/api.ts` (referência — `tenantsApi`)
- `apps/admin/src/lib/query-client.ts` (referência — configuração do QueryClient)
- `packages/shared/src/types/tenant.ts` (tipos consumidos)
