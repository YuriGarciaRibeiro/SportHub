using NetTopologySuite.Geometries;

namespace Domain.ValueObjects;

public class Address
{
    public Address(string street, string number, string? complement, string neighborhood, string city, string state, string zipCode, Point? location = null)
    {
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Location = location;
    }

    public string Street { get; set; } = null!;
    public string Number { get; set; } = null!;
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public Point? Location { get; set; }
    public bool HasLocation => Location is not null;
    public double? Latitude => Location?.Y;
    public double? Longitude => Location?.X;
    public string FullAddress => $"{Street}, {Number}" +
                                (string.IsNullOrEmpty(Complement) ? "" : $", {Complement}") +
                                $", {Neighborhood}, {City} - {State}, {ZipCode}";
}
