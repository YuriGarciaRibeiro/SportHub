using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

/// <summary>
/// Garante que o EF Core compile um modelo separado por schema de tenant.
/// Com HasDefaultSchema, cada schema gera SQL diferente, então precisamos
/// de uma chave de cache por schema.
/// </summary>
public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var schema = (context as ApplicationDbContext)?.CurrentSchema ?? "public";
        return new TenantModelCacheKey(context.GetType(), schema, designTime);
    }
}

/// <summary>
/// Chave de cache que inclui o schema do tenant para garantir que modelos
/// de schemas diferentes não compartilhem cache.
/// </summary>
internal sealed class TenantModelCacheKey : IEquatable<TenantModelCacheKey>
{
    private readonly Type _contextType;
    private readonly string _schema;
    private readonly bool _designTime;

    public TenantModelCacheKey(Type contextType, string schema, bool designTime)
    {
        _contextType = contextType;
        _schema = schema;
        _designTime = designTime;
    }

    public bool Equals(TenantModelCacheKey? other)
    {
        if (other is null) return false;
        return _contextType == other._contextType
            && _schema == other._schema
            && _designTime == other._designTime;
    }

    public override bool Equals(object? obj) => obj is TenantModelCacheKey other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_contextType, _schema, _designTime);
}
