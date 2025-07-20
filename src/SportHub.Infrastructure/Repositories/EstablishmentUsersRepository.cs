using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence;

public class EstablishmentUsersRepository : IEstablishmentUsersRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EstablishmentUsersRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EstablishmentUser establishmentUser)
    {
        var user = await _dbContext.Users.FindAsync(establishmentUser.UserId);

        if (user is null)
            throw new InvalidOperationException("Usuário não encontrado.");
        
       

        // Agora o EF já está rastreando o usuário, e a FK será válida.
        await _dbContext.EstablishmentUsers.AddAsync(establishmentUser);
        await _dbContext.SaveChangesAsync();
    }

}
