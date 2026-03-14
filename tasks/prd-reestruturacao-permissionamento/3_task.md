# Task 3.0: Implementação de Authorization Policies

> **Complexidade**: MEDIUM  
> **Dependências**: Task 1.0, 2.0  
> **Estimativa**: 3-4 horas  
> **Status**: Pendente

---

## Objetivo

Registrar e implementar as authorization policies baseadas no enum `UserRole` unificado, atualizando o `GlobalRoleHandler` e o `CurrentUserService` para funcionar corretamente com a nova estrutura.

---

## Contexto

As policies atuais (`IsStaff`, `IsManager`, `IsOwner`) estão declaradas mas não registradas, e o `GlobalRoleHandler` tenta parsear o claim como `EstablishmentRole` (que não existe mais). Esta tarefa corrige toda a infraestrutura de autorização.

---

## Subtarefas

### 3.1 - Atualizar PolicyNames
**Descrição**: Limpar `PolicyNames` removendo policies órfãs e mantendo apenas as corretas.

**Arquivos**:
- `src/SportHub.Infrastructure/Authorization/PolicyNames.cs` (ou similar)

**Implementação**:
```csharp
public static class PolicyNames
{
    public const string IsStaff = "IsStaff";
    public const string IsManager = "IsManager";
    public const string IsOwner = "IsOwner";
    public const string IsSuperAdmin = "IsSuperAdmin";
    
    // Remover:
    // public const string IsEstablishmentStaff = "IsEstablishmentStaff";
    // public const string IsEstablishmentManager = "IsEstablishmentManager";
    // public const string IsEstablishmentOwner = "IsEstablishmentOwner";
}
```

**Validação**:
- Apenas 4 policies declaradas
- Nenhuma referência a `EstablishmentRole`

---

### 3.2 - Atualizar GlobalRoleRequirement
**Descrição**: Modificar `GlobalRoleRequirement` para usar `UserRole` em vez de `EstablishmentRole`.

**Arquivos**:
- `src/SportHub.Infrastructure/Authorization/GlobalRoleRequirement.cs`

**Implementação**:
```csharp
public class GlobalRoleRequirement : IAuthorizationRequirement
{
    public UserRole MinimumRole { get; }

    public GlobalRoleRequirement(UserRole minimumRole)
    {
        MinimumRole = minimumRole;
    }
}
```

**Validação**:
- Requirement usa `UserRole`
- Compila sem erros

---

### 3.3 - Atualizar GlobalRoleHandler
**Descrição**: Modificar o handler para parsear o claim como `UserRole` e comparar hierarquicamente.

**Arquivos**:
- `src/SportHub.Infrastructure/Authorization/GlobalRoleHandler.cs`

**Implementação**:
```csharp
public class GlobalRoleHandler : AuthorizationHandler<GlobalRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GlobalRoleRequirement requirement)
    {
        var roleClaim = context.User.FindFirst("Role");
        
        if (roleClaim == null)
        {
            return Task.CompletedTask;
        }

        if (!Enum.TryParse<UserRole>(roleClaim.Value, out var userRole))
        {
            return Task.CompletedTask;
        }

        // Comparação hierárquica: userRole >= requirement.MinimumRole
        if (userRole >= requirement.MinimumRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

**Validação**:
- Handler parseia claim como `UserRole`
- Comparação hierárquica funciona (`>=`)
- Compila sem erros

---

### 3.4 - Registrar Policies em AddAuthorization
**Descrição**: Registrar as 4 policies na configuração de autorização.

**Arquivos**:
- `src/SportHub.Api/Extensions/ServiceCollectionExtensions.cs` (ou onde `AddAuthorization` é chamado)

**Implementação**:
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.IsStaff, policy =>
        policy.Requirements.Add(new GlobalRoleRequirement(UserRole.Staff)));
    
    options.AddPolicy(PolicyNames.IsManager, policy =>
        policy.Requirements.Add(new GlobalRoleRequirement(UserRole.Manager)));
    
    options.AddPolicy(PolicyNames.IsOwner, policy =>
        policy.Requirements.Add(new GlobalRoleRequirement(UserRole.Owner)));
    
    options.AddPolicy(PolicyNames.IsSuperAdmin, policy =>
        policy.Requirements.Add(new GlobalRoleRequirement(UserRole.SuperAdmin)));
});

// Registrar o handler
services.AddScoped<IAuthorizationHandler, GlobalRoleHandler>();
```

