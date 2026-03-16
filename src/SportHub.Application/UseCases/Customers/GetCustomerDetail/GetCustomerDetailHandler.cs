using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Customers.GetCustomerDetail;

public class GetCustomerDetailHandler(IUsersRepository usersRepository, IReservationRepository reservationRepository)
    : IQueryHandler<GetCustomerDetailQuery, CustomerDetailDto>
{
    public async Task<Result<CustomerDetailDto>> Handle(GetCustomerDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.CustomerId);

        if (user is null || user.Role != UserRole.Customer)
            return Result.Fail(new NotFound($"Cliente com ID {request.CustomerId} não encontrado."));

        var metrics = await reservationRepository.GetMetricsByUserIdAsync(request.CustomerId, cancellationToken);
        var topCourts = await reservationRepository.GetTopCourtsByUserAsync(request.CustomerId, 3, cancellationToken);
        var recentReservations = await reservationRepository.GetPagedAsync(
            page: 1,
            pageSize: 20,
            userId: request.CustomerId);

        var favoriteCourts = topCourts.Select(c => new CourtFrequencyDto
        {
            CourtId = c.CourtId,
            CourtName = c.CourtName,
            Count = c.Count
        }).ToList();

        var reservationHistory = recentReservations.Items.Select(r => new CustomerReservationDto
        {
            Id = r.Id,
            CourtName = r.Court.Name,
            StartTimeUtc = r.StartTimeUtc,
            EndTimeUtc = r.EndTimeUtc,
            Amount = (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour
        }).ToList();

        var dto = new CustomerDetailDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            TotalReservations = metrics?.TotalReservations ?? 0,
            TotalSpent = metrics?.TotalSpent ?? 0m,
            LastReservationAt = metrics?.LastReservationAt,
            FavoriteCourts = favoriteCourts,
            RecentReservations = reservationHistory
        };

        return Result.Ok(dto);
    }
}
