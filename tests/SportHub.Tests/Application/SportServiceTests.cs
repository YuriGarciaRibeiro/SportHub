using Application.Common.Interfaces.Base;
using Application.Common.Interfaces.Sports;
using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace SportHub.Tests.Application;

/// <summary>
/// Testes unitários para o SportService
/// Estes testes verificam funcionalidades específicas do serviço de esportes
/// </summary>
public class SportServiceTests
{
    private readonly Mock<ISportsRepository> _mockSportsRepository;
    private readonly Mock<ICacheService> _mockCache;
    private readonly SportService _service;
    private readonly Sport _testSport;

    public SportServiceTests()
    {
        _mockSportsRepository = new Mock<ISportsRepository>();
        _mockCache = new Mock<ICacheService>();
        _service = new SportService(_mockSportsRepository.Object, _mockCache.Object);

        _testSport = new Sport
        {
            Id = Guid.NewGuid(),
            Name = "Football",
            Description = "Association football sport",
            ImageUrl = "https://test.com/football.jpg"
        };
    }

    [Fact]
    public async Task GetSportsByEstablishmentIdAsync_Should_Return_Sports_From_Repository()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var expectedSports = new List<SportSummaryDto>
        {
            new(Guid.NewGuid(), "Football", "Association football"),
            new(Guid.NewGuid(), "Basketball", "Team sport with ball")
        };

        _mockSportsRepository
            .Setup(x => x.GetByEstablishmentIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSports);

        // Act
        var result = await _service.GetSportsByEstablishmentIdAsync(establishmentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedSports);
        _mockSportsRepository.Verify(
            x => x.GetByEstablishmentIdAsync(establishmentId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetSportByNameAsync_Should_Return_Sport_When_Found()
    {
        // Arrange
        var sportName = "Football";
        _mockSportsRepository
            .Setup(x => x.GetByNameAsync(sportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testSport);

        // Act
        var result = await _service.GetSportByNameAsync(sportName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testSport.Id);
        result.Name.Should().Be(_testSport.Name);
        result.Description.Should().Be(_testSport.Description);
        _mockSportsRepository.Verify(
            x => x.GetByNameAsync(sportName, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetSportByNameAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var sportName = "NonExistentSport";
        _mockSportsRepository
            .Setup(x => x.GetByNameAsync(sportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sport?)null);

        // Act
        var result = await _service.GetSportByNameAsync(sportName);

        // Assert
        result.Should().BeNull();
        _mockSportsRepository.Verify(
            x => x.GetByNameAsync(sportName, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Theory]
    [InlineData("football")]
    [InlineData("FOOTBALL")]
    [InlineData("Football")]
    [InlineData("fOoTbAlL")]
    public async Task GetSportByNameAsync_Should_Handle_Different_Case_Variations(string sportName)
    {
        // Arrange
        _mockSportsRepository
            .Setup(x => x.GetByNameAsync(sportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testSport);

        // Act
        var result = await _service.GetSportByNameAsync(sportName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(_testSport.Name);
        _mockSportsRepository.Verify(
            x => x.GetByNameAsync(sportName, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetSportsByEstablishmentIdAsync_Should_Return_Empty_List_When_No_Sports_Found()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var emptySportsList = new List<SportSummaryDto>();

        _mockSportsRepository
            .Setup(x => x.GetByEstablishmentIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptySportsList);

        // Act
        var result = await _service.GetSportsByEstablishmentIdAsync(establishmentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockSportsRepository.Verify(
            x => x.GetByEstablishmentIdAsync(establishmentId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public void Service_Should_Have_Correct_Cache_TTL()
    {
        // Arrange & Act
        var service = new SportService(_mockSportsRepository.Object, _mockCache.Object);

        // Assert
        // Usando reflection para verificar o TTL padrão (proteção contra mudanças futuras)
        var defaultTtlProperty = typeof(SportService)
            .GetProperty("DefaultTtl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        defaultTtlProperty.Should().NotBeNull();
        var ttlValue = (TimeSpan)defaultTtlProperty!.GetValue(service)!;
        ttlValue.Should().Be(TimeSpan.FromMinutes(60), 
            "Sports don't change frequently so should have longer cache TTL");
    }

    [Fact]
    public async Task GetSportsByEstablishmentIdAsync_Should_Handle_Large_Number_Of_Sports()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var largeSportsList = new List<SportSummaryDto>();
        
        // Criar uma lista grande de esportes
        for (int i = 0; i < 100; i++)
        {
            largeSportsList.Add(new SportSummaryDto(
                Guid.NewGuid(), 
                $"Sport {i}", 
                $"Description for sport {i}"));
        }

        _mockSportsRepository
            .Setup(x => x.GetByEstablishmentIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(largeSportsList);

        // Act
        var result = await _service.GetSportsByEstablishmentIdAsync(establishmentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(100);
        result.Should().BeEquivalentTo(largeSportsList);
        _mockSportsRepository.Verify(
            x => x.GetByEstablishmentIdAsync(establishmentId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
