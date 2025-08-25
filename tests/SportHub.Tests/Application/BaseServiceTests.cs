using System.Text;
using Application.Common.Interfaces.Base;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;

namespace SportHub.Tests.Application;

/// <summary>
/// Testes unitários para o BaseService
/// Estes testes verificam o comportamento básico do service base incluindo cache
/// </summary>
public class BaseServiceTests
{
    private readonly Mock<IBaseRepository<Sport>> _mockRepository;
    private readonly Mock<ICacheService> _mockCache;
    private readonly BaseService<Sport> _service;
    private readonly Sport _testSport;

    public BaseServiceTests()
    {
        _mockRepository = new Mock<IBaseRepository<Sport>>();
        _mockCache = new Mock<ICacheService>();
        _service = new BaseService<Sport>(_mockRepository.Object, _mockCache.Object);

        _testSport = new Sport
        {
            Id = Guid.NewGuid(),
            Name = "Football",
            Description = "Association football sport",
            ImageUrl = "https://test.com/football.jpg"
        };
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Entity_From_Repository()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetByIdAsync(_testSport.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testSport);

        // Act
        var result = await _service.GetByIdAsync(_testSport.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testSport.Id);
        result.Name.Should().Be(_testSport.Name);
        _mockRepository.Verify(x => x.GetByIdAsync(_testSport.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdNoTrackingAsync_Should_Return_Cached_Entity_When_Available()
    {
        // Arrange
        var cacheKey = "test-cache-key";
        _mockCache.Setup(x => x.GenerateCacheKey(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(cacheKey);
        _mockCache.Setup(x => x.GetAsync<Sport>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testSport);

        // Act
        var result = await _service.GetByIdNoTrackingAsync(_testSport.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testSport.Id);
        _mockRepository.Verify(x => x.GetByIdAsNoTrackingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockCache.Verify(x => x.GetAsync<Sport>(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdNoTrackingAsync_Should_Fetch_From_Repository_And_Cache_When_Not_Cached()
    {
        // Arrange
        var cacheKey = "test-cache-key";
        _mockCache.Setup(x => x.GenerateCacheKey(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(cacheKey);
        _mockCache.Setup(x => x.GetAsync<Sport>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sport?)null);
        _mockRepository.Setup(x => x.GetByIdAsNoTrackingAsync(_testSport.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testSport);

        // Act
        var result = await _service.GetByIdNoTrackingAsync(_testSport.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testSport.Id);
        _mockRepository.Verify(x => x.GetByIdAsNoTrackingAsync(_testSport.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetAsync(cacheKey, _testSport, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Entity_And_Invalidate_Cache()
    {
        // Arrange
        _mockRepository.Setup(x => x.AddAsync(_testSport, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(_testSport);

        // Assert
        result.Should().Be(_testSport);
        _mockRepository.Verify(x => x.AddAsync(_testSport, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity_And_Invalidate_Cache()
    {
        // Arrange
        _mockRepository.Setup(x => x.UpdateAsync(_testSport, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(_testSport);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(_testSport, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Entity_And_Invalidate_Cache()
    {
        // Arrange
        _mockRepository.Setup(x => x.RemoveAsync(_testSport, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(_testSport);

        // Assert
        _mockRepository.Verify(x => x.RemoveAsync(_testSport, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_Repository_Result()
    {
        // Arrange
        _mockRepository.Setup(x => x.ExistsAsync(_testSport.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(_testSport.Id);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.ExistsAsync(_testSport.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Entities_From_Repository()
    {
        // Arrange
        var sports = new List<Sport> { _testSport };
        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sports);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(_testSport);
        _mockRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCountAsync_Should_Return_Repository_Count_And_Cache_Result()
    {
        // Arrange
        var expectedCount = 5;
        var cacheKey = "test-count-key";
        _mockCache.Setup(x => x.GenerateCacheKey(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(cacheKey);
        _mockCache.Setup(x => x.GetAsync<string>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _mockRepository.Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        result.Should().Be(expectedCount);
        _mockRepository.Verify(x => x.GetCountAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetAsync(cacheKey, expectedCount.ToString(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateManyAsync_Should_Add_Entities_To_Database()
    {
        // Arrange
        var sports = new List<Sport>
        {
            new Sport { Id = Guid.NewGuid(), Name = "Football", Description = "Association football sport", ImageUrl = "https://test.com/football.jpg" },
            new Sport { Id = Guid.NewGuid(), Name = "Basketball", Description = "Basketball sport", ImageUrl = "https://test.com/basketball.jpg" },
            new Sport { Id = Guid.NewGuid(), Name = "Tennis", Description = "Tennis sport", ImageUrl = "https://test.com/tennis.jpg" }
        };

        _mockRepository.Setup(x => x.AddManyAsync(sports, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateManyAsync(sports);

        // Assert
        _mockRepository.Verify(x => x.AddManyAsync(sports, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteManyAsync_Should_Remove_Entities_And_Invalidate_Cache()
    {
        // Arrange
        var sports = new List<Sport> { _testSport };
        var ids = sports.Select(s => s.Id).ToList();
        _mockRepository.Setup(x => x.RemoveByIdsAsync(ids, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteManyAsync(ids);

        // Assert
        _mockRepository.Verify(x => x.RemoveByIdsAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetByIdsAsync_Should_Return_Entities()
    {
        // Arrange
        var sports = new List<Sport> { _testSport };
        var ids = sports.Select(s => s.Id).ToList();
        _mockRepository.Setup(x => x.GetByIdsAsync(ids, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sports);

        // Act
        var result = await _service.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(_testSport);
        _mockRepository.Verify(x => x.GetByIdsAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdsAsync_Should_Return_Empty_List_When_No_Entities_Found()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid() };
        _mockRepository.Setup(x => x.GetByIdsAsync(ids, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Sport>());

        // Act
        var result = await _service.GetByIdsAsync(ids);

        // Assert
        result.Should().BeEmpty();
        _mockRepository.Verify(x => x.GetByIdsAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Entities()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var sports = new List<Sport> { _testSport };
        _mockRepository.Setup(x => x.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sports);

        // Act
        var result = await _service.GetPagedAsync(page, pageSize);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(_testSport);
        _mockRepository.Verify(x => x.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCountAsync_Should_Return_Total_Cached_Count()
    {
        // Arrange
        var totalCount = 100;
        _mockCache.Setup(x => x.GenerateCacheKey(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns("test-count-key");
        _mockCache.Setup(x => x.GetAsync<string>("test-count-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount.ToString());

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        result.Should().Be(totalCount);
        _mockCache.Verify(x => x.GetAsync<string>("test-count-key", It.IsAny<CancellationToken>()), Times.Once);
    }
}
