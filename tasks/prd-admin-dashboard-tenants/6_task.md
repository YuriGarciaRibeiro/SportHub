# Tarefa 6.0: Dashboard inicial — cards de métricas e tabela de recentes

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>MEDIUM</complexity>

Implementar a página de dashboard (`/`) com os 3 cards de métricas de tenants e a tabela dos 5 tenants mais recentes. Substituir o placeholder criado na tarefa 4.0 pela tela real.

Depende das tarefas **4.0** (layout base) e **5.0** (hooks React Query).

<requirements>
- Card: total de tenants cadastrados
- Card: total de tenants ativos
- Card: total de tenants suspensos
- Cards exibem skeleton enquanto carregam
- Tabela com os 5 tenants mais recentes: Nome, Slug, Status (badge colorido), Data de criação
- Link "Ver todos" na tabela aponta para `/tenants`
- Botão "Provisionar Tenant" que abre o `ProvisionModal` (componente criado na tarefa 8.0 — usar um placeholder de modal por enquanto ou integrar direto na tarefa 8.0)
- Estado vazio na tabela: "Nenhum tenant cadastrado ainda"
</requirements>

## Subtarefas

- [ ] 6.1 Criar `apps/admin/src/components/dashboard/stats-cards.tsx` — 3 cards usando `useTenantStats()`, com `Skeleton` durante loading
- [ ] 6.2 Criar `apps/admin/src/components/dashboard/recent-tenants-table.tsx` — tabela com `useTenants({ pageSize: 5 })`, colunas: Nome, Slug, Status badge, Criado em
- [ ] 6.3 Criar `apps/admin/src/components/tenants/tenant-status-badge.tsx` — componente `<Badge>` com cor por status: verde (Active), amarelo (Suspended), cinza (Canceled)
- [ ] 6.4 Atualizar `apps/admin/src/app/(dashboard)/page.tsx` — compor `<StatsCards>` + `<RecentTenantsTable>` + botão "Provisionar Tenant"
- [ ] 6.5 Formatar datas usando `formatDate` de `@workspace/shared/utils/format`

## Detalhes de Implementação

Ver seção **"Dashboard Inicial"** do `prd.md` (RF-05 a RF-10) e seção **"Endpoints de API Consumidos"** do `techspec.md`.

Os 3 cards vêm de `useTenantStats()` que faz 3 queries com `pageSize=1` e filtros de status diferentes — o `totalCount` do `PagedResult` já retorna o total correto sem precisar carregar todos os itens.

Layout sugerido dos cards:
```
[ Total de Tenants ] [ Tenants Ativos ] [ Tenants Suspensos ]
```

## Critérios de Sucesso

- Dashboard carrega em menos de 5 segundos (conforme objetivo do PRD)
- Os 3 cards exibem valores numéricos corretos buscados do backend
- Tabela exibe os 5 tenants mais recentes com badge de status colorido
- Link "Ver todos" navega para `/tenants`
- Esqueletos (skeletons) são exibidos durante o carregamento inicial

## Testes da Tarefa

- [ ] Teste de unidade (`StatsCards`): mockar `useTenantStats` retornando `{ total: 10, active: 8, suspended: 2 }` e verificar que os 3 valores aparecem na tela
- [ ] Teste de unidade (`RecentTenantsTable`): mockar `useTenants` e verificar que a tabela renderiza os itens e o link "Ver todos"
- [ ] Teste de unidade (`TenantStatusBadge`): verificar cor correta para cada status (Active, Suspended, Canceled)
- [ ] Teste de unidade: estado vazio — quando `items` é `[]`, exibir "Nenhum tenant cadastrado ainda"

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `apps/admin/src/app/(dashboard)/page.tsx` ← atualizar (substituir placeholder)
- `apps/admin/src/components/dashboard/stats-cards.tsx` ← criar
- `apps/admin/src/components/dashboard/recent-tenants-table.tsx` ← criar
- `apps/admin/src/components/tenants/tenant-status-badge.tsx` ← criar
- `apps/admin/src/hooks/use-tenants.ts` (referência — `useTenants`, `useTenantStats`)
- `packages/shared/src/utils/format.ts` (referência — `formatDate`)
- `packages/ui/src/components/skeleton.tsx`
- `packages/ui/src/components/badge.tsx`
- `packages/ui/src/components/card.tsx`
- `packages/ui/src/components/table.tsx`
