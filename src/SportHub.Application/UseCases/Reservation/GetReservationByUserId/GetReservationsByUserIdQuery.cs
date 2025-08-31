namespace SportHub.Application.UseCases.Reservation.GetReservationByUserId;

public class GetReservationsByUserIdQuery : IQuery<GetReservationsByUserIdResponse>
{
    public Guid UserId { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
