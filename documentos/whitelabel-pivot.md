# SportHub — Pivot para Plataforma SaaS Whitelabel

> **Versão:** 1.1 | **Data:** 2026-03-03 | **Escopo:** Backend (.NET 9)

---

## 1. Contexto e Motivação

O SportHub foi construído originalmente como um **marketplace centralizado de quadras esportivas** — um modelo similar ao iFood, onde todos os estabelecimentos convivem num sistema único com dados compartilhados. Esse modelo limita a monetização: o produto precisa de escala de usuários finais para funcionar.

O pivot transforma o SportHub em uma **plataforma SaaS whitelabel**: você vende o sistema para donos de complexos esportivos, e cada cliente opera uma instância que parece ser o produto deles (logo, cores, domínio próprio). A receita vem da assinatura do software, não de comissão sobre reservas.

### O que muda conceitualmente

| Antes (Marketplace) | Depois (SaaS Whitelabel) |
|---|---|
| Todos os estabelecimentos num sistema único | Cada cliente tem seu ambiente isolado |
| Usuário final vê todos os estabelecimentos | Usuário final vê apenas o estabelecimento do tenant |
| Receita via comissão por reserva | Receita via assinatura mensal |
| Uma marca: SportHub | Cada cliente tem sua própria marca |
| Você gerencia todos os dados | Cada tenant gerencia seus próprios dados |

---

## 2. Decisões de Arquitetura

### 2.1 Estratégia de Isolamento: Schema por Tenant

Cada cliente (tenant) tem seu próprio **schema PostgreSQL** dentro do mesmo banco de dados. O schema `public` é reservado exclusivamente para os metadados globais dos tenants.

**Comparação das três estratégias:**

| Critério | Row-level (situação atual) | **Schema por tenant (escolhido)** | Database por tenant |
|---|---|---|---|
| Custo operacional | Mínimo | **Baixo** | Alto |
| Isolamento de dados | Fraco (dados misturados) | **Forte** | Máximo |
| Facilidade de vender | Difícil de convencer | **Profissional** | Difícil de provisionar |
| Backup por cliente | Impossível | **`pg_dump --schema=tenant_x`** | `pg_dump` trivial |
| Segurança (vazamento entre tenants) | Alto risco | **Praticamente impossível** | Zero risco |
| Migração de schema | Uma migration | **Uma migration → aplicada N vezes** | N migrations independentes |
| Número de conexões PG | 1 pool | **1 pool compartilhado** | N pools (problemático) |

**Por que não database por tenant?** Para dezenas de clientes, gerenciar N conexões de banco, N backups, N restore procedures e N ambientes de migration seria inviável operacionalmente para um time pequeno. Schema por tenant oferece 90% dos benefícios com 20% da complexidade.

**Por que não row-level?** Não é possível comercializar software whitelabel com dados de diferentes clientes na mesma tabela. Qualquer bug de filtragem expõe dados de outros clientes. Além disso, não há isolamento real para apresentar ao cliente como produto "deles".

### 2.2 Roteamento de Tenant: Subdomínio

O tenant é identificado pelo **subdomínio** da requisição HTTP:

```
abc.sporthub.app   → tenant "abc"
silvaesportes.sporthub.app → tenant "silvaesportes"
```

Um `TenantResolutionMiddleware` extrai o slug do header `Host` e resolve o tenant correspondente antes de qualquer handler executar.

**Domínio customizado (futuro):** O campo `CustomDomain` na entidade `Tenant` já está previsto para suportar mapeamento `app.clientedomain.com → tenant "abc"` sem mudança de arquitetura.

**Ambiente de desenvolvimento:** Adicionar entradas em `/etc/hosts` (ex: `abc.localhost`) ou usar o header `X-Tenant-Slug` como fallback quando o host for `localhost`.

### 2.3 Billing: Fora do Sistema

O backend não gerencia cobranças. Ele apenas mantém o `TenantStatus` (Active / Suspended / Canceled). Quando um tenant está suspenso (ex: boleto não pago), todas as requests retornam `403 Forbidden`. O controle de cobrança é manual ou via ferramenta externa (Stripe, Asaas, etc.).

---

## 3. Visão Geral da Arquitetura

### 3.1 Diagrama de schemas no PostgreSQL

```
┌──────────────────────────────────────────────────────────────────┐
│                         PostgreSQL 16                             │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │                    Schema: public                         │   │
│  │  Tabela: Tenants  (conta SaaS — gerenciada por você)      │   │
│  │  ├─ Id (GUID)                                             │   │
│  │  ├─ Slug          → "academiasilva"                       │   │
│  │  ├─ Name          → "Academia Silva Esportes"             │   │
│  │  ├─ Status        → Active | Suspended | Canceled         │   │
│  │  ├─ LogoUrl       → "https://cdn.../logo.png"             │   │
│  │  ├─ PrimaryColor  → "#2563eb"                             │   │
│  │  ├─ CustomDomain  → null (futuro)                         │   │
│  │  └─ CreatedAt                                             │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌────────────────────────────┐  ┌────────────────────────────┐  │
│  │   Schema: tenant_academia  │  │   Schema: tenant_rede      │  │
│  │                            │  │                            │  │
│  │  Users                     │  │  Users                     │  │
│  │                            │  │                            │  │
│  │  Establishments (unidades) │  │  Establishments (unidades) │  │
│  │  ├─ "Unidade Maceió"       │  │  ├─ "Unidade Recife"       │  │
│  │  └─ (pode ter mais)        │  │  └─ "Unidade Fortaleza"    │  │
│  │                            │  │                            │  │
│  │  EstablishmentUsers        │  │  EstablishmentUsers        │  │
│  │  Courts  ←─ EstabId FK     │  │  Courts  ←─ EstabId FK     │  │
│  │  Sports                    │  │  Sports                    │  │
│  │  Reservations              │  │  Reservations              │  │
│  └────────────────────────────┘  └────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘

Tenant   = conta SaaS (quem paga a assinatura)
Establishment = unidade física do negócio (o que o usuário final vê)
Court         = quadra dentro de uma unidade
```

### 3.2 Fluxo completo de uma requisição

```
1. Cliente faz: GET https://abc.sporthub.app/api/courts
                                │
2. TenantResolutionMiddleware   │
   ├─ Extrai slug "abc" do Host header
   ├─ Verifica Redis → cache miss
   ├─ Busca Tenant no TenantDbContext (schema public)
   ├─ Tenant não existe → 404 ProblemDetails (para aqui)
   ├─ Tenant.Status == Suspended → 403 ProblemDetails (para aqui)
   ├─ Tenant válido → popula ITenantContext no DI scoped
   └─ Persiste tenant em cache Redis (TTL: 1h)
                                │
3. UseAuthentication / UseAuthorization (JWT normal)
                                │
4. CourtsHandler executa
   └─ injeta ApplicationDbContext
      └─ OnModelCreating → HasDefaultSchema("tenant_abc")
         └─ todas as queries rodam em tenant_abc.courts
                                │
5. Retorna resultado isolado do tenant "abc"
```

### 3.3 Dois DbContexts

