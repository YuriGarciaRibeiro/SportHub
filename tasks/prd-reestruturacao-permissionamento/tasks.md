# Tasks: Reestruturação do Sistema de Permissionamento

> **Versão**: 1.0  
> **Data**: 2026-03-14  
> **PRD**: `prd-reestruturacao-permissionamento/prd.md`  
> **Status**: Planejamento

---

## Visão Geral

Este documento lista as tarefas necessárias para implementar a reestruturação completa do sistema de permissionamento do SportHub Backend, unificando os enums de roles, corrigindo as authorization policies e implementando gestão de membros.

---

## Resumo das Tarefas

| # | Tarefa | Complexidade | Dependências | Status |
|---|---|---|---|---|
| 1.0 | Refatoração do Modelo de Roles | HIGH | - | ✅ Completo |
| 2.0 | Atualização do Sistema de Autenticação | MEDIUM | 1.0 | ✅ Completo |
| 3.0 | Implementação de Authorization Policies | MEDIUM | 1.0, 2.0 | ✅ Completo |
| 4.0 | Aplicação de Policies nos Endpoints Existentes | MEDIUM | 3.0 | ✅ Completo |
| 5.0 | Implementação de Lógica de Cancelamento com Permissões | LOW | 3.0, 4.0 | ✅ Completo |
| 6.0 | Implementação de Endpoints de Gestão de Membros | HIGH | 3.0, 4.0 | ✅ Completo |
| 7.0 | Migração de Dados Existentes | MEDIUM | 1.0, 2.0 | ✅ Completo |
| 8.0 | Testes de Integração do Sistema de Permissionamento | HIGH | 1.0-7.0 | ✅ Completo |

---

## Detalhamento das Tarefas

### 1.0 - Refatoração do Modelo de Roles
**Complexidade**: HIGH  
**Objetivo**: Unificar os enums `UserRole` e `EstablishmentRole` em um único enum hierárquico.

**Entregáveis**:
- Enum `UserRole` atualizado com valores: Customer(0), Staff(1), Manager(2), Owner(3), SuperAdmin(99)
- Remoção completa do enum `EstablishmentRole`
- Migration EF Core para refletir mudanças
- Código legado removido

**Critérios de Sucesso**:
- ✅ Apenas 1 enum `UserRole` existe no codebase
- ✅ Hierarquia numérica permite comparação `>=`
- ✅ Nenhuma referência a `EstablishmentRole` permanece
- ✅ Testes unitários passam

---

### 2.0 - Atualização do Sistema de Autenticação
**Complexidade**: MEDIUM  
**Objetivo**: Garantir que registro, login e provisioning usem os novos valores de role.

**Entregáveis**:
- `RegisterUserHandler` atribui `UserRole.Customer`
- `TenantProvisioningService.SeedOwnerUserAsync` cria usuário com `UserRole.Owner`
- JWT claim `Role` contém valores do enum unificado
- `AuthResponse` retorna role correto

**Critérios de Sucesso**:
- ✅ Novo usuário registrado recebe role `Customer`
- ✅ Owner criado no provisioning tem role `Owner`
- ✅ Token JWT contém claim `Role` com valor correto
- ✅ Testes de integração de auth passam

---

### 3.0 - Implementação de Authorization Policies
**Complexidade**: MEDIUM  
**Objetivo**: Registrar e implementar policies baseadas no enum unificado.

**Entregáveis**:
- Policies registradas: `IsStaff`, `IsManager`, `IsOwner`, `IsSuperAdmin`
- `GlobalRoleHandler` atualizado para parsear `UserRole`
- `CurrentUserService` refatorado (remove `EstablishmentRole`, adiciona `UserRole`)
- `PolicyNames` limpo (remove policies órfãs)

**Critérios de Sucesso**:
- ✅ 4 policies funcionais registradas em `AddAuthorization`
- ✅ `GlobalRoleHandler` compara hierarquia corretamente
- ✅ `CurrentUserService.UserRole` retorna enum correto
- ✅ Testes unitários de policies passam

---

### 4.0 - Aplicação de Policies nos Endpoints Existentes
**Complexidade**: MEDIUM  
**Objetivo**: Aplicar matriz de permissões em todos os endpoints conforme PRD.

**Entregáveis**:
- Sports endpoints com policies corretas
- Courts endpoints com policies corretas
- Reservations endpoints com policies corretas
- Settings/Branding endpoints com policies corretas
- Stats endpoint com `IsManager`

