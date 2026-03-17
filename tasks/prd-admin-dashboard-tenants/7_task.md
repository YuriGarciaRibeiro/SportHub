# Tarefa 7.0: Tela de gestão de tenants — tabela paginada com filtros

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>HIGH</complexity>

Implementar a página `/tenants` completa com tabela paginada, filtros de busca por texto e status, ações inline de Suspender/Ativar e botão para abrir o modal de provisionar. Os modais e drawers são implementados na tarefa 8.0 — esta tarefa deixa os pontos de integração prontos (callbacks e estado de seleção).

Esta é uma tarefa de alta complexidade por combinar: estado local de filtros, paginação controlada, invalidação de cache após mutations e coordenação com os componentes de modal/drawer da tarefa 8.0.

Depende das tarefas **4.0** (layout) e **5.0** (hooks).

**Seguir processo Red-Green-Refactor: escrever os testes antes da implementação.**

<requirements>
- Tabela paginada com colunas: Nome, Slug, Status (badge), Owner (email), Criado em, Ações
- Filtro de texto livre (`searchTerm`) e seletor de status (Todos / Ativo / Suspenso)
- Paginação: controles de página anterior/próxima, exibir "Página X de Y"
- pageSize padrão: 10 por página
- Estado de loading: skeleton nas linhas da tabela (não recarregar a página inteira)
- Estado vazio: mensagem "Nenhum tenant encontrado"
- Botão "Novo Tenant" (abre modal — integrar com tarefa 8.0)
- Botão "Ver detalhes" por linha (abre drawer — integrar com tarefa 8.0)
- Botão "Suspender" visível apenas para status Active, com confirmação antes de executar
- Botão "Ativar" visível apenas para status Suspended
- Manter estado de filtros ao navegar de volta para a página
</requirements>

## Subtarefas

- [ ] 7.1 Escrever testes (Red): casos para filtro, paginação, ações inline e estados de loading/vazio **antes** de implementar
- [ ] 7.2 Criar `apps/admin/src/components/tenants/tenants-filters.tsx` — input de texto + `Select` de status, com debounce de 300ms no searchTerm
- [ ] 7.3 Criar `apps/admin/src/components/tenants/tenants-table.tsx` — tabela com `useTenants(params)`, skeleton rows durante loading, ações por linha
- [ ] 7.4 Implementar paginação controlada: estado `page` local, botões Anterior/Próximo habilitados com base em `hasPreviousPage`/`hasNextPage`
- [ ] 7.5 Implementar ações inline: "Suspender" chama `useSuspendTenant` (com dialog de confirmação da tarefa 8.0 — usar um `window.confirm` provisório se 8.0 ainda não estiver pronta); "Ativar" chama `useActivateTenant` diretamente
- [ ] 7.6 Adicionar estados de seleção para drawer: `selectedTenantId: string | null` — passar para o componente de drawer da tarefa 8.0
- [ ] 7.7 Atualizar `apps/admin/src/app/(dashboard)/tenants/page.tsx` — compor filtros + tabela + botão "Novo Tenant"
- [ ] 7.8 Refactor (Green→Refactor): garantir que os testes da 7.1 passam, limpar código

## Detalhes de Implementação

Ver seção **"Gestão de Tenants"** do `prd.md` (RF-11 a RF-20) e **"Considerações Técnicas — Estado dos filtros"** do `techspec.md`.

Decisão da tech spec: estado dos filtros em `useState` local na página (não URL params nesta fase).

Debounce no searchTerm evita uma requisição por tecla digitada — usar `setTimeout`/`clearTimeout` simples ou `useDebounce` customizado.

Ao mudar qualquer filtro, resetar `page` para 1.

## Critérios de Sucesso

- Filtrar por "Active" exibe apenas tenants ativos na tabela
- Digitar no campo de busca filtra os resultados após 300ms
- Botão "Próximo" desaparece na última página
- Clicar em "Suspender" em um tenant ativo muda o status na tabela após confirmação
- Clicar em "Ativar" em um tenant suspenso muda o status imediatamente após confirmação
- Skeleton aparece durante carregamento ao mudar de página ou filtro

## Testes da Tarefa

- [ ] Teste de unidade (`TenantsFilters`): mudar o select para "Active" chama o callback `onStatusChange('Active')`
- [ ] Teste de unidade (`TenantsFilters`): debounce — digitar não chama `onSearchChange` imediatamente, mas sim após 300ms
- [ ] Teste de unidade (`TenantsTable`): lista vazia exibe "Nenhum tenant encontrado"
- [ ] Teste de unidade (`TenantsTable`): linha com status Active exibe botão "Suspender" e não exibe "Ativar"; linha Suspended exibe o inverso
- [ ] Teste de unidade (`TenantsTable`): skeleton rows são renderizados quando `isLoading = true`
- [ ] Teste de integração: mudar filtro de status dispara nova query com `status` correto no `tenantsApi.list`
- [ ] Teste de integração: clicar "Ativar" em tenant Suspended chama `tenantsApi.activate(id)` e invalida a query de lista

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `apps/admin/src/app/(dashboard)/tenants/page.tsx` ← atualizar (substituir placeholder)
- `apps/admin/src/components/tenants/tenants-table.tsx` ← criar
- `apps/admin/src/components/tenants/tenants-filters.tsx` ← criar
- `apps/admin/src/components/tenants/tenant-status-badge.tsx` (criado na tarefa 6.0)
- `apps/admin/src/hooks/use-tenants.ts` (referência)
- `apps/admin/src/hooks/use-tenant-mutations.ts` (referência)
- `packages/ui/src/components/table.tsx`
- `packages/ui/src/components/select.tsx`
- `packages/ui/src/components/skeleton.tsx`
- `packages/ui/src/components/input.tsx`
