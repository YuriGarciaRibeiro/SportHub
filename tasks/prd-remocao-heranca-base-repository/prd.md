# PRD: Remoção da Herança de BaseRepository

**Versão:** 1.0  
**Data:** 14 de março de 2026  
**Autor:** Yuri Garcia Ribeiro  
**Status:** Em Revisão

---

## 1. Resumo Executivo

### Contexto
O SportHub atualmente utiliza um padrão Repository com uma classe base genérica `BaseRepository<T>` que fornece operações CRUD padrão. Todos os repositórios específicos (UsersRepository, CourtsRepository, SportsRepository, ReservationRepository, TenantRepository) herdam desta classe base e adicionam métodos específicos quando necessário.

### Problema
A herança do `BaseRepository<T>` introduz limitações e complexidade desnecessária:
- **Falta de controle granular**: Cada entidade tem necessidades específicas, mas herda todos os métodos genéricos
- **Métodos desnecessários**: Nem todas as entidades precisam de todos os métodos (GetAll, GetByIds, Query, etc.)
- **Dificuldade de teste**: A herança torna o mock e teste mais complexo
- **Falta de clareza**: Não fica explícito quais operações cada repositório realmente suporta
- **Sobrescrita com `new`**: Alguns repositórios (ex: CourtsRepository) precisam sobrescrever métodos base usando `new`, o que é um code smell

### Solução Proposta
Remover completamente a herança de `BaseRepository<T>` e implementar cada repositório manualmente com seus métodos específicos, mantendo consistência nos métodos comuns mas com implementação explícita.

### Objetivos de Negócio
- **Maior manutenibilidade**: Código mais explícito e fácil de entender
- **Melhor testabilidade**: Repositórios independentes são mais fáceis de testar
- **Flexibilidade**: Cada repositório pode ser otimizado para suas necessidades específicas
- **Qualidade de código**: Eliminar code smells como sobrescrita com `new`

### Métricas de Sucesso
- ✅ 100% dos repositórios refatorados sem herança
- ✅ 0 usos de `BaseRepository<T>` no código
- ✅ Todos os testes existentes continuam passando
- ✅ Cobertura de testes mantida ou aumentada
- ✅ Nenhuma regressão funcional

---

## 2. Contexto e Motivação

### Por Que Agora?
A arquitetura atual funciona, mas apresenta sinais de que a abstração genérica não está agregando valor:
1. Repositórios estão sobrescrevendo métodos base (CourtsRepository usa `new`)
2. Alguns repositórios só adicionam métodos específicos sem usar os genéricos
3. A interface `IBaseRepository<T>` força contratos que nem sempre fazem sentido
4. Dificuldade em implementar queries complexas com includes e projections

### Impacto Esperado
- **Positivo**: Código mais claro, melhor testabilidade, maior controle
- **Neutro**: Quantidade de código aumenta ligeiramente (duplicação controlada)
- **Risco**: Refatoração pode introduzir bugs se não for bem testada

### Alternativas Consideradas
1. **Manter BaseRepository**: Descartado - não resolve os problemas identificados
2. **Usar apenas DbContext diretamente**: Descartado - perde a camada de abstração útil
3. **Extension Methods para operações comuns**: Considerado mas descartado - prefere-se explicitação

---

## 3. Objetivos e Não-Objetivos

### Objetivos ✅
- Remover completamente `BaseRepository<T>` e sua implementação
- Remover `IBaseRepository<T>` e todas as heranças de interface
- Implementar cada repositório manualmente com todos os métodos necessários
- Manter consistência nos métodos comuns (GetByIdAsync, AddAsync, etc.)
- Garantir que todos os testes existentes continuem passando
- Refatorar todos os 5 repositórios existentes: Users, Courts, Sports, Reservation, Tenant

### Não-Objetivos ❌
- Adicionar novos métodos ou funcionalidades aos repositórios
- Alterar a lógica de negócio existente
- Modificar o padrão Unit of Work (ApplicationDbContext como IUnitOfWork)
- Mudar a estrutura de pastas ou namespaces
- Refatorar handlers ou use cases que usam os repositórios
- Alterar a injeção de dependência ou registros no DI container