**Critérios de Sucesso**:
- ✅ Todos os endpoints seguem a matriz do PRD (RF-04)
- ✅ Endpoints de gestão bloqueiam Customers
- ✅ Endpoints públicos permanecem anônimos
- ✅ Testes de autorização por endpoint passam

---

### 5.0 - Implementação de Lógica de Cancelamento com Permissões
**Complexidade**: LOW  
**Objetivo**: Permitir que Manager+ cancele qualquer reserva, Customer apenas a própria.

**Entregáveis**:
- `CancelReservationHandler` com verificação de role
- Lógica: próprio user OU role >= Manager
- Retorno 403 Forbidden quando não autorizado

**Critérios de Sucesso**:
- ✅ Customer cancela apenas própria reserva
- ✅ Manager/Owner cancelam qualquer reserva
- ✅ Staff não consegue cancelar reservas de outros
- ✅ Testes de cancelamento com permissões passam

---

### 6.0 - Implementação de Endpoints de Gestão de Membros
**Complexidade**: HIGH  
**Objetivo**: Criar endpoints para Owner gerenciar membros operacionais do tenant.

**Entregáveis**:
- `GET /api/members` - Lista membros com role >= Staff
- `PATCH /api/members/{userId}/role` - Altera role (Customer/Staff/Manager)
- `DELETE /api/members/{userId}` - Rebaixa para Customer
- Commands/Queries via MediatR
- Validators via FluentValidation
- Regras de negócio: Owner único, sem auto-rebaixamento

**Critérios de Sucesso**:
- ✅ Owner lista todos os membros operacionais
- ✅ Owner altera role entre Customer/Staff/Manager
- ✅ Owner não pode se auto-rebaixar
- ✅ Owner não pode promover a Owner
- ✅ Apenas Owner acessa esses endpoints
- ✅ Testes unitários e de integração passam

---

### 7.0 - Migração de Dados Existentes
**Complexidade**: MEDIUM  
**Objetivo**: Migrar roles existentes para os novos valores.

**Entregáveis**:
- Migration EF Core ou script SQL
- `User` → `Customer`
- `Admin` → `Owner`
- Validação de integridade pós-migração

**Critérios de Sucesso**:
- ✅ Todos os users com role `User` viram `Customer`
- ✅ Todos os users com role `Admin` viram `Owner`
- ✅ Nenhum user fica com role inválido
- ✅ Dados validados em ambiente de teste

---

### 8.0 - Testes de Integração do Sistema de Permissionamento
**Complexidade**: HIGH  
**Objetivo**: Garantir que todo o sistema de permissionamento funciona end-to-end.

**Entregáveis**:
- Testes de autorização por role em todos os endpoints
- Testes de gestão de membros (listar, alterar, rebaixar)
- Testes de cancelamento de reservas com permissões
- Testes de fluxos completos por persona (Customer, Staff, Manager, Owner, SuperAdmin)
- Testes de edge cases (auto-rebaixamento, promoção a Owner, etc.)

**Critérios de Sucesso**:
- ✅ 100% dos endpoints testados com todos os roles
- ✅ Todos os cenários de negócio cobertos
- ✅ Edge cases validados
- ✅ Suite de testes passa completamente

---

## Ordem de Execução Recomendada

```
1.0 (Refatoração Modelo) 
  ↓
2.0 (Autenticação) + 7.0 (Migração) [podem rodar em paralelo após 1.0]
  ↓
3.0 (Policies)
  ↓
4.0 (Aplicar Policies) + 6.0 (Gestão Membros) [podem rodar em paralelo após 3.0]
  ↓
5.0 (Cancelamento)
  ↓
8.0 (Testes Integração)
```

---

## Observações Importantes

- **TDD Recomendado**: Tarefas 6.0 e 8.0 devem seguir abordagem Red-Green-Refactor
- **Breaking Changes**: Tokens JWT existentes serão invalidados (forçará re-login)
- **Frontend Impact**: Admin e Storefront precisarão ajustar após conclusão
- **Documentação**: Atualizar README e documentação de API após conclusão

---

## Arquivos de Tarefas Individuais

- `1_task.md` - Refatoração do Modelo de Roles
- `2_task.md` - Atualização do Sistema de Autenticação
- `3_task.md` - Implementação de Authorization Policies
- `4_task.md` - Aplicação de Policies nos Endpoints Existentes
- `5_task.md` - Implementação de Lógica de Cancelamento com Permissões
- `6_task.md` - Implementação de Endpoints de Gestão de Membros
- `7_task.md` - Migração de Dados Existentes
- `8_task.md` - Testes de Integração do Sistema de Permissionamento
