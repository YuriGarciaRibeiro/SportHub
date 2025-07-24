using Domain.Entities;

public interface ICourtsRepository
{
    Task<Court?> GetByIdAsync(Guid id);
    Task CreateAsync(Court court);
    Task UpdateAsync(Court court);
    Task DeleteAsync(Guid id);
}
