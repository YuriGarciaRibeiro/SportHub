using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICourtsRepository : IBaseRepository<Court>
{
    Task<IEnumerable<Court>> GetByEstablishmentIdAsync(Guid establishmentId);
    Task<IEnumerable<Court>> GetByFilterAsync(CourtQueryFilter filter);
}

