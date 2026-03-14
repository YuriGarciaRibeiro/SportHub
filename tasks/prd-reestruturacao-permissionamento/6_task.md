# Task 6.0: Implementação de Endpoints de Gestão de Membros

> **Complexidade**: HIGH  
> **Dependências**: Task 3.0, 4.0  
> **Estimativa**: 6-8 horas  
> **Status**: Pendente  
> **Abordagem**: TDD (Red-Green-Refactor)

---

## Objetivo

Criar endpoints completos para o Owner gerenciar membros operacionais do tenant (listar, alterar role, rebaixar), seguindo os padrões CQRS, Result Pattern e FluentValidation.

---

## Contexto

Atualmente não existem endpoints para gestão de membros. O Owner precisa poder:
- Listar todos os membros operacionais (Staff, Manager, Owner)
- Alterar o role de um membro (Customer ↔ Staff ↔ Manager)
- Rebaixar um membro para Customer

**Regras de Negócio**:
- Apenas Owner pode acessar esses endpoints
- Owner não pode se auto-rebaixar
- Owner não pode promover outro user a Owner (apenas 1 Owner por tenant)
- Roles permitidos: Customer, Staff, Manager

---

## Abordagem TDD

Esta tarefa deve seguir **Red-Green-Refactor**:
1. **Red**: Escrever testes que falham
2. **Green**: Implementar código mínimo para passar
3. **Refactor**: Melhorar código mantendo testes passando

---

## Subtarefas

### 6.1 - Criar Testes para GET /api/members (RED)
**Descrição**: Criar testes ANTES da implementação.

**Arquivos**:
- `tests/SportHub.Tests/Members/GetMembersQueryTests.cs` (novo)

**Testes a Criar**:
```csharp
[Fact]
public async Task GetMembers_ShouldReturnOnlyOperationalMembers()
{
    // Deve retornar apenas users com role >= Staff
}

[Fact]
public async Task GetMembers_ShouldNotReturnCustomers()
{
    // Customers não devem aparecer na lista
}

[Fact]
public async Task GetMembers_AsNonOwner_ShouldBeForbidden()
{
    // Apenas Owner pode listar membros
}
```

**Validação**:
- Testes compilam mas falham (RED)

---

### 6.2 - Implementar GET /api/members (GREEN)
**Descrição**: Implementar query, handler e endpoint.

**Arquivos a Criar**:
- `src/SportHub.Application/CQRS/Members/Queries/GetMembers/GetMembersQuery.cs`
- `src/SportHub.Application/CQRS/Members/Queries/GetMembers/GetMembersQueryHandler.cs`
- `src/SportHub.Application/CQRS/Members/Queries/GetMembers/MemberDto.cs`

**GetMembersQuery.cs**:
```csharp
public record GetMembersQuery : IQuery<List<MemberDto>>;
```

**MemberDto.cs**:
```csharp
public class MemberDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**GetMembersQueryHandler.cs**:
```csharp
public class GetMembersQueryHandler : IQueryHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IUserRepository _userRepository;

    public GetMembersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<MemberDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        // Buscar apenas users com role >= Staff
        var members = await _userRepository.GetMembersAsync(); // Novo método no repository

        var memberDtos = members.Select(m => new MemberDto
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Role = m.Role.ToString(),
            CreatedAt = m.CreatedAt
        }).ToList();

        return Result.Ok(memberDtos);
    }
}
```

**Endpoint** (`src/SportHub.Api/Endpoints/MembersEndpoints.cs` - novo):
```csharp
public static class MembersEndpoints
{
    public static void MapMembersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        // GET /api/members
        group.MapGet("", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMembersQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .RequireAuthorization(PolicyNames.IsOwner);
    }
}
```

**Registrar Endpoint** (`src/SportHub.Api/Program.cs`):
```csharp
app.MapMembersEndpoints();
```

**Adicionar Método no Repository** (`src/SportHub.Infrastructure/Persistence/Repositories/UserRepository.cs`):
```csharp
public async Task<List<User>> GetMembersAsync()
{
    return await _context.Users
        .Where(u => u.Role >= UserRole.Staff)
        .OrderBy(u => u.Name)
        .ToListAsync();
}
```

**Atualizar Interface** (`src/SportHub.Application/Common/Interfaces/IUserRepository.cs`):
```csharp
Task<List<User>> GetMembersAsync();
```

**Validação**:
- Testes passam (GREEN)

---

### 6.3 - Refatorar GET /api/members (REFACTOR)
**Descrição**: Melhorar código mantendo testes passando.

**Possíveis Melhorias**:
- Adicionar paginação (se necessário)
- Adicionar filtros (por role, por nome)
- Otimizar query

**Validação**:
- Testes continuam passando após refatoração

---

### 6.4 - Criar Testes para PATCH /api/members/{userId}/role (RED)
**Descrição**: Criar testes ANTES da implementação.

**Arquivos**:
- `tests/SportHub.Tests/Members/UpdateMemberRoleCommandTests.cs` (novo)

**Testes a Criar**:
```csharp
[Fact]
public async Task UpdateMemberRole_CustomerToStaff_ShouldSucceed()