---

## 4. Requisitos Funcionais

### RF-001: Remover BaseRepository
**Prioridade:** Alta  
**Descrição:** Deletar a classe `BaseRepository<T>` de `Infrastructure/Repositories/BaseRepository.cs`

**Critérios de Aceitação:**
- Arquivo `BaseRepository.cs` removido do projeto
- Nenhuma referência a `BaseRepository<T>` no código

---

### RF-002: Remover IBaseRepository
**Prioridade:** Alta  
**Descrição:** Deletar a interface `IBaseRepository<T>` de `Application/Common/Interfaces/IBaseRepository.cs`

**Critérios de Aceitação:**
- Arquivo `IBaseRepository.cs` removido do projeto
- Nenhuma interface herda de `IBaseRepository<T>`

---

### RF-003: Refatorar IUsersRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `IBaseRepository<User>` e adicionar métodos base explicitamente

**Critérios de Aceitação:**
- Interface não herda de `IBaseRepository<User>`
- Contém todos os métodos base: GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, RemoveAsync, GetByIdsAsync, ExistsAsync, Query, AddManyAsync
- Mantém métodos específicos: GetByEmailAsync, GetByRefreshTokenAsync, EmailExistsAsync

---

### RF-004: Refatorar UsersRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `BaseRepository<User>` e implementar todos os métodos manualmente

**Critérios de Aceitação:**
- Classe não herda de `BaseRepository<User>`
- Implementa todos os métodos da interface IUsersRepository
- Mantém o mesmo comportamento funcional
- Remove a duplicação de `_dbContext` (já existe no base)

---

### RF-005: Refatorar ICourtsRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `IBaseRepository<Court>` e adicionar métodos base explicitamente

**Critérios de Aceitação:**
- Interface não herda de `IBaseRepository<Court>`
- Contém todos os métodos base explicitamente declarados

---

### RF-006: Refatorar CourtsRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `BaseRepository<Court>` e implementar todos os métodos manualmente

**Critérios de Aceitação:**
- Classe não herda de `BaseRepository<Court>`
- Implementa todos os métodos da interface ICourtsRepository
- Remove o uso de `new` para sobrescrever métodos
- Mantém includes (Sports) nos métodos GetAllAsync e GetByIdAsync

---

### RF-007: Refatorar ISportsRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `IBaseRepository<Sport>` e adicionar métodos base explicitamente

**Critérios de Aceitação:**
- Interface não herda de `IBaseRepository<Sport>`
- Contém todos os métodos base explicitamente declarados

---

### RF-008: Refatorar SportsRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `BaseRepository<Sport>` e implementar todos os métodos manualmente

**Critérios de Aceitação:**
- Classe não herda de `BaseRepository<Sport>`
- Implementa todos os métodos da interface ISportsRepository

---

### RF-009: Refatorar IReservationRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `IBaseRepository<Reservation>` e adicionar métodos base explicitamente

**Critérios de Aceitação:**
- Interface não herda de `IBaseRepository<Reservation>`
- Contém todos os métodos base explicitamente declarados

---

### RF-010: Refatorar ReservationRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `BaseRepository<Reservation>` e implementar todos os métodos manualmente

**Critérios de Aceitação:**
- Classe não herda de `BaseRepository<Reservation>`
- Implementa todos os métodos da interface IReservationRepository

---

### RF-011: Refatorar ITenantRepository (se existir)
**Prioridade:** Alta  
**Descrição:** Remover herança de `IBaseRepository<Tenant>` e adicionar métodos base explicitamente

**Critérios de Aceitação:**
- Interface não herda de `IBaseRepository<Tenant>`
- Contém todos os métodos base explicitamente declarados
- Mantém métodos específicos de tenant

---

### RF-012: Refatorar TenantRepository
**Prioridade:** Alta  
**Descrição:** Remover herança de `BaseRepository<Tenant>` e implementar todos os métodos manualmente

