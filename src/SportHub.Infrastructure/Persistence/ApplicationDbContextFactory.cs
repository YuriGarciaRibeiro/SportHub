using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

/// <summary>
/// Factory para criar ApplicationDbContext fora do pipeline HTTP (migrations, provisioning, seeds).
/// Centraliza a configuração que antes estava espalhada em ServiceExtensions e TenantProvisioningService.
/// </summary>
public class ApplicationDbContextFactory
{
    private readonly string _connectionString;

    public ApplicationDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Cria um ApplicationDbContext configurado para um schema específico.
    /// Útil para migrations e provisioning de tenants.
    /// </summary>
    /// <param name="schema">Nome do schema PostgreSQL (ex: "tenant_arena1" ou "public")</param>
    /// <param name="migrationsHistorySchema">Schema onde a tabela __EFMigrationsHistory será criada. Se null, usa o schema padrão.</param>
    public ApplicationDbContext Create(string schema, string? migrationsHistorySchema = null)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(_connectionString)
        {
            SearchPath = schema
        };

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(builder.ConnectionString, npgsql =>
        {
            npgsql.MigrationsAssembly("SportHub.Infrastructure");
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", migrationsHistorySchema ?? schema);
        });
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        var tenantContext = new FactoryTenantContext(schema);
        var currentUserService = new FactoryCurrentUserService();
        var timeProvider = TimeProvider.System;

        return new ApplicationDbContext(optionsBuilder.Options, currentUserService, tenantContext, timeProvider);
    }

    /// <summary>
    /// Cria um ApplicationDbContext configurado para um tenant específico.
    /// </summary>
    public ApplicationDbContext CreateForTenant(Tenant tenant)
    {
        var schema = tenant.GetSchemaName();
        return Create(schema, schema);
    }

    /// <summary>
    /// Cria um ApplicationDbContext configurado para o schema public.
    /// </summary>
    public ApplicationDbContext CreateForPublic()
    {
        return Create("public", "public");
    }

    // --- Classes internas (substituem as 5+ classes mock espalhadas) ---

    private sealed class FactoryTenantContext : ITenantContext
    {
        public FactoryTenantContext(string schema) => Schema = schema;

        public Guid TenantId => Guid.Empty;
        public string TenantSlug => Schema;
        public string TenantName => Schema;
        public string? LogoUrl => null;
        public string? PrimaryColor => null;
        public string Schema { get; }
        public bool IsResolved => true;
        public void Resolve(Tenant tenant) { }
    }

    private sealed class FactoryCurrentUserService : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;
        public Domain.Enums.UserRole? UserRole => null;
    }
}
