# PRD: Reestruturação do Sistema de Permissionamento

> **Versão**: 1.0
> **Data**: 2026-03-14
> **Status**: Draft
> **Autor**: Cascade (assistido)

---

## 1. Problema

O sistema de autorização atual do SportHub Backend apresenta **inconsistências estruturais** que impedem o controle de acesso correto dentro de cada tenant:

1. **Dois enums de role com sobreposição semântica** — `UserRole` (User, EstablishmentMember, Admin, SuperAdmin) e `EstablishmentRole` (Staff, Manager, Owner) coexistem sem relação clara.
2. **JWT incompatível com as policies** — O login grava `UserRole.ToString()` no claim `Role` (ex: `"Admin"`), mas as policies `IsStaff`/`IsManager`/`IsOwner` tentam parsear esse claim como `EstablishmentRole` — **nunca dá match**.
3. **Policies declaradas mas não registradas** — `PolicyNames` declara `IsEstablishmentStaff/Manager/Owner`, mas elas nunca são registradas em `AddAuthorization`.
4. **Endpoints sensíveis sem restrição** — Criar quadra, criar esporte, ver stats usam `.RequireAuthorization()` genérico — qualquer usuário autenticado (inclusive um cliente) pode executar essas ações.
5. **Sem separação cliente vs. backoffice** — Não há distinção entre um cliente que faz reservas e um funcionário que gerencia o estabelecimento.
6. **Sem gestão de membros** — Não existem endpoints para o Owner convidar/remover Staff e Managers do tenant.

---

## 2. Objetivos

| # | Objetivo | Métrica de Sucesso |
|---|---|---|
| O1 | Unificar o modelo de roles em um único enum coerente | 1 enum `UserRole` com 5 valores, `EstablishmentRole` removido |
| O2 | Garantir que o JWT reflita corretamente o role do user | Claim `Role` no token corresponde ao enum unificado |
| O3 | Aplicar policies restritivas em todos os endpoints | Cada endpoint tem a policy correta conforme a matriz de permissões |
| O4 | Separar claramente Customer (cliente) de operacional (Staff/Manager/Owner) | Endpoints de gestão inacessíveis para Customers |
| O5 | Permitir que o Owner gerencie membros do tenant | Endpoints CRUD de membros funcionais e protegidos |

---

## 3. Usuários e Personas

| Persona | Role | Descrição |
|---|---|---|
| **Cliente** | `Customer` | Usuário final que busca quadras, faz reservas e gerencia sua própria conta. Não tem acesso a funcionalidades administrativas. |
| **Funcionário** | `Staff` | Empregado do estabelecimento com acesso limitado ao backoffice: visualiza reservas, auxilia no dia-a-dia operacional. |
| **Gerente** | `Manager` | Gerencia o operacional do tenant: CRUD de quadras e esportes, visualiza stats, cancela reservas de outros usuários. |
| **Dono** | `Owner` | Proprietário do tenant. Controle total: settings, branding, gestão de membros (Staff/Manager), tudo que Manager faz. |
| **Operador da Plataforma** | `SuperAdmin` | Administrador global do SportHub. Gerencia tenants (provisioning, suspensão, ativação). Opera no schema `public`. |

---

## 4. Histórias de Usuário

### Customer
- **US-01**: Como Customer, quero ver quadras e esportes disponíveis para escolher onde jogar.
- **US-02**: Como Customer, quero fazer uma reserva em uma quadra disponível.
- **US-03**: Como Customer, quero cancelar minha própria reserva.
- **US-04**: Como Customer, quero gerenciar meu perfil (nome, senha, deletar conta).
- **US-05**: Como Customer, **não devo** conseguir acessar funcionalidades administrativas (criar quadra, ver stats, etc.).

### Staff
- **US-06**: Como Staff, quero visualizar as reservas de qualquer quadra para organizar o dia-a-dia.
- **US-07**: Como Staff, **não devo** conseguir criar/editar/deletar quadras ou esportes.

### Manager
- **US-08**: Como Manager, quero criar, editar e deletar quadras.
- **US-09**: Como Manager, quero criar, editar e deletar esportes.
- **US-10**: Como Manager, quero ver as estatísticas (stats) do tenant.
- **US-11**: Como Manager, quero cancelar reservas de qualquer usuário.
- **US-12**: Como Manager, quero visualizar as reservas de qualquer quadra.

