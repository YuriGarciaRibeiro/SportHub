namespace Application.UseCases.Reservations.CreateReservation;

public class CreateReservationResponse
{
    public Guid ReservationId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsPeakHours { get; set; }
    public int NormalSlots { get; set; }
    public int PeakSlots { get; set; }
    public decimal NormalSubtotal { get; set; }
    public decimal PeakSubtotal { get; set; }
    public decimal? NormalPricePerSlot { get; set; }
    public decimal? PeakPricePerSlot { get; set; }
}
