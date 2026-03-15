# Task 4.0: Refatorar ReservationRepository e IReservationRepository

**Complexidade:** MEDIUM  
**Status:** Pendente  
**Responsável:** TBD  
**Estimativa:** 4 horas

---

## Objetivo

Remover a herança de `BaseRepository<Reservation>` e `IBaseRepository<Reservation>`, implementando todos os métodos base explicitamente mais os métodos específicos de reserva com suas regras de negócio.

---

## Contexto

O `ReservationRepository` atualmente herda de `BaseRepository<Reservation>` e possui 4 métodos específicos relacionados a lógica de reservas: GetByCourtAndDayAsync, ExistsConflictAsync, GetByUserAsync e GetByCourtAsync. Estes métodos incluem lógica importante de negócio como detecção de conflitos e includes de Court.

### Estado Atual
- **Interface:** `IReservationRepository : IBaseRepository<Reservation>`
- **Implementação:** `ReservationRepository : BaseRepository<Reservation>`
- **Métodos específicos:** 
  - GetByCourtAndDayAsync (filtra por quadra e dia)
  - ExistsConflictAsync (detecta conflitos de horário)
  - GetByUserAsync (lista reservas do usuário com Include de Court)
  - GetByCourtAsync (lista reservas da quadra com Include de Court)
- **Duplicação:** Campo `_dbContext` declarado tanto no base quanto na classe derivada

---

## Subtarefas

### 4.1 Refatorar Interface IReservationRepository
**Descrição:** Remover herança de `IBaseRepository<Reservation>` e adicionar todos os métodos base explicitamente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Application/Common/Interfaces/IReservationRepository.cs`
2. Remover `: IBaseRepository<Reservation>` da declaração da interface
3. Adicionar explicitamente todos os métodos base:
   - `Task<Reservation?> GetByIdAsync(Guid id);`
   - `Task<List<Reservation>> GetAllAsync();`
   - `Task AddAsync(Reservation entity);`
   - `Task UpdateAsync(Reservation entity);`
   - `Task RemoveAsync(Reservation entity);`
   - `Task<List<Reservation>> GetByIdsAsync(IEnumerable<Guid> ids);`
   - `Task<bool> ExistsAsync(Guid id);`
   - `IQueryable<Reservation> Query();`
   - `Task AddManyAsync(IEnumerable<Reservation> entities);`
4. Manter métodos específicos existentes

**Critério de Sucesso:**
- Interface não herda de `IBaseRepository<Reservation>`
- Todos os 13 métodos estão declarados (9 base + 4 específicos)
- Código compila sem erros

---

### 4.2 Criar Testes Unitários para IReservationRepository (TDD - Red)
**Descrição:** Criar testes unitários para todos os métodos antes da implementação.

**Passos:**
1. Criar ou atualizar arquivo de testes: `tests/SportHub.Tests/Repositories/ReservationRepositoryTests.cs`
2. Criar testes para métodos base:
   - `GetByIdAsync_WhenReservationExists_ReturnsReservation`
   - `GetByIdAsync_WhenReservationNotExists_ReturnsNull`
   - `GetAllAsync_ReturnsAllReservations`
   - `AddAsync_AddsReservationToDbSet`
   - `UpdateAsync_UpdatesReservationInDbSet`
   - `RemoveAsync_RemovesReservationFromDbSet`
   - `GetByIdsAsync_ReturnsReservationsWithMatchingIds`
   - `ExistsAsync_WhenReservationExists_ReturnsTrue`
   - `ExistsAsync_WhenReservationNotExists_ReturnsFalse`
   - `Query_ReturnsQueryableOfReservations`
   - `AddManyAsync_AddsMultipleReservations`
3. Criar testes para métodos específicos:
   - `GetByCourtAndDayAsync_ReturnsReservationsForCourtAndDay`
   - `GetByCourtAndDayAsync_FiltersCorrectlyByDate`
   - `GetByCourtAndDayAsync_HandlesUtcCorrectly`
   - `ExistsConflictAsync_WhenConflictExists_ReturnsTrue`
   - `ExistsConflictAsync_WhenNoConflict_ReturnsFalse`
   - `ExistsConflictAsync_DetectsOverlappingTimeRanges`
   - `GetByUserAsync_ReturnsUserReservationsWithCourt`
   - `GetByUserAsync_OrdersByStartTimeDescending`
   - `GetByCourtAsync_ReturnsCourtReservationsWithCourt`
   - `GetByCourtAsync_FiltersCorrectlyByDate`
   - `GetByCourtAsync_OrdersByStartTimeDescending`

**Critério de Sucesso:**
- Mínimo 22 testes criados
- Todos os testes falham (Red phase do TDD)
- Testes validam lógica de conflitos
- Testes validam includes de Court
- Testes validam ordenação

---

### 4.3 Refatorar Implementação ReservationRepository (TDD - Green)
**Descrição:** Remover herança de `BaseRepository<Reservation>` e implementar todos os métodos manualmente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Infrastructure/Repositories/ReservationRepository.cs`
2. Remover `: BaseRepository<Reservation>` da declaração da classe
3. Remover duplicação do campo `_dbContext` (manter apenas um)
4. Adicionar campo `private readonly DbSet<Reservation> _dbSet;`
5. Atualizar construtor:
   ```csharp
   public ReservationRepository(ApplicationDbContext context)
   {
       _dbContext = context;
       _dbSet = context.Set<Reservation>();
   }
   ```
