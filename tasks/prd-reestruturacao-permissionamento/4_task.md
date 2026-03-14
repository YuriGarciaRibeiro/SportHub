# Task 4.0: Aplicação de Policies nos Endpoints Existentes

> **Complexidade**: MEDIUM  
> **Dependências**: Task 3.0  
> **Estimativa**: 4-5 horas  
> **Status**: Pendente

---

## Objetivo

Aplicar a matriz de permissões definida no PRD em todos os endpoints existentes, garantindo que cada endpoint tenha a policy de autorização correta.

---

## Contexto

Atualmente, muitos endpoints usam `.RequireAuthorization()` genérico, permitindo que qualquer usuário autenticado (inclusive Customers) execute ações administrativas. Esta tarefa aplica as policies corretas conforme a matriz do PRD (RF-04).

---

## Subtarefas

### 4.1 - Aplicar Policies em Sports Endpoints
**Descrição**: Atualizar endpoints de esportes com as policies corretas.

**Arquivos**:
- `src/SportHub.Api/Endpoints/SportsEndpoints.cs` (ou similar)

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| GET | `/api/sports` | Anônimo |
| GET | `/api/sports/{id}` | Anônimo |
| POST | `/api/sports` | `IsManager` |
| PUT | `/api/sports/{id}` | `IsManager` |
| DELETE | `/api/sports/{id}` | `IsManager` |

**Implementação**:
```csharp
// GET /api/sports - Anônimo
app.MapGet("/api/sports", async (ISender sender) =>
{
    // Sem .RequireAuthorization()
    var result = await sender.Send(new GetAllSportsQuery());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
});

// GET /api/sports/{id} - Anônimo
app.MapGet("/api/sports/{id}", async (Guid id, ISender sender) =>
{
    // Sem .RequireAuthorization()
    var result = await sender.Send(new GetSportByIdQuery(id));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
});

// POST /api/sports - IsManager
app.MapPost("/api/sports", async (CreateSportCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Created($"/api/sports/{result.Value.Id}", result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);

// PUT /api/sports/{id} - IsManager
app.MapPut("/api/sports/{id}", async (Guid id, UpdateSportCommand command, ISender sender) =>
{
    if (id != command.Id) return Results.BadRequest("ID mismatch");
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);

// DELETE /api/sports/{id} - IsManager
app.MapDelete("/api/sports/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new DeleteSportCommand(id));
    return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);
```

**Validação**:
- Customer não consegue criar/editar/deletar esportes
- Manager/Owner conseguem criar/editar/deletar
- Qualquer um (inclusive anônimo) consegue listar

---

### 4.2 - Aplicar Policies em Courts Endpoints
**Descrição**: Atualizar endpoints de quadras com as policies corretas.

**Arquivos**:
- `src/SportHub.Api/Endpoints/CourtsEndpoints.cs` (ou similar)

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| GET | `/api/courts` | Anônimo |
| GET | `/api/courts/{id}` | RequireAuth |
| POST | `/api/courts` | `IsManager` |
| PUT | `/api/courts/{id}` | `IsManager` |
| DELETE | `/api/courts/{id}` | `IsManager` |
| GET | `/api/courts/{id}/availability/{date}` | Anônimo |
| POST | `/api/courts/{id}/reservations` | RequireAuth |
| GET | `/api/courts/{id}/reservations` | `IsStaff` |

**Implementação**:
```csharp
// GET /api/courts - Anônimo
app.MapGet("/api/courts", async (ISender sender) =>
{
    var result = await sender.Send(new GetAllCourtsQuery());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
});

// GET /api/courts/{id} - RequireAuth
app.MapGet("/api/courts/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetCourtByIdQuery(id));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
})
.RequireAuthorization();

// POST /api/courts - IsManager
app.MapPost("/api/courts", async (CreateCourtCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Created($"/api/courts/{result.Value.Id}", result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);

// PUT /api/courts/{id} - IsManager
app.MapPut("/api/courts/{id}", async (Guid id, UpdateCourtCommand command, ISender sender) =>
{
    if (id != command.Id) return Results.BadRequest("ID mismatch");
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);

// DELETE /api/courts/{id} - IsManager
app.MapDelete("/api/courts/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new DeleteCourtCommand(id));
    return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);

// GET /api/courts/{id}/availability/{date} - Anônimo
app.MapGet("/api/courts/{id}/availability/{date}", async (Guid id, DateOnly date, ISender sender) =>
{
    var result = await sender.Send(new GetCourtAvailabilityQuery(id, date));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
});

// POST /api/courts/{id}/reservations - RequireAuth
app.MapPost("/api/courts/{id}/reservations", async (Guid id, CreateReservationCommand command, ISender sender) =>
{
    if (id != command.CourtId) return Results.BadRequest("Court ID mismatch");
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Created($"/api/reservations/{result.Value.Id}", result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization();

// GET /api/courts/{id}/reservations - IsStaff
app.MapGet("/api/courts/{id}/reservations", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetCourtReservationsQuery(id));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsStaff);
```

