# Task 5.0: Implementação de Lógica de Cancelamento com Permissões

> **Complexidade**: LOW  
> **Dependências**: Task 3.0, 4.0  
> **Estimativa**: 2-3 horas  
> **Status**: Pendente

---

## Objetivo

Atualizar o `CancelReservationHandler` para implementar lógica de permissão: Customer pode cancelar apenas sua própria reserva, enquanto Manager/Owner podem cancelar qualquer reserva.

---

## Contexto

Atualmente, o endpoint de cancelamento usa `.RequireAuthorization()` genérico, mas não valida se o usuário tem permissão para cancelar aquela reserva específica. Esta tarefa implementa a regra de negócio: próprio usuário OU role >= Manager.

---

## Subtarefas

### 5.1 - Atualizar CancelReservationHandler
**Descrição**: Adicionar lógica de verificação de permissão no handler.

**Arquivos**:
- `src/SportHub.Application/CQRS/Reservations/Commands/CancelReservation/CancelReservationHandler.cs`

**Implementação**:
```csharp
public class CancelReservationHandler : ICommandHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelReservationHandler(
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar reserva
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
        if (reservation == null)
        {
            return Result.Fail("Reserva não encontrada");
        }

        // 2. Verificar permissão
        var currentUserId = _currentUserService.UserId;
        var currentUserRole = _currentUserService.UserRole;

        var isOwner = reservation.UserId == currentUserId;
        var isManagerOrAbove = currentUserRole >= UserRole.Manager;

        if (!isOwner && !isManagerOrAbove)
        {
            return Result.Fail("Você não tem permissão para cancelar esta reserva");
        }

        // 3. Cancelar reserva
        reservation.Cancel(); // Método de domínio que marca como cancelada
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
```

**Validação**:
- Handler compila
- Lógica de permissão implementada
- Retorna erro se não autorizado

---

### 5.2 - Atualizar Entidade Reservation (se necessário)
**Descrição**: Garantir que a entidade `Reservation` tem método `Cancel()` ou propriedade `Status`.

**Arquivos**:
- `src/SportHub.Domain/entities/Reservation.cs`

**Verificação**:
```csharp
public class Reservation : AuditEntity
{
    // ... propriedades existentes
    public ReservationStatus Status { get; private set; }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Reserva já está cancelada");
        }

        Status = ReservationStatus.Cancelled;
    }
}
```

**Ação**:
- Se método `Cancel()` já existe → nenhuma mudança
- Se não existe → adicionar método de domínio

**Validação**:
- Entidade tem método `Cancel()` ou lógica equivalente

---

### 5.3 - Atualizar CancelReservationCommand (se necessário)
**Descrição**: Verificar se o command precisa de ajustes.

**Arquivos**:
- `src/SportHub.Application/CQRS/Reservations/Commands/CancelReservation/CancelReservationCommand.cs`

**Verificação**:
```csharp
public record CancelReservationCommand(Guid ReservationId) : ICommand;
```

**Ação**:
- Se command está correto → nenhuma mudança
- Se precisa de ajustes → atualizar

**Validação**:
- Command compila e está correto

---

### 5.4 - Atualizar Validator (se necessário)
**Descrição**: Verificar se existe validator para o command.

**Arquivos**:
- `src/SportHub.Application/CQRS/Reservations/Commands/CancelReservation/CancelReservationValidator.cs`

**Implementação** (se não existir):
```csharp
public class CancelReservationValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationValidator()
    {
        RuleFor(x => x.ReservationId)
            .NotEmpty()
            .WithMessage("ID da reserva é obrigatório");
    }
}
```

**Validação**:
- Validator existe e valida corretamente

---

### 5.5 - Criar Testes Unitários
**Descrição**: Criar testes para validar a lógica de permissão.

**Arquivos**:
- `tests/SportHub.Tests/Reservations/CancelReservationHandlerTests.cs`

**Validação**:
- Testes compilam
- Testes passam

---

## Testes

### Testes Unitários