O sistema passa a ter dois `DbContext` com responsabilidades distintas:

| | `TenantDbContext` | `ApplicationDbContext` |
|---|---|---|
| **Schema** | `public` (fixo) | `tenant_{slug}` (dinâmico por request) |
| **Conteúdo** | Só `DbSet<Tenant>` | Users, Establishments, Courts, Sports, Reservations |
| **Ciclo de vida DI** | Singleton | Scoped (muda por request) |
| **Quem usa** | Middleware, TenantProvisioningService | Todos os handlers/repositórios existentes |
| **Migrations** | Pasta `Migrations/Global/` | Pasta `Migrations/Tenant/` (aplicada por tenant) |

---

## 4. Entidades e Contratos

### 4.1 Nova Entidade: `Tenant`

**Localização:** `src/SportHub.Domain/Entities/Tenant.cs`

`Tenant` **não herda `AuditEntity`** porque existe no schema `public` e não faz parte do ciclo de vida por tenant. Também não implementa `IEntity` pelo mesmo motivo (não usa `BaseRepository<T>`).

```csharp
// src/SportHub.Domain/Entities/Tenant.cs
namespace Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador único na URL. Ex: "academiasilva" → academiasilva.sporthub.app
    /// Regras: só letras minúsculas, números e hífens. Máx 63 chars (limite DNS).
    /// </summary>
    public string Slug { get; private set; } = null!;

    /// <summary>Nome exibido no sistema. Ex: "Academia Silva Esportes"</summary>
    public string Name { get; private set; } = null!;

    public TenantStatus Status { get; private set; } = TenantStatus.Active;

    // Branding
    public string? LogoUrl { get; private set; }
    public string? PrimaryColor { get; private set; }  // hex: "#2563eb"

    // Futuro: domínio customizado do cliente
    public string? CustomDomain { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core constructor
    private Tenant() { }

    public static Tenant Create(string slug, string name)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = slug.ToLowerInvariant(),
            Name = name,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateBranding(string? logoUrl, string? primaryColor)
    {
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
    }

    public void Suspend() => Status = TenantStatus.Suspended;
    public void Activate() => Status = TenantStatus.Active;
    public void Cancel() => Status = TenantStatus.Canceled;

    /// <summary>
    /// Deriva o nome do schema PostgreSQL a partir do slug.
    /// Ex: "academia-silva" → "tenant_academia_silva"
    /// </summary>
    public string GetSchemaName() =>
        $"tenant_{Slug.Replace("-", "_")}";
}
```

**Enum `TenantStatus`:**

```csharp
// src/SportHub.Domain/Enums/TenantStatus.cs
namespace Domain.Enums;

public enum TenantStatus
{
    Active = 0,     // Operando normalmente
    Suspended = 1,  // Bloqueado (ex: inadimplência) — 403 em todas as requests
    Canceled = 2    // Encerrado — pode ser deletado futuramente
}
```

### 4.2 Modificação em `UserRole`

**Localização:** `src/SportHub.Domain/Enums/UserRole.cs`

Adicionamos `SuperAdmin` para identificar quem pode gerenciar tenants (você, o operador da plataforma).

```csharp
// src/SportHub.Domain/Enums/UserRole.cs
namespace Domain.Enums;

public enum UserRole
{
    User = 0,               // Usuário final (faz reservas)
    EstablishmentMember = 1, // Membro de um estabelecimento (staff/manager/owner)
    Admin = 2,              // Admin dentro de um tenant específico
    SuperAdmin = 99         // Operador da plataforma (acesso global a /api/tenants)
}

public static class UserRoleExtensions
{
    public static string ToRoleName(this UserRole role) => role.ToString();
}
```

> **Importante:** `SuperAdmin` existe no schema `public` (é um User global, não por tenant). Na prática, é o seu usuário de operação da plataforma. O `EstablishmentHandler` atual já faz bypass para `Admin` — o mesmo padrão será aplicado para `SuperAdmin`.

### 4.3 Interface `ITenantContext`

**Localização:** `src/SportHub.Application/Common/Interfaces/ITenantContext.cs`

```csharp
// src/SportHub.Application/Common/Interfaces/ITenantContext.cs
namespace Application.Common.Interfaces;

/// <summary>
/// Contexto do tenant resolvido para a request atual.
/// Registrado como Scoped — uma instância por request HTTP.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }

    /// <summary>Nome do schema PostgreSQL. Ex: "tenant_academia_silva"</summary>
    string Schema { get; }

    /// <summary>False antes do middleware resolver. Handlers não devem executar se false.</summary>
    bool IsResolved { get; }

    void Resolve(Tenant tenant);
}
```

### 4.4 Interface `ITenantRepository`

**Localização:** `src/SportHub.Application/Common/Interfaces/ITenantRepository.cs`

```csharp
// src/SportHub.Application/Common/Interfaces/ITenantRepository.cs
namespace Application.Common.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetByCustomDomainAsync(string domain, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<List<Tenant>> GetAllAsync(CancellationToken ct = default);
}
```

### 4.5 Entidade `Establishment` — Mantida sem alterações

#### Por que não apagar?

Ao pivotar para whitelabel, surge a dúvida natural: se cada **tenant** já representa "o cliente que compra o sistema", qual é o papel do **Establishment**? Eles não seriam a mesma coisa?

A resposta depende do modelo de negócio do seu cliente. Há dois cenários possíveis:

**Cenário A — Tenant tem um único estabelecimento (1:1)**

O cliente é uma arena única. Ex: "Arena Silva" compra o sistema. Ela é o estabelecimento.

```
Tenant: arena-silva (schema: tenant_arena_silva)
  └── Establishment: Arena Silva ← parece redundante com o Tenant
       └── Court: Quadra 1
       └── Court: Quadra 2
```

Nesse cenário `Establishment` parece redundante, mas eliminar exigiria mover todos os campos de nome, endereço, telefone e imagem para o `Tenant`, refatorar `Court.EstablishmentId`, reescrever repositórios e migrations. Custo alto, benefício baixo.

**Cenário B — Tenant tem múltiplos estabelecimentos (1:N)**

O cliente é uma rede ou franquia. Ex: "Rede Arena" compra o sistema e opera 3 unidades em cidades diferentes.

```
Tenant: rede-arena (schema: tenant_rede_arena)
  ├── Establishment: Unidade Maceió   → Courts 1, 2, 3
  ├── Establishment: Unidade Recife   → Courts 4, 5
  └── Establishment: Unidade Fortaleza → Court 6
```

Nesse cenário `Establishment` é **essencial** — cada unidade tem endereço, horários e equipe próprios.

#### Decisão: manter `Establishment` sem alterações

`Tenant` e `Establishment` têm **responsabilidades distintas** e não se sobrepõem:

| | `Tenant` (schema `public`) | `Establishment` (schema `tenant_*`) |
|---|---|---|
| **O que representa** | O cliente que compra o software | Uma unidade física de negócio |
| **Dados** | Slug, branding, status, domínio | Nome, endereço, telefone, email, imagem |
| **Ciclo de vida** | Gerenciado por você (operador) | Gerenciado pelo próprio cliente |
| **Visibilidade** | Invisível para o usuário final | Visível: é o que o usuário final vê |
| **Quantidade** | Um por cliente | Um ou mais por tenant |

