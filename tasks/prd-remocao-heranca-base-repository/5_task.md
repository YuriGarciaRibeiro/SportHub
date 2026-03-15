# Task 5.0: Refatorar TenantRepository e ITenantRepository

**Complexidade:** MEDIUM  
**Status:** Pendente  
**Responsável:** TBD  
**Estimativa:** 3 horas

---

## Objetivo

Validar que TenantRepository já está independente (não herda de BaseRepository) e ajustar ITenantRepository se necessário para remover qualquer referência a IBaseRepository. Garantir uso correto do TenantDbContext (schema public) e padrão consistente com outros repositórios.

---

## Contexto

O `TenantRepository` é especial porque **já não herda de BaseRepository** (Tenant não implementa IEntity). Ele usa `TenantDbContext` que opera no schema "public" ao invés do schema dinâmico de tenant. Esta tarefa foca em validar a implementação existente e garantir consistência com o novo padrão.

### Estado Atual
- **Interface:** `ITenantRepository` (não herda de IBaseRepository)
- **Implementação:** `TenantRepository` (não herda de BaseRepository)
- **DbContext:** Usa `TenantDbContext` (schema public)
- **Métodos específicos:** GetBySlugAsync, GetByCustomDomainAsync, SlugExistsAsync
- **Diferença:** Alguns métodos fazem SaveChangesAsync (AddAsync, UpdateAsync)

---

## Subtarefas

### 5.1 Analisar Interface ITenantRepository
**Descrição:** Verificar se a interface está consistente com o novo padrão e não tem referências a IBaseRepository.

**Passos:**
1. Abrir arquivo: `src/SportHub.Application/Common/Interfaces/ITenantRepository.cs`
2. Verificar se não herda de `IBaseRepository<Tenant>`
3. Listar todos os métodos declarados
4. Comparar com métodos base padrão
5. Identificar métodos que faltam ou que são diferentes

**Critério de Sucesso:**
- Interface não herda de IBaseRepository
- Todos os métodos necessários estão declarados
- Assinaturas são consistentes com outros repositórios

---

### 5.2 Padronizar Interface ITenantRepository
**Descrição:** Ajustar interface para seguir o padrão dos outros repositórios, adicionando métodos base que faltam.

**Passos:**
1. Adicionar métodos base que faltam (se necessário):
   - `Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);` ✅ (já existe)
   - `Task<List<Tenant>> GetAllAsync(CancellationToken ct = default);` ✅ (já existe)
   - `Task AddAsync(Tenant entity, CancellationToken ct = default);` ✅ (já existe)
   - `Task UpdateAsync(Tenant entity, CancellationToken ct = default);` ✅ (já existe)
   - Avaliar se outros métodos base são necessários (RemoveAsync, GetByIdsAsync, etc.)
2. Manter métodos específicos existentes
3. Garantir uso de CancellationToken em todos os métodos

**Critério de Sucesso:**
- Interface tem todos os métodos necessários
- CancellationToken presente em todos os métodos assíncronos
- Código compila sem erros

---

### 5.3 Criar Testes Unitários para ITenantRepository (TDD - Red)
**Descrição:** Criar testes unitários para todos os métodos.

**Passos:**
1. Criar ou atualizar arquivo de testes: `tests/SportHub.Tests/Repositories/TenantRepositoryTests.cs`
2. Criar testes para métodos base:
   - `GetByIdAsync_WhenTenantExists_ReturnsTenant`
   - `GetByIdAsync_WhenTenantNotExists_ReturnsNull`
   - `GetByIdAsync_UsesNoTracking`
   - `GetAllAsync_ReturnsAllTenantsOrderedByName`
   - `GetAllAsync_UsesNoTracking`
   - `AddAsync_AddsTenantAndSavesChanges`
   - `UpdateAsync_UpdatesTenantAndSavesChanges`