**Teste 1: Customer cancela própria reserva - sucesso**
```csharp
[Fact]
public async Task CancelReservation_AsOwner_ShouldSucceed()
{
    // Arrange
    var userId = Guid.NewGuid();
    var reservationId = Guid.NewGuid();
    var reservation = new Reservation
    {
        Id = reservationId,
        UserId = userId,
        Status = ReservationStatus.Confirmed
    };

    _reservationRepository.Setup(x => x.GetByIdAsync(reservationId))
        .ReturnsAsync(reservation);
    
    _currentUserService.Setup(x => x.UserId).Returns(userId);
    _currentUserService.Setup(x => x.UserRole).Returns(UserRole.Customer);

    var command = new CancelReservationCommand(reservationId);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

**Teste 2: Customer tenta cancelar reserva de outro - falha**
```csharp
[Fact]
public async Task CancelReservation_AsCustomerForOtherUser_ShouldFail()
{
    // Arrange
    var currentUserId = Guid.NewGuid();
    var otherUserId = Guid.NewGuid();
    var reservationId = Guid.NewGuid();
    var reservation = new Reservation
    {
        Id = reservationId,
        UserId = otherUserId, // Reserva de outro usuário
        Status = ReservationStatus.Confirmed
    };

    _reservationRepository.Setup(x => x.GetByIdAsync(reservationId))
        .ReturnsAsync(reservation);
    
    _currentUserService.Setup(x => x.UserId).Returns(currentUserId);
    _currentUserService.Setup(x => x.UserRole).Returns(UserRole.Customer);

    var command = new CancelReservationCommand(reservationId);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("não tem permissão", result.Errors.First().Message);
    Assert.Equal(ReservationStatus.Confirmed, reservation.Status); // Não foi cancelada
    _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

**Teste 3: Manager cancela reserva de outro - sucesso**
```csharp
[Fact]
public async Task CancelReservation_AsManager_ShouldSucceed()
{
    // Arrange
    var managerId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var reservationId = Guid.NewGuid();
    var reservation = new Reservation
    {
        Id = reservationId,
        UserId = customerId, // Reserva de outro usuário
        Status = ReservationStatus.Confirmed
    };

    _reservationRepository.Setup(x => x.GetByIdAsync(reservationId))
        .ReturnsAsync(reservation);
    
    _currentUserService.Setup(x => x.UserId).Returns(managerId);
    _currentUserService.Setup(x => x.UserRole).Returns(UserRole.Manager);

    var command = new CancelReservationCommand(reservationId);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

**Teste 4: Staff tenta cancelar reserva de outro - falha**
```csharp
[Fact]
public async Task CancelReservation_AsStaffForOtherUser_ShouldFail()
{
    // Arrange
    var staffId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var reservationId = Guid.NewGuid();
    var reservation = new Reservation
    {
        Id = reservationId,
        UserId = customerId,
        Status = ReservationStatus.Confirmed
    };

    _reservationRepository.Setup(x => x.GetByIdAsync(reservationId))
        .ReturnsAsync(reservation);
    
    _currentUserService.Setup(x => x.UserId).Returns(staffId);
    _currentUserService.Setup(x => x.UserRole).Returns(UserRole.Staff);

    var command = new CancelReservationCommand(reservationId);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("não tem permissão", result.Errors.First().Message);
}
```

**Teste 5: Reserva não encontrada - falha**
```csharp
[Fact]
public async Task CancelReservation_ReservationNotFound_ShouldFail()
{
    // Arrange
    var reservationId = Guid.NewGuid();
    
    _reservationRepository.Setup(x => x.GetByIdAsync(reservationId))
        .ReturnsAsync((Reservation)null);

    var command = new CancelReservationCommand(reservationId);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("não encontrada", result.Errors.First().Message);
}
```

### Testes de Integração

**Teste 6: E2E - Customer cancela própria reserva**
```csharp
[Fact]
public async Task CancelReservation_E2E_AsOwner_ShouldSucceed()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Customer, out var userId);
    
    // Criar uma reserva para o usuário
    var reservation = await CreateReservationForUser(userId);

    // Act
    var response = await client.DeleteAsync($"/api/courts/{reservation.CourtId}/reservations/{reservation.Id}");

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    
    // Verificar que foi cancelada
    var cancelledReservation = await GetReservationById(reservation.Id);
    Assert.Equal(ReservationStatus.Cancelled, cancelledReservation.Status);
}
```

**Teste 7: E2E - Customer tenta cancelar reserva de outro**
```csharp
[Fact]
public async Task CancelReservation_E2E_AsCustomerForOther_ShouldReturn403()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Customer, out var userId);
    
    // Criar uma reserva para outro usuário
    var otherUserId = Guid.NewGuid();
    var reservation = await CreateReservationForUser(otherUserId);

    // Act
    var response = await client.DeleteAsync($"/api/courts/{reservation.CourtId}/reservations/{reservation.Id}");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Ou Forbidden, dependendo da implementação
}
```

---

## Critérios de Sucesso

- ✅ `CancelReservationHandler` verifica permissão antes de cancelar
- ✅ Customer pode cancelar apenas própria reserva
- ✅ Manager/Owner podem cancelar qualquer reserva
- ✅ Staff não pode cancelar reservas de outros
- ✅ Retorna erro apropriado quando não autorizado
- ✅ Todos os testes unitários passam
- ✅ Testes de integração E2E passam

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Lógica de permissão incorreta | Baixa | Alto | Testes abrangentes com todos os cenários |
| Método Cancel() não existe na entidade | Média | Médio | Verificar entidade antes de implementar handler |
| Performance ao buscar reserva | Baixa | Baixo | Query já existe, sem impacto adicional |

---

## Notas para o Desenvolvedor

- **Comparação Hierárquica**: Use `currentUserRole >= UserRole.Manager` para verificar Manager/Owner
- **Mensagens de Erro**: Use mensagens claras para o usuário entender por que não pode cancelar
- **Status Code**: Considere retornar 403 Forbidden em vez de 400 BadRequest para erros de autorização
- **Domínio**: Prefira método de domínio `Cancel()` em vez de setar `Status` diretamente

---

## Checklist de Conclusão

- [ ] `CancelReservationHandler` atualizado com lógica de permissão
- [ ] Entidade `Reservation` tem método `Cancel()`
- [ ] Command e Validator verificados/atualizados
- [ ] Testes unitários criados
- [ ] Testes de integração E2E criados
- [ ] Todos os testes passam
- [ ] Code review solicitado
