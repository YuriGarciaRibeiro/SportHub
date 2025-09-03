using Domain.ValueObjects;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Application.Common.Extensions;

namespace SportHub.Tests.Domain;

/// <summary>
/// Testes unitários para o Value Object Address
/// Estes testes verificam a criação e comportamento do objeto de valor Address
/// </summary>
public class AddressTests
{
    [Fact]
    public void Address_Should_Be_Created_With_Valid_Properties()
    {
        // Arrange
        var street = "Rua das Flores";
        var number = "123";
        var complement = "Apto 45";
        var neighborhood = "Centro";
        var city = "São Paulo";
        var state = "SP";
        var zipCode = "01234-567";

        // Act
        var address = new Address(street, number, complement, neighborhood, city, state, zipCode);

        // Assert
        address.Street.Should().Be(street);
        address.Number.Should().Be(number);
        address.Complement.Should().Be(complement);
        address.Neighborhood.Should().Be(neighborhood);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.ZipCode.Should().Be(zipCode);
        address.HasLocation.Should().BeFalse();
        address.Latitude.Should().BeNull();
        address.Longitude.Should().BeNull();
    }

    [Fact]
    public void Address_Should_Be_Created_With_Location()
    {
        // Arrange
        var street = "Rua das Flores";
        var number = "123";
        var complement = "Apto 45";
        var neighborhood = "Centro";
        var city = "São Paulo";
        var state = "SP";
        var zipCode = "01234-567";
        var latitude = -23.5505;
        var longitude = -46.6333;
        var location = GeometryExtensions.CreatePoint(latitude, longitude);

        // Act
        var address = new Address(street, number, complement, neighborhood, city, state, zipCode, location);

        // Assert
        address.Street.Should().Be(street);
        address.Number.Should().Be(number);
        address.Complement.Should().Be(complement);
        address.Neighborhood.Should().Be(neighborhood);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.ZipCode.Should().Be(zipCode);
        address.HasLocation.Should().BeTrue();
        address.Latitude.Should().BeApproximately(latitude, 0.0001);
        address.Longitude.Should().BeApproximately(longitude, 0.0001);
        address.Location.Should().NotBeNull();
    }

    [Fact]
    public void Address_Should_Be_Created_Without_Complement()
    {
        // Arrange
        var street = "Avenida Paulista";
        var number = "1578";
        string? complement = null;
        var neighborhood = "Bela Vista";
        var city = "São Paulo";
        var state = "SP";
        var zipCode = "01310-200";

        // Act
        var address = new Address(street, number, complement, neighborhood, city, state, zipCode);

        // Assert
        address.Street.Should().Be(street);
        address.Number.Should().Be(number);
        address.Complement.Should().BeNull();
        address.Neighborhood.Should().Be(neighborhood);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.ZipCode.Should().Be(zipCode);
    }
}