3. Criar testes para métodos específicos:
   - `GetBySlugAsync_WhenSlugExists_ReturnsTenant`
   - `GetBySlugAsync_WhenSlugNotExists_ReturnsNull`
   - `GetBySlugAsync_IsCaseInsensitive`
   - `GetBySlugAsync_UsesNoTracking`
   - `GetByCustomDomainAsync_WhenDomainExists_ReturnsTenant`
   - `GetByCustomDomainAsync_WhenDomainNotExists_ReturnsNull`
   - `GetByCustomDomainAsync_IsCaseInsensitive`
   - `GetByCustomDomainAsync_UsesNoTracking`
   - `SlugExistsAsync_WhenSlugExists_ReturnsTrue`
   - `SlugExistsAsync_WhenSlugNotExists_ReturnsFalse`
   - `SlugExistsAsync_IsCaseInsensitive`
4. Criar testes de isolamento de schema:
   - `TenantRepository_OperatesOnPublicSchema`
   - `TenantRepository_DoesNotAffectTenantSchemas`

**Critério de Sucesso:**
- Mínimo 20 testes criados
- Testes validam uso de TenantDbContext (schema public)
- Testes validam AsNoTracking em queries read-only
- Testes validam SaveChangesAsync em AddAsync e UpdateAsync

---

### 5.4 Validar e Ajustar Implementação TenantRepository
**Descrição:** Validar que a implementação atual está correta e fazer ajustes se necessário.

**Passos:**
1. Abrir arquivo: `src/SportHub.Infrastructure/Repositories/TenantRepository.cs`
2. Validar que usa `TenantDbContext` (não ApplicationDbContext)
3. Validar que métodos read-only usam `AsNoTracking()`
4. Validar que AddAsync e UpdateAsync fazem `SaveChangesAsync()`
5. Validar que busca por slug e domain são case-insensitive (ToLowerInvariant)
6. Adicionar comentários XML se necessário
7. Garantir consistência de nomenclatura com outros repositórios

**Critério de Sucesso:**
- Implementação usa TenantDbContext corretamente
- AsNoTracking usado em queries read-only
- SaveChangesAsync presente em AddAsync e UpdateAsync
- Case-insensitive funcionando corretamente
- Todos os testes passam

---

### 5.5 Documentar Diferenças do TenantRepository
**Descrição:** Documentar as diferenças específicas do TenantRepository em relação aos outros repositórios.

**Passos:**
1. Criar ou atualizar comentários no código explicando:
   - Por que usa TenantDbContext (schema public)
   - Por que AddAsync e UpdateAsync fazem SaveChangesAsync
   - Por que Tenant não implementa IEntity
2. Atualizar documentação técnica se necessário
3. Adicionar exemplos de uso

**Critério de Sucesso:**
- Código bem documentado
- Diferenças claramente explicadas
- Documentação técnica atualizada

---

### 5.6 Validar Integração com Handlers
**Descrição:** Garantir que todos os handlers que usam ITenantRepository continuam funcionando.

**Passos:**
1. Identificar handlers que injetam ITenantRepository
2. Executar testes de integração relacionados a tenants
3. Validar endpoints de CRUD de tenants
4. Validar provisioning de novos tenants
5. Validar multi-tenancy (resolução por slug e custom domain)

**Critério de Sucesso:**
- Todos os handlers compilam sem erros
- Todos os testes de integração passam
- Endpoints funcionam corretamente
- Provisioning funciona corretamente
- Multi-tenancy funciona corretamente

---

## Testes

### Testes Unitários
- [ ] GetByIdAsync retorna tenant quando existe
- [ ] GetByIdAsync retorna null quando não existe
- [ ] GetByIdAsync usa AsNoTracking
- [ ] GetAllAsync retorna todos os tenants ordenados por nome
- [ ] GetAllAsync usa AsNoTracking
- [ ] AddAsync adiciona tenant e salva mudanças
- [ ] UpdateAsync atualiza tenant e salva mudanças
- [ ] GetBySlugAsync encontra tenant por slug
- [ ] GetBySlugAsync retorna null quando slug não existe
- [ ] GetBySlugAsync é case-insensitive
- [ ] GetBySlugAsync usa AsNoTracking
- [ ] GetByCustomDomainAsync encontra tenant por domínio
- [ ] GetByCustomDomainAsync retorna null quando domínio não existe
- [ ] GetByCustomDomainAsync é case-insensitive
- [ ] GetByCustomDomainAsync usa AsNoTracking
- [ ] SlugExistsAsync retorna true quando slug existe
- [ ] SlugExistsAsync retorna false quando slug não existe
- [ ] SlugExistsAsync é case-insensitive
- [ ] TenantRepository opera no schema public
- [ ] TenantRepository não afeta schemas de tenant

