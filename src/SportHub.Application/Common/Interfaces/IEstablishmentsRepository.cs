using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentsRepository : IBaseRepository<Establishment>
{
    public Task<List<Establishment>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids);
}