**Critérios de Aceitação:**
- Classe não herda de `BaseRepository<Tenant>`
- Implementa todos os métodos da interface ITenantRepository
- Mantém o uso correto do TenantDbContext (schema public)

---

## 5. Requisitos Não-Funcionais

### RNF-001: Compatibilidade
**Descrição:** A refatoração não deve quebrar nenhum código existente que usa os repositórios

**Critérios:**
- Todos os handlers continuam funcionando sem modificação
- Todas as injeções de dependência continuam funcionando
- Nenhuma mudança de assinatura nos métodos públicos

---

### RNF-002: Testes
**Descrição:** Todos os testes existentes devem continuar passando

**Critérios:**
- 100% dos testes unitários passando
- 100% dos testes de integração passando
- Cobertura de código mantida ou aumentada

---

### RNF-003: Performance
**Descrição:** A refatoração não deve degradar a performance

**Critérios:**
- Tempo de resposta das queries mantido ou melhorado
- Uso de memória mantido ou reduzido
- Nenhuma query N+1 introduzida

---

### RNF-004: Consistência
**Descrição:** Todos os repositórios devem seguir o mesmo padrão de implementação

**Critérios:**
- Mesma estrutura de código em todos os repositórios
- Mesmos nomes de métodos para operações equivalentes
- Mesma forma de injetar e usar o DbContext

---

## 6. Escopo Detalhado

### O Que Está Incluído ✅
1. Remoção de `BaseRepository<T>` e `IBaseRepository<T>`
2. Refatoração de 5 repositórios: Users, Courts, Sports, Reservation, Tenant
3. Refatoração de 5 interfaces correspondentes
4. Validação de que todos os testes passam
5. Documentação das mudanças no código

### O Que NÃO Está Incluído ❌
1. Criação de novos repositórios
2. Adição de novos métodos aos repositórios existentes
3. Refatoração de handlers ou use cases
4. Mudanças no padrão Unit of Work
5. Alteração da estrutura de pastas
6. Mudanças no sistema de multi-tenancy
7. Otimizações de performance não relacionadas

---

## 7. Histórias de Usuário

### HU-001: Como desenvolvedor, quero repositórios explícitos
**Como** desenvolvedor do SportHub  
**Quero** que cada repositório implemente seus métodos explicitamente  
**Para** entender claramente quais operações são suportadas sem precisar navegar pela herança

**Critérios de Aceitação:**
- Posso ver todos os métodos de um repositório em sua própria classe
- Não preciso olhar classes base para entender o comportamento
- Cada método tem sua implementação visível

---

### HU-002: Como desenvolvedor, quero repositórios testáveis
**Como** desenvolvedor escrevendo testes  
**Quero** mockar repositórios sem depender de herança  
**Para** criar testes unitários mais simples e confiáveis

**Critérios de Aceitação:**
- Posso mockar qualquer repositório facilmente
- Não preciso mockar métodos de classes base
- Testes ficam mais legíveis e diretos

---

### HU-003: Como desenvolvedor, quero flexibilidade para otimizar
**Como** desenvolvedor otimizando queries  
**Quero** implementar métodos específicos sem sobrescrever com `new`  
**Para** ter código mais limpo e sem code smells

**Critérios de Aceitação:**
- Nenhum uso de `new` para sobrescrever métodos
- Posso adicionar includes, projections e otimizações livremente
- Cada repositório pode ter sua própria estratégia de query

---

## 8. Fluxos e Jornadas

### Fluxo 1: Refatoração de Um Repositório
```
1. Identificar repositório a refatorar (ex: UsersRepository)
2. Criar nova versão da interface sem herança de IBaseRepository
   - Adicionar todos os métodos base explicitamente
   - Manter métodos específicos
3. Criar nova versão da implementação sem herança de BaseRepository
   - Implementar todos os métodos base manualmente
   - Manter métodos específicos
   - Garantir mesmo comportamento
4. Executar testes do repositório
5. Corrigir eventuais problemas
6. Commit da refatoração
```