### Testes de Integração
- [ ] Criar tenant funciona
- [ ] Atualizar tenant funciona
- [ ] Buscar tenant por ID funciona
- [ ] Buscar tenant por slug funciona (case-insensitive)
- [ ] Buscar tenant por custom domain funciona (case-insensitive)
- [ ] Listar todos os tenants funciona
- [ ] Provisioning de novo tenant funciona
- [ ] Multi-tenancy resolve tenant por slug
- [ ] Multi-tenancy resolve tenant por custom domain

---

## Critérios de Aceitação

- ✅ ITenantRepository não herda de IBaseRepository
- ✅ TenantRepository não herda de BaseRepository
- ✅ TenantRepository usa TenantDbContext (schema public)
- ✅ AsNoTracking usado em queries read-only
- ✅ SaveChangesAsync presente em AddAsync e UpdateAsync
- ✅ Busca por slug e domain são case-insensitive
- ✅ Métodos específicos mantidos e funcionando
- ✅ Mínimo 20 testes unitários passando
- ✅ Todos os testes de integração passando
- ✅ Código compila sem erros ou warnings
- ✅ Multi-tenancy funcionando corretamente
- ✅ Diferenças documentadas no código

---

## Arquivos Afetados

- `src/SportHub.Application/Common/Interfaces/ITenantRepository.cs`
- `src/SportHub.Infrastructure/Repositories/TenantRepository.cs`
- `tests/SportHub.Tests/Repositories/TenantRepositoryTests.cs` (novo ou atualizado)
- `documentos/techspec-codebase.md` (atualização de documentação)

---

## Dependências

- **Nenhuma** - Esta tarefa pode ser executada independentemente

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Quebra de multi-tenancy | Baixa | Crítico | Testes extensivos de resolução de tenant |
| Problemas com schema public | Baixa | Alto | Validar uso correto do TenantDbContext |
| Quebra de provisioning | Baixa | Alto | Testar criação de novos tenants |
| Case-sensitivity quebrada | Baixa | Médio | Validar ToLowerInvariant nos testes |

---

## Notas Técnicas

### Padrão de Implementação (Atual)
```csharp
public class TenantRepository : ITenantRepository
{
    private readonly TenantDbContext _context;

    public TenantRepository(TenantDbContext context)
    {
        _context = context;
    }

    // Queries read-only com AsNoTracking
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    // Mutations COM SaveChangesAsync
    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.AddAsync(tenant, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(ct);
    }
}
```

### Importante - Diferenças do TenantRepository

1. **TenantDbContext vs ApplicationDbContext:**
   - TenantRepository usa `TenantDbContext` (schema "public")
   - Outros repositórios usam `ApplicationDbContext` (schema dinâmico por tenant)

2. **SaveChangesAsync:**
   - TenantRepository faz `SaveChangesAsync()` em AddAsync e UpdateAsync
   - Outros repositórios NÃO fazem SaveChangesAsync (handler decide)
   - **Razão:** Tenant é gerenciado no schema public, não faz parte do Unit of Work de tenant

3. **IEntity:**
   - Tenant NÃO implementa IEntity (não tem soft delete, audit, etc.)
   - Por isso nunca herdou de BaseRepository

4. **CancellationToken:**
   - TenantRepository usa CancellationToken em todos os métodos
   - Considerar adicionar em outros repositórios no futuro

### Importante - Multi-tenancy
- Slug e CustomDomain são convertidos para lowercase (ToLowerInvariant)
- Busca é case-insensitive
- TenantResolutionMiddleware depende destes métodos

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **TenantRepository atual:** `src/SportHub.Infrastructure/Repositories/TenantRepository.cs`
- **Tech Spec:** `documentos/techspec-codebase.md`
- **Multi-tenancy:** `src/SportHub.Infrastructure/Middleware/TenantResolutionMiddleware.cs`