`Tenant` é a **conta SaaS**. `Establishment` é o **negócio real** dentro dessa conta. Separar as duas responsabilidades é arquiteturalmente correto e permite que o sistema escale para franquias sem mudança de modelo.

#### O que não muda em `Establishment`

A entidade, os repositórios, os use cases e os endpoints de `Establishment` **não precisam de nenhuma alteração** para o pivot. Eles já vivem dentro do schema do tenant (por serem parte do `ApplicationDbContext`), portanto o isolamento acontece automaticamente.

```csharp
// src/SportHub.Domain/Entities/Establishment.cs — SEM ALTERAÇÃO
public class Establishment : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Website { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public Address Address { get; set; } = null!;

    public ICollection<EstablishmentUser> Users { get; set; } = new List<EstablishmentUser>();
    public ICollection<Court> Courts { get; set; } = new List<Court>();
    public ICollection<Sport> Sports { get; set; } = new List<Sport>();
}
```

`Court.EstablishmentId` também permanece intacto — a quadra continua pertencendo a um estabelecimento, e o estabelecimento pertence implicitamente ao tenant pelo fato de existir no schema dele.

#### Hierarquia completa de propriedade

```
[você, operador]
    │
    ├── Tenant: rede-arena       (schema public — TenantDbContext)
    │       Slug, Status, Branding
    │
    └── [schema: tenant_rede_arena]   ← ApplicationDbContext
            │
            ├── Establishment: Unidade Maceió
            │       Address, Phone, Email, ImageUrl
            │       ├── Court: Quadra 1  (SlotDuration, Hours)
            │       ├── Court: Quadra 2
            │       └── EstablishmentUser: João (Owner), Maria (Manager)
            │
            └── Establishment: Unidade Recife
                    Address, Phone, Email, ImageUrl
                    ├── Court: Quadra 3
                    └── EstablishmentUser: Carlos (Owner)
```

#### Consequências práticas

- Nenhuma migration existente precisa ser alterada
- Nenhum repositório de `Establishment` precisa ser tocado
- Nenhum endpoint de `Establishment` precisa ser tocado
- O isolamento entre tenants é garantido pelo schema — não por filtros em `Establishment`
- Ao provisionar um novo tenant, o `TenantProvisioningService` **não cria** nenhum `Establishment` automaticamente — o próprio cliente faz isso após logar pela primeira vez

#### Possível evolução futura (não fazer agora)

Se no futuro os clientes forem **apenas** de unidade única e você quiser simplificar a UX (eliminar a tela de "criar estabelecimento"), seria possível fundir `Establishment` no `Tenant`. Mas isso é uma decisão de produto, não de arquitetura, e não deve ser feita antecipadamente.

---

## 5. Infraestrutura — Persistência

### 5.1 `TenantDbContext` (schema `public`)

**Localização:** `src/SportHub.Infrastructure/Persistence/TenantDbContext.cs`

```csharp
// src/SportHub.Infrastructure/Persistence/TenantDbContext.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// DbContext exclusivo para o schema "public".
/// Contém apenas a tabela de Tenants (metadados globais da plataforma).
/// Registrado como Singleton no DI — não muda por request.
/// </summary>
public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");
        builder.ApplyConfiguration(new TenantConfiguration());
    }
}
```

**EF Core Configuration:**

```csharp
// src/SportHub.Infrastructure/Persistence/Configurations/TenantConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "public");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(63);         // limite de label DNS

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.PrimaryColor)
            .HasMaxLength(7);          // "#xxxxxx"

        builder.Property(t => t.CustomDomain)
            .HasMaxLength(253);        // limite de FQDN

        // Índices
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.CustomDomain).IsUnique();
        builder.HasIndex(t => t.Status);
    }
}
```

### 5.2 Modificação no `ApplicationDbContext`

**Localização:** `src/SportHub.Infrastructure/Persistence/AppDbContext.cs`

A mudança central: o `ApplicationDbContext` passa a receber `ITenantContext` e usa `HasDefaultSchema` para redirecionar todas as queries para o schema do tenant atual.

```csharp
// src/SportHub.Infrastructure/Persistence/AppDbContext.cs
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SportHub.Domain.Common;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Establishment> Establishments { get; set; } = null!;
    public DbSet<EstablishmentUser> EstablishmentUsers { get; set; } = null!;
    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<Sport> Sports { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Ponto crítico: define o schema dinamicamente por tenant
        // Todas as tabelas herdam esse schema por padrão
        builder.HasDefaultSchema(_tenantContext.Schema);

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreated(userId);
            else if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdated(userId);
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.MarkAsDeleted(userId);

                foreach (var reference in entry.References)
                {
                    if (reference.TargetEntry != null && reference.TargetEntry.Metadata.IsOwned())
                        reference.TargetEntry.State = EntityState.Unchanged;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

> **Gotcha — Migrations com schema dinâmico:** O EF Core gera migrations com schema hardcoded. A solução é usar um `DesignTimeTenantContext` que retorna um schema placeholder (`"tenant_placeholder"`) apenas para geração de migrations via CLI. Em produção, o schema real é injetado via `ITenantContext`.

```csharp
// src/SportHub.Infrastructure/Persistence/DesignTimeTenantContext.cs
// Usado APENAS pelo EF CLI (dotnet ef migrations add).
// Nunca é instanciado em produção.
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=sporthub;Username=postgres;Password=postgres");

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new NullCurrentUserService(),
            new PlaceholderTenantContext()
        );
    }
}

// Implementações mínimas para o design-time:
internal class NullCurrentUserService : ICurrentUserService
{
    public Guid UserId => Guid.Empty;
}

internal class PlaceholderTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public string TenantSlug => "placeholder";
    public string Schema => "tenant_placeholder";
    public bool IsResolved => true;
    public void Resolve(Tenant tenant) { }
}
```

### 5.3 `TenantRepository`

**Localização:** `src/SportHub.Infrastructure/Repositories/TenantRepository.cs`

```csharp
// src/SportHub.Infrastructure/Repositories/TenantRepository.cs
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repositório de tenants. Opera SEMPRE no schema "public" via TenantDbContext.
/// Não herda BaseRepository porque Tenant não implementa IEntity.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly TenantDbContext _context;

    public TenantRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task<Tenant?> GetByCustomDomainAsync(string domain, CancellationToken ct = default) =>
        await _context.Tenants
            .FirstOrDefaultAsync(t => t.CustomDomain == domain.ToLowerInvariant(), ct);

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

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        await _context.Tenants.AnyAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task<List<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Tenants.OrderBy(t => t.Name).ToListAsync(ct);
}
```

---

## 6. Middleware de Resolução de Tenant

### 6.1 `TenantContext` (implementação Scoped)

**Localização:** `src/SportHub.Infrastructure/Services/TenantContext.cs`

```csharp
// src/SportHub.Infrastructure/Services/TenantContext.cs
using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Implementação Scoped de ITenantContext.
/// Começa não resolvida. O TenantResolutionMiddleware chama Resolve() com o tenant encontrado.
/// </summary>
public class TenantContext : ITenantContext
{
    private Tenant? _tenant;

