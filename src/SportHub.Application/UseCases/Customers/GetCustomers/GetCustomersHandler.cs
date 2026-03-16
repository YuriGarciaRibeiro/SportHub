using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Customers.GetCustomers;

public class GetCustomersHandler(IUsersRepository usersRepository, IReservationRepository reservationRepository)
    : IQueryHandler<GetCustomersQuery, PagedResult<CustomerSummaryDto>>
{
    public async Task<Result<PagedResult<CustomerSummaryDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var pagedUsers = await usersRepository.GetPagedAsync(
            page: filter.Page ?? 1,
            pageSize: filter.PageSize ?? 10,
            isActive: filter.IsActive,
            searchTerm: filter.SearchTerm,
            allowedRoles: [UserRole.Customer]);

        var userIds = pagedUsers.Items.Select(u => u.Id).ToList();

        var metrics = await reservationRepository.GetMetricsByUserIdsAsync(userIds, cancellationToken);
        var metricsMap = metrics.ToDictionary(m => m.UserId);

        var items = pagedUsers.Items.Select(u =>
        {
            metricsMap.TryGetValue(u.Id, out var m);
            return new CustomerSummaryDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                IsActive = u.IsActive,
                TotalReservations = m?.TotalReservations ?? 0,
                TotalSpent = m?.TotalSpent ?? 0m,
                LastReservationAt = m?.LastReservationAt,
                CreatedAt = u.CreatedAt
            };
        }).ToList();

        return Result.Ok(new PagedResult<CustomerSummaryDto>
        {
            Items = items,
            TotalCount = pagedUsers.TotalCount,
            Page = pagedUsers.Page,
            PageSize = pagedUsers.PageSize
        });
    }
}