**Validação**:
- 4 policies registradas
- Handler registrado no DI container
- Aplicação inicia sem erros

---

### 3.5 - Atualizar ICurrentUserService
**Descrição**: Remover propriedade `EstablishmentRole` e adicionar `UserRole`.

**Arquivos**:
- `src/SportHub.Application/Common/Interfaces/ICurrentUserService.cs`

**Implementação**:
```csharp
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    UserRole UserRole { get; }  // ✅ Nova propriedade
    bool IsAuthenticated { get; }
    
    // Remover:
    // EstablishmentRole EstablishmentRole { get; }
}
```

**Validação**:
- Interface compila
- Propriedade `UserRole` adicionada
- Propriedade `EstablishmentRole` removida

---

### 3.6 - Atualizar CurrentUserService
**Descrição**: Implementar a propriedade `UserRole` parseando o claim correto.

**Arquivos**:
- `src/SportHub.Infrastructure/Services/CurrentUserService.cs`

**Implementação**:
```csharp
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
        }
    }

    public string Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }

    public UserRole UserRole
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("Role");
            
            if (roleClaim == null)
                return UserRole.Customer; // Default
            
            return Enum.TryParse<UserRole>(roleClaim.Value, out var role)
                ? role
                : UserRole.Customer;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
```

**Validação**:
- Service compila
- Propriedade `UserRole` retorna enum correto
- Default é `Customer` se claim não existir

---

### 3.7 - Atualizar Código que Usa CurrentUserService
**Descrição**: Buscar e atualizar código que usa `CurrentUserService.EstablishmentRole`.

**Comando de Busca**:
```bash
grep -r "EstablishmentRole" src/SportHub.Application/
grep -r "EstablishmentRole" src/SportHub.Infrastructure/
```

**Ação**:
- Substituir `_currentUserService.EstablishmentRole` por `_currentUserService.UserRole`
- Atualizar comparações para usar `UserRole` enum

**Validação**:
- Nenhuma referência a `EstablishmentRole` permanece
- Código compila

---

### 3.8 - Criar Testes Unitários de Policies
**Descrição**: Criar testes para validar que as policies funcionam corretamente.

**Arquivos**:
- `tests/SportHub.Tests/Authorization/GlobalRoleHandlerTests.cs` (novo)

**Validação**:
- Testes compilam
- Testes passam

---

## Testes

### Testes Unitários

**Teste 1: IsStaff policy permite Staff, Manager e Owner**
```csharp
[Theory]
[InlineData(UserRole.Staff, true)]
[InlineData(UserRole.Manager, true)]
[InlineData(UserRole.Owner, true)]
[InlineData(UserRole.Customer, false)]
public async Task IsStaffPolicy_ShouldAuthorizeCorrectly(UserRole role, bool shouldSucceed)
{
    // Arrange
    var requirement = new GlobalRoleRequirement(UserRole.Staff);
    var user = CreateClaimsPrincipal(role);
    var context = new AuthorizationHandlerContext(
        new[] { requirement }, user, null);
    var handler = new GlobalRoleHandler();

    // Act
    await handler.HandleAsync(context);

    // Assert
    Assert.Equal(shouldSucceed, context.HasSucceeded);
}
```