### Owner
- **US-13**: Como Owner, quero fazer tudo que o Manager faz.
- **US-14**: Como Owner, quero alterar as configurações do tenant (nome, logo, cor).
- **US-15**: Como Owner, quero convidar um usuário existente como Staff ou Manager.
- **US-16**: Como Owner, quero alterar o role de um membro (ex: promover Staff a Manager).
- **US-17**: Como Owner, quero remover um membro do backoffice (rebaixar para Customer).
- **US-18**: Como Owner, quero listar todos os membros operacionais do meu tenant.

### SuperAdmin
- **US-19**: Como SuperAdmin, quero gerenciar tenants (provisionar, suspender, ativar) — **sem alteração no escopo atual**.

---

## 5. Requisitos Funcionais

### RF-01: Enum Unificado de Roles

Substituir os dois enums atuais (`UserRole` + `EstablishmentRole`) por um único `UserRole`:

```
UserRole:
  Customer    = 0   (antigo User)
  Staff       = 1   (novo)
  Manager     = 2   (novo)
  Owner       = 3   (antigo Admin)
  SuperAdmin  = 99  (mantém)
```

O enum `EstablishmentRole` deve ser **removido** do codebase.

### RF-02: JWT com Role Unificado

O claim `Role` no JWT deve conter o valor do enum unificado (`Customer`, `Staff`, `Manager`, `Owner`, `SuperAdmin`). O `JwtService.GenerateToken` já recebe `role` como string — basta que login, register e refresh passem o `UserRole.ToString()` correto.

### RF-03: Authorization Policies Corrigidas

Registrar as seguintes policies em `AddAuthorization`:

| Policy | Regra | Descrição |
|---|---|---|
| `IsStaff` | `UserRole >= Staff` | Staff, Manager ou Owner |
| `IsManager` | `UserRole >= Manager` | Manager ou Owner |
| `IsOwner` | `UserRole >= Owner` | Somente Owner |
| `IsSuperAdmin` | `UserRole == SuperAdmin` | Somente SuperAdmin |

O `GlobalRoleHandler` deve parsear o claim como `UserRole` (não mais `EstablishmentRole`) e comparar com hierarquia numérica (`>=`).

### RF-04: Matriz de Permissões por Endpoint

| Método | Rota | Policy Requerida |
|---|---|---|
| `GET` | `/api/sports` | Anônimo |
| `GET` | `/api/sports/{id}` | Anônimo |
| `POST` | `/api/sports` | `IsManager` |
| `PUT` | `/api/sports/{id}` | `IsManager` |
| `DELETE` | `/api/sports/{id}` | `IsManager` |
| `GET` | `/api/courts` | Anônimo |
| `GET` | `/api/courts/{id}` | RequireAuth |
| `POST` | `/api/courts` | `IsManager` |
| `PUT` | `/api/courts/{id}` | `IsManager` |
| `DELETE` | `/api/courts/{id}` | `IsManager` |
| `GET` | `/api/courts/{id}/availability/{date}` | Anônimo |
| `POST` | `/api/courts/{id}/reservations` | RequireAuth |
| `GET` | `/api/courts/{id}/reservations` | `IsStaff` |
| `DELETE` | `/api/courts/{id}/reservations/{rid}` | RequireAuth (lógica: própria reserva OU `IsManager`) |
| `GET` | `/api/reservations/me` | RequireAuth |
| `GET` | `/admin/stats` | `IsManager` |
| `PUT` | `/api/settings` | `IsOwner` |
| `GET` | `/api/branding` | Anônimo |
| `POST` | `/auth/register` | Anônimo |
| `POST` | `/auth/login` | Anônimo |
| `POST` | `/auth/refresh` | Anônimo |
| `GET` | `/auth/me` | RequireAuth |
| `PUT` | `/auth/me` | RequireAuth |
| `DELETE` | `/auth/me` | RequireAuth |
| `*` | `/api/tenants/**` | `IsSuperAdmin` |

### RF-05: Registro de Novo Usuário como Customer

`RegisterUserHandler` deve atribuir `UserRole.Customer` (em vez do atual `UserRole.User`).

### RF-06: Provisioning — Owner do Tenant

`TenantProvisioningService.SeedOwnerUserAsync` deve criar o usuário com `UserRole.Owner` (em vez do atual `UserRole.Admin`).

### RF-07: CurrentUserService Atualizado

`CurrentUserService.EstablishmentRole` deve ser substituído por uma propriedade `UserRole` que parseia o claim como o enum unificado `UserRole`.

