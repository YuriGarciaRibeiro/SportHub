using Application.Common.Enums;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hubs;

/// <summary>
/// Hub SignalR para notificações de reservas em tempo real.
/// O front conecta em /hubs/reservations e escuta o evento "ReservationCreated".
///
/// Isolamento por tenant: ao conectar, o hub lê o X-Tenant-Slug do header,
/// resolve o schema e adiciona o cliente ao grupo correspondente automaticamente.
/// O front não precisa (e não pode) escolher o grupo — isso é feito no servidor.
/// </summary>
public class ReservationHub : Hub
{
    private readonly IServiceProvider _services;

    public ReservationHub(IServiceProvider services)
    {
        _services = services;
    }

    public override async Task OnConnectedAsync()
    {
        var schema = await ResolveSchemaAsync();

        if (schema is null)
        {
            Context.Abort();
            return;
        }

        // Salva o schema no contexto da conexão para uso posterior
        Context.Items["TenantSchema"] = schema;
        await Groups.AddToGroupAsync(Context.ConnectionId, schema);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("TenantSchema", out var schema) && schema is string tenantSchema)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantSchema);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<string?> ResolveSchemaAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is null) return null;

        // Ordem: subdomínio → query string ?tenant= → header X-Tenant-Slug
        // Query string é necessário porque WebSockets no browser não suportam headers customizados
        var host = httpContext.Request.Host.Host;
        var parts = host.Split('.');
        var slug = parts.Length >= 3
            ? parts[0].ToLowerInvariant()
            : httpContext.Request.Query.TryGetValue("tenant", out var q) && !string.IsNullOrWhiteSpace(q)
                ? q.ToString().ToLowerInvariant()
                : httpContext.Request.Headers.TryGetValue("X-Tenant-Slug", out var h)
                    ? h.ToString().ToLowerInvariant()
                    : null;

        if (string.IsNullOrWhiteSpace(slug)) return null;

        using var scope = _services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var cacheKey = cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, slug);

        var tenant = await cache.GetAsync<Domain.Entities.Tenant>(cacheKey);
        if (tenant is null)
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
            tenant = await repo.GetBySlugAsync(slug);
            if (tenant is not null)
                await cache.SetAsync(cacheKey, tenant, TimeSpan.FromHours(1));
        }

        return tenant?.Slug;
    }
}