### Fluxo 2: Validação Final
```
1. Todos os repositórios refatorados
2. Remover BaseRepository.cs
3. Remover IBaseRepository.cs
4. Executar todos os testes do projeto
5. Verificar que nenhuma referência a BaseRepository existe
6. Validar que aplicação compila sem erros
7. Testar endpoints principais manualmente
8. Commit final e documentação
```

---

## 9. Dependências e Integrações

### Dependências Internas
- **ApplicationDbContext**: Todos os repositórios de tenant dependem dele
- **TenantDbContext**: TenantRepository depende dele (schema public)
- **IUnitOfWork**: Handlers dependem para SaveChangesAsync
- **Injeção de Dependência**: Registros em Program.cs ou DI extensions

### Dependências Externas
- **Entity Framework Core 9.0.7**: Para operações de banco de dados
- **PostgreSQL 16**: Banco de dados subjacente

### Integrações Afetadas
- ✅ **Handlers (MediatR)**: Continuam funcionando sem mudanças
- ✅ **Endpoints**: Continuam funcionando sem mudanças
- ✅ **Testes**: Podem precisar ajustes nos mocks

---

## 10. Considerações Técnicas

### Arquitetura
- **Clean Architecture mantida**: Repositórios continuam na camada Infrastructure
- **Interfaces na Application**: Interfaces continuam na camada Application
- **Separação de concerns**: Cada repositório responsável por sua entidade

### Tecnologias
- **.NET 10 Preview**: Versão atual do projeto
- **EF Core 9.0.7**: ORM utilizado
- **PostgreSQL 16**: Banco de dados
- **MediatR 13**: Para CQRS (não afetado)

### Padrões de Código
```csharp
// Estrutura padrão de cada repositório
public class XRepository : IXRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<X> _dbSet;

    public XRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<X>();
    }

    // Métodos base (sempre os mesmos)
    public async Task<X?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<List<X>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public Task AddAsync(X entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(X entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(X entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<X>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();

    public async Task<bool> ExistsAsync(Guid id) =>
        await _dbSet.AnyAsync(e => e.Id == id);

    public IQueryable<X> Query() =>
        _dbSet.AsQueryable();

    public Task AddManyAsync(IEnumerable<X> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    // Métodos específicos da entidade
    // ...
}
```

### Segurança
- Nenhuma mudança de segurança
- Multi-tenancy continua funcionando via middleware
- Autenticação e autorização não afetadas

---

## 11. Riscos e Mitigações

### Risco 1: Regressão Funcional
**Probabilidade:** Média  
**Impacto:** Alto  
**Mitigação:**
- Executar todos os testes após cada refatoração
- Testar manualmente endpoints principais
- Fazer refatoração incremental (um repositório por vez)
- Code review rigoroso

### Risco 2: Quebra de Testes
**Probabilidade:** Média  
**Impacto:** Médio  
**Mitigação:**
- Ajustar mocks conforme necessário
- Manter mesmas assinaturas de métodos
- Validar cobertura de testes

### Risco 3: Inconsistência entre Repositórios
**Probabilidade:** Baixa  
**Impacto:** Médio  
**Mitigação:**
- Seguir template padrão para todos os repositórios
- Code review para garantir consistência
- Documentar padrão claramente

### Risco 4: Performance Degradada
**Probabilidade:** Baixa  
**Impacto:** Alto  
**Mitigação:**
- Manter mesmas queries e estratégias
- Testar performance antes e depois
- Usar AsNoTracking onde apropriado

---

## 12. Plano de Implementação

### Fase 1: Preparação (1 dia)
- [ ] Criar branch de feature
- [ ] Executar todos os testes para baseline
- [ ] Documentar estado atual dos repositórios
- [ ] Definir template padrão de implementação

