# Task 8.0: Testes de Integração do Sistema de Permissionamento

> **Complexidade**: HIGH  
> **Dependências**: Task 1.0-7.0  
> **Estimativa**: 6-8 horas  
> **Status**: Pendente  
> **Abordagem**: TDD (Red-Green-Refactor)

---

## Objetivo

Criar suite completa de testes de integração end-to-end para validar que todo o sistema de permissionamento funciona corretamente, cobrindo todos os roles, endpoints e cenários de negócio.

---

## Contexto

Esta é a tarefa final que valida a implementação completa. Os testes devem cobrir:
- Autorização por role em todos os endpoints
- Fluxos completos por persona (Customer, Staff, Manager, Owner, SuperAdmin)
- Edge cases e regras de negócio
- Gestão de membros
- Cancelamento de reservas com permissões

---

## Subtarefas

### 8.1 - Configurar Infraestrutura de Testes
**Descrição**: Preparar WebApplicationFactory e helpers para testes de integração.

**Arquivos**:
- `tests/SportHub.Tests/Infrastructure/TestWebApplicationFactory.cs`
- `tests/SportHub.Tests/Infrastructure/TestAuthenticationHandler.cs`
- `tests/SportHub.Tests/Infrastructure/HttpClientExtensions.cs`

**TestWebApplicationFactory.cs**:
```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover DbContext real
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Adicionar DbContext de teste (in-memory)
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Adicionar authentication de teste
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        });

        builder.UseEnvironment("Testing");
    }

    public HttpClient CreateAuthenticatedClient(UserRole role, out Guid userId)
    {
        userId = Guid.NewGuid();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role.ToString());
        return client;
    }
}
```

**TestAuthenticationHandler.cs**:
```csharp
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("X-Test-UserId"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers["X-Test-UserId"].ToString();
        var role = Request.Headers["X-Test-Role"].ToString();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("Role", role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

**Validação**:
- Factory cria clients autenticados com diferentes roles
- Authentication handler funciona

---

### 8.2 - Testes de Autorização: Sports Endpoints
**Descrição**: Testar matriz de permissões para endpoints de Sports.

**Arquivos**:
- `tests/SportHub.Tests/Integration/SportsAuthorizationTests.cs`

**Testes**:
```csharp
[Theory]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Manager, HttpStatusCode.Created)]
[InlineData(UserRole.Owner, HttpStatusCode.Created)]
public async Task CreateSport_AuthorizationMatrix(UserRole role, HttpStatusCode expectedStatus)
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(role, out _);
    var command = new CreateSportCommand { Name = "Futebol" };

    // Act
    var response = await client.PostAsJsonAsync("/api/sports", command);

    // Assert
    Assert.Equal(expectedStatus, response.StatusCode);
}

[Fact]
public async Task GetSports_Anonymous_ShouldSucceed()
{
    // Arrange
    var client = _factory.CreateClient(); // Sem autenticação

    // Act
    var response = await client.GetAsync("/api/sports");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

// Repetir para PUT e DELETE
```

**Validação**:
- Todos os cenários de autorização cobertos

---

### 8.3 - Testes de Autorização: Courts Endpoints
**Descrição**: Testar matriz de permissões para endpoints de Courts.

**Arquivos**:
- `tests/SportHub.Tests/Integration/CourtsAuthorizationTests.cs`

**Testes**:
```csharp
[Theory]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Manager, HttpStatusCode.Created)]
[InlineData(UserRole.Owner, HttpStatusCode.Created)]
public async Task CreateCourt_AuthorizationMatrix(UserRole role, HttpStatusCode expectedStatus)

[Theory]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.OK)]
[InlineData(UserRole.Manager, HttpStatusCode.OK)]
[InlineData(UserRole.Owner, HttpStatusCode.OK)]
public async Task GetCourtReservations_AuthorizationMatrix(UserRole role, HttpStatusCode expectedStatus)

[Fact]
public async Task GetCourtAvailability_Anonymous_ShouldSucceed()
```

**Validação**:
- Matriz completa testada

---

### 8.4 - Testes de Autorização: Reservations Endpoints
**Descrição**: Testar permissões de reservas, incluindo cancelamento.

**Arquivos**:
- `tests/SportHub.Tests/Integration/ReservationsAuthorizationTests.cs`

**Testes**:
```csharp
[Fact]
public async Task CreateReservation_AsCustomer_ShouldSucceed()

[Fact]
public async Task GetMyReservations_RequiresAuth()