    public Guid TenantId => _tenant?.Id ?? throw new InvalidOperationException("Tenant não resolvido.");
    public string TenantSlug => _tenant?.Slug ?? throw new InvalidOperationException("Tenant não resolvido.");
    public string Schema => _tenant?.GetSchemaName() ?? throw new InvalidOperationException("Tenant não resolvido.");
    public bool IsResolved => _tenant is not null;

    public void Resolve(Tenant tenant)
    {
        _tenant = tenant;
    }
}
```

### 6.2 `TenantResolutionMiddleware`

**Localização:** `src/SportHub.Infrastructure/Middleware/TenantResolutionMiddleware.cs`

```csharp
// src/SportHub.Infrastructure/Middleware/TenantResolutionMiddleware.cs
using Application.Common.Enums;
using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Infrastructure.Middleware;

/// <summary>
/// Middleware que resolve o tenant a partir do subdomínio antes de qualquer handler.
///
/// Ordem de resolução:
/// 1. Subdomínio do Host header (produção): abc.sporthub.app → "abc"
/// 2. Header X-Tenant-Slug (fallback para desenvolvimento local)
///
/// Rotas excluídas do middleware (passam direto):
/// - /api/tenants/** (gestão de tenants — só SuperAdmin)
/// - /health
/// - /scalar/** e /openapi/** (documentação)
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    // Prefixos de rota que não precisam de tenant resolvido
    private static readonly string[] _bypassPaths =
    [
        "/api/tenants",
        "/health",
        "/scalar",
        "/openapi"
    ];

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider services)
    {
        var path = context.Request.Path.Value ?? "";

        // Bypass para rotas globais
        if (_bypassPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var slug = ExtractSlug(context);

        if (string.IsNullOrWhiteSpace(slug))
        {
            await WriteProblemAsync(context, 400, "Tenant não identificado.",
                "Acesse via subdomínio (ex: abc.sporthub.app) ou informe o header X-Tenant-Slug.");
            return;
        }

        // Resolve via cache ou banco
        var cache = services.GetRequiredService<ICacheService>();
        var cacheKey = cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, slug);

        var tenant = await cache.GetAsync<Domain.Entities.Tenant>(cacheKey);

        if (tenant is null)
        {
            var repo = services.GetRequiredService<ITenantRepository>();
            tenant = await repo.GetBySlugAsync(slug);

            if (tenant is not null)
                await cache.SetAsync(cacheKey, tenant, TimeSpan.FromHours(1));
        }

        if (tenant is null)
        {
            await WriteProblemAsync(context, 404, "Tenant não encontrado.",
                $"Nenhum tenant com slug '{slug}' foi encontrado.");
            return;
        }

        if (tenant.Status == TenantStatus.Suspended)
        {
            await WriteProblemAsync(context, 403, "Tenant suspenso.",
                "Este tenant está temporariamente suspenso. Entre em contato com o suporte.");
            return;
        }

        if (tenant.Status == TenantStatus.Canceled)
        {
            await WriteProblemAsync(context, 410, "Tenant cancelado.",
                "Este tenant foi encerrado.");
            return;
        }

        // Popula ITenantContext no DI scoped
        var tenantContext = services.GetRequiredService<ITenantContext>();
        tenantContext.Resolve(tenant);

        await _next(context);
    }

    private static string? ExtractSlug(HttpContext context)
    {
        // 1. Tenta pelo subdomínio
        var host = context.Request.Host.Host; // "abc.sporthub.app"
        var parts = host.Split('.');

        // Host com subdomínio real (ex: abc.sporthub.app tem 3+ partes)
        if (parts.Length >= 3)
            return parts[0].ToLowerInvariant();

        // 2. Fallback para desenvolvimento: header X-Tenant-Slug
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug))
            return headerSlug.ToString().ToLowerInvariant();

        return null;
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.io/{status}",
            title,
            status,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
```

---

## 7. Provisioning de Tenant

Quando um novo cliente contrata o sistema, o processo de **provisioning** cria tudo automaticamente: schema PostgreSQL, tabelas e dados iniciais (seeds).

### 7.1 `TenantProvisioningService`

**Localização:** `src/SportHub.Infrastructure/Services/TenantProvisioningService.cs`

```csharp
// src/SportHub.Infrastructure/Services/TenantProvisioningService.cs
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TenantProvisioningService
{
    private readonly TenantDbContext _globalCtx;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        TenantDbContext globalCtx,
        IServiceProvider serviceProvider,
        ITenantRepository tenantRepository,
        ILogger<TenantProvisioningService> logger)
    {
        _globalCtx = globalCtx;
        _serviceProvider = serviceProvider;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Provisiona um novo tenant:
    /// 1. Salva o Tenant na tabela global (schema public)
    /// 2. Cria o schema PostgreSQL: CREATE SCHEMA IF NOT EXISTS tenant_{slug}
    /// 3. Executa migrations do ApplicationDbContext nesse schema
    /// 4. Faz seed dos Sports padrão
    /// </summary>
    public async Task ProvisionAsync(Tenant tenant, CancellationToken ct = default)
    {
        _logger.LogInformation("Iniciando provisioning do tenant {Slug}", tenant.Slug);

        // Passo 1: Salvar metadados globais
        await _tenantRepository.AddAsync(tenant, ct);
        _logger.LogInformation("Tenant {Slug} salvo no schema public", tenant.Slug);

        // Passo 2: Criar schema no PostgreSQL
        var schemaName = tenant.GetSchemaName();
        await _globalCtx.Database.ExecuteSqlRawAsync(
            $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"", ct);
        _logger.LogInformation("Schema {Schema} criado", schemaName);

        // Passo 3: Executar migrations no schema do tenant
        // Cria um ITenantContext temporário apontando para o novo tenant
        var tenantCtxForMigration = new StaticTenantContext(tenant);

        using var scope = _serviceProvider.CreateScope();
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Pega a connection string do DbContext global
        var connectionString = _globalCtx.Database.GetConnectionString()!;
        optionsBuilder.UseNpgsql(connectionString);

        await using var tenantDb = new ApplicationDbContext(
            optionsBuilder.Options,
            new NullCurrentUserService(),
            tenantCtxForMigration
        );

        await tenantDb.Database.MigrateAsync(ct);
        _logger.LogInformation("Migrations aplicadas no schema {Schema}", schemaName);

        // Passo 4: Seed de Sports padrão
        await SeedDefaultSportsAsync(tenantDb, ct);
        _logger.LogInformation("Provisioning do tenant {Slug} concluído", tenant.Slug);
    }

    private static async Task SeedDefaultSportsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var defaultSports = new[]
        {
            Sport.Create("Futebol Society", "Futebol em campo society (7 a side)", ""),
            Sport.Create("Beach Tennis", "Beach Tennis em quadra de areia", ""),
            Sport.Create("Padel", "Padel em quadra fechada", ""),
            Sport.Create("Tênis", "Tênis em quadra de saibro ou dura", ""),
            Sport.Create("Vôlei de Praia", "Vôlei em quadra de areia", ""),
            Sport.Create("Basquete", "Basquete em quadra coberta", ""),
        };

        db.Sports.AddRange(defaultSports);
        await db.SaveChangesAsync(ct);
    }
}