6. Implementar todos os métodos base manualmente:
   - GetByIdAsync: `await _dbSet.FindAsync(id);`
   - GetAllAsync: `await _dbSet.ToListAsync();`
   - AddAsync: `_dbSet.Add(entity); return Task.CompletedTask;`
   - UpdateAsync: `_dbSet.Update(entity); return Task.CompletedTask;`
   - RemoveAsync: `_dbSet.Remove(entity); return Task.CompletedTask;`
   - GetByIdsAsync: `await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();`
   - ExistsAsync: `await _dbSet.AnyAsync(e => e.Id == id);`
   - Query: `_dbSet.AsQueryable();`
   - AddManyAsync: `_dbSet.AddRange(entities); return Task.CompletedTask;`
7. Manter implementação dos métodos específicos (GetByCourtAndDayAsync, ExistsConflictAsync, GetByUserAsync, GetByCourtAsync)

**Critério de Sucesso:**
- Classe não herda de `BaseRepository<Reservation>`
- Todos os 13 métodos implementados
- Nenhuma duplicação de campos
- Lógica de conflitos mantida
- Includes de Court mantidos
- Código compila sem erros
- Todos os testes passam (Green phase do TDD)

---

### 4.4 Refatorar e Otimizar (TDD - Refactor)
**Descrição:** Revisar código para melhorias de qualidade e performance.

**Passos:**
1. Adicionar `AsNoTracking()` em queries read-only onde apropriado
2. Validar lógica de detecção de conflitos (overlap de horários)
3. Validar tratamento correto de UTC nas datas
4. Considerar adicionar índices no banco para queries frequentes
5. Garantir consistência de nomenclatura
6. Adicionar comentários XML para métodos públicos

**Critério de Sucesso:**
- Código otimizado mantendo testes passando
- Lógica de conflitos robusta e testada
- Tratamento de UTC correto
- Nenhum warning de análise estática
- Performance mantida ou melhorada

---

### 4.5 Validar Integração com Handlers
**Descrição:** Garantir que todos os handlers que usam IReservationRepository continuam funcionando.

**Passos:**
1. Identificar handlers que injetam IReservationRepository
2. Executar testes de integração relacionados a reservas
3. Validar endpoints de CRUD de reservas
4. Validar detecção de conflitos de horário
5. Validar que Court é carregado corretamente nas respostas

**Critério de Sucesso:**
- Todos os handlers compilam sem erros
- Todos os testes de integração passam
- Endpoints funcionam corretamente
- Conflitos de horário são detectados corretamente
- Court é incluído nas respostas

---

## Testes

### Testes Unitários
- [ ] GetByIdAsync com reserva existente
- [ ] GetByIdAsync com reserva inexistente
- [ ] GetAllAsync retorna todas as reservas
- [ ] AddAsync adiciona reserva ao DbSet
- [ ] UpdateAsync atualiza reserva no DbSet
- [ ] RemoveAsync remove reserva do DbSet
- [ ] GetByIdsAsync retorna reservas com IDs correspondentes
- [ ] ExistsAsync retorna true quando reserva existe
- [ ] ExistsAsync retorna false quando reserva não existe
- [ ] Query retorna IQueryable de reservas
- [ ] AddManyAsync adiciona múltiplas reservas
- [ ] GetByCourtAndDayAsync filtra por quadra e dia
- [ ] GetByCourtAndDayAsync trata UTC corretamente
- [ ] ExistsConflictAsync detecta conflito quando horários se sobrepõem
- [ ] ExistsConflictAsync retorna false quando não há conflito
- [ ] ExistsConflictAsync testa edge cases (início/fim exatos)
- [ ] GetByUserAsync retorna reservas do usuário com Court
- [ ] GetByUserAsync ordena por StartTimeUtc descendente
- [ ] GetByCourtAsync retorna reservas da quadra com Court
- [ ] GetByCourtAsync filtra por data quando fornecida
- [ ] GetByCourtAsync retorna todas quando data não fornecida
- [ ] GetByCourtAsync ordena por StartTimeUtc descendente