[Fact]
public async Task CancelOwnReservation_AsCustomer_ShouldSucceed()

[Fact]
public async Task CancelOtherReservation_AsCustomer_ShouldFail()

[Fact]
public async Task CancelOtherReservation_AsManager_ShouldSucceed()

[Fact]
public async Task CancelOtherReservation_AsStaff_ShouldFail()
```

**Validação**:
- Lógica de cancelamento testada para todos os roles

---

### 8.5 - Testes de Autorização: Settings/Stats Endpoints
**Descrição**: Testar endpoints administrativos.

**Arquivos**:
- `tests/SportHub.Tests/Integration/AdminAuthorizationTests.cs`

**Testes**:
```csharp
[Theory]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Manager, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Owner, HttpStatusCode.OK)]
public async Task UpdateSettings_AuthorizationMatrix(UserRole role, HttpStatusCode expectedStatus)

[Theory]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Manager, HttpStatusCode.OK)]
[InlineData(UserRole.Owner, HttpStatusCode.OK)]
public async Task GetStats_AuthorizationMatrix(UserRole role, HttpStatusCode expectedStatus)

[Fact]
public async Task GetBranding_Anonymous_ShouldSucceed()
```

**Validação**:
- Apenas Owner acessa settings
- Apenas Manager+ acessa stats

---

### 8.6 - Testes de Gestão de Membros
**Descrição**: Testar todos os cenários de gestão de membros.

**Arquivos**:
- `tests/SportHub.Tests/Integration/MembersManagementTests.cs`

**Testes**:
```csharp
[Fact]
public async Task GetMembers_AsOwner_ShouldReturnOnlyOperationalMembers()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Owner, out _);
    
    // Criar users com diferentes roles
    await SeedUsers();

    // Act
    var response = await client.GetAsync("/api/members");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var members = await response.Content.ReadFromJsonAsync<List<MemberDto>>();
    Assert.All(members, m => Assert.True(
        m.Role == "Staff" || m.Role == "Manager" || m.Role == "Owner"));
    Assert.DoesNotContain(members, m => m.Role == "Customer");
}

[Fact]
public async Task UpdateMemberRole_PromoteCustomerToStaff_ShouldSucceed()

[Fact]
public async Task UpdateMemberRole_PromoteToOwner_ShouldFail()

[Fact]
public async Task UpdateMemberRole_SelfDemotion_ShouldFail()

[Fact]
public async Task RemoveMember_ShouldDemoteToCustomer()

[Fact]
public async Task RemoveMember_SelfRemoval_ShouldFail()