**Validação**:
- Customer consegue criar reserva mas não listar reservas de uma quadra
- Staff consegue listar reservas
- Manager/Owner conseguem tudo
- Availability é pública

---

### 4.3 - Aplicar Policies em Reservations Endpoints
**Descrição**: Atualizar endpoints de reservas com as policies corretas.

**Arquivos**:
- `src/SportHub.Api/Endpoints/ReservationsEndpoints.cs` (ou similar)

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| GET | `/api/reservations/me` | RequireAuth |
| DELETE | `/api/courts/{id}/reservations/{rid}` | RequireAuth (lógica interna) |

**Implementação**:
```csharp
// GET /api/reservations/me - RequireAuth
app.MapGet("/api/reservations/me", async (ISender sender) =>
{
    var result = await sender.Send(new GetMyReservationsQuery());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization();

// DELETE /api/courts/{courtId}/reservations/{reservationId} - RequireAuth
// Nota: A lógica de permissão (própria reserva OU Manager+) será implementada na Task 5.0
app.MapDelete("/api/courts/{courtId}/reservations/{reservationId}", 
    async (Guid courtId, Guid reservationId, ISender sender) =>
{
    var result = await sender.Send(new CancelReservationCommand(reservationId));
    return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
})
.RequireAuthorization();
```

**Validação**:
- Qualquer usuário autenticado consegue ver suas próprias reservas
- Cancelamento tem lógica especial (Task 5.0)

---

### 4.4 - Aplicar Policies em Settings/Branding Endpoints
**Descrição**: Atualizar endpoints de configurações e branding.

**Arquivos**:
- `src/SportHub.Api/Endpoints/SettingsEndpoints.cs`
- `src/SportHub.Api/Endpoints/BrandingEndpoints.cs`

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| PUT | `/api/settings` | `IsOwner` |
| GET | `/api/branding` | Anônimo |

**Implementação**:
```csharp
// PUT /api/settings - IsOwner
app.MapPut("/api/settings", async (UpdateSettingsCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsOwner);

// GET /api/branding - Anônimo
app.MapGet("/api/branding", async (ISender sender) =>
{
    var result = await sender.Send(new GetBrandingQuery());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
});
```

**Validação**:
- Apenas Owner consegue alterar settings
- Branding é público

---

### 4.5 - Aplicar Policies em Stats Endpoints
**Descrição**: Atualizar endpoint de estatísticas.

**Arquivos**:
- `src/SportHub.Api/Endpoints/StatsEndpoints.cs` (ou similar)

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| GET | `/admin/stats` | `IsManager` |

**Implementação**:
```csharp
// GET /admin/stats - IsManager
app.MapGet("/admin/stats", async (ISender sender) =>
{
    var result = await sender.Send(new GetStatsQuery());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
})
.RequireAuthorization(PolicyNames.IsManager);
```

**Validação**:
- Apenas Manager/Owner conseguem ver stats
- Customer e Staff não conseguem

---

### 4.6 - Verificar Auth Endpoints
**Descrição**: Confirmar que endpoints de autenticação permanecem anônimos ou RequireAuth.

**Arquivos**:
- `src/SportHub.Api/Endpoints/AuthEndpoints.cs`

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| POST | `/auth/register` | Anônimo |
| POST | `/auth/login` | Anônimo |
| POST | `/auth/refresh` | Anônimo |
| GET | `/auth/me` | RequireAuth |
| PUT | `/auth/me` | RequireAuth |
| DELETE | `/auth/me` | RequireAuth |

**Validação**:
- Register, login, refresh são públicos
- Me (get/put/delete) requerem autenticação

---

### 4.7 - Verificar Tenant Endpoints
**Descrição**: Confirmar que endpoints de tenants são `IsSuperAdmin`.

**Arquivos**:
- `src/SportHub.Api/Endpoints/TenantEndpoints.cs`

