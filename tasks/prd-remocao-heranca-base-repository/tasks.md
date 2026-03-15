# Tasks: Remoção da Herança de BaseRepository

**PRD:** prd-remocao-heranca-base-repository  
**Versão:** 1.0  
**Data:** 14 de março de 2026  
**Status:** Planejamento

---

## Visão Geral

Este documento lista todas as tarefas necessárias para remover a herança de `BaseRepository<T>` e `IBaseRepository<T>` do projeto SportHub, implementando cada repositório de forma explícita e independente.

### Objetivos
- ✅ Remover completamente a herança de BaseRepository
- ✅ Implementar cada repositório manualmente com seus métodos específicos
- ✅ Eliminar code smells (uso de `new` para sobrescrever métodos)
- ✅ Manter 100% dos testes passando
- ✅ Melhorar testabilidade e manutenibilidade do código

### Métricas de Sucesso
- 0 referências a `BaseRepository<T>` no código
- 0 referências a `IBaseRepository<T>` no código
- 5 repositórios refatorados (Users, Courts, Sports, Reservation, Tenant)
- 100% dos testes existentes passando
- Nenhuma regressão funcional

---

## Lista de Tarefas

### 1.0 Refatorar UsersRepository e IUsersRepository
**Complexidade:** MEDIUM  
**Status:** ✅ Concluído  
**Dependências:** Nenhuma  
**Arquivo:** `1_task.md`

Remover herança de `BaseRepository<User>` e `IBaseRepository<User>`, implementando todos os métodos base explicitamente mais os métodos específicos (GetByEmailAsync, GetByRefreshTokenAsync, EmailExistsAsync).

**Entregável:** UsersRepository independente com todos os testes passando

---

### 2.0 Refatorar CourtsRepository e ICourtsRepository
**Complexidade:** MEDIUM  
**Status:** ✅ Concluído  
**Dependências:** Nenhuma  
**Arquivo:** `2_task.md`

Remover herança de `BaseRepository<Court>` e `IBaseRepository<Court>`, eliminando o uso de `new` para sobrescrever métodos. Manter includes de Sports nos métodos GetAllAsync e GetByIdAsync.

**Entregável:** CourtsRepository independente sem code smells, com includes funcionando

---

### 3.0 Refatorar SportsRepository e ISportsRepository
**Complexidade:** LOW  
**Status:** ✅ Concluído  
**Dependências:** Nenhuma  
**Arquivo:** `3_task.md`

Remover herança de `BaseRepository<Sport>` e `IBaseRepository<Sport>`, implementando métodos base mais os específicos (GetByNameAsync, GetSportsByIdsAsync, ExistsByNameAsync).

**Entregável:** SportsRepository independente funcional

---

### 4.0 Refatorar ReservationRepository e IReservationRepository
**Complexidade:** MEDIUM  
**Status:** ✅ Concluído  
**Dependências:** Nenhuma  
**Arquivo:** `4_task.md`

Remover herança de `BaseRepository<Reservation>` e `IBaseRepository<Reservation>`, implementando métodos base mais os específicos de reserva (GetByCourtAndDayAsync, ExistsConflictAsync, GetByUserAsync, GetByCourtAsync).

**Entregável:** ReservationRepository independente com lógica de conflitos funcionando

---

### 5.0 Refatorar TenantRepository e ITenantRepository
**Complexidade:** MEDIUM  
**Status:** ✅ Concluído  
**Dependências:** Nenhuma  
**Arquivo:** `5_task.md`

Validar que TenantRepository já está independente (não herda de BaseRepository) e ajustar ITenantRepository se necessário para remover qualquer referência a IBaseRepository. Garantir uso correto do TenantDbContext (schema public).

**Entregável:** TenantRepository validado e ITenantRepository sem herança

---

### 6.0 Remover BaseRepository e IBaseRepository
**Complexidade:** LOW  
**Status:** ✅ Concluído  
**Dependências:** Tasks 1.0, 2.0, 3.0, 4.0, 5.0  
**Arquivo:** `6_task.md`

Deletar arquivos `BaseRepository.cs` e `IBaseRepository.cs`, validar que não há referências remanescentes no código e garantir que o projeto compila sem erros.

**Entregável:** Código sem BaseRepository, compilando sem erros ou warnings

---

### 7.0 Testes de Integração e Validação Final
**Complexidade:** MEDIUM  
**Status:** ✅ Concluído  
**Dependências:** Task 6.0  
**Arquivo:** `7_task.md`

Executar suite completa de testes (unitários + integração), validar endpoints principais manualmente, verificar performance e documentar mudanças.

**Entregável:** Sistema validado sem regressões, documentação atualizada

---

## Sequenciamento e Paralelização

### Fase 1: Refatoração de Repositórios (Paralelo)
- Task 1.0, 2.0, 3.0, 4.0, 5.0 podem ser executadas em paralelo
- Cada uma é independente e pode ser desenvolvida simultaneamente

### Fase 2: Limpeza (Sequencial)
- Task 6.0 depende da conclusão de todas as tasks da Fase 1

### Fase 3: Validação (Sequencial)
- Task 7.0 depende da conclusão da Task 6.0

---

## Estimativa de Esforço

| Task | Complexidade | Tempo Estimado |
|------|--------------|----------------|
| 1.0  | MEDIUM       | 4 horas        |
| 2.0  | MEDIUM       | 4 horas        |
| 3.0  | LOW          | 2 horas        |
| 4.0  | MEDIUM       | 4 horas        |
| 5.0  | MEDIUM       | 3 horas        |
| 6.0  | LOW          | 1 hora         |
| 7.0  | MEDIUM       | 4 horas        |
| **Total** | | **22 horas** |

---

## Critérios de Aceitação Gerais

- ✅ Todos os repositórios refatorados sem herança
- ✅ Código compila sem erros ou warnings
- ✅ 100% dos testes unitários passando
- ✅ 100% dos testes de integração passando
- ✅ Nenhuma regressão funcional detectada
- ✅ Performance mantida ou melhorada
- ✅ Documentação técnica atualizada
- ✅ Code review aprovado

---

## Notas Importantes

1. **Consistência:** Todos os repositórios devem seguir o mesmo padrão de implementação
2. **Testes:** Cada tarefa deve incluir testes unitários completos
3. **Commits:** Fazer commits incrementais após cada repositório refatorado
4. **Rollback:** Manter possibilidade de rollback em caso de problemas
5. **Multi-tenancy:** Garantir que o sistema de multi-tenancy continua funcionando corretamente

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **Tech Spec:** `documentos/techspec-codebase.md`
- **Regras Windsurf:** `.windsurf/rules/techspec-codebase.md`