**Teste 2: IsManager policy permite apenas Manager e Owner**
```csharp
[Theory]
[InlineData(UserRole.Manager, true)]
[InlineData(UserRole.Owner, true)]
[InlineData(UserRole.Staff, false)]
[InlineData(UserRole.Customer, false)]
public async Task IsManagerPolicy_ShouldAuthorizeCorrectly(UserRole role, bool shouldSucceed)
{
    // Arrange
    var requirement = new GlobalRoleRequirement(UserRole.Manager);
    var user = CreateClaimsPrincipal(role);
    var context = new AuthorizationHandlerContext(
        new[] { requirement }, user, null);
    var handler = new GlobalRoleHandler();

    // Act
    await handler.HandleAsync(context);

    // Assert
    Assert.Equal(shouldSucceed, context.HasSucceeded);
}
```

**Teste 3: IsOwner policy permite apenas Owner**
```csharp
[Theory]
[InlineData(UserRole.Owner, true)]
[InlineData(UserRole.Manager, false)]
[InlineData(UserRole.Staff, false)]
[InlineData(UserRole.Customer, false)]
public async Task IsOwnerPolicy_ShouldAuthorizeCorrectly(UserRole role, bool shouldSucceed)
{
    // Arrange
    var requirement = new GlobalRoleRequirement(UserRole.Owner);
    var user = CreateClaimsPrincipal(role);
    var context = new AuthorizationHandlerContext(
        new[] { requirement }, user, null);
    var handler = new GlobalRoleHandler();

    // Act
    await handler.HandleAsync(context);

    // Assert
    Assert.Equal(shouldSucceed, context.HasSucceeded);
}
```

**Teste 4: CurrentUserService retorna role correto**
```csharp
[Fact]
public void CurrentUserService_ShouldReturnCorrectUserRole()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    var claims = new List<Claim>
    {
        new Claim("Role", UserRole.Manager.ToString())
    };
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
    
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
    var service = new CurrentUserService(httpContextAccessor);

    // Act
    var role = service.UserRole;

    // Assert
    Assert.Equal(UserRole.Manager, role);
}
```

**Helper Method**:
```csharp
private ClaimsPrincipal CreateClaimsPrincipal(UserRole role)
{
    var claims = new List<Claim>
    {
        new Claim("Role", role.ToString())
    };
    return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
}
```

---

## Critérios de Sucesso

- ✅ `PolicyNames` contém apenas 4 policies (IsStaff, IsManager, IsOwner, IsSuperAdmin)
- ✅ `GlobalRoleRequirement` usa `UserRole`
- ✅ `GlobalRoleHandler` parseia claim como `UserRole` e compara hierarquicamente
- ✅ 4 policies registradas em `AddAuthorization`
- ✅ `ICurrentUserService` tem propriedade `UserRole`
- ✅ `CurrentUserService` implementa `UserRole` corretamente
- ✅ Nenhuma referência a `EstablishmentRole` permanece
- ✅ Todos os testes de policies passam

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Código existente usa `EstablishmentRole` | Média | Alto | Busca completa no codebase |
| Policies não funcionam após mudança | Baixa | Alto | Testes unitários abrangentes |
| Comparação hierárquica falha | Baixa | Alto | Validar com testes de todos os roles |

---

## Notas para o Desenvolvedor

- **Hierarquia Numérica**: A comparação `>=` funciona porque os valores do enum são: Customer(0) < Staff(1) < Manager(2) < Owner(3)
- **SuperAdmin**: SuperAdmin(99) é tratado separadamente - não faz parte da hierarquia normal
- **Default Role**: Se o claim não existir, `CurrentUserService` retorna `Customer` por segurança
- **Testes Primeiro**: Crie os testes unitários ANTES de implementar as mudanças

---

## Checklist de Conclusão

- [ ] `PolicyNames` limpo
- [ ] `GlobalRoleRequirement` atualizado
- [ ] `GlobalRoleHandler` atualizado
- [ ] Policies registradas em `AddAuthorization`
- [ ] Handler registrado no DI
- [ ] `ICurrentUserService` atualizado
- [ ] `CurrentUserService` implementado
- [ ] Código que usa `EstablishmentRole` atualizado
- [ ] Testes unitários criados
- [ ] Todos os testes passam
- [ ] Code review solicitado
