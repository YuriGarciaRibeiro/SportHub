# Tarefa 8.0: Modais e drawers — provisionar, detalhe e confirmar suspensão

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>HIGH</complexity>

Implementar os três componentes de sobreposição que completam os fluxos de ação da tela de tenants:
1. **Modal de provisionar** — formulário com validação Zod para criar novo tenant
2. **Drawer de detalhe** — painel lateral com informações do tenant e lista de usuários paginada
3. **AlertDialog de confirmação** — confirmação antes de suspender um tenant

Depende da tarefa **7.0** (tela de tenants, que expõe os pontos de integração: `selectedTenantId`, callbacks `onProvision`, `onSuspendConfirm`).

**Seguir processo Red-Green-Refactor: escrever os testes antes da implementação.**

<requirements>
- Modal de provisionar: campos Slug (kebab-case, obrigatório), Nome (obrigatório), Email do owner (opcional), Nome e Sobrenome do owner (opcionais); validação Zod; fechar ao submeter com sucesso; toast de sucesso
- Drawer de detalhe: exibir todos os campos de `TenantResponse`; lista paginada de usuários com busca; skeleton durante carregamento; botões de Suspender/Ativar dentro do drawer também
- AlertDialog de suspensão: mensagem de confirmação clara com nome do tenant; botão "Confirmar" (destructive) e "Cancelar"; só executar a mutation após confirmação
- Todos os erros de API exibidos via toast (não travar a tela)
</requirements>

## Subtarefas

- [ ] 8.1 Escrever testes (Red): casos para validação do formulário, submit com sucesso/erro, abertura/fechamento dos componentes **antes** de implementar
- [ ] 8.2 Criar `apps/admin/src/components/tenants/provision-modal.tsx`:
  - Usar `Dialog`, `DialogContent`, `DialogHeader`, `DialogFooter` de `@workspace/ui`
  - Formulário com `react-hook-form` + `zodResolver` e o schema `provisionSchema` da tech spec
  - Slug: validar regex `/^[a-z0-9-]+$/`, mínimo 3 caracteres
  - Submit: chamar `useProvisionTenant().mutate(data)` → fechar modal no `onSuccess`
  - Botão de submit com spinner durante `isPending`
- [ ] 8.3 Criar `apps/admin/src/components/tenants/tenant-detail-drawer.tsx`:
  - Usar `Drawer`, `DrawerContent`, `DrawerHeader` de `@workspace/ui`
  - Buscar detalhe via `useTenant(selectedTenantId)` (não fazer prop drilling de todos os campos)
  - Lista de usuários via `useTenantUsers(id, { page, searchTerm })` com paginação interna ao drawer
  - Exibir: Nome, Slug, Status badge, Owner (nome + email), Domínio customizado, Criado em, Logo URL, Cor primária
  - Botões Suspender/Ativar dentro do drawer (reutilizar lógica da tarefa 7.0)
- [ ] 8.4 Criar `apps/admin/src/components/tenants/suspend-confirm-dialog.tsx`:
  - Usar `AlertDialog` de `@workspace/ui`
  - Exibir nome do tenant na mensagem: "Tem certeza que deseja suspender **[nome]**?"
  - Botão "Confirmar" com variant `destructive`; botão "Cancelar" fecha sem executar
  - Ao confirmar: chamar `useSuspendTenant().mutate(id)`
- [ ] 8.5 Integrar os 3 componentes na tela `/tenants` (tarefa 7.0): substituir os placeholders provisórios (`window.confirm`) pelos componentes reais
- [ ] 8.6 Integrar o botão "Provisionar Tenant" no dashboard (tarefa 6.0): abrir `ProvisionModal` ao clicar
- [ ] 8.7 Refactor (Green→Refactor): garantir testes passando, remover duplicações

## Detalhes de Implementação

Ver seção **"Funcionalidades Principais — Provisionar novo tenant"** e **"Detalhe do Tenant"** do `prd.md` (RF-15 a RF-23) e **"Interfaces Principais — Zod schema do formulário"** do `techspec.md`.

Schema Zod do formulário de provisionar:
```ts
const provisionSchema = z.object({
  slug: z.string().min(3).regex(/^[a-z0-9-]+$/, 'Apenas letras minúsculas, números e hífens'),
  name: z.string().min(2, 'Nome obrigatório'),
  ownerEmail: z.string().email('E-mail inválido').optional().or(z.literal('')),
  ownerFirstName: z.string().optional(),
  ownerLastName: z.string().optional(),
});
```

O drawer busca os dados do tenant via query separada (`useTenant(id)`) — não recebe props do componente pai além do `id`. Isso aproveita o cache do TanStack Query: se o tenant já foi buscado na listagem, o detalhe abre instantaneamente.

## Critérios de Sucesso

- Submeter formulário com slug inválido (`"Meu Tenant"`) exibe erro de validação sem chamar a API
- Submeter formulário válido cria o tenant, fecha o modal e atualiza a listagem automaticamente
- Drawer de detalhe abre com os dados corretos do tenant selecionado
- Lista de usuários no drawer é paginável
- AlertDialog de suspensão não executa a ação se o usuário clicar em "Cancelar"
- Após suspender via drawer, o badge de status na tabela principal atualiza

## Testes da Tarefa

- [ ] Teste de unidade (`ProvisionModal`): submit com slug `"Meu Tenant"` exibe erro "Apenas letras minúsculas, números e hífens"
- [ ] Teste de unidade (`ProvisionModal`): campos obrigatórios vazios bloqueiam o submit
- [ ] Teste de unidade (`ProvisionModal`): submit válido chama `useProvisionTenant().mutate` com os dados corretos
- [ ] Teste de unidade (`ProvisionModal`): modal fecha quando `isPending = false` e a mutation teve sucesso
- [ ] Teste de unidade (`SuspendConfirmDialog`): clicar "Cancelar" não chama `useSuspendTenant`
- [ ] Teste de unidade (`SuspendConfirmDialog`): clicar "Confirmar" chama `useSuspendTenant().mutate(id)` com o id correto
- [ ] Teste de unidade (`TenantDetailDrawer`): renderiza nome, slug e status do tenant mockado
- [ ] Teste de integração: provisionar tenant com sucesso → modal fecha → tenant aparece na lista (via invalidação de cache)

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `apps/admin/src/components/tenants/provision-modal.tsx` ← criar
- `apps/admin/src/components/tenants/tenant-detail-drawer.tsx` ← criar
- `apps/admin/src/components/tenants/suspend-confirm-dialog.tsx` ← criar
- `apps/admin/src/app/(dashboard)/tenants/page.tsx` ← integrar os novos componentes
- `apps/admin/src/app/(dashboard)/page.tsx` ← integrar botão de provisionar com o modal
- `apps/admin/src/hooks/use-tenants.ts` (referência — `useTenant`, `useTenantUsers`)
- `apps/admin/src/hooks/use-tenant-mutations.ts` (referência — mutations)
- `packages/ui/src/components/dialog.tsx`
- `packages/ui/src/components/drawer.tsx`
- `packages/ui/src/components/alert-dialog.tsx`
- `packages/ui/src/components/scroll-area.tsx`
- `packages/shared/src/types/tenant.ts`