### RF-08: Endpoints de Gestão de Membros (Novo)

Novos endpoints para o Owner gerenciar membros operacionais do tenant:

| Método | Rota | Policy | Descrição |
|---|---|---|---|
| `GET` | `/api/members` | `IsOwner` | Lista todos os usuários com role Staff, Manager ou Owner no tenant |
| `PATCH` | `/api/members/{userId}/role` | `IsOwner` | Altera o role de um user (Customer↔Staff↔Manager). Owner não pode alterar o próprio role nem promover a Owner. |
| `DELETE` | `/api/members/{userId}` | `IsOwner` | Rebaixa um membro para Customer (não deleta o user) |

**Regras de negócio:**
- Owner não pode se auto-rebaixar.
- Owner não pode promover outro user a Owner (deve haver apenas 1 Owner por tenant — o criado no provisioning).
- O role só pode ser alterado entre `Customer`, `Staff` e `Manager`.
- O endpoint `GET /api/members` retorna apenas users com role `>= Staff`.

### RF-09: Cancelamento de Reserva com Verificação de Permissão

O `CancelReservationHandler` deve verificar:
- Se o user é o dono da reserva → permite cancelar.
- Se o user tem role `>= Manager` → permite cancelar reserva de outro user.
- Caso contrário → retorna 403 Forbidden.

### RF-10: Limpeza de Código Legado

- Remover `EstablishmentRole` enum.
- Remover `GlobalRoleRequirement` que referencia `EstablishmentRole`.
- Atualizar `GlobalRoleHandler` para usar `UserRole`.
- Remover policies órfãs (`IsEstablishmentStaff`, `IsEstablishmentManager`, `IsEstablishmentOwner`) de `PolicyNames`.
- Atualizar `ICurrentUserService` e `CurrentUserService`.

---

## 6. Requisitos Não-Funcionais

| # | Requisito |
|---|---|
| RNF-01 | Manter compatibilidade com o modelo multi-tenant (schema-per-tenant). O role vive na tabela `Users` do schema do tenant. |
| RNF-02 | SuperAdmin continua no schema `public` com seu fluxo separado. |
| RNF-03 | A hierarquia numérica do enum (`Customer=0 < Staff=1 < Manager=2 < Owner=3`) permite comparação `>=` simples nas policies. |
| RNF-04 | Tokens JWT existentes serão invalidados (roles antigos não vão parsear corretamente). Isso é aceitável — forçará re-login. |
| RNF-05 | Os endpoints de membros devem seguir os padrões existentes: CQRS via MediatR, Result Pattern, FluentValidation. |

---

## 7. Fora do Escopo

| Item | Motivo |
|---|---|
| UI/Frontend para gestão de membros | Será feito em PRD separado no front-end |
| Sistema de convites por email | Complexidade adicional — Owner altera role de users já cadastrados no tenant |
| Permissões granulares por recurso (ex: "pode editar quadra X mas não Y") | Over-engineering para o estágio atual |
| Migração automática de dados existentes | Será tratado como task técnica separada (SQL de update dos roles existentes) |
| Audit log de alterações de role | Pode ser adicionado futuramente |

---

## 8. Dependências

| Dependência | Impacto |
|---|---|
| Modelo de dados atual (`User.Role` como `UserRole` enum) | O campo já existe, só muda os valores do enum |
| JWT claim `Role` | Tokens existentes serão invalidados com a renomeação dos roles |
| Front-end (admin + storefront) | Precisará ajustar chamadas que dependem do role retornado no login |

---

## 9. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Tokens JWT existentes param de funcionar | Alta | Médio | Aceitável — força re-login. Comunicar no changelog. |
| Endpoints que antes funcionavam passam a retornar 403 | Alta | Médio | Esperado — é o objetivo. Documentar a nova matriz de permissões. |
| Owner único por tenant é limitante no futuro | Baixa | Baixo | Pode ser relaxado futuramente sem quebrar a arquitetura. |

---

## 10. Questões em Aberto

1. **Migração de dados**: Os users existentes com `UserRole.User` devem virar `Customer`, e `UserRole.Admin` devem virar `Owner`. Definir se será feito via migration EF Core ou script SQL manual.
2. **Transferência de ownership**: Se um Owner quiser transferir o tenant para outra pessoa, isso entra em escopo futuro?
3. **Role no response do login**: O front-end precisa saber o role do user para exibir menus corretos. Confirmar que `AuthResponse` já retorna essa info (ou adicionar).
