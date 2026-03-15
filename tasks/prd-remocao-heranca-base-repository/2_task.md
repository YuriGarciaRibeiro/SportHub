# Task 2.0: Refatorar CourtsRepository e ICourtsRepository

**Complexidade:** MEDIUM  
**Status:** Pendente  
**Responsável:** TBD  
**Estimativa:** 4 horas

---

## Objetivo

Remover a herança de `BaseRepository<Court>` e `IBaseRepository<Court>`, eliminando o uso de `new` para sobrescrever métodos. Implementar todos os métodos base explicitamente mantendo os includes de Sports.

---

## Contexto

O `CourtsRepository` atualmente herda de `BaseRepository<Court>` e usa a palavra-chave `new` para sobrescrever os métodos `GetAllAsync` e `GetByIdAsync`, adicionando includes de Sports. Este é um code smell que deve ser eliminado. A interface `ICourtsRepository` herda de `IBaseRepository<Court>` mas não declara métodos específicos.

### Estado Atual
- **Interface:** `ICourtsRepository : IBaseRepository<Court>` (sem métodos específicos)
- **Implementação:** `CourtsRepository : BaseRepository<Court>`
- **Code Smell:** Uso de `new` para sobrescrever GetAllAsync e GetByIdAsync
- **Includes:** Sports (com AsSplitQuery e AsNoTracking)
- **Duplicação:** Campo `_dbContext` declarado tanto no base quanto na classe derivada

---

## Subtarefas