[Theory]
[InlineData(UserRole.Customer, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Staff, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Manager, HttpStatusCode.Forbidden)]
[InlineData(UserRole.Owner, HttpStatusCode.OK)]
public async Task GetMembers_AuthorizationMatrix(UserRole role, HttpStatusCode expectedStatus)
```

**Validação**:
- Todas as regras de negócio de membros testadas

---

### 8.7 - Testes de Fluxos por Persona: Customer
**Descrição**: Testar fluxo completo de um Customer.

**Arquivos**:
- `tests/SportHub.Tests/Integration/Personas/CustomerFlowTests.cs`

**Testes**:
```csharp
[Fact]
public async Task CustomerFlow_CompleteJourney()
{
    // 1. Register
    var registerResponse = await _client.PostAsJsonAsync("/auth/register", new RegisterUserCommand
    {
        Email = "customer@test.com",
        Password = "Password123!",
        Name = "Customer Test"
    });
    Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

    // 2. Login
    var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginUserCommand
    {
        Email = "customer@test.com",
        Password = "Password123!"
    });
    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
    Assert.Equal("Customer", authResponse.Role);

    // 3. Ver esportes (público)
    var sportsResponse = await _client.GetAsync("/api/sports");
    Assert.Equal(HttpStatusCode.OK, sportsResponse.StatusCode);

    // 4. Ver quadras (público)
    var courtsResponse = await _client.GetAsync("/api/courts");
    Assert.Equal(HttpStatusCode.OK, courtsResponse.StatusCode);

    // 5. Criar reserva (autenticado)
    var authenticatedClient = _factory.CreateAuthenticatedClient(UserRole.Customer, out var userId);
    var createReservationResponse = await authenticatedClient.PostAsJsonAsync(
        $"/api/courts/{courtId}/reservations", 
        new CreateReservationCommand { /* ... */ });
    Assert.Equal(HttpStatusCode.Created, createReservationResponse.StatusCode);

    // 6. Ver minhas reservas
    var myReservationsResponse = await authenticatedClient.GetAsync("/api/reservations/me");
    Assert.Equal(HttpStatusCode.OK, myReservationsResponse.StatusCode);

    // 7. Cancelar minha reserva
    var cancelResponse = await authenticatedClient.DeleteAsync(
        $"/api/courts/{courtId}/reservations/{reservationId}");
    Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

    // 8. Tentar criar esporte (deve falhar)
    var createSportResponse = await authenticatedClient.PostAsJsonAsync(
        "/api/sports", 
        new CreateSportCommand { Name = "Futebol" });
    Assert.Equal(HttpStatusCode.Forbidden, createSportResponse.StatusCode);

    // 9. Tentar ver stats (deve falhar)
    var statsResponse = await authenticatedClient.GetAsync("/admin/stats");
    Assert.Equal(HttpStatusCode.Forbidden, statsResponse.StatusCode);
}
```

**Validação**:
- Customer consegue fazer tudo que deve
- Customer não consegue acessar admin

---

### 8.8 - Testes de Fluxos por Persona: Staff
**Descrição**: Testar fluxo completo de um Staff.

**Arquivos**:
- `tests/SportHub.Tests/Integration/Personas/StaffFlowTests.cs`

**Testes**:
```csharp
[Fact]
public async Task StaffFlow_CanViewReservationsButNotManage()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Staff, out _);

    // 1. Ver reservas de quadra (permitido)
    var reservationsResponse = await client.GetAsync($"/api/courts/{courtId}/reservations");
    Assert.Equal(HttpStatusCode.OK, reservationsResponse.StatusCode);

    // 2. Tentar criar quadra (negado)
    var createCourtResponse = await client.PostAsJsonAsync("/api/courts", new CreateCourtCommand { /* ... */ });
    Assert.Equal(HttpStatusCode.Forbidden, createCourtResponse.StatusCode);

    // 3. Tentar cancelar reserva de outro (negado)
    var cancelResponse = await client.DeleteAsync($"/api/courts/{courtId}/reservations/{reservationId}");
    Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode); // Ou Forbidden

    // 4. Tentar ver stats (negado)
    var statsResponse = await client.GetAsync("/admin/stats");
    Assert.Equal(HttpStatusCode.Forbidden, statsResponse.StatusCode);
}
```

**Validação**:
- Staff tem acesso limitado correto

---

### 8.9 - Testes de Fluxos por Persona: Manager
**Descrição**: Testar fluxo completo de um Manager.

**Arquivos**:
- `tests/SportHub.Tests/Integration/Personas/ManagerFlowTests.cs`

**Testes**:
```csharp
[Fact]
public async Task ManagerFlow_CanManageOperations()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Manager, out _);

    // 1. Criar esporte
    var createSportResponse = await client.PostAsJsonAsync("/api/sports", new CreateSportCommand { Name = "Futebol" });
    Assert.Equal(HttpStatusCode.Created, createSportResponse.StatusCode);

    // 2. Criar quadra
    var createCourtResponse = await client.PostAsJsonAsync("/api/courts", new CreateCourtCommand { /* ... */ });
    Assert.Equal(HttpStatusCode.Created, createCourtResponse.StatusCode);

    // 3. Ver stats
    var statsResponse = await client.GetAsync("/admin/stats");
    Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);

    // 4. Cancelar reserva de outro usuário
    var cancelResponse = await client.DeleteAsync($"/api/courts/{courtId}/reservations/{reservationId}");
    Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

    // 5. Tentar alterar settings (negado - só Owner)
    var updateSettingsResponse = await client.PutAsJsonAsync("/api/settings", new UpdateSettingsCommand { /* ... */ });
    Assert.Equal(HttpStatusCode.Forbidden, updateSettingsResponse.StatusCode);

    // 6. Tentar gerenciar membros (negado - só Owner)
    var getMembersResponse = await client.GetAsync("/api/members");
    Assert.Equal(HttpStatusCode.Forbidden, getMembersResponse.StatusCode);
}
```

**Validação**:
- Manager gerencia operações mas não settings/membros

---

### 8.10 - Testes de Fluxos por Persona: Owner
**Descrição**: Testar fluxo completo de um Owner.

**Arquivos**:
- `tests/SportHub.Tests/Integration/Personas/OwnerFlowTests.cs`

**Testes**:
```csharp
[Fact]
public async Task OwnerFlow_HasFullControl()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(UserRole.Owner, out var ownerId);

    // 1. Fazer tudo que Manager faz
    var createSportResponse = await client.PostAsJsonAsync("/api/sports", new CreateSportCommand { Name = "Futebol" });
    Assert.Equal(HttpStatusCode.Created, createSportResponse.StatusCode);

    var statsResponse = await client.GetAsync("/admin/stats");
    Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);

    // 2. Alterar settings
    var updateSettingsResponse = await client.PutAsJsonAsync("/api/settings", new UpdateSettingsCommand { /* ... */ });
    Assert.Equal(HttpStatusCode.OK, updateSettingsResponse.StatusCode);

    // 3. Gerenciar membros
    var getMembersResponse = await client.GetAsync("/api/members");
    Assert.Equal(HttpStatusCode.OK, getMembersResponse.StatusCode);

    // 4. Promover Customer a Staff
    var promoteResponse = await client.PatchAsJsonAsync(
        $"/api/members/{customerId}/role", 
        new UpdateMemberRoleRequest("Staff"));
    Assert.Equal(HttpStatusCode.OK, promoteResponse.StatusCode);

    // 5. Rebaixar Staff a Customer
    var demoteResponse = await client.DeleteAsync($"/api/members/{staffId}");
    Assert.Equal(HttpStatusCode.NoContent, demoteResponse.StatusCode);
}
```

**Validação**:
- Owner tem controle total

---

### 8.11 - Testes de Edge Cases
**Descrição**: Testar cenários extremos e regras de negócio específicas.

**Arquivos**:
- `tests/SportHub.Tests/Integration/EdgeCasesTests.cs`

**Testes**:
```csharp
[Fact]
public async Task OwnerCannotDemoteThemselves()

