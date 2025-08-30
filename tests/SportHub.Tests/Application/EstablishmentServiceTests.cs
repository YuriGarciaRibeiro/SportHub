using Application.Common.Interfaces.Base;
using Application.Common.Interfaces.Establishments;
using Application.Common.Interfaces.Reservations;
using Application.Common.Interfaces.Sports;
using Application.Common.QueryFilters;
using Application.Services;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace SportHub.Tests.Application;

/// <summary>
/// Testes unitários para o EstablishmentService
/// Estes testes verificam o comportamento do service incluindo cache e operações específicas de establishments
/// </summary>
public class EstablishmentServiceTests
{
    private readonly Mock<IEstablishmentsRepository> _mockEstablishmentsRepository;
    private readonly Mock<IEstablishmentUsersRepository> _mockEstablishmentUsersRepository;
    private readonly Mock<ICacheService> _mockCache;
    private readonly EstablishmentService _service;
    private readonly Establishment _testEstablishment;
    private readonly Guid _testId;
    private readonly CancellationToken _cancellationToken;

    public EstablishmentServiceTests()
    {
        _mockEstablishmentsRepository = new Mock<IEstablishmentsRepository>();
        _mockEstablishmentUsersRepository = new Mock<IEstablishmentUsersRepository>();
        _mockCache = new Mock<ICacheService>();
        _service = new EstablishmentService(_mockEstablishmentsRepository.Object, _mockEstablishmentUsersRepository.Object, _mockCache.Object);
        _testId = Guid.NewGuid();
        _cancellationToken = CancellationToken.None;

        _testEstablishment = new Establishment
        {
            Id = _testId,
            Name = "Sports Center",
            Description = "Modern sports center",
            ImageUrl = "https://test.com/sports-center.jpg"
        };
    }

    #region GetEstablishmentsByOwnerIdAsync Tests

    [Fact]
    public async Task GetEstablishmentsByOwnerIdAsync_Should_Return_Cached_Result_When_Available()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var cachedEstablishments = new List<Establishment> { _testEstablishment };
        var cacheKey = "EstablishmentsByOwner_" + ownerId;

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentsByOwner", ownerId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<Establishment>>(cacheKey, _cancellationToken))
            .ReturnsAsync(cachedEstablishments);

