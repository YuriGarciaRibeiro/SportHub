using Domain.Entities;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public class ReservationSeeder : BaseSeeder
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationService _reservationService;
    private readonly ApplicationDbContext _dbContext;

    public ReservationSeeder(
        IReservationRepository reservationRepository,
        IReservationService reservationService,
        ApplicationDbContext dbContext,
        ILogger<ReservationSeeder> logger) : base(logger)
    {
        _reservationRepository = reservationRepository;
        _reservationService = reservationService;
        _dbContext = dbContext;
    }

    public override int Order => 5; // Fifth to be executed (after Courts)

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting reservations seeding...");

        // Verificar se já existem reservas
        var existingReservations = await _dbContext.Reservations.AnyAsync(cancellationToken);
        if (existingReservations)
        {
            LogInfo("Reservations already exist, skipping seeding.");
            return;
        }

        // Buscar todos os usuários e quadras existentes
        var users = await _dbContext.Users.ToListAsync(cancellationToken);
        var courts = await _dbContext.Courts.ToListAsync(cancellationToken);

        if (!users.Any() || !courts.Any())
        {
            LogWarning("No users or courts found. Cannot seed reservations.");
            return;
        }

        var reservations = GenerateReservationsForAllUsers(users, courts);

        foreach (var reservation in reservations)
        {
            await _reservationRepository.AddAsync(reservation, cancellationToken);
            LogInfo($"Created reservation for court {reservation.CourtId} by user {reservation.UserId}");
        }

        LogInfo($"Reservations seeding completed. Created {reservations.Count} reservations.");
    }

    private List<Reservation> GenerateReservationsForAllUsers(List<User> users, List<Court> courts)
    {
        var reservations = new List<Reservation>();
        var random = new Random(42); // Seed fixo para resultados consistentes
        var baseDate = DateTime.UtcNow;

        // Criar reservas para cada usuário
        foreach (var user in users)
        {
            // Cada usuário terá entre 1 a 4 reservas
            var numberOfReservations = random.Next(1, 5);
            
            for (int i = 0; i < numberOfReservations; i++)
            {
                // Selecionar uma quadra aleatória
                var court = courts[random.Next(courts.Count)];
                
                // Gerar uma data aleatória (mix de passado, presente e futuro)
                var daysOffset = random.Next(-10, 15); // 10 dias no passado até 15 dias no futuro
                var hour = random.Next(8, 22); // Entre 8h e 22h
                var startTime = baseDate.AddDays(daysOffset).Date.AddHours(hour);
                
                // Duração aleatória (1-3 horas)
                var duration = random.Next(1, 4);
                var endTime = startTime.AddHours(duration);
                
                // Preço baseado na duração e tipo de quadra (simulado)
                var pricePerHour = random.Next(10, 50);
                var totalPrice = pricePerHour * duration;
                
                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    CourtId = court.Id,
                    UserId = user.Id,
                    StartTimeUtc = startTime.ToUniversalTime(),
                    EndTimeUtc = endTime.ToUniversalTime(),
                    SlotsBooked = duration * 2, // Assumindo slots de 30 minutos
                    TotalPrice = totalPrice
                };
                
                reservations.Add(reservation);
            }
        }
        
        return reservations;
    }
}
