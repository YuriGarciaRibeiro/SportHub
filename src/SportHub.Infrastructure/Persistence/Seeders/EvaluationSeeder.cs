using Application.Common.Interfaces.Evaluations;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public class EvaluationSeeder : BaseSeeder
{
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly ApplicationDbContext _dbContext;

    public EvaluationSeeder(
        IEvaluationRepository evaluationRepository,
        ApplicationDbContext dbContext,
        ILogger<EvaluationSeeder> logger) : base(logger)
    {
        _evaluationRepository = evaluationRepository;
        _dbContext = dbContext;
    }

    public override int Order => 6; // Após Establishments, Courts, Users e Reservations

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting evaluations seeding...");

        // Verificar se já existem avaliações
        var existingEvaluations = await _dbContext.Evaluations.AnyAsync(cancellationToken);
        if (existingEvaluations)
        {
            LogInfo("Evaluations already exist, skipping seeding.");
            return;
        }

        // Buscar estabelecimentos e usuários existentes
        var establishments = await _dbContext.Establishments.Take(3).ToListAsync(cancellationToken);
        var users = await _dbContext.Users.Where(u => u.Role == UserRole.User).Take(6).ToListAsync(cancellationToken);

        if (!establishments.Any() || !users.Any())
        {
            LogWarning("No establishments or users found. Cannot seed evaluations.");
            return;
        }

        var evaluations = GetTestEvaluations(establishments, users);

        foreach (var evaluation in evaluations)
        {
            await _evaluationRepository.AddAsync(evaluation, cancellationToken);
            LogInfo($"Created evaluation: {evaluation.Rating}⭐ for establishment by user {evaluation.UserId}");
        }

        LogInfo($"Evaluations seeding completed. Created {evaluations.Count} evaluations.");
    }

    private List<Evaluation> GetTestEvaluations(List<Establishment> establishments, List<User> users)
    {
        var evaluations = new List<Evaluation>();
        var random = new Random(42); // Seed fixo para resultados consistentes

        // Para cada estabelecimento, criar várias avaliações de diferentes usuários
        foreach (var establishment in establishments)
        {
            // Número aleatório de avaliações por estabelecimento (3-6)
            var numberOfEvaluations = random.Next(3, 7);
            var selectedUsers = users.OrderBy(x => random.Next()).Take(numberOfEvaluations).ToList();

            foreach (var user in selectedUsers)
            {
                var rating = GenerateRealisticRating(random, establishment.Name);
                var comment = GenerateComment(rating, establishment.Name, random);

                evaluations.Add(new Evaluation
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TargetType = EvaluationTargetType.Establishment,
                    TargetId = establishment.Id,
                    Rating = rating,
                    Comment = comment
                });
            }
        }

        return evaluations;
    }

    private int GenerateRealisticRating(Random random, string establishmentName)
    {
        // Simular diferentes perfis de estabelecimentos baseado no nome
        if (establishmentName.Contains("Premium") || establishmentName.Contains("Elite"))
        {
            // Estabelecimentos premium tendem a ter ratings melhores
            return random.Next(1, 101) switch
            {
                <= 10 => 3,
                <= 25 => 4,
                _ => 5
            };
        }
        else if (establishmentName.Contains("Centro") || establishmentName.Contains("Clube"))
        {
            // Estabelecimentos médios
            return random.Next(1, 101) switch
            {
                <= 5 => 2,
                <= 15 => 3,
                <= 40 => 4,
                _ => 5
            };
        }
        else
        {
            // Distribuição geral mais realista
            return random.Next(1, 101) switch
            {
                <= 3 => 1,
                <= 8 => 2,
                <= 20 => 3,
                <= 50 => 4,
                _ => 5
            };
        }
    }

    private string? GenerateComment(int rating, string establishmentName, Random random)
    {
        // Nem todas as avaliações têm comentários
        if (random.Next(1, 101) <= 30) return null; // 30% sem comentários

        var positiveComments = new[]
        {
            "Excelente estabelecimento! Instalações modernas e bem cuidadas.",
            "Ótima experiência! Staff muito atencioso e quadras em perfeito estado.",
            "Lugar incrível para praticar esportes. Recomendo!",
            "Instalações top! Voltarei com certeza.",
            "Ambiente limpo e organizado. Adorei!",
            "Profissionais qualificados e estrutura de primeira.",
            "Melhor lugar da região para praticar esportes!",
            "Quadras excelentes e preço justo."
        };

        var neutralComments = new[]
        {
            "Lugar ok, atende às necessidades básicas.",
            "Instalações razoáveis, nada excepcional.",
            "Cumpre o que promete, sem grandes surpresas.",
            "Ambiente agradável, mas pode melhorar.",
            "Boa localização, mas instalações podem ser modernizadas."
        };

        var negativeComments = new[]
        {
            "Instalações precisam de manutenção.",
            "Atendimento deixa a desejar.",
            "Quadras em estado regular, precisa de reformas.",
            "Preço um pouco salgado para o que oferece.",
            "Poderia estar mais limpo e organizado."
        };

        return rating switch
        {
            5 => positiveComments[random.Next(positiveComments.Length)],
            4 => random.Next(1, 101) <= 70 
                ? positiveComments[random.Next(positiveComments.Length)]
                : neutralComments[random.Next(neutralComments.Length)],
            3 => neutralComments[random.Next(neutralComments.Length)],
            2 => random.Next(1, 101) <= 70
                ? negativeComments[random.Next(negativeComments.Length)]
                : neutralComments[random.Next(neutralComments.Length)],
            1 => negativeComments[random.Next(negativeComments.Length)],
            _ => null
        };
    }
}
