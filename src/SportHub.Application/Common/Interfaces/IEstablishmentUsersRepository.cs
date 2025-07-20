using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentUsersRepository
{
    Task AddAsync(EstablishmentUser establishmentUser);
}