/// <summary>Implementação de ITenantContext com valores fixos, para uso interno no provisioning.</summary>
internal class StaticTenantContext : ITenantContext
{
    private readonly Tenant _tenant;
    public StaticTenantContext(Tenant tenant) => _tenant = tenant;

    public Guid TenantId => _tenant.Id;
    public string TenantSlug => _tenant.Slug;
    public string Schema => _tenant.GetSchemaName();
    public bool IsResolved => true;
    public void Resolve(Tenant tenant) { } // no-op
}

internal class NullCurrentUserService : ICurrentUserService
{
    public Guid UserId => Guid.Empty;
}
```

---

## 8. Application Layer — Use Cases CQRS

### 8.1 `ProvisionTenantCommand`

**Localização:** `src/SportHub.Application/UseCases/Tenant/ProvisionTenant/`

```csharp
// ProvisionTenantCommand.cs
public record ProvisionTenantCommand(string Slug, string Name) : ICommand<ProvisionTenantResponse>;

// ProvisionTenantResponse.cs
public record ProvisionTenantResponse(Guid Id, string Slug, string Name, string Schema);

// ProvisionTenantValidator.cs
public class ProvisionTenantValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantValidator(ITenantRepository repo)
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(63)
            .Matches(@"^[a-z0-9][a-z0-9\-]*[a-z0-9]$")
            .WithMessage("Slug deve conter apenas letras minúsculas, números e hífens. Não pode começar ou terminar com hífen.")
            .MustAsync(async (slug, ct) => !await repo.SlugExistsAsync(slug, ct))
            .WithMessage("Este slug já está em uso.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);
    }
}

// ProvisionTenantHandler.cs
public class ProvisionTenantHandler : ICommandHandler<ProvisionTenantCommand, ProvisionTenantResponse>
{
    private readonly TenantProvisioningService _provisioning;

    public ProvisionTenantHandler(TenantProvisioningService provisioning)
    {
        _provisioning = provisioning;
    }

    public async Task<Result<ProvisionTenantResponse>> Handle(
        ProvisionTenantCommand request, CancellationToken ct)
    {
        var tenant = Tenant.Create(request.Slug, request.Name);

        await _provisioning.ProvisionAsync(tenant, ct);

        return Result.Ok(new ProvisionTenantResponse(
            tenant.Id,
            tenant.Slug,
            tenant.Name,
            tenant.GetSchemaName()
        ));
    }
}
```

### 8.2 `GetTenantQuery`

```csharp
// GetTenantQuery.cs
public record GetTenantQuery(string Slug) : IQuery<GetTenantResponse>;

// GetTenantResponse.cs
public record GetTenantResponse(
    Guid Id,
    string Slug,
    string Name,
    string Status,
    string? LogoUrl,
    string? PrimaryColor,
    string? CustomDomain,
    DateTime CreatedAt
);

// GetTenantHandler.cs
public class GetTenantHandler : IQueryHandler<GetTenantQuery, GetTenantResponse>
{
    private readonly ITenantRepository _repo;

    public GetTenantHandler(ITenantRepository repo) => _repo = repo;

    public async Task<Result<GetTenantResponse>> Handle(GetTenantQuery request, CancellationToken ct)
    {
        var tenant = await _repo.GetBySlugAsync(request.Slug, ct);
        if (tenant is null)
            return Result.Fail(new NotFound($"Tenant '{request.Slug}' não encontrado."));

        return Result.Ok(new GetTenantResponse(
            tenant.Id, tenant.Slug, tenant.Name,
            tenant.Status.ToString(), tenant.LogoUrl,
            tenant.PrimaryColor, tenant.CustomDomain, tenant.CreatedAt
        ));
    }
}
```

### 8.3 `UpdateTenantCommand` (branding)

```csharp
// UpdateTenantCommand.cs
public record UpdateTenantCommand(string Slug, string? LogoUrl, string? PrimaryColor) : ICommand<Unit>;

// UpdateTenantValidator.cs
public class UpdateTenantValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantValidator()
    {
        RuleFor(x => x.PrimaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => x.PrimaryColor is not null)
            .WithMessage("Cor deve ser um hex válido: #RRGGBB");

        RuleFor(x => x.LogoUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => x.LogoUrl is not null)
            .WithMessage("LogoUrl deve ser uma URL válida.");
    }
}

// UpdateTenantHandler.cs
public class UpdateTenantHandler : ICommandHandler<UpdateTenantCommand, Unit>
{
    private readonly ITenantRepository _repo;
    private readonly ICacheService _cache;

    public UpdateTenantHandler(ITenantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<Result<Unit>> Handle(UpdateTenantCommand request, CancellationToken ct)
    {
        var tenant = await _repo.GetBySlugAsync(request.Slug, ct);
        if (tenant is null)
            return Result.Fail(new NotFound($"Tenant '{request.Slug}' não encontrado."));

        tenant.UpdateBranding(request.LogoUrl, request.PrimaryColor);
        await _repo.UpdateAsync(tenant, ct);

        // Invalida cache do tenant
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, request.Slug);
        await _cache.RemoveAsync(key, ct);

        return Result.Ok(Unit.Value);
    }
}
```

### 8.4 `SuspendTenantCommand`

```csharp
// SuspendTenantCommand.cs
public record SuspendTenantCommand(string Slug) : ICommand<Unit>;

// SuspendTenantHandler.cs
public class SuspendTenantHandler : ICommandHandler<SuspendTenantCommand, Unit>
{
    private readonly ITenantRepository _repo;
    private readonly ICacheService _cache;

