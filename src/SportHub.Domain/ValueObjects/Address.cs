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

    /// <summary>
    /// Indica se o endereço possui coordenadas de localização
    /// </summary>
    public bool HasLocation => Location is not null;

    /// <summary>
    /// Latitude do endereço (se disponível)
    /// </summary>
    public double? Latitude => Location?.Y;

    /// <summary>
    /// Longitude do endereço (se disponível)
    /// </summary>
    public double? Longitude => Location?.X;

    /// <summary>
    /// Endereço completo formatado
    /// </summary>
    public string FullAddress => $"{Street}, {Number}" +
                                (string.IsNullOrEmpty(Complement) ? "" : $", {Complement}") +
                                $", {Neighborhood}, {City} - {State}, {ZipCode}";
}