### 2.1 Refatorar Interface ICourtsRepository
**Descrição:** Remover herança de `IBaseRepository<Court>` e adicionar todos os métodos base explicitamente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Application/Common/Interfaces/ICourtsRepository.cs`
2. Remover `: IBaseRepository<Court>` da declaração da interface
3. Adicionar explicitamente todos os métodos base:
   - `Task<Court?> GetByIdAsync(Guid id);`
   - `Task<List<Court>> GetAllAsync();`
   - `Task AddAsync(Court entity);`
   - `Task UpdateAsync(Court entity);`
   - `Task RemoveAsync(Court entity);`
   - `Task<List<Court>> GetByIdsAsync(IEnumerable<Guid> ids);`
   - `Task<bool> ExistsAsync(Guid id);`
   - `IQueryable<Court> Query();`
   - `Task AddManyAsync(IEnumerable<Court> entities);`

**Critério de Sucesso:**
- Interface não herda de `IBaseRepository<Court>`
- Todos os 9 métodos base estão declarados explicitamente
- Código compila sem erros

---

### 2.2 Criar Testes Unitários para ICourtsRepository (TDD - Red)
**Descrição:** Criar testes unitários para todos os métodos antes da implementação.

**Passos:**
1. Criar ou atualizar arquivo de testes: `tests/SportHub.Tests/Repositories/CourtsRepositoryTests.cs`
2. Criar testes para métodos base:
   - `GetByIdAsync_WhenCourtExists_ReturnsCourtWithSports`
   - `GetByIdAsync_WhenCourtNotExists_ReturnsNull`
   - `GetAllAsync_ReturnsAllCourtsWithSports`
   - `GetAllAsync_UsesSplitQuery`
   - `GetAllAsync_UsesNoTracking`
   - `AddAsync_AddsCourtToDbSet`
   - `UpdateAsync_UpdatesCourtInDbSet`
   - `RemoveAsync_RemovesCourtFromDbSet`
   - `GetByIdsAsync_ReturnsCourtsWithMatchingIds`
   - `ExistsAsync_WhenCourtExists_ReturnsTrue`
   - `ExistsAsync_WhenCourtNotExists_ReturnsFalse`
   - `Query_ReturnsQueryableOfCourts`
   - `AddManyAsync_AddsMultipleCourts`

**Critério de Sucesso:**
- Mínimo 13 testes criados
- Todos os testes falham (Red phase do TDD)
- Testes validam includes de Sports
- Testes validam uso de AsSplitQuery e AsNoTracking

---

### 2.3 Refatorar Implementação CourtsRepository (TDD - Green)
**Descrição:** Remover herança de `BaseRepository<Court>` e implementar todos os métodos manualmente, eliminando uso de `new`.

**Passos:**
1. Abrir arquivo: `src/SportHub.Infrastructure/Repositories/CourtsRepository.cs`
2. Remover `: BaseRepository<Court>` da declaração da classe
3. Remover duplicação do campo `_dbContext` (manter apenas um)
4. Adicionar campo `private readonly DbSet<Court> _dbSet;`
5. Atualizar construtor:
   ```csharp
   public CourtsRepository(ApplicationDbContext context)
   {
       _dbContext = context;
       _dbSet = context.Set<Court>();
   }
   ```
6. Implementar métodos base com otimizações específicas:
   - **GetByIdAsync:** Com Include de Sports e AsSplitQuery
   ```csharp
   public async Task<Court?> GetByIdAsync(Guid id)
   {
       return await _dbContext.Courts
           .Include(c => c.Sports)
           .AsSplitQuery()
           .FirstOrDefaultAsync(c => c.Id == id);
   }
   ```
   - **GetAllAsync:** Com Include de Sports, AsSplitQuery e AsNoTracking
   ```csharp
   public async Task<List<Court>> GetAllAsync()
   {
       return await _dbContext.Courts
           .Include(c => c.Sports)
           .AsSplitQuery()
           .AsNoTracking()
           .ToListAsync();
   }
   ```
   - **Outros métodos:** Implementação padrão sem includes
7. Remover palavra-chave `new` (não é mais necessária)

**Critério de Sucesso:**
- Classe não herda de `BaseRepository<Court>`
- Todos os 9 métodos implementados
- Nenhum uso de `new` para sobrescrever métodos
- Includes de Sports mantidos em GetByIdAsync e GetAllAsync
- Código compila sem erros
- Todos os testes passam (Green phase do TDD)

---

### 2.4 Refatorar e Otimizar (TDD - Refactor)
**Descrição:** Revisar código para melhorias de qualidade e performance.

**Passos:**
1. Validar uso correto de `AsSplitQuery()` para evitar queries cartesianas
2. Validar uso de `AsNoTracking()` em queries read-only
3. Considerar adicionar Include de Sports em GetByIdsAsync se necessário
4. Garantir consistência de nomenclatura
5. Adicionar comentários XML para métodos públicos

**Critério de Sucesso:**
- Código otimizado mantendo testes passando
- Nenhum warning de análise estática
- Performance mantida ou melhorada
- Nenhuma query N+1

---

### 2.5 Validar Integração com Handlers
**Descrição:** Garantir que todos os handlers que usam ICourtsRepository continuam funcionando.

**Passos:**
1. Identificar handlers que injetam ICourtsRepository
2. Executar testes de integração relacionados a quadras
3. Validar endpoints de CRUD de quadras
4. Validar que Sports são carregados corretamente nas respostas

**Critério de Sucesso:**
- Todos os handlers compilam sem erros
- Todos os testes de integração passam
- Endpoints retornam quadras com Sports incluídos
- Nenhuma query N+1 detectada

---

## Testes

### Testes Unitários
- [ ] GetByIdAsync retorna quadra com Sports incluídos
- [ ] GetByIdAsync retorna null quando quadra não existe
- [ ] GetByIdAsync usa AsSplitQuery
- [ ] GetAllAsync retorna todas as quadras com Sports
- [ ] GetAllAsync usa AsSplitQuery
- [ ] GetAllAsync usa AsNoTracking
- [ ] AddAsync adiciona quadra ao DbSet
- [ ] UpdateAsync atualiza quadra no DbSet
- [ ] RemoveAsync remove quadra do DbSet
- [ ] GetByIdsAsync retorna quadras com IDs correspondentes
- [ ] ExistsAsync retorna true quando quadra existe
- [ ] ExistsAsync retorna false quando quadra não existe
- [ ] Query retorna IQueryable de quadras

### Testes de Integração
- [ ] Criar quadra funciona
- [ ] Atualizar quadra funciona
- [ ] Buscar quadra por ID retorna Sports
- [ ] Listar todas as quadras retorna Sports
- [ ] Deletar quadra funciona
- [ ] Nenhuma query N+1 ao listar quadras

---

## Critérios de Aceitação

- ✅ ICourtsRepository não herda de IBaseRepository<Court>
- ✅ CourtsRepository não herda de BaseRepository<Court>
- ✅ Todos os 9 métodos base implementados explicitamente
- ✅ Nenhum uso de palavra-chave `new`
- ✅ Includes de Sports mantidos em GetByIdAsync e GetAllAsync
- ✅ AsSplitQuery usado corretamente
- ✅ AsNoTracking usado em queries read-only
- ✅ Nenhuma duplicação de campo _dbContext
- ✅ Mínimo 13 testes unitários passando
- ✅ Todos os testes de integração passando
- ✅ Código compila sem erros ou warnings
- ✅ Nenhuma query N+1 detectada

---

## Arquivos Afetados

- `src/SportHub.Application/Common/Interfaces/ICourtsRepository.cs`
- `src/SportHub.Infrastructure/Repositories/CourtsRepository.cs`
- `tests/SportHub.Tests/Repositories/CourtsRepositoryTests.cs` (novo ou atualizado)

---

## Dependências

- **Nenhuma** - Esta tarefa pode ser executada independentemente

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Perda de includes de Sports | Baixa | Alto | Validar que includes estão presentes nos testes |
| Query N+1 ao carregar Sports | Média | Alto | Usar AsSplitQuery e validar com testes |
| Quebra de handlers existentes | Baixa | Alto | Executar todos os testes após refatoração |

---

## Notas Técnicas

### Padrão de Implementação
```csharp
public class CourtsRepository : ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Court> _dbSet;

    public CourtsRepository(ApplicationDbContext context)
    {
        _dbContext = context;
        _dbSet = context.Set<Court>();
    }

    // Métodos com includes (otimizados)
    public async Task<Court?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Court>> GetAllAsync()
    {
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    // Métodos base padrão (sem includes)
    public Task AddAsync(Court entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    // ... outros métodos base
}
```

### Importante
- **AsSplitQuery()** evita queries cartesianas ao carregar coleções
- **AsNoTracking()** melhora performance em queries read-only
- **Include()** deve ser usado apenas onde necessário (GetByIdAsync, GetAllAsync)
- **NÃO** adicionar `SaveChangesAsync()` nos métodos

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **CourtsRepository atual:** `src/SportHub.Infrastructure/Repositories/CourtsRepository.cs`
- **BaseRepository atual:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`