    public SuspendTenantHandler(ITenantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<Result<Unit>> Handle(SuspendTenantCommand request, CancellationToken ct)
    {
        var tenant = await _repo.GetBySlugAsync(request.Slug, ct);
        if (tenant is null)
            return Result.Fail(new NotFound($"Tenant '{request.Slug}' não encontrado."));

        tenant.Suspend();
        await _repo.UpdateAsync(tenant, ct);

        // Remove do cache — próxima request vai ver o status atualizado
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, request.Slug);
        await _cache.RemoveAsync(key, ct);

        return Result.Ok(Unit.Value);
    }
}
```

---

## 9. Api Layer — Endpoints

### 9.1 `TenantEndpoints`

**Localização:** `src/SportHub.Api/Endpoints/TenantEndpoints.cs`

```csharp
// src/SportHub.Api/Endpoints/TenantEndpoints.cs
using Application.UseCases.Tenant.GetTenant;
using Application.UseCases.Tenant.ProvisionTenant;
using Application.UseCases.Tenant.SuspendTenant;
using Application.UseCases.Tenant.UpdateTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SportHub.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        // Grupo fora do middleware de tenant — acessível sem subdomínio
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .RequireAuthorization(PolicyNames.IsSuperAdmin);

        // POST /api/tenants — Provisionar novo tenant
        group.MapPost("/", async (
            [FromBody] ProvisionTenantCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("ProvisionTenant")
        .WithSummary("Provisiona um novo tenant (cria schema + tabelas + seed)")
        .Produces<ProvisionTenantResponse>(201)
        .Produces<ProblemDetails>(422);

        // GET /api/tenants/{slug}
        group.MapGet("/{slug}", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new GetTenantQuery(slug));
            return result.ToIResult();
        })
        .WithName("GetTenant")
        .WithSummary("Retorna metadados de um tenant pelo slug")
        .Produces<GetTenantResponse>()
        .Produces<ProblemDetails>(404);

        // GET /api/tenants — Lista todos os tenants
        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllTenantsQuery());
            return result.ToIResult();
        })
        .WithName("GetAllTenants")
        .WithSummary("Lista todos os tenants da plataforma");

        // PATCH /api/tenants/{slug}/branding — Atualizar branding
        group.MapPatch("/{slug}/branding", async (
            string slug,
            [FromBody] UpdateTenantBrandingRequest body,
            ISender sender) =>
        {
            var result = await sender.Send(new UpdateTenantCommand(slug, body.LogoUrl, body.PrimaryColor));
            return result.ToIResult();
        })
        .WithName("UpdateTenantBranding")
        .WithSummary("Atualiza logo e cor primária do tenant")
        .Produces(204)
        .Produces<ProblemDetails>(404);

        // POST /api/tenants/{slug}/suspend
        group.MapPost("/{slug}/suspend", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new SuspendTenantCommand(slug));
            return result.ToIResult();
        })
        .WithName("SuspendTenant")
        .WithSummary("Suspende um tenant (403 em todas as requests do subdomínio)")
        .Produces(204)
        .Produces<ProblemDetails>(404);

        // POST /api/tenants/{slug}/activate
        group.MapPost("/{slug}/activate", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new ActivateTenantCommand(slug));
            return result.ToIResult();
        })
        .WithName("ActivateTenant")
        .WithSummary("Reativa um tenant suspenso")
        .Produces(204)
        .Produces<ProblemDetails>(404);
    }
}

public record UpdateTenantBrandingRequest(string? LogoUrl, string? PrimaryColor);
```

### 9.2 Endpoint público de branding

Este endpoint é acessado **com** subdomínio (o middleware resolve o tenant), mas **sem autenticação**. O frontend usa para carregar o tema visual do tenant.

```csharp
// Adicionar em TenantEndpoints ou em BrandingEndpoints separado

// GET /api/branding  (acessado como abc.sporthub.app/api/branding)
app.MapGet("/api/branding", (ITenantContext tenantCtx) =>
{
    if (!tenantCtx.IsResolved)
        return Results.NotFound();

    // ITenantContext já tem os dados do tenant carregados pelo middleware
    // Para retornar branding, o TenantContext precisa expor os campos do Tenant
    // Alternativa: injetar ITenantRepository e buscar pelo slug do contexto
    return Results.Ok(new BrandingResponse(
        tenantCtx.TenantName,       // adicionar à interface
        tenantCtx.LogoUrl,
        tenantCtx.PrimaryColor
    ));
})
.WithName("GetBranding")
.WithSummary("Retorna informações de branding do tenant atual (público, sem auth)")
.AllowAnonymous()
.Produces<BrandingResponse>();

public record BrandingResponse(string Name, string? LogoUrl, string? PrimaryColor);
```

---

## 10. Modificações no `Program.cs` e DI

### 10.1 `Program.cs` atualizado

```csharp
// src/SportHub.Api/Program.cs
using Infrastructure.Middleware;
// ... demais usings

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.AddAuthentication()
        .AddServices()
        .AddRepositories()
        .AddCustomExceptionHandler()
        .AddDatabase(builder.Configuration)   // agora registra DOIS DbContexts
        .AddMediatR()
        .AddSettings()
        .AddSeeders()
        .AddSerilogLogging()
        .AddCaching();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ExecuteMigrations();   // migra schema public (TenantDbContext)
}

app.UseRouting();

// NOVO: Middleware de resolução de tenant
// Deve vir ANTES de UseAuthentication para que o schema já esteja definido
// quando o ApplicationDbContext for usado na validação de JWT (se aplicável)
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints();   // inclui MapTenantEndpoints()

// Seed global (apenas do schema public — admin SuperAdmin)
using (var scope = app.Services.CreateScope())
{
    var superAdminSeeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
    await superAdminSeeder.SeedAsync();
}

app.Run();
```

### 10.2 `ServiceExtensions.AddDatabase` atualizado

```csharp
public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")!;

    // DbContext do schema public (tenants globais) — Singleton
    builder.Services.AddDbContext<TenantDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    }, ServiceLifetime.Singleton);

    // DbContext por tenant (schema dinâmico) — Scoped
    builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        options.UseNpgsql(connectionString);
        // ITenantContext é Scoped e injetado automaticamente no construtor
    });

    return builder;
}
```

### 10.3 `ServiceExtensions.AddServices` atualizado

```csharp
public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
{
    // Serviços existentes (mantidos)
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IEstablishmentService, EstablishmentService>();
    builder.Services.AddScoped<IEstablishmentRoleService, EstablishmentRoleService>();
    builder.Services.AddScoped<IAuthorizationHandler, EstablishmentHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, GlobalRoleHandler>();
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();
    builder.Services.AddScoped<ICacheService, CacheService>();

    // NOVOS: Tenant
    builder.Services.AddScoped<ITenantContext, TenantContext>();  // Scoped (uma instância por request)
    builder.Services.AddScoped<TenantProvisioningService>();

    return builder;
}

public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
{
    // Repositórios existentes (mantidos)
    builder.Services.AddScoped<IEstablishmentsRepository, EstablishmentsRepository>();
    builder.Services.AddScoped<IEstablishmentUsersRepository, EstablishmentUsersRepository>();
    builder.Services.AddScoped<IUsersRepository, UsersRepository>();
    builder.Services.AddScoped<ICourtsRepository, CourtsRepository>();
    builder.Services.AddScoped<ISportsRepository, SportsRepository>();
    builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

    // NOVO: Tenant
    builder.Services.AddScoped<ITenantRepository, TenantRepository>();

    return builder;
}
```

### 10.4 `AppExtensions.UseEndpoints` atualizado

```csharp
public static WebApplication UseEndpoints(this WebApplication app)
{
    // Endpoints existentes (dentro do middleware de tenant)
    app.MapAuthEndpoints();
    app.MapEstablishmentsEndpoints();
    app.MapSportsEndpoints();
    app.MapCourtsEndpoints();

    // NOVO: Tenant endpoints (fora do middleware de tenant)
    app.MapTenantEndpoints();

    return app;
}
```

---

## 11. Authorization — Policy `IsSuperAdmin`

### 11.1 Nova requirement e handler

```csharp
// src/SportHub.Application/Security/SuperAdminRequirement.cs
using Microsoft.AspNetCore.Authorization;