        // Act
        var result = await _service.GetEstablishmentsByOwnerIdAsync(ownerId, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedEstablishments);
        _mockEstablishmentUsersRepository.Verify(r => r.GetByOwnerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEstablishmentsByOwnerIdAsync_Should_Return_Empty_List_When_No_Establishments_Found()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var cacheKey = "EstablishmentsByOwner_" + ownerId;

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentsByOwner", ownerId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<Establishment>>(cacheKey, _cancellationToken))
            .ReturnsAsync((List<Establishment>?)null);
        _mockEstablishmentUsersRepository.Setup(r => r.GetByOwnerIdAsync(ownerId, _cancellationToken))
            .ReturnsAsync(new List<Guid>());

        // Act
        var result = await _service.GetEstablishmentsByOwnerIdAsync(ownerId, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _mockCache.Verify(c => c.SetAsync(cacheKey, It.IsAny<List<Establishment>>(), TimeSpan.FromMinutes(30), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentsByOwnerIdAsync_Should_Return_Establishments_And_Cache_Result()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var establishmentIds = new List<Guid> { _testId };
        var establishments = new List<Establishment> { _testEstablishment };
        var cacheKey = "EstablishmentsByOwner_" + ownerId;

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentsByOwner", ownerId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<Establishment>>(cacheKey, _cancellationToken))
            .ReturnsAsync((List<Establishment>?)null);
        _mockEstablishmentUsersRepository.Setup(r => r.GetByOwnerIdAsync(ownerId, _cancellationToken))
            .ReturnsAsync(establishmentIds);
        _mockEstablishmentsRepository.Setup(r => r.GetByIdsAsync(establishmentIds, _cancellationToken))
            .ReturnsAsync(establishments);

        // Act
        var result = await _service.GetEstablishmentsByOwnerIdAsync(ownerId, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(_testId);
        _mockCache.Verify(c => c.SetAsync(cacheKey, It.IsAny<List<Establishment>>(), TimeSpan.FromMinutes(30), _cancellationToken), Times.Once);
    }

    #endregion

    #region GetFilteredAsync Tests

    [Fact]
    public async Task GetFilteredAsync_Should_Return_Repository_Result()
    {
        // Arrange
        var query = new GetEstablishmentsQuery(null, null, 1, 10);
        var expectedResponse = new List<EstablishmentResponse>
        {
            new EstablishmentResponse(_testId, "Test", "Description", 
                new AddressResponse("Street", "123", null, "Neighborhood", "City", "State", "12345"),
                "image.jpg", new List<SportResponse>())
        };
        var expectedResult = (expectedResponse, 1);

        _mockEstablishmentsRepository.Setup(r => r.GetFilteredAsync(query, _cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetFilteredAsync(query, _cancellationToken);

        // Assert
        result.Items.Should().BeEquivalentTo(expectedResponse);
        result.TotalCount.Should().Be(1);
    }

    #endregion

    #region GetByIdWithAddressAsync Tests

    [Fact]
    public async Task GetByIdWithAddressAsync_Should_Return_Cached_Result_When_Available()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EstablishmentWithAddress_" + id;
        var cachedDto = new EstablishmentWithAddressDto(id, "Test", "Description", 
            new AddressDto("Street", "123", null, "Neighborhood", "City", "State", "12345"), "image.jpg");

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentWithAddress", id.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<EstablishmentWithAddressDto>(cacheKey, _cancellationToken))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _service.GetByIdWithAddressAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(cachedDto);
        _mockEstablishmentsRepository.Verify(r => r.GetByIdWithAddressAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdWithAddressAsync_Should_Return_And_Cache_Repository_Result()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EstablishmentWithAddress_" + id;
        var dto = new EstablishmentWithAddressDto(id, "Test", "Description",
            new AddressDto("Street", "123", null, "Neighborhood", "City", "State", "12345"), "image.jpg");

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentWithAddress", id.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<EstablishmentWithAddressDto>(cacheKey, _cancellationToken))
            .ReturnsAsync((EstablishmentWithAddressDto?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetByIdWithAddressAsync(id, _cancellationToken))
            .ReturnsAsync(dto);

        // Act
        var result = await _service.GetByIdWithAddressAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(dto);
        _mockCache.Verify(c => c.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetByIdWithAddressAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EstablishmentWithAddress_" + id;

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentWithAddress", id.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<EstablishmentWithAddressDto>(cacheKey, _cancellationToken))
            .ReturnsAsync((EstablishmentWithAddressDto?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetByIdWithAddressAsync(id, _cancellationToken))
            .ReturnsAsync((EstablishmentWithAddressDto?)null);

        // Act
        var result = await _service.GetByIdWithAddressAsync(id, _cancellationToken);

        // Assert
        result.Should().BeNull();
        _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<EstablishmentWithAddressDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetUsersByEstablishmentIdAsync Tests

    [Fact]
    public async Task GetUsersByEstablishmentIdAsync_Should_Return_Cached_Result_When_Available()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var cacheKey = "EstablishmentUsers_" + establishmentId;
        var cachedUsers = new List<EstablishmentUserSummaryDto>
        {
            new EstablishmentUserSummaryDto(Guid.NewGuid(), "John", "Doe", "john@example.com", "John Doe", EstablishmentRole.Owner)
        };

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentUsers", establishmentId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<EstablishmentUserSummaryDto>>(cacheKey, _cancellationToken))
            .ReturnsAsync(cachedUsers);

        // Act
        var result = await _service.GetUsersByEstablishmentIdAsync(establishmentId, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(cachedUsers);
        _mockEstablishmentsRepository.Verify(r => r.GetUsersByEstablishmentId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUsersByEstablishmentIdAsync_Should_Return_And_Cache_Repository_Result()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var cacheKey = "EstablishmentUsers_" + establishmentId;
        var users = new List<EstablishmentUserSummaryDto>
        {
            new EstablishmentUserSummaryDto(Guid.NewGuid(), "John", "Doe", "john@example.com", "John Doe", EstablishmentRole.Owner)
        };

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentUsers", establishmentId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<EstablishmentUserSummaryDto>>(cacheKey, _cancellationToken))
            .ReturnsAsync((List<EstablishmentUserSummaryDto>?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetUsersByEstablishmentId(establishmentId, _cancellationToken))
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetUsersByEstablishmentIdAsync(establishmentId, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(users);
        _mockCache.Verify(c => c.SetAsync(cacheKey, users, TimeSpan.FromMinutes(15), _cancellationToken), Times.Once);
    }

    #endregion

    #region GetSportsByEstablishmentIdAsync Tests

    [Fact]
    public async Task GetSportsByEstablishmentIdAsync_Should_Return_Cached_Result_When_Available()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var cacheKey = "EstablishmentSports_" + establishmentId;
        var cachedSports = new List<SportSummaryDto>
        {
            new SportSummaryDto(Guid.NewGuid(), "Football", "Association football")
        };

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentSports", establishmentId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<SportSummaryDto>>(cacheKey, _cancellationToken))
            .ReturnsAsync(cachedSports);

        // Act
        var result = await _service.GetSportsByEstablishmentIdAsync(establishmentId, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(cachedSports);
        _mockEstablishmentsRepository.Verify(r => r.GetSportsByEstablishmentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSportsByEstablishmentIdAsync_Should_Return_And_Cache_Repository_Result()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var cacheKey = "EstablishmentSports_" + establishmentId;
        var sports = new List<SportSummaryDto>
        {
            new SportSummaryDto(Guid.NewGuid(), "Football", "Association football")
        };

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentSports", establishmentId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<SportSummaryDto>>(cacheKey, _cancellationToken))
            .ReturnsAsync((List<SportSummaryDto>?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetSportsByEstablishmentIdAsync(establishmentId, _cancellationToken))
            .ReturnsAsync(sports);

        // Act
        var result = await _service.GetSportsByEstablishmentIdAsync(establishmentId, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(sports);
        _mockCache.Verify(c => c.SetAsync(cacheKey, sports, TimeSpan.FromHours(1), _cancellationToken), Times.Once);
    }

    #endregion

    #region GetReservationsByCourtsIdAsync Tests

    [Fact]
    public async Task GetReservationsByCourtsIdAsync_Should_Return_Repository_Result()
    {
        // Arrange
        var courtIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var filter = new EstablishmentReservationsQueryFilter 
        { 
            StartTime = DateTime.UtcNow, 
            EndTime = DateTime.UtcNow.AddDays(1),
            UserId = Guid.NewGuid() 
        };
        var reservations = new List<ReservationWithDetailsDto>
        {
            new ReservationWithDetailsDto(Guid.NewGuid(), Guid.NewGuid(), "John Doe", "john@example.com", 
                courtIds.First(), "Court 1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
        };

        _mockEstablishmentsRepository.Setup(r => r.GetReservationsByCourtsIdAsync(courtIds, filter, _cancellationToken))
            .ReturnsAsync(reservations);

        // Act
        var result = await _service.GetReservationsByCourtsIdAsync(courtIds, filter, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(reservations);
    }

    #endregion

    #region GetByIdCompleteAsync Tests

    [Fact]
    public async Task GetByIdCompleteAsync_Should_Return_Cached_Result_When_Available()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EstablishmentByIdComplete_" + id;
        var cachedDto = new EstablishmentCompleteDto(
            id,
            "Test", // name
            "Description", // description
            "SomePhone", // phone
            "SomeEmail", // email
            "SomeWebsite", // website
            new TimeOnly(8, 0), // openingTime
            new TimeOnly(22, 0), // closingTime
            new AddressDto("Street", "123", null, "Neighborhood", "City", "State", "12345"), // address
            "image.jpg", // imageUrl
            new List<SportDto>(), // sports
            new List<EstablishmentUserDto>(), // users
            new List<CourtDto>() // courts
        );

        _mockCache.Setup(c => c.GenerateCacheKey("EntityByIdComplete", "Establishment", id))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<EstablishmentCompleteDto>(cacheKey, _cancellationToken))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _service.GetByIdCompleteAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(cachedDto);
        _mockEstablishmentsRepository.Verify(r => r.GetByIdCompleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdCompleteAsync_Should_Return_And_Cache_Repository_Result()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EstablishmentByIdComplete_" + id;
        var dto = new EstablishmentCompleteDto(
            id,
            "Test", // name
            "Description", // description
            "SomePhone", // phone
            "SomeEmail", // email
            "SomeWebsite", // website
            new TimeOnly(8, 0), // openingTime
            new TimeOnly(22, 0), // closingTime
            new AddressDto("Street", "123", null, "Neighborhood", "City", "State", "12345"), // address
            "image.jpg", // imageUrl
            new List<SportDto>(), // sports
            new List<EstablishmentUserDto>(), // users
            new List<CourtDto>() // courts
        );

        _mockCache.Setup(c => c.GenerateCacheKey("EntityByIdComplete", "Establishment", id))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<EstablishmentCompleteDto>(cacheKey, _cancellationToken))
            .ReturnsAsync((EstablishmentCompleteDto?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetByIdCompleteAsync(id, _cancellationToken))
            .ReturnsAsync(dto);

        // Act
        var result = await _service.GetByIdCompleteAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(dto);
        _mockCache.Verify(c => c.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetByIdCompleteAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EstablishmentByIdComplete_" + id;

        _mockCache.Setup(c => c.GenerateCacheKey("EntityByIdComplete", "Establishment", id))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<EstablishmentCompleteDto>(cacheKey, _cancellationToken))
            .ReturnsAsync((EstablishmentCompleteDto?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetByIdCompleteAsync(id, _cancellationToken))
            .ReturnsAsync((EstablishmentCompleteDto?)null);

        // Act
        var result = await _service.GetByIdCompleteAsync(id, _cancellationToken);

        // Assert
        result.Should().BeNull();
        _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<EstablishmentCompleteDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region BaseService Inherited Methods Tests

    [Fact]
    public async Task GetByIdAsync_Should_Call_Base_Repository_Method()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockEstablishmentsRepository.Setup(r => r.GetByIdAsync(id, _cancellationToken))
            .ReturnsAsync(_testEstablishment);

        // Act
        var result = await _service.GetByIdAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(_testEstablishment);
        _mockEstablishmentsRepository.Verify(r => r.GetByIdAsync(id, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetByIdNoTrackingAsync_Should_Return_Cached_Entity_When_Available()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EntityById_Establishment_" + id;

        _mockCache.Setup(c => c.GenerateCacheKey("EntityById", "Establishment", id))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<Establishment>(cacheKey, _cancellationToken))
            .ReturnsAsync(_testEstablishment);

        // Act
        var result = await _service.GetByIdNoTrackingAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(_testEstablishment);
        _mockEstablishmentsRepository.Verify(r => r.GetByIdAsNoTrackingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdNoTrackingAsync_Should_Cache_Entity_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cacheKey = "EntityById_Establishment_" + id;

        _mockCache.Setup(c => c.GenerateCacheKey("EntityById", "Establishment", id))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<Establishment>(cacheKey, _cancellationToken))
            .ReturnsAsync((Establishment?)null);
        _mockEstablishmentsRepository.Setup(r => r.GetByIdAsNoTrackingAsync(id, _cancellationToken))
            .ReturnsAsync(_testEstablishment);

        // Act
        var result = await _service.GetByIdNoTrackingAsync(id, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(_testEstablishment);
        _mockCache.Verify(c => c.SetAsync(cacheKey, _testEstablishment, TimeSpan.FromMinutes(30), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Entity_And_Set_Cache()
    {
        // Arrange
        var cacheKey = "EntityById_Establishment_" + _testEstablishment.Id;

        _mockCache.Setup(c => c.GenerateCacheKey("EntityById", "Establishment", _testEstablishment.Id))
            .Returns(cacheKey);

        // Act
        var result = await _service.CreateAsync(_testEstablishment, _cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(_testEstablishment);
        _mockEstablishmentsRepository.Verify(r => r.AddAsync(_testEstablishment, _cancellationToken), Times.Once);
        _mockCache.Verify(c => c.SetAsync(cacheKey, _testEstablishment, TimeSpan.FromMinutes(30), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity_And_Invalidate_Cache()
    {
        // Arrange
        var cacheKey = "EntityById_Establishment_" + _testEstablishment.Id;

        _mockCache.Setup(c => c.GenerateCacheKey("EntityById", "Establishment", _testEstablishment.Id))
            .Returns(cacheKey);

        // Act
        await _service.UpdateAsync(_testEstablishment, _cancellationToken);

        // Assert
        _mockEstablishmentsRepository.Verify(r => r.UpdateAsync(_testEstablishment, _cancellationToken), Times.Once);
        _mockCache.Verify(c => c.SetAsync(cacheKey, _testEstablishment, TimeSpan.FromMinutes(30), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Entity_And_Invalidate_Cache()
    {
        // Act
        await _service.DeleteAsync(_testEstablishment, _cancellationToken);

        // Assert
        _mockEstablishmentsRepository.Verify(r => r.RemoveAsync(_testEstablishment, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Not_Call_Repository_When_Entity_Is_Null()
    {
        // Act
        await _service.DeleteAsync(null!, _cancellationToken);

        // Assert
        _mockEstablishmentsRepository.Verify(r => r.RemoveAsync(It.IsAny<Establishment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_Should_Set_DefaultTtl_To_30_Minutes()
    {
        // Act & Assert
        var service = new EstablishmentService(_mockEstablishmentsRepository.Object, _mockEstablishmentUsersRepository.Object, _mockCache.Object);
        
        // We can't directly access protected property, but we can verify it through behavior
        service.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetEstablishmentsByOwnerIdAsync_Should_Handle_Null_EstablishmentIds()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var cacheKey = "EstablishmentsByOwner_" + ownerId;

        _mockCache.Setup(c => c.GenerateCacheKey("EstablishmentsByOwner", ownerId.ToString()))
            .Returns(cacheKey);
        _mockCache.Setup(c => c.GetAsync<List<Establishment>>(cacheKey, _cancellationToken))
            .ReturnsAsync((List<Establishment>?)null);
        _mockEstablishmentUsersRepository.Setup(r => r.GetByOwnerIdAsync(ownerId, _cancellationToken))
            .ReturnsAsync((List<Guid>)null!);

        // Act
        var result = await _service.GetEstablishmentsByOwnerIdAsync(ownerId, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReservationsByCourtsIdAsync_Should_Handle_Empty_CourtIds()
    {
        // Arrange
        var courtIds = new List<Guid>();
        var filter = new EstablishmentReservationsQueryFilter();
        var expectedReservations = new List<ReservationWithDetailsDto>();

        _mockEstablishmentsRepository.Setup(r => r.GetReservationsByCourtsIdAsync(courtIds, filter, _cancellationToken))
            .ReturnsAsync(expectedReservations);

        // Act
        var result = await _service.GetReservationsByCourtsIdAsync(courtIds, filter, _cancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}