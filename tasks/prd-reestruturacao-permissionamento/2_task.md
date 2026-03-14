# Task 2.0: Atualização do Sistema de Autenticação

> **Complexidade**: MEDIUM  
> **Dependências**: Task 1.0  
> **Estimativa**: 3-4 horas  
> **Status**: Pendente

---

## Objetivo

Garantir que os fluxos de registro, login, refresh token e provisioning de tenant usem os novos valores do enum `UserRole` unificado, e que o JWT contenha o claim correto.

---

## Contexto

Com o enum unificado, precisamos atualizar:
- Registro de novos usuários → `UserRole.Customer`
- Provisioning de tenant → Owner com `UserRole.Owner`
- JWT claim `Role` → valores do enum unificado
- Response de autenticação → incluir role correto

---

## Subtarefas

### 2.1 - Atualizar RegisterUserHandler
**Descrição**: Modificar o handler de registro para atribuir `UserRole.Customer` aos novos usuários.

**Arquivos**:
- `src/SportHub.Application/CQRS/Auth/Commands/RegisterUser/RegisterUserHandler.cs`

**Mudança**:
```csharp
// ANTES
var user = new User
{
    // ...
    Role = UserRole.User  // ❌ Valor antigo
};

// DEPOIS
var user = new User
{
    // ...
    Role = UserRole.Customer  // ✅ Novo valor
};
```

**Validação**:
- Handler compila
- Novo usuário criado tem `Role = Customer`

---

### 2.2 - Atualizar TenantProvisioningService
**Descrição**: Modificar o seeding do Owner para usar `UserRole.Owner`.

**Arquivos**:
- `src/SportHub.Infrastructure/Persistence/TenantProvisioningService.cs`
- Método: `SeedOwnerUserAsync`

**Mudança**:
```csharp
// ANTES
var ownerUser = new User
{
    // ...
    Role = UserRole.Admin  // ❌ Valor antigo
};

// DEPOIS
var ownerUser = new User
{
    // ...
    Role = UserRole.Owner  // ✅ Novo valor
};
```

**Validação**:
- Service compila
- Owner criado no provisioning tem `Role = Owner`

---

### 2.3 - Verificar JwtService.GenerateToken
**Descrição**: Confirmar que o `JwtService` já grava o claim `Role` corretamente.

**Arquivos**:
- `src/SportHub.Infrastructure/Services/JwtService.cs`

**Verificação**:
```csharp
// O método GenerateToken deve receber role como string
// e adicionar ao claim "Role"
claims.Add(new Claim("Role", role));
```

**Ação**:
- Se já está correto → nenhuma mudança necessária
- Se não existe claim `Role` → adicionar

**Validação**:
- JWT gerado contém claim `Role` com valor string do enum

---

### 2.4 - Atualizar LoginUserHandler
**Descrição**: Garantir que o login passa `user.Role.ToString()` para o `JwtService`.

**Arquivos**:
- `src/SportHub.Application/CQRS/Auth/Commands/LoginUser/LoginUserHandler.cs`

**Verificação**:
```csharp
var token = _jwtService.GenerateToken(
    user.Id.ToString(),
    user.Email,
    user.Role.ToString()  // ✅ Deve passar o enum como string
);
```

**Validação**:
- Handler compila
- Token gerado contém role correto

---

### 2.5 - Atualizar RefreshTokenHandler
**Descrição**: Garantir que o refresh token também usa o role correto.

**Arquivos**:
- `src/SportHub.Application/CQRS/Auth/Commands/RefreshToken/RefreshTokenHandler.cs`

**Verificação**:
```csharp
var newAccessToken = _jwtService.GenerateToken(
    user.Id.ToString(),
    user.Email,
    user.Role.ToString()  // ✅ Deve passar o enum como string
);
```

**Validação**:
- Handler compila
- Novo token contém role correto

---

### 2.6 - Atualizar AuthResponse DTO
**Descrição**: Garantir que o response de autenticação retorna o role do usuário.

**Arquivos**:
- `src/SportHub.Application/CQRS/Auth/Common/AuthResponse.cs` (ou similar)

**Verificação**:
```csharp
public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string Role { get; set; }  // ✅ Deve existir
    // ... outros campos
}
```

**Ação**:
- Se propriedade `Role` não existe → adicionar
- Atualizar handlers para popular este campo

**Validação**:
- DTO compila
- Response de login/refresh contém role

