# Task 1.0: Refatoração do Modelo de Roles

> **Complexidade**: HIGH  
> **Dependências**: Nenhuma  
> **Estimativa**: 4-6 horas  
> **Status**: Pendente

---

## Objetivo

Unificar os enums `UserRole` e `EstablishmentRole` em um único enum hierárquico, removendo completamente o código legado e criando a base para o novo sistema de permissionamento.

---

## Contexto

Atualmente existem dois enums de role com sobreposição semântica:
- `UserRole`: User, EstablishmentMember, Admin, SuperAdmin
- `EstablishmentRole`: Staff, Manager, Owner

Isso causa confusão e impede que as policies de autorização funcionem corretamente. Esta tarefa unifica tudo em um único `UserRole` com hierarquia clara.

---

## Subtarefas

### 1.1 - Atualizar Enum UserRole
**Descrição**: Modificar o enum `UserRole` no Domain para incluir os novos valores hierárquicos.

**Arquivos**:
- `src/SportHub.Domain/Enums/UserRole.cs`

**Implementação**:
```csharp
public enum UserRole
{
    Customer = 0,      // Antigo: User
    Staff = 1,         // Novo
    Manager = 2,       // Novo
    Owner = 3,         // Antigo: Admin
    SuperAdmin = 99    // Mantém
}
```

**Validação**:
- Enum compila sem erros
- Valores numéricos permitem comparação hierárquica (`Customer < Staff < Manager < Owner`)

---

### 1.2 - Remover Enum EstablishmentRole
**Descrição**: Deletar completamente o enum `EstablishmentRole` e todas as suas referências.

**Arquivos a Remover**:
- `src/SportHub.Domain/Enums/EstablishmentRole.cs` (se existir)

**Arquivos a Atualizar**:
- Buscar todas as referências a `EstablishmentRole` no codebase
- Substituir por `UserRole` onde apropriado
- Remover imports/usings de `EstablishmentRole`

**Comando de Busca**:
```bash
grep -r "EstablishmentRole" src/
```

**Validação**:
- Nenhuma referência a `EstablishmentRole` permanece
- Projeto compila sem erros

---

### 1.3 - Atualizar Entidade User
**Descrição**: Garantir que a entidade `User` usa o enum `UserRole` atualizado.

**Arquivos**:
- `src/SportHub.Domain/entities/User.cs`

**Verificações**:
- Propriedade `Role` é do tipo `UserRole`
- Não há propriedades relacionadas a `EstablishmentRole`

**Validação**:
- Entidade compila corretamente
- Tipo da propriedade `Role` é `UserRole`

---

### 1.4 - Criar Migration EF Core
**Descrição**: Criar migration para refletir as mudanças no enum (se necessário).

**Nota**: Como estamos apenas renomeando valores do enum (não mudando a estrutura da tabela), a migration pode ser vazia ou apenas documentação. A migração de dados será feita na Task 7.0.

**Comando**:
```bash
cd src/SportHub.Api
dotnet ef migrations add RefactorUserRoleEnum --project ../SportHub.Infrastructure
```

**Validação**:
- Migration criada sem erros
- Revisar arquivo de migration gerado

---

### 1.5 - Atualizar Testes Unitários
**Descrição**: Atualizar testes que referenciam os valores antigos do enum.

**Arquivos**:
- `tests/SportHub.Tests/**/*.cs`

**Mudanças**:
- `UserRole.User` → `UserRole.Customer`
- `UserRole.Admin` → `UserRole.Owner`
- Remover testes que usam `EstablishmentRole`

**Validação**:
- Todos os testes compilam
- Testes unitários passam

---

### 1.6 - Remover GlobalRoleRequirement Legado
**Descrição**: Remover ou atualizar `GlobalRoleRequirement` que referencia `EstablishmentRole`.

**Arquivos**:
- `src/SportHub.Infrastructure/Authorization/GlobalRoleRequirement.cs` (ou similar)

**Ação**:
- Se o requirement usa `EstablishmentRole`, atualizar para `UserRole`
- Se houver duplicação, consolidar em um único requirement

**Validação**:
- Código compila
- Nenhuma referência a `EstablishmentRole` permanece

---

## Testes

### Testes Unitários

**Teste 1: Enum UserRole tem valores corretos**
```csharp
[Fact]
public void UserRole_ShouldHaveCorrectValues()
{
    Assert.Equal(0, (int)UserRole.Customer);
    Assert.Equal(1, (int)UserRole.Staff);
    Assert.Equal(2, (int)UserRole.Manager);
    Assert.Equal(3, (int)UserRole.Owner);
    Assert.Equal(99, (int)UserRole.SuperAdmin);
}
```

**Teste 2: Hierarquia numérica funciona**
```csharp
[Fact]
public void UserRole_ShouldSupportHierarchicalComparison()
{
    Assert.True(UserRole.Customer < UserRole.Staff);
    Assert.True(UserRole.Staff < UserRole.Manager);
    Assert.True(UserRole.Manager < UserRole.Owner);
    Assert.True(UserRole.Owner < UserRole.SuperAdmin);
}
```

**Teste 3: EstablishmentRole não existe mais**
```csharp
[Fact]
public void EstablishmentRole_ShouldNotExist()
{
    // Este teste garante que o enum foi removido
    var assembly = typeof(UserRole).Assembly;
    var establishmentRoleType = assembly.GetTypes()
        .FirstOrDefault(t => t.Name == "EstablishmentRole");
    
    Assert.Null(establishmentRoleType);
}
```

---

## Critérios de Sucesso

- ✅ Enum `UserRole` contém exatamente 5 valores: Customer(0), Staff(1), Manager(2), Owner(3), SuperAdmin(99)
- ✅ Enum `EstablishmentRole` não existe mais no codebase
- ✅ Nenhuma referência a `EstablishmentRole` permanece (grep retorna 0 resultados)
- ✅ Hierarquia numérica permite comparação `>=` entre roles
- ✅ Projeto compila sem erros
- ✅ Todos os testes unitários passam
- ✅ Migration criada (mesmo que vazia)

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Referências a `EstablishmentRole` em locais inesperados | Média | Alto | Fazer busca completa no codebase antes de remover |
| Breaking changes em código que depende dos valores antigos | Alta | Alto | Esperado - será corrigido nas próximas tasks |
| Migration complexa devido a constraints | Baixa | Médio | Revisar migration gerada antes de aplicar |

---

## Notas para o Desenvolvedor

- **Abordagem TDD**: Crie os testes unitários ANTES de fazer as mudanças no enum
- **Busca Completa**: Use `grep -r "EstablishmentRole" src/` para garantir que nada foi esquecido
- **Compilação Incremental**: Após cada subtask, compile o projeto para identificar erros rapidamente
- **Não Aplicar Migration**: A migration será aplicada junto com a Task 7.0 (migração de dados)
- **Documentação**: Anote quais arquivos foram modificados para facilitar code review

---

## Checklist de Conclusão

- [ ] Enum `UserRole` atualizado com 5 valores
- [ ] Enum `EstablishmentRole` removido
- [ ] Entidade `User` usa `UserRole` correto
- [ ] Migration criada
- [ ] Testes unitários atualizados
- [ ] `GlobalRoleRequirement` atualizado
- [ ] Busca por `EstablishmentRole` retorna 0 resultados
- [ ] Projeto compila sem erros
- [ ] Todos os testes passam
- [ ] Code review solicitado
