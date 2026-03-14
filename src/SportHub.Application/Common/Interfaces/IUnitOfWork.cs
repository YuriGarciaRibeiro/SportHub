namespace Application.Common.Interfaces;

/// <summary>
/// Abstração do padrão Unit of Work.
/// Commita todas as mudanças pendentes no DbContext de uma vez.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todas as mudanças pendentes no banco de dados.
    /// Deve ser chamado uma vez no final do handler/use case.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
