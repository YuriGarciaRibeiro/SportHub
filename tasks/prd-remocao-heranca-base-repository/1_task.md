# Task 1.0: Refatorar UsersRepository e IUsersRepository

**Complexidade:** MEDIUM  
**Status:** Pendente  
**Responsável:** TBD  
**Estimativa:** 4 horas

---

## Objetivo

Remover a herança de `BaseRepository<User>` e `IBaseRepository<User>`, implementando todos os métodos base explicitamente no UsersRepository, mantendo os métodos específicos de usuário.

---

## Contexto

O `UsersRepository` atualmente herda de `BaseRepository<User>` e possui uma duplicação do campo `_dbContext` (já existe no base). A interface `IUsersRepository` herda de `IBaseRepository<User>`. Esta tarefa visa tornar o repositório completamente independente e explícito.

### Estado Atual
- **Interface:** `IUsersRepository : IBaseRepository<User>`
- **Implementação:** `UsersRepository : BaseRepository<User>`
- **Métodos específicos:** GetByEmailAsync, GetByRefreshTokenAsync, EmailExistsAsync
- **Duplicação:** Campo `_dbContext` declarado tanto no base quanto na classe derivada

---

## Subtarefas

### 1.1 Refatorar Interface IUsersRepository
**Descrição:** Remover herança de `IBaseRepository<User>` e adicionar todos os métodos base explicitamente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Application/Common/Interfaces/IUsersRepository.cs`
2. Remover `: IBaseRepository<User>` da declaração da interface
3. Adicionar explicitamente todos os métodos base:
   - `Task<User?> GetByIdAsync(Guid id);`
   - `Task<List<User>> GetAllAsync();`
   - `Task AddAsync(User entity);`
   - `Task UpdateAsync(User entity);`
   - `Task RemoveAsync(User entity);`
   - `Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids);`
   - `Task<bool> ExistsAsync(Guid id);`
   - `IQueryable<User> Query();`
   - `Task AddManyAsync(IEnumerable<User> entities);`
4. Manter métodos específicos existentes

**Critério de Sucesso:**
- Interface não herda de `IBaseRepository<User>`
- Todos os 12 métodos estão declarados (9 base + 3 específicos)
- Código compila sem erros

---

### 1.2 Criar Testes Unitários para IUsersRepository (TDD - Red)
**Descrição:** Criar testes unitários para todos os métodos antes da implementação.

**Passos:**
1. Criar ou atualizar arquivo de testes: `tests/SportHub.Tests/Repositories/UsersRepositoryTests.cs`
2. Criar testes para métodos base:
   - `GetByIdAsync_WhenUserExists_ReturnsUser`
   - `GetByIdAsync_WhenUserNotExists_ReturnsNull`
   - `GetAllAsync_ReturnsAllUsers`
   - `AddAsync_AddsUserToDbSet`
   - `UpdateAsync_UpdatesUserInDbSet`
   - `RemoveAsync_RemovesUserFromDbSet`
   - `GetByIdsAsync_ReturnsUsersWithMatchingIds`
   - `ExistsAsync_WhenUserExists_ReturnsTrue`
   - `ExistsAsync_WhenUserNotExists_ReturnsFalse`
   - `Query_ReturnsQueryableOfUsers`
   - `AddManyAsync_AddsMultipleUsers`
3. Criar testes para métodos específicos:
   - `GetByEmailAsync_WhenEmailExists_ReturnsUser`
   - `GetByEmailAsync_WhenEmailNotExists_ReturnsNull`
   - `GetByRefreshTokenAsync_WhenTokenExists_ReturnsUser`
   - `GetByRefreshTokenAsync_WhenTokenNotExists_ReturnsNull`
   - `EmailExistsAsync_WhenEmailExists_ReturnsTrue`
   - `EmailExistsAsync_WhenEmailNotExists_ReturnsFalse`

**Critério de Sucesso:**
- Mínimo 17 testes criados
- Todos os testes falham (Red phase do TDD)
- Testes usam mocks do ApplicationDbContext

---

### 1.3 Refatorar Implementação UsersRepository (TDD - Green)
**Descrição:** Remover herança de `BaseRepository<User>` e implementar todos os métodos manualmente.

**Passos:**
1. Abrir arquivo: `src/SportHub.Infrastructure/Repositories/UsersRepository.cs`
2. Remover `: BaseRepository<User>` da declaração da classe
3. Remover duplicação do campo `_dbContext` (manter apenas um)
4. Adicionar campo `private readonly DbSet<User> _dbSet;`
5. Atualizar construtor:
   ```csharp
   public UsersRepository(ApplicationDbContext context)
   {
       _dbContext = context;
       _dbSet = context.Set<User>();
   }
   ```
6. Implementar todos os métodos base manualmente (copiar lógica do BaseRepository):
   - GetByIdAsync: `await _dbSet.FindAsync(id);`
   - GetAllAsync: `await _dbSet.ToListAsync();`
   - AddAsync: `_dbSet.Add(entity); return Task.CompletedTask;`
   - UpdateAsync: `_dbSet.Update(entity); return Task.CompletedTask;`
   - RemoveAsync: `_dbSet.Remove(entity); return Task.CompletedTask;`
   - GetByIdsAsync: `await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();`
   - ExistsAsync: `await _dbSet.AnyAsync(e => e.Id == id);`
   - Query: `_dbSet.AsQueryable();`
   - AddManyAsync: `_dbSet.AddRange(entities); return Task.CompletedTask;`
7. Manter implementação dos métodos específicos (GetByEmailAsync, GetByRefreshTokenAsync, EmailExistsAsync)

**Critério de Sucesso:**
- Classe não herda de `BaseRepository<User>`
- Todos os 12 métodos implementados
- Nenhuma duplicação de campos
- Código compila sem erros
- Todos os testes passam (Green phase do TDD)

---

### 1.4 Refatorar e Otimizar (TDD - Refactor)
**Descrição:** Revisar código para melhorias de qualidade e performance.

**Passos:**
1. Adicionar `AsNoTracking()` onde apropriado (queries read-only)
2. Verificar se há oportunidades de usar `AsSplitQuery()` ou `Include()`
3. Garantir consistência de nomenclatura
4. Adicionar comentários XML para métodos públicos se necessário
5. Verificar tratamento de casos edge (null, empty collections, etc.)

**Critério de Sucesso:**
- Código otimizado mantendo testes passando
- Nenhum warning de análise estática
- Performance mantida ou melhorada

---

### 1.5 Validar Integração com Handlers
**Descrição:** Garantir que todos os handlers que usam IUsersRepository continuam funcionando.

**Passos:**
1. Identificar handlers que injetam IUsersRepository
2. Executar testes de integração relacionados a usuários
3. Validar endpoints de autenticação (login, refresh token)
4. Validar endpoints de CRUD de usuários

**Critério de Sucesso:**
- Todos os handlers compilam sem erros
- Todos os testes de integração passam
- Endpoints funcionam corretamente

---

## Testes

### Testes Unitários
- [ ] GetByIdAsync com usuário existente
- [ ] GetByIdAsync com usuário inexistente
- [ ] GetAllAsync retorna todos os usuários
- [ ] AddAsync adiciona usuário ao DbSet
- [ ] UpdateAsync atualiza usuário no DbSet
- [ ] RemoveAsync remove usuário do DbSet
- [ ] GetByIdsAsync retorna usuários com IDs correspondentes
- [ ] ExistsAsync retorna true quando usuário existe
- [ ] ExistsAsync retorna false quando usuário não existe
- [ ] Query retorna IQueryable de usuários
- [ ] AddManyAsync adiciona múltiplos usuários
- [ ] GetByEmailAsync encontra usuário por email
- [ ] GetByEmailAsync retorna null quando email não existe
- [ ] GetByRefreshTokenAsync encontra usuário por refresh token
- [ ] GetByRefreshTokenAsync retorna null quando token não existe
- [ ] EmailExistsAsync retorna true quando email existe
- [ ] EmailExistsAsync retorna false quando email não existe

### Testes de Integração
- [ ] Login com email e senha funciona
- [ ] Refresh token funciona
- [ ] Criar usuário funciona
- [ ] Atualizar usuário funciona
- [ ] Buscar usuário por ID funciona
- [ ] Listar todos os usuários funciona

---

## Critérios de Aceitação

- ✅ IUsersRepository não herda de IBaseRepository<User>
- ✅ UsersRepository não herda de BaseRepository<User>
- ✅ Todos os 9 métodos base implementados explicitamente
- ✅ Todos os 3 métodos específicos mantidos
- ✅ Nenhuma duplicação de campo _dbContext
- ✅ Mínimo 17 testes unitários passando
- ✅ Todos os testes de integração passando
- ✅ Código compila sem erros ou warnings
- ✅ Handlers que usam IUsersRepository funcionam corretamente

---

## Arquivos Afetados

- `src/SportHub.Application/Common/Interfaces/IUsersRepository.cs`
- `src/SportHub.Infrastructure/Repositories/UsersRepository.cs`
- `tests/SportHub.Tests/Repositories/UsersRepositoryTests.cs` (novo ou atualizado)

---

## Dependências

- **Nenhuma** - Esta tarefa pode ser executada independentemente

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Quebra de handlers existentes | Média | Alto | Executar todos os testes após refatoração |
| Perda de funcionalidade | Baixa | Alto | Seguir implementação exata do BaseRepository |
| Regressão em autenticação | Média | Alto | Testar login e refresh token manualmente |

---

## Notas Técnicas

### Padrão de Implementação
```csharp
public class UsersRepository : IUsersRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<User> _dbSet;

    public UsersRepository(ApplicationDbContext context)
    {
        _dbContext = context;
        _dbSet = context.Set<User>();
    }

    // Métodos base (9)
    public async Task<User?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    // ... outros métodos base

    // Métodos específicos (3)
    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

    // ... outros métodos específicos
}
```

### Importante
- **NÃO** adicionar `SaveChangesAsync()` nos métodos - o handler decide quando commitar
- Manter mesma assinatura de métodos para compatibilidade
- Usar `_dbSet` para operações genéricas e `_dbContext.Users` quando precisar de queries específicas

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **BaseRepository atual:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`
- **IBaseRepository atual:** `src/SportHub.Application/Common/Interfaces/IBaseRepository.cs`
