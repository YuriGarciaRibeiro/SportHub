# Tarefa 2.0: Adição de componentes shadcn ao `@workspace/ui`

<critical>Ler os arquivos de prd.md e techspec.md desta pasta, se você não ler esses arquivos sua tarefa será invalidada</critical>

## Visão Geral

<complexity>LOW</complexity>

Instalar os componentes shadcn/ui necessários para as novas telas no pacote compartilhado `@workspace/ui`. Hoje o pacote tem apenas Button, Card, Input e Label. As telas de dashboard e tenants precisam de Table, Dialog, Drawer, Select, Skeleton, Badge, AlertDialog, Separator e ScrollArea.

Esta tarefa pode rodar **em paralelo com a 1.0**.

<requirements>
- Instalar via shadcn CLI no diretório `packages/ui`: Table, Dialog, Drawer, Select, Skeleton, Badge, AlertDialog, Separator, ScrollArea
- Garantir que todos os novos componentes são exportados pelo barrel do pacote
- Não quebrar os componentes já existentes (Button, Card, Input, Label)
</requirements>

## Subtarefas

- [ ] 2.1 Rodar `npx shadcn@latest add table dialog drawer select skeleton badge alert-dialog separator scroll-area` dentro de `packages/ui`
- [ ] 2.2 Verificar que os novos arquivos foram criados em `packages/ui/src/components/`
- [ ] 2.3 Exportar os novos componentes no barrel `packages/ui/src/index.ts` (ou equivalente)
- [ ] 2.4 Verificar que `apps/admin` e `apps/storefront` conseguem importar os novos componentes sem erro de build

## Detalhes de Implementação

Ver seção **"Pontos de Integração — `@workspace/ui`"** da `techspec.md`.

Comando a executar dentro de `packages/ui`:
```bash
npx shadcn@latest add table dialog drawer select skeleton badge alert-dialog separator scroll-area
```

## Critérios de Sucesso

- Todos os 9 componentes novos existem em `packages/ui/src/components/`
- `npm run build` em `packages/ui` passa sem erros
- Importação de `Badge` e `Skeleton` de `@workspace/ui` funciona em `apps/admin`

## Testes da Tarefa

- [ ] Teste de compilação: `npm run type-check` em `apps/admin` sem erros após importar os novos componentes
- [ ] Teste visual manual: renderizar um `<Badge>`, `<Skeleton>` e `<Table>` em uma página de teste para confirmar que o estilo está correto

<critical>SEMPRE CRIE E EXECUTE OS TESTES DA TAREFA ANTES DE CONSIDERÁ-LA FINALIZADA</critical>

## Arquivos relevantes

- `packages/ui/src/components/` (novos arquivos criados pelo shadcn CLI)
- `packages/ui/src/index.ts` (barrel de exportações)
- `packages/ui/package.json`
- `apps/admin/src/app/page.tsx` (usar para teste visual temporário)
