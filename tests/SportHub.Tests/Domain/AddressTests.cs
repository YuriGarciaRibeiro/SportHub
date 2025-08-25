using Domain.ValueObjects;
using FluentAssertions;

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