### Fase 2: Refatoração de Repositórios (3 dias)
- [ ] Refatorar UsersRepository e IUsersRepository
- [ ] Executar testes de Users
- [ ] Refatorar CourtsRepository e ICourtsRepository
- [ ] Executar testes de Courts
- [ ] Refatorar SportsRepository e ISportsRepository
- [ ] Executar testes de Sports
- [ ] Refatorar ReservationRepository e IReservationRepository
- [ ] Executar testes de Reservation
- [ ] Refatorar TenantRepository e ITenantRepository
- [ ] Executar testes de Tenant

### Fase 3: Limpeza (1 dia)
- [ ] Remover BaseRepository.cs
- [ ] Remover IBaseRepository.cs
- [ ] Verificar que não há referências remanescentes
- [ ] Executar todos os testes do projeto
- [ ] Validar compilação sem erros

### Fase 4: Validação e Documentação (1 dia)
- [ ] Testar endpoints principais manualmente
- [ ] Validar performance
- [ ] Atualizar documentação técnica
- [ ] Code review
- [ ] Merge para branch principal

**Tempo Total Estimado:** 6 dias úteis

---

## 13. Critérios de Sucesso

### Critérios Técnicos
- ✅ 0 referências a `BaseRepository<T>` no código
- ✅ 0 referências a `IBaseRepository<T>` no código
- ✅ 5 repositórios refatorados com sucesso
- ✅ 100% dos testes passando
- ✅ Projeto compila sem erros ou warnings
- ✅ Nenhuma query N+1 introduzida

### Critérios de Qualidade
- ✅ Código mais legível e explícito
- ✅ Nenhum uso de `new` para sobrescrever métodos
- ✅ Consistência entre todos os repositórios
- ✅ Documentação atualizada

### Critérios de Negócio
- ✅ Aplicação funciona sem regressões
- ✅ Performance mantida ou melhorada
- ✅ Facilidade de manutenção aumentada
- ✅ Base de código mais testável

---

## 14. Questões em Aberto

1. **Testes de integração**: Precisam ser ajustados ou continuam funcionando?
2. **Mocks existentes**: Precisam ser refatorados ou são compatíveis?
3. **Documentação técnica**: Precisa ser atualizada em `documentos/techspec-codebase.md`?
4. **Novos repositórios**: Qual template seguir para futuros repositórios?

---

## 15. Aprovações

| Papel | Nome | Data | Status |
|-------|------|------|--------|
| Product Owner | - | - | Pendente |
| Tech Lead | - | - | Pendente |
| Desenvolvedor | Yuri Garcia Ribeiro | 14/03/2026 | Aprovado |

---

## 16. Histórico de Revisões

| Versão | Data | Autor | Mudanças |
|--------|------|-------|----------|
| 1.0 | 14/03/2026 | Yuri Garcia Ribeiro | Versão inicial do PRD |

---

## Anexos

### A. Repositórios Existentes

1. **UsersRepository**
   - Métodos específicos: GetByEmailAsync, GetByRefreshTokenAsync, EmailExistsAsync
   - Usa: ApplicationDbContext

2. **CourtsRepository**
   - Sobrescreve: GetAllAsync (com Include de Sports), GetByIdAsync (com Include de Sports)
   - Usa: ApplicationDbContext

3. **SportsRepository**
   - Apenas herança, sem métodos específicos
   - Usa: ApplicationDbContext

4. **ReservationRepository**
   - A verificar métodos específicos
   - Usa: ApplicationDbContext

5. **TenantRepository**
   - Métodos específicos relacionados a tenant
   - Usa: TenantDbContext (schema public)

### B. Métodos Base a Implementar

Todos os repositórios devem implementar:
- `Task<T?> GetByIdAsync(Guid id)`
- `Task<List<T>> GetAllAsync()`
- `Task AddAsync(T entity)`
- `Task UpdateAsync(T entity)`
- `Task RemoveAsync(T entity)`
- `Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids)`
- `Task<bool> ExistsAsync(Guid id)`
- `IQueryable<T> Query()`
- `Task AddManyAsync(IEnumerable<T> entities)`

### C. Referências
- Documentação técnica: `documentos/techspec-codebase.md`
- Regras Windsurf: `.windsurf/rules/techspec-codebase.md`
