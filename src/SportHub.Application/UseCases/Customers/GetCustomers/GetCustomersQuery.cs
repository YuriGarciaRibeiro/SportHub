using Application.Common.Models;
using Application.CQRS;

namespace Application.UseCases.Customers.GetCustomers;

public record GetCustomersQuery(GetCustomersFilter Filter) : IQuery<PagedResult<CustomerSummaryDto>>;

public class GetCustomersFilter
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public int TotalReservations { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastReservationAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