[Fact]
public async Task UpdateMemberRole_StaffToManager_ShouldSucceed()

[Fact]
public async Task UpdateMemberRole_ToOwner_ShouldFail()
// Owner não pode promover a Owner

[Fact]
public async Task UpdateMemberRole_SelfDemotion_ShouldFail()
// Owner não pode se auto-rebaixar

[Fact]
public async Task UpdateMemberRole_AsNonOwner_ShouldBeForbidden()
```

**Validação**:
- Testes compilam mas falham (RED)

---

### 6.5 - Implementar PATCH /api/members/{userId}/role (GREEN)
**Descrição**: Implementar command, validator, handler e endpoint.

**Arquivos a Criar**:
- `src/SportHub.Application/CQRS/Members/Commands/UpdateMemberRole/UpdateMemberRoleCommand.cs`
- `src/SportHub.Application/CQRS/Members/Commands/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs`
- `src/SportHub.Application/CQRS/Members/Commands/UpdateMemberRole/UpdateMemberRoleValidator.cs`

**UpdateMemberRoleCommand.cs**:
```csharp
public record UpdateMemberRoleCommand(Guid UserId, string NewRole) : ICommand;
```

**UpdateMemberRoleValidator.cs**:
```csharp
public class UpdateMemberRoleValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.NewRole)
            .NotEmpty()
            .WithMessage("Role é obrigatório")
            .Must(BeValidRole)
            .WithMessage("Role deve ser Customer, Staff ou Manager");
    }

    private bool BeValidRole(string role)
    {
        if (!Enum.TryParse<UserRole>(role, out var userRole))
            return false;

        // Apenas Customer, Staff e Manager são permitidos
        return userRole == UserRole.Customer 
            || userRole == UserRole.Staff 
            || userRole == UserRole.Manager;
    }
}
```

**UpdateMemberRoleCommandHandler.cs**:
```csharp
public class UpdateMemberRoleCommandHandler : ICommandHandler<UpdateMemberRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberRoleCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar usuário alvo
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Fail("Usuário não encontrado");
        }

        // 2. Parsear novo role
        if (!Enum.TryParse<UserRole>(request.NewRole, out var newRole))
        {
            return Result.Fail("Role inválido");
        }

        // 3. Validar regras de negócio
        var currentUserId = _currentUserService.UserId;

        // Regra: Owner não pode se auto-rebaixar
        if (user.Id == currentUserId && newRole < UserRole.Owner)
        {
            return Result.Fail("Você não pode alterar seu próprio role");
        }

        // Regra: Não pode promover a Owner
        if (newRole == UserRole.Owner)
        {
            return Result.Fail("Não é permitido promover usuários a Owner");
        }

        // Regra: Apenas Customer, Staff e Manager são permitidos
        if (newRole != UserRole.Customer && newRole != UserRole.Staff && newRole != UserRole.Manager)
        {
            return Result.Fail("Role deve ser Customer, Staff ou Manager");
        }

        // 4. Atualizar role
        user.UpdateRole(newRole); // Método de domínio

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
```

**Endpoint**:
```csharp
// PATCH /api/members/{userId}/role
group.MapPatch("{userId}/role", async (Guid userId, UpdateMemberRoleRequest request, ISender sender) =>
{
    var command = new UpdateMemberRoleCommand(userId, request.NewRole);
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsOwner);

// DTO para request
public record UpdateMemberRoleRequest(string NewRole);
```

**Adicionar Método na Entidade User**:
```csharp
public void UpdateRole(UserRole newRole)
{
    Role = newRole;
}
```

**Validação**:
- Testes passam (GREEN)

---

### 6.6 - Refatorar PATCH /api/members/{userId}/role (REFACTOR)
**Descrição**: Melhorar código mantendo testes passando.

**Validação**:
- Testes continuam passando

---

### 6.7 - Criar Testes para DELETE /api/members/{userId} (RED)
**Descrição**: Criar testes ANTES da implementação.

**Arquivos**:
- `tests/SportHub.Tests/Members/RemoveMemberCommandTests.cs` (novo)

**Testes a Criar**:
```csharp
[Fact]
public async Task RemoveMember_ShouldDemoteToCustomer()

[Fact]
public async Task RemoveMember_SelfRemoval_ShouldFail()

[Fact]
public async Task RemoveMember_AsNonOwner_ShouldBeForbidden()
```

**Validação**:
- Testes compilam mas falham (RED)

---

### 6.8 - Implementar DELETE /api/members/{userId} (GREEN)
**Descrição**: Implementar command, validator, handler e endpoint.

**Arquivos a Criar**:
- `src/SportHub.Application/CQRS/Members/Commands/RemoveMember/RemoveMemberCommand.cs`
- `src/SportHub.Application/CQRS/Members/Commands/RemoveMember/RemoveMemberCommandHandler.cs`
- `src/SportHub.Application/CQRS/Members/Commands/RemoveMember/RemoveMemberValidator.cs`

**RemoveMemberCommand.cs**:
```csharp
public record RemoveMemberCommand(Guid UserId) : ICommand;
```

**RemoveMemberValidator.cs**:
```csharp
public class RemoveMemberValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");
    }
}
```

**RemoveMemberCommandHandler.cs**:
```csharp
public class RemoveMemberCommandHandler : ICommandHandler<RemoveMemberCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveMemberCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar usuário
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Fail("Usuário não encontrado");
        }

        // 2. Validar regras de negócio
        var currentUserId = _currentUserService.UserId;

        // Regra: Owner não pode se auto-remover
        if (user.Id == currentUserId)
        {
            return Result.Fail("Você não pode remover a si mesmo");
        }

        // 3. Rebaixar para Customer (não deleta o user)
        user.UpdateRole(UserRole.Customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
```

**Endpoint**:
```csharp
// DELETE /api/members/{userId}
group.MapDelete("{userId}", async (Guid userId, ISender sender) =>
{
    var command = new RemoveMemberCommand(userId);
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsOwner);
```

**Validação**:
- Testes passam (GREEN)

---

### 6.9 - Refatorar DELETE /api/members/{userId} (REFACTOR)
**Descrição**: Melhorar código mantendo testes passando.

**Validação**:
- Testes continuam passando

---

### 6.10 - Criar Testes de Integração E2E
**Descrição**: Criar testes end-to-end para todos os endpoints de membros.

**Arquivos**:
- `tests/SportHub.Tests/Endpoints/MembersEndpointsTests.cs` (novo)

**Testes a Criar**:
- GET /api/members retorna lista correta
- PATCH /api/members/{id}/role altera role
- DELETE /api/members/{id} rebaixa para Customer
- Validar que apenas Owner acessa

**Validação**:
- Testes E2E passam

---

## Testes

### Resumo de Testes a Criar

**Unitários**:
- GetMembersQueryTests (3+ testes)
- UpdateMemberRoleCommandTests (5+ testes)
- RemoveMemberCommandTests (3+ testes)

**Integração**:
- MembersEndpointsTests (10+ testes)

**Total**: ~20 testes

---

## Critérios de Sucesso

- ✅ GET /api/members lista apenas membros operacionais (role >= Staff)
- ✅ PATCH /api/members/{userId}/role altera role corretamente
- ✅ DELETE /api/members/{userId} rebaixa para Customer
- ✅ Owner não pode se auto-rebaixar
- ✅ Owner não pode promover a Owner
- ✅ Apenas roles Customer/Staff/Manager são permitidos
- ✅ Apenas Owner acessa esses endpoints
- ✅ Todos os testes unitários passam
- ✅ Todos os testes de integração passam
- ✅ Código segue padrões CQRS, Result Pattern, FluentValidation

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Regras de negócio complexas | Média | Alto | TDD garante cobertura completa |
| Owner se auto-remove acidentalmente | Baixa | Alto | Validação explícita no handler |
| Performance ao listar membros | Baixa | Baixo | Query simples, sem joins complexos |

---

## Notas para o Desenvolvedor

- **TDD Rigoroso**: Siga Red-Green-Refactor estritamente
- **Regras de Negócio**: Valide TODAS as regras no handler
- **Mensagens de Erro**: Use mensagens claras e específicas
- **Soft Delete**: DELETE rebaixa para Customer, não deleta fisicamente
- **Repository Method**: Adicione `GetMembersAsync()` no repository

---

## Checklist de Conclusão

- [ ] Testes RED criados para GET /api/members
- [ ] GET /api/members implementado (GREEN)
- [ ] GET /api/members refatorado
- [ ] Testes RED criados para PATCH /api/members/{userId}/role
- [ ] PATCH implementado (GREEN)
- [ ] PATCH refatorado
- [ ] Testes RED criados para DELETE /api/members/{userId}
- [ ] DELETE implementado (GREEN)
- [ ] DELETE refatorado
- [ ] Testes de integração E2E criados
- [ ] Todos os testes passam
- [ ] Endpoints registrados em Program.cs
- [ ] Code review solicitado