[Fact]
public async Task OwnerCannotPromoteToOwner()

[Fact]
public async Task CannotCancelAlreadyCancelledReservation()

[Fact]
public async Task InvalidRoleInUpdateMemberRole_ShouldFail()

[Fact]
public async Task UpdateNonExistentMember_ShouldFail()

[Fact]
public async Task MultipleOwnersInSameTenant_ShouldNotExist()
```

**Validação**:
- Todos os edge cases cobertos

---

### 8.12 - Criar Relatório de Cobertura
**Descrição**: Gerar relatório de cobertura de testes.

**Comando**:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Meta**:
- Cobertura > 80% no código de autorização
- Cobertura > 90% nos handlers de membros

**Validação**:
- Relatório gerado
- Metas de cobertura atingidas

---

## Resumo de Testes

### Por Categoria
- **Autorização por Endpoint**: ~40 testes
- **Fluxos por Persona**: ~15 testes
- **Gestão de Membros**: ~15 testes
- **Edge Cases**: ~10 testes

**Total Estimado**: ~80 testes de integração

---

## Critérios de Sucesso

- ✅ Todos os endpoints testados com todos os roles
- ✅ Matriz de permissões 100% coberta
- ✅ Fluxos completos por persona validados
- ✅ Gestão de membros completamente testada
- ✅ Edge cases e regras de negócio validados
- ✅ Cobertura de testes > 80%
- ✅ Todos os testes passam
- ✅ Nenhum falso positivo

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Testes flaky (intermitentes) | Média | Médio | Usar in-memory database, evitar dependências externas |
| Cobertura insuficiente | Baixa | Alto | Revisar matriz de permissões sistematicamente |
| Testes muito lentos | Média | Baixo | Paralelizar quando possível |

---

## Notas para o Desenvolvedor

- **WebApplicationFactory**: Use factory para testes de integração reais
- **In-Memory Database**: Evita dependência de banco real
- **Test Authentication**: Handler customizado para simular diferentes roles
- **Seed Data**: Crie helpers para popular dados de teste
- **Assertions Claras**: Use mensagens descritivas nos asserts
- **Organização**: Agrupe testes por feature/endpoint

---

## Checklist de Conclusão

- [ ] Infraestrutura de testes configurada
- [ ] Testes de Sports endpoints criados
- [ ] Testes de Courts endpoints criados
- [ ] Testes de Reservations endpoints criados
- [ ] Testes de Settings/Stats endpoints criados
- [ ] Testes de gestão de membros criados
- [ ] Testes de fluxo Customer criados
- [ ] Testes de fluxo Staff criados
- [ ] Testes de fluxo Manager criados
- [ ] Testes de fluxo Owner criados
- [ ] Testes de edge cases criados
- [ ] Relatório de cobertura gerado
- [ ] Todos os testes passam
- [ ] Cobertura > 80%
- [ ] Code review solicitado
