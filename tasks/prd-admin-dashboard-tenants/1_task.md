# Tarefa 1.0: Correção de tipos compartilhados (`@workspace/shared`)

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>LOW</complexity>

Corrigir e expandir os tipos TypeScript no pacote `@workspace/shared` para refletir exatamente o que o backend retorna. Esta tarefa é um pré-requisito bloqueante para todas as demais — sem os tipos corretos, `api.ts` e os componentes não compilam corretamente.

<requirements>
- Adicionar `ownerFirstName: string | null` e `ownerLastName: string | null` ao `TenantResponse`
- Corrigir `TenantStatus`: remover `Provisioning` (não existe no backend), adicionar `Canceled = 'Canceled'`
- Adicionar interface `TenantUserResponse` (usuários de um tenant)
- Adicionar interface `ProvisionTenantResponse` (resposta do provisioning)
- Confirmar que `PaginatedResponse<T>` possui `hasPreviousPage` e `hasNextPage`
</requirements>

## Subtarefas

- [ ] 1.1 Atualizar `TenantResponse` em `packages/shared/src/types/tenant.ts` — adicionar `ownerFirstName` e `ownerLastName`
- [ ] 1.2 Corrigir `TenantStatus` em `packages/shared/src/types/enums.ts` — remover `Provisioning`, adicionar `Canceled`
- [ ] 1.3 Adicionar `TenantUserResponse` e `ProvisionTenantResponse` em `tenant.ts`
- [ ] 1.4 Verificar `PaginatedResponse<T>` em `common.ts` e adicionar `hasPreviousPage`/`hasNextPage` se ausentes
- [ ] 1.5 Garantir que os novos tipos são exportados no barrel (`packages/shared/src/index.ts`)
- [ ] 1.6 Buscar por usos de `TenantStatus.Provisioning` no repositório inteiro e corrigir antes de remover o valor

## Detalhes de Implementação

Ver seção **"Correções em `packages/shared`"** da `techspec.md` para os diffs exatos de cada tipo.

Ponto de atenção: o `apps/storefront` também importa `TenantStatus` — verificar se há algum uso de `Provisioning` antes de remover.

## Critérios de Sucesso

- `tsc --noEmit` passa sem erros em `packages/shared`, `apps/admin` e `apps/storefront`
- Nenhuma referência a `TenantStatus.Provisioning` restante no repositório
- `TenantResponse` reflete todos os campos retornados pelo endpoint `GET /api/tenants`

## Testes da Tarefa

- [ ] Teste de unidade: verificar que `TenantStatus` contém exatamente `Active`, `Suspended` e `Canceled`
- [ ] Teste de compilação: `npm run type-check` nos três pacotes (`shared`, `admin`, `storefront`) sem erros

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `packages/shared/src/types/tenant.ts`
- `packages/shared/src/types/enums.ts`
- `packages/shared/src/types/common.ts`
- `packages/shared/src/index.ts`
- `apps/admin/src/lib/api.ts` (referência — não modificar nesta tarefa)
- `apps/storefront/src/**` (verificar usos de `TenantStatus.Provisioning`)
