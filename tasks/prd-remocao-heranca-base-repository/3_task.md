# Task 3.0: Refatorar SportsRepository e ISportsRepository

**Complexidade:** LOW  
**Status:** Pendente  
**Responsável:** TBD  
**Estimativa:** 2 horas

---

## Objetivo

Remover a herança de `BaseRepository<Sport>` e `IBaseRepository<Sport>`, implementando todos os métodos base explicitamente mais os métodos específicos de esportes.

---

## Contexto

O `SportsRepository` atualmente herda de `BaseRepository<Sport>` e possui 3 métodos específicos: GetByNameAsync, GetSportsByIdsAsync e ExistsByNameAsync. A interface `ISportsRepository` herda de `IBaseRepository<Sport>`. Esta é a refatoração mais simples pois não há sobrescrita de métodos.

### Estado Atual
- **Interface:** `ISportsRepository : IBaseRepository<Sport>`
- **Implementação:** `SportsRepository : BaseRepository<Sport>`
- **Métodos específicos:** GetByNameAsync, GetSportsByIdsAsync, ExistsByNameAsync
- **Duplicação:** Campo `_dbContext` declarado tanto no base quanto na classe derivada

---

## Subtarefas

### 3.1 Refatorar Interface ISportsRepository
**Descrição:** Remover herança de `IBaseRepository<Sport>` e adicionar todos os métodos base explicitamente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Application/Common/Interfaces/ISportsRepository.cs`
2. Remover `: IBaseRepository<Sport>` da declaração da interface
3. Adicionar explicitamente todos os métodos base:
   - `Task<Sport?> GetByIdAsync(Guid id);`
   - `Task<List<Sport>> GetAllAsync();`
   - `Task AddAsync(Sport entity);`
   - `Task UpdateAsync(Sport entity);`
   - `Task RemoveAsync(Sport entity);`
   - `Task<List<Sport>> GetByIdsAsync(IEnumerable<Guid> ids);`
   - `Task<bool> ExistsAsync(Guid id);`
   - `IQueryable<Sport> Query();`
   - `Task AddManyAsync(IEnumerable<Sport> entities);`
4. Manter métodos específicos existentes

**Critério de Sucesso:**
- Interface não herda de `IBaseRepository<Sport>`
- Todos os 12 métodos estão declarados (9 base + 3 específicos)
- Código compila sem erros

---

### 3.2 Criar Testes Unitários para ISportsRepository (TDD - Red)
**Descrição:** Criar testes unitários para todos os métodos antes da implementação.

**Passos:**
1. Criar ou atualizar arquivo de testes: `tests/SportHub.Tests/Repositories/SportsRepositoryTests.cs`
2. Criar testes para métodos base:
   - `GetByIdAsync_WhenSportExists_ReturnsSport`
   - `GetByIdAsync_WhenSportNotExists_ReturnsNull`
   - `GetAllAsync_ReturnsAllSports`
   - `AddAsync_AddsSportToDbSet`
   - `UpdateAsync_UpdatesSportInDbSet`
   - `RemoveAsync_RemovesSportFromDbSet`
   - `GetByIdsAsync_ReturnsSportsWithMatchingIds`
   - `ExistsAsync_WhenSportExists_ReturnsTrue`
   - `ExistsAsync_WhenSportNotExists_ReturnsFalse`
   - `Query_ReturnsQueryableOfSports`
   - `AddManyAsync_AddsMultipleSports`
3. Criar testes para métodos específicos:
   - `GetByNameAsync_WhenNameExists_ReturnsSport`
   - `GetByNameAsync_WhenNameNotExists_ReturnsNull`
   - `GetByNameAsync_IsCaseInsensitive`
   - `GetSportsByIdsAsync_ReturnsSportsWithMatchingIds`
   - `ExistsByNameAsync_WhenNameExists_ReturnsTrue`
   - `ExistsByNameAsync_WhenNameNotExists_ReturnsFalse`
   - `ExistsByNameAsync_IsCaseInsensitive`

**Critério de Sucesso:**
- Mínimo 18 testes criados
- Todos os testes falham (Red phase do TDD)
- Testes validam case-insensitive para métodos de nome

---

### 3.3 Refatorar Implementação SportsRepository (TDD - Green)
**Descrição:** Remover herança de `BaseRepository<Sport>` e implementar todos os métodos manualmente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Infrastructure/Repositories/SportsRepository.cs`
2. Remover `: BaseRepository<Sport>` da declaração da classe
3. Remover duplicação do campo `_dbContext` (manter apenas um)
4. Adicionar campo `private readonly DbSet<Sport> _dbSet;`
5. Atualizar construtor:
   ```csharp
   public SportsRepository(ApplicationDbContext context)
   {
       _dbContext = context;
       _dbSet = context.Set<Sport>();
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
7. Manter implementação dos métodos específicos (GetByNameAsync, GetSportsByIdsAsync, ExistsByNameAsync)

**Critério de Sucesso:**
- Classe não herda de `BaseRepository<Sport>`
- Todos os 12 métodos implementados
- Nenhuma duplicação de campos
- Métodos específicos mantêm uso de EF.Functions.ILike para case-insensitive
- Código compila sem erros
- Todos os testes passam (Green phase do TDD)

---

### 3.4 Refatorar e Otimizar (TDD - Refactor)
**Descrição:** Revisar código para melhorias de qualidade e performance.

**Passos:**
1. Adicionar `AsNoTracking()` em GetAllAsync e queries read-only
2. Verificar se GetSportsByIdsAsync pode ser otimizado
3. Garantir consistência de nomenclatura
4. Adicionar comentários XML para métodos públicos
5. Validar que EF.Functions.ILike está sendo usado corretamente

**Critério de Sucesso:**
- Código otimizado mantendo testes passando
- Nenhum warning de análise estática
- Performance mantida ou melhorada

---

### 3.5 Validar Integração com Handlers
**Descrição:** Garantir que todos os handlers que usam ISportsRepository continuam funcionando.

**Passos:**
1. Identificar handlers que injetam ISportsRepository
2. Executar testes de integração relacionados a esportes
3. Validar endpoints de CRUD de esportes
4. Validar busca por nome (case-insensitive)

**Critério de Sucesso:**
- Todos os handlers compilam sem erros
- Todos os testes de integração passam
- Endpoints funcionam corretamente
- Busca por nome é case-insensitive

---

## Testes

### Testes Unitários
- [ ] GetByIdAsync com esporte existente
- [ ] GetByIdAsync com esporte inexistente
- [ ] GetAllAsync retorna todos os esportes
- [ ] AddAsync adiciona esporte ao DbSet
- [ ] UpdateAsync atualiza esporte no DbSet
- [ ] RemoveAsync remove esporte do DbSet
- [ ] GetByIdsAsync retorna esportes com IDs correspondentes
- [ ] ExistsAsync retorna true quando esporte existe
- [ ] ExistsAsync retorna false quando esporte não existe
- [ ] Query retorna IQueryable de esportes
- [ ] AddManyAsync adiciona múltiplos esportes
- [ ] GetByNameAsync encontra esporte por nome
- [ ] GetByNameAsync retorna null quando nome não existe
- [ ] GetByNameAsync é case-insensitive
- [ ] GetSportsByIdsAsync retorna esportes com IDs correspondentes
- [ ] ExistsByNameAsync retorna true quando nome existe
- [ ] ExistsByNameAsync retorna false quando nome não existe
- [ ] ExistsByNameAsync é case-insensitive

### Testes de Integração
- [ ] Criar esporte funciona
- [ ] Atualizar esporte funciona
- [ ] Buscar esporte por ID funciona
- [ ] Buscar esporte por nome funciona (case-insensitive)
- [ ] Listar todos os esportes funciona
- [ ] Deletar esporte funciona

---

## Critérios de Aceitação

- ✅ ISportsRepository não herda de IBaseRepository<Sport>
- ✅ SportsRepository não herda de BaseRepository<Sport>
- ✅ Todos os 9 métodos base implementados explicitamente
- ✅ Todos os 3 métodos específicos mantidos
- ✅ Nenhuma duplicação de campo _dbContext
- ✅ Busca por nome é case-insensitive (EF.Functions.ILike)
- ✅ Mínimo 18 testes unitários passando
- ✅ Todos os testes de integração passando
- ✅ Código compila sem erros ou warnings

---

## Arquivos Afetados

- `src/SportHub.Application/Common/Interfaces/ISportsRepository.cs`
- `src/SportHub.Infrastructure/Repositories/SportsRepository.cs`
- `tests/SportHub.Tests/Repositories/SportsRepositoryTests.cs` (novo ou atualizado)

---

## Dependências

- **Nenhuma** - Esta tarefa pode ser executada independentemente

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Perda de case-insensitive | Baixa | Médio | Validar uso de EF.Functions.ILike nos testes |
| Quebra de handlers existentes | Baixa | Alto | Executar todos os testes após refatoração |

---

## Notas Técnicas

### Padrão de Implementação
```csharp
public class SportsRepository : ISportsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Sport> _dbSet;

    public SportsRepository(ApplicationDbContext context)
    {
        _dbContext = context;
        _dbSet = context.Set<Sport>();
    }

    // Métodos base (9)
    public async Task<Sport?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<List<Sport>> GetAllAsync() =>
        await _dbSet.AsNoTracking().ToListAsync();

    // ... outros métodos base

    // Métodos específicos (3)
    public async Task<Sport?> GetByNameAsync(string name)
    {
        return await _dbContext.Sports
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, name));
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbContext.Sports
            .AnyAsync(s => EF.Functions.ILike(s.Name, name));
    }

    public async Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _dbContext.Sports
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }
}
```

### Importante
- **EF.Functions.ILike** garante busca case-insensitive no PostgreSQL
- **AsNoTracking()** deve ser usado em queries read-only
- **NÃO** adicionar `SaveChangesAsync()` nos métodos

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **SportsRepository atual:** `src/SportHub.Infrastructure/Repositories/SportsRepository.cs`
- **BaseRepository atual:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`
