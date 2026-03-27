using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetByCustomDomainAsync(string domain, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<List<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Tenant>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        string? slug = null,
        TenantStatus? status = null,
        string? searchTerm = null,
        CancellationToken ct = default);

    Task<List<Guid>> GetAllPeakHoursEnabledAsync(CancellationToken ct = default);
}