### Testes de Integração
- [ ] Criar reserva funciona
- [ ] Criar reserva com conflito é rejeitada
- [ ] Atualizar reserva funciona
- [ ] Buscar reserva por ID funciona
- [ ] Listar reservas por usuário retorna Court
- [ ] Listar reservas por quadra retorna Court
- [ ] Deletar reserva funciona

---

## Critérios de Aceitação

- ✅ IReservationRepository não herda de IBaseRepository<Reservation>
- ✅ ReservationRepository não herda de BaseRepository<Reservation>
- ✅ Todos os 9 métodos base implementados explicitamente
- ✅ Todos os 4 métodos específicos mantidos
- ✅ Nenhuma duplicação de campo _dbContext
- ✅ Lógica de detecção de conflitos funcionando corretamente
- ✅ Includes de Court mantidos em GetByUserAsync e GetByCourtAsync
- ✅ Tratamento correto de UTC nas datas
- ✅ Ordenação por StartTimeUtc descendente mantida
- ✅ Mínimo 22 testes unitários passando
- ✅ Todos os testes de integração passando
- ✅ Código compila sem erros ou warnings

---

## Arquivos Afetados

- `src/SportHub.Application/Common/Interfaces/IReservationRepository.cs`
- `src/SportHub.Infrastructure/Repositories/ReservationRepository.cs`
- `tests/SportHub.Tests/Repositories/ReservationRepositoryTests.cs` (novo ou atualizado)

---

## Dependências

- **Nenhuma** - Esta tarefa pode ser executada independentemente

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Quebra de lógica de conflitos | Média | Alto | Testes extensivos de overlap de horários |
| Problemas com UTC | Média | Alto | Validar tratamento de DateTime.SpecifyKind |
| Perda de includes de Court | Baixa | Médio | Validar que includes estão presentes nos testes |
| Quebra de handlers existentes | Baixa | Alto | Executar todos os testes após refatoração |

---

## Notas Técnicas

### Padrão de Implementação
```csharp
public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Reservation> _dbSet;

    public ReservationRepository(ApplicationDbContext context)
    {
        _dbContext = context;
        _dbSet = context.Set<Reservation>();
    }

    // Métodos base (9)
    public async Task<Reservation?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    // ... outros métodos base

    // Métodos específicos (4)
    public async Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day)
    {
        var dateUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
        return await _dbContext.Reservations
            .Where(r => r.CourtId == courtId && r.StartTimeUtc.Date == dateUtc)
            .ToListAsync();
    }

    public async Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc)
    {
        return await _dbContext.Reservations
            .AnyAsync(r => r.CourtId == courtId && 
                          r.StartTimeUtc < endUtc && 
                          r.EndTimeUtc > startUtc);
    }

    public async Task<List<Reservation>> GetByUserAsync(Guid userId)
    {
        return await _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartTimeUtc)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByCourtAsync(Guid courtId, DateTime? date = null)
    {
        var query = _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.CourtId == courtId);

        if (date.HasValue)
        {
            var dateUtc = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.StartTimeUtc.Date == dateUtc);
        }

        return await query
            .OrderByDescending(r => r.StartTimeUtc)
            .ToListAsync();
    }
}
```

### Importante - Lógica de Conflitos
A detecção de conflitos usa a lógica: `startA < endB && endA > startB`
- Detecta overlap completo
- Detecta overlap parcial
- Detecta reservas contidas
- **NÃO** detecta reservas adjacentes (correto)

### Importante - UTC
- Sempre usar `DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)` ao comparar datas
- StartTimeUtc e EndTimeUtc devem estar em UTC
- **NÃO** adicionar `SaveChangesAsync()` nos métodos

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **ReservationRepository atual:** `src/SportHub.Infrastructure/Repositories/ReservationRepository.cs`
- **BaseRepository atual:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`