**Matriz de Permissões**:
| Método | Rota | Policy |
|---|---|---|
| * | `/api/tenants/**` | `IsSuperAdmin` |

**Implementação**:
```csharp
// Todos os endpoints de tenant devem ter:
.RequireAuthorization(PolicyNames.IsSuperAdmin);
```

**Validação**:
- Apenas SuperAdmin acessa endpoints de tenant

---

### 4.8 - Criar Testes de Autorização por Endpoint
**Descrição**: Criar testes de integração para validar autorização em cada endpoint.

**Arquivos**:
- `tests/SportHub.Tests/Endpoints/AuthorizationTests.cs` (novo)

**Validação**:
- Testes compilam
- Testes passam

---

## Testes

### Testes de Integração

**Teste 1: Customer não pode criar esporte**
```csharp
[Fact]
public async Task CreateSport_AsCustomer_ShouldReturn403()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Customer);
    var command = new CreateSportCommand { Name = "Futebol" };

    // Act
    var response = await client.PostAsJsonAsync("/api/sports", command);

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

**Teste 2: Manager pode criar esporte**
```csharp
[Fact]
public async Task CreateSport_AsManager_ShouldReturn201()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Manager);
    var command = new CreateSportCommand { Name = "Futebol" };

    // Act
    var response = await client.PostAsJsonAsync("/api/sports", command);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

**Teste 3: Staff pode listar reservas de quadra**
```csharp
[Fact]
public async Task GetCourtReservations_AsStaff_ShouldReturn200()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Staff);
    var courtId = Guid.NewGuid();

    // Act
    var response = await client.GetAsync($"/api/courts/{courtId}/reservations");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

**Teste 4: Customer não pode listar reservas de quadra**
```csharp
[Fact]
public async Task GetCourtReservations_AsCustomer_ShouldReturn403()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Customer);
    var courtId = Guid.NewGuid();

    // Act
    var response = await client.GetAsync($"/api/courts/{courtId}/reservations");

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

**Teste 5: Apenas Owner pode alterar settings**
```csharp
[Theory]
[InlineData(UserRole.Owner, HttpStatusCode.OK)]
[InlineData(UserRole.Manager, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
public async Task UpdateSettings_ShouldAuthorizeCorrectly(UserRole role, HttpStatusCode expectedStatus)
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(role);
    var command = new UpdateSettingsCommand { Name = "New Name" };

    // Act
    var response = await client.PutAsJsonAsync("/api/settings", command);

    // Assert
    Assert.Equal(expectedStatus, response.StatusCode);
}
```

---

## Critérios de Sucesso

- ✅ Todos os endpoints de Sports têm policies corretas
- ✅ Todos os endpoints de Courts têm policies corretas
- ✅ Todos os endpoints de Reservations têm policies corretas
- ✅ Settings endpoint requer `IsOwner`
- ✅ Stats endpoint requer `IsManager`
- ✅ Tenant endpoints requerem `IsSuperAdmin`
- ✅ Endpoints públicos permanecem sem `.RequireAuthorization()`
- ✅ Testes de autorização por endpoint passam
- ✅ Customer não acessa funcionalidades administrativas

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Esquecer de aplicar policy em algum endpoint | Média | Alto | Revisar matriz do PRD endpoint por endpoint |
| Quebrar funcionalidade existente | Média | Alto | Testes de integração abrangentes |
| Policy muito restritiva | Baixa | Médio | Validar com cenários de uso reais |

---

## Notas para o Desenvolvedor

- **Matriz de Referência**: Use a tabela do PRD (RF-04) como checklist
- **Anônimo vs RequireAuth**: Anônimo = sem `.RequireAuthorization()`, RequireAuth = `.RequireAuthorization()` sem policy específica
- **Testes Primeiro**: Crie testes de autorização ANTES de aplicar as policies
- **WebApplicationFactory**: Use factory de testes para criar clients autenticados com diferentes roles

---

## Checklist de Conclusão

- [ ] Sports endpoints atualizados
- [ ] Courts endpoints atualizados
- [ ] Reservations endpoints atualizados
- [ ] Settings/Branding endpoints atualizados
- [ ] Stats endpoint atualizado
- [ ] Auth endpoints verificados
- [ ] Tenant endpoints verificados
- [ ] Testes de autorização criados
- [ ] Todos os testes passam
- [ ] Matriz do PRD completamente implementada
- [ ] Code review solicitado