namespace Application.Security;

public class SuperAdminRequirement : IAuthorizationRequirement { }

// src/SportHub.Infrastructure/Security/SuperAdminHandler.cs
public class SuperAdminHandler : AuthorizationHandler<SuperAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        var roleClaimValue = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (Enum.TryParse<UserRole>(roleClaimValue, out var role) && role == UserRole.SuperAdmin)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

### 11.2 Registro da policy

```csharp
// Em ServiceExtensions.AddAuthentication, adicionar:
options.AddPolicy(PolicyNames.IsSuperAdmin, policy =>
    policy.Requirements.Add(new SuperAdminRequirement()));
```

```csharp
// Em PolicyNames.cs, adicionar:
public const string IsSuperAdmin = "IsSuperAdmin";
```

---

## 12. Cache — `CacheKeyPrefix` atualizado

```csharp
// src/SportHub.Application/Common/Enums/CacheKeyPrefix.cs
namespace Application.Common.Enums;

public enum CacheKeyPrefix
{
    UserById,
    CourtAvailability,
    ReservationList,
    EstablishmentSummary,
    GetAvailability,
    TenantBySlug,      // NOVO: cache do tenant por slug (TTL: 1h)
    TenantBranding     // NOVO: cache do branding público (TTL: 5min)
}
```

---

## 13. Migrations — Estratégia Detalhada

### 13.1 Dois grupos de migrations

```
src/SportHub.Infrastructure/
└── Persistence/
    └── Migrations/
        ├── Global/          ← migrations do TenantDbContext (schema public)
        │   └── 20260303_AddTenants.cs
        └── Tenant/          ← migrations do ApplicationDbContext (schema tenant_*)
            ├── 20250719_InitialCreate.cs
            ├── ...
            └── [migrations existentes movidas aqui]
```

### 13.2 Como gerar migrations

```bash
# Migration do schema public (TenantDbContext)
dotnet ef migrations add AddTenants \
  --context TenantDbContext \
  --output-dir Persistence/Migrations/Global \
  --project src/SportHub.Infrastructure \
  --startup-project src/SportHub.Api

# Migration do schema de tenant (ApplicationDbContext)
# Usa o DesignTimeDbContextFactory que retorna schema "tenant_placeholder"
dotnet ef migrations add NomeDaMigration \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations/Tenant \
  --project src/SportHub.Infrastructure \
  --startup-project src/SportHub.Api
```

### 13.3 Como aplicar migrations

```csharp
// Em ServiceExtensions / Program.cs:

// Schema public (ao iniciar a aplicação, como hoje)
public static WebApplication ExecuteMigrations(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var tenantDb = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
    tenantDb.Database.Migrate();
    return app;
}

// Schema de tenant (durante provisioning — TenantProvisioningService)
await tenantDb.Database.MigrateAsync(ct);
// O schema já está setado via HasDefaultSchema no OnModelCreating
```

---

## 14. Configuração de Desenvolvimento Local

### 14.1 `/etc/hosts` para subdomínios locais

```
# Adicionar no /etc/hosts (sudo nano /etc/hosts)
127.0.0.1  testclub.localhost
127.0.0.1  academiasilva.localhost
```

### 14.2 Header fallback (sem editar `/etc/hosts`)

Em desenvolvimento, enviar o header `X-Tenant-Slug: testclub` com qualquer request para `localhost:5001`.

### 14.3 `appsettings.Development.json` sugerido

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sporthub;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "7f924c24e3ba48f3bb6c4cbe013998cd",
    "Issuer": "SportHub",
    "Audience": "SportHubUsers"
  },
  "AdminUser": {
    "Email": "superadmin@sporthub.app",
    "Password": "Admin@123",
    "FirstName": "Super",
    "LastName": "Admin"
  },
  "TenantResolution": {
    "BaseDomain": "localhost",
    "AllowHeaderFallback": true
  }
}
```

---

## 15. Estrutura Final de Arquivos

```
src/
├── SportHub.Domain/
│   ├── Entities/
│   │   ├── Tenant.cs                          ← NOVO
│   │   ├── User.cs
│   │   ├── Establishment.cs
│   │   ├── Court.cs
│   │   ├── Reservation.cs
│   │   ├── Sport.cs
│   │   └── EstablishmentUser.cs
│   └── Enums/
│       ├── UserRole.cs                        ← MODIFICADO (+ SuperAdmin = 99)
│       ├── TenantStatus.cs                    ← NOVO
│       └── EstablishmentRole.cs
│
├── SportHub.Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── ITenantContext.cs              ← NOVO
│   │   │   ├── ITenantRepository.cs           ← NOVO
│   │   │   ├── ICurrentUserService.cs
│   │   │   ├── ICacheService.cs
│   │   │   └── IBaseRepository.cs
│   │   └── Enums/
│   │       └── CacheKeyPrefix.cs              ← MODIFICADO (+ TenantBySlug, TenantBranding)
│   ├── Security/
│   │   ├── PolicyNames.cs                     ← MODIFICADO (+ IsSuperAdmin)
│   │   ├── SuperAdminRequirement.cs           ← NOVO
│   │   └── GlobalRoleRequirement.cs
│   └── UseCases/
│       └── Tenant/                            ← NOVO (pasta completa)
│           ├── ProvisionTenant/
│           │   ├── ProvisionTenantCommand.cs
│           │   ├── ProvisionTenantHandler.cs
│           │   ├── ProvisionTenantValidator.cs
│           │   └── ProvisionTenantResponse.cs
│           ├── GetTenant/
│           │   ├── GetTenantQuery.cs
│           │   ├── GetTenantHandler.cs
│           │   └── GetTenantResponse.cs
│           ├── GetAllTenants/
│           │   ├── GetAllTenantsQuery.cs
│           │   ├── GetAllTenantsHandler.cs
│           │   └── GetAllTenantsResponse.cs
│           ├── UpdateTenant/
│           │   ├── UpdateTenantCommand.cs
│           │   ├── UpdateTenantHandler.cs
│           │   └── UpdateTenantValidator.cs
│           ├── SuspendTenant/
│           │   ├── SuspendTenantCommand.cs
│           │   └── SuspendTenantHandler.cs
│           └── ActivateTenant/
│               ├── ActivateTenantCommand.cs
│               └── ActivateTenantHandler.cs
│
├── SportHub.Infrastructure/
│   ├── Middleware/
│   │   └── TenantResolutionMiddleware.cs      ← NOVO
│   ├── Persistence/
│   │   ├── AppDbContext.cs                    ← MODIFICADO (+ ITenantContext + HasDefaultSchema)
│   │   ├── TenantDbContext.cs                 ← NOVO
│   │   ├── DesignTimeDbContextFactory.cs      ← NOVO (só para CLI migrations)
│   │   ├── Configurations/
│   │   │   ├── TenantConfiguration.cs         ← NOVO
│   │   │   └── ... (existentes sem alteração)
│   │   └── Migrations/
│   │       ├── Global/                        ← NOVO (migrations do TenantDbContext)
│   │       └── Tenant/                        ← NOVO (migrations do ApplicationDbContext)
│   ├── Repositories/
│   │   ├── TenantRepository.cs                ← NOVO
│   │   └── ... (existentes sem alteração)
│   ├── Security/
│   │   ├── SuperAdminHandler.cs               ← NOVO
│   │   └── ... (existentes sem alteração)
│   └── Services/
│       ├── TenantContext.cs                   ← NOVO
│       ├── TenantProvisioningService.cs        ← NOVO
│       └── ... (existentes sem alteração)
│
└── SportHub.Api/
    ├── Endpoints/
    │   ├── TenantEndpoints.cs                 ← NOVO
    │   └── ... (existentes sem alteração)
    ├── Extensions/
    │   ├── ServiceExtensions.cs               ← MODIFICADO (+ DI Tenant)
    │   └── AppExtensions.cs                   ← MODIFICADO (+ MapTenantEndpoints)
    └── Program.cs                             ← MODIFICADO (+ UseMiddleware<TenantResolutionMiddleware>)
