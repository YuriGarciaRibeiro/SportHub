namespace Application.Common.QueryFilters;

public class EstablishmentReservationsQueryFilter
{
    public Guid? UserId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Guid? CourtId { get; set; }
}