---

### 2.7 - Atualizar Testes de Autenticação
**Descrição**: Atualizar testes unitários e de integração de auth.

**Arquivos**:
- `tests/SportHub.Tests/**/*Auth*.cs`

**Mudanças**:
- Testes de registro devem esperar `UserRole.Customer`
- Testes de provisioning devem esperar `UserRole.Owner`
- Testes de JWT devem validar claim `Role`

**Validação**:
- Testes compilam
- Testes passam

---

## Testes

### Testes Unitários

**Teste 1: RegisterUserHandler atribui Customer**
```csharp
[Fact]
public async Task RegisterUser_ShouldAssignCustomerRole()
{
    // Arrange
    var command = new RegisterUserCommand
    {
        Email = "test@example.com",
        Password = "Password123!",
        Name = "Test User"
    };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    var user = await _userRepository.GetByEmailAsync(command.Email);
    Assert.Equal(UserRole.Customer, user.Role);
}
```

**Teste 2: TenantProvisioningService cria Owner**
```csharp
[Fact]
public async Task SeedOwnerUser_ShouldCreateOwnerRole()
{
    // Arrange
    var tenantSlug = "test-tenant";

    // Act
    await _provisioningService.SeedOwnerUserAsync(tenantSlug);

    // Assert
    var owner = await _userRepository.GetByEmailAsync($"owner@{tenantSlug}.com");
    Assert.NotNull(owner);
    Assert.Equal(UserRole.Owner, owner.Role);
}
```

**Teste 3: JWT contém claim Role correto**
```csharp
[Fact]
public void GenerateToken_ShouldIncludeRoleClaim()
{
    // Arrange
    var userId = Guid.NewGuid().ToString();
    var email = "test@example.com";
    var role = UserRole.Manager.ToString();

    // Act
    var token = _jwtService.GenerateToken(userId, email, role);

    // Assert
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);
    var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Role");
    
    Assert.NotNull(roleClaim);
    Assert.Equal("Manager", roleClaim.Value);
}
```

### Testes de Integração

**Teste 4: Login retorna role no response**
```csharp
[Fact]
public async Task Login_ShouldReturnRoleInResponse()
{
    // Arrange
    var registerCommand = new RegisterUserCommand
    {
        Email = "customer@test.com",
        Password = "Password123!",
        Name = "Customer Test"
    };
    await _mediator.Send(registerCommand);

    var loginCommand = new LoginUserCommand
    {
        Email = "customer@test.com",
        Password = "Password123!"
    };

    // Act
    var result = await _mediator.Send(loginCommand);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Customer", result.Value.Role);
}
```

---

## Critérios de Sucesso

- ✅ `RegisterUserHandler` atribui `UserRole.Customer` a novos usuários
- ✅ `TenantProvisioningService` cria Owner com `UserRole.Owner`
- ✅ JWT contém claim `Role` com valor string do enum
- ✅ `AuthResponse` inclui propriedade `Role`
- ✅ Login retorna role correto no response
- ✅ Refresh token mantém role correto
- ✅ Todos os testes de autenticação passam

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Tokens existentes param de funcionar | Alta | Médio | Esperado - documentar que forçará re-login |
| AuthResponse não tem campo Role | Média | Médio | Adicionar campo e atualizar handlers |
| JwtService não adiciona claim Role | Baixa | Alto | Verificar implementação atual antes de modificar |

---

## Notas para o Desenvolvedor

- **Tokens Existentes**: Esta mudança invalidará tokens JWT existentes. Isso é aceitável e forçará re-login.
- **Frontend Impact**: O front-end precisará ajustar para usar os novos valores de role (`Customer`, `Staff`, `Manager`, `Owner`).
- **Verificação de Claims**: Use um JWT decoder online para validar que o token contém o claim `Role` correto.
- **Seed Data**: Se houver dados de seed/fixtures, atualize para usar os novos roles.

---

## Checklist de Conclusão

- [ ] `RegisterUserHandler` atualizado
- [ ] `TenantProvisioningService` atualizado
- [ ] `JwtService` verificado/atualizado
- [ ] `LoginUserHandler` verificado/atualizado
- [ ] `RefreshTokenHandler` verificado/atualizado
- [ ] `AuthResponse` inclui campo `Role`
- [ ] Testes unitários atualizados
- [ ] Testes de integração atualizados
- [ ] Todos os testes passam
- [ ] Code review solicitado