```

---

## 16. Ordem de Implementação

### Fase 1 — Domain + Contratos (sem tocar no que existe)
1. Criar `src/SportHub.Domain/Entities/Tenant.cs`
2. Criar `src/SportHub.Domain/Enums/TenantStatus.cs`
3. Modificar `src/SportHub.Domain/Enums/UserRole.cs` (adicionar `SuperAdmin = 99`)
4. Criar `src/SportHub.Application/Common/Interfaces/ITenantContext.cs`
5. Criar `src/SportHub.Application/Common/Interfaces/ITenantRepository.cs`
6. Modificar `src/SportHub.Application/Common/Enums/CacheKeyPrefix.cs` (+ `TenantBySlug`)
7. Modificar `src/SportHub.Application/Security/PolicyNames.cs` (+ `IsSuperAdmin`)
8. Criar `src/SportHub.Application/Security/SuperAdminRequirement.cs`

### Fase 2 — Persistência Global (TenantDbContext)
9. Criar `src/SportHub.Infrastructure/Persistence/TenantConfiguration.cs`
10. Criar `src/SportHub.Infrastructure/Persistence/TenantDbContext.cs`
11. Criar `src/SportHub.Infrastructure/Repositories/TenantRepository.cs`
12. Criar `src/SportHub.Infrastructure/Security/SuperAdminHandler.cs`
13. Gerar migration Global (`dotnet ef migrations add AddTenants --context TenantDbContext`)

### Fase 3 — Schema Dinâmico (AppDbContext)
14. Criar `src/SportHub.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
15. Modificar `src/SportHub.Infrastructure/Persistence/AppDbContext.cs` (+ `ITenantContext`)
16. Criar `src/SportHub.Infrastructure/Services/TenantContext.cs`
17. Mover migrations existentes para `Migrations/Tenant/` (ajustar namespaces)

### Fase 4 — Middleware
18. Criar `src/SportHub.Infrastructure/Middleware/TenantResolutionMiddleware.cs`
19. Modificar `src/SportHub.Api/Extensions/ServiceExtensions.cs` (registrar novos serviços)
20. Modificar `src/SportHub.Api/Program.cs` (registrar middleware)

### Fase 5 — Provisioning + Use Cases + Endpoints
21. Criar `src/SportHub.Infrastructure/Services/TenantProvisioningService.cs`
22. Criar todos os Use Cases CQRS em `src/SportHub.Application/UseCases/Tenant/`
23. Criar `src/SportHub.Api/Endpoints/TenantEndpoints.cs`
24. Modificar `src/SportHub.Api/Extensions/AppExtensions.cs` (+ MapTenantEndpoints)

### Fase 6 — Validação e Hardening
25. Testar criação de tenant via `POST /api/tenants`
26. Testar isolamento: criar user no tenant A, confirmar que não aparece no tenant B
27. Testar suspensão de tenant
28. Testar endpoint `/api/branding`
29. Atualizar `documentos/techspec-codebase.md` com seção Whitelabel

---

## 17. Checklist de Verificação

### Funcional
- [ ] `POST /api/tenants` com `{ slug: "testclub", name: "Test Club" }` cria schema `tenant_testclub` com todas as tabelas
- [ ] Request para `testclub.localhost/api/courts` (sem auth) retorna 401, não 404 (tenant foi resolvido)
- [ ] Request para `inexistente.localhost/api/courts` retorna 404
- [ ] Suspender tenant → requests retornam 403
- [ ] Reativar tenant → requests voltam ao normal
- [ ] `GET testclub.localhost/api/branding` retorna `{ name, logoUrl, primaryColor }` sem autenticação

### Isolamento de Dados
- [ ] Criar usuário no tenant A → não aparece em `SELECT * FROM tenant_b.users`
- [ ] Criar quadra no tenant A → não aparece no tenant B
- [ ] Reserva no tenant A não aparece no tenant B

### Cache
- [ ] Primeira request: cache miss → busca no banco
- [ ] Segunda request: cache hit (verificar via log ou Redis CLI)
- [ ] Atualizar branding → cache do tenant é invalidado
- [ ] Suspender tenant → cache é removido imediatamente

### Performance
- [ ] 100 requests simultâneas para o mesmo tenant → sem condição de corrida no schema
- [ ] Provisioning de tenant novo < 5 segundos

---

## 18. Gotchas e Armadilhas Conhecidas

| Situação | Problema | Solução |
|---|---|---|
| `HasDefaultSchema` em `OnModelCreating` | EF Core pode cachear o model (compilado) por tipo de DbContext | Garantir que `ApplicationDbContext` é Scoped (não Singleton) — já é o padrão do `AddDbContext` |
| Migrations geradas com schema hardcoded | `dotnet ef` usa o `DesignTimeDbContextFactory` que retorna `"tenant_placeholder"` | As migrations existentes ainda compilam, mas o schema real é definido em runtime pelo `HasDefaultSchema` |
| `EstablishmentUser` sem `IEntity` | Não pode usar `BaseRepository<EstablishmentUser>` | Continua igual — sem mudança necessária |
| `TenantDbContext` como Singleton | Problemas de concorrência com `SaveChangesAsync` | Para reads-heavy (o caso de Tenant), Singleton funciona. Para writes, garantir que `SaveChangesAsync` é chamado em scope thread-safe |
| Tenant suspenso no meio de uma request longa | Middleware já passou, tenant validado como Active | Aceitável — a verificação ocorre no início de cada request. Requests em andamento continuam |
| `SuperAdmin` no schema `public` ou no schema do tenant? | `SuperAdmin` é um usuário global, vive no schema `public` | Criar uma tabela `SuperAdmins` simples em `TenantDbContext`, separada dos `Users` por tenant |
| Slug com caracteres especiais no nome do schema | `"academia-silva"` → `tenant_academia_silva` (hífen vira underscore) | Método `GetSchemaName()` na entidade `Tenant` faz essa normalização |
| Desenvolvimento local sem subdomínio real | `localhost` não tem subdomínio | Usar `/etc/hosts` ou header `X-Tenant-Slug` como fallback |
