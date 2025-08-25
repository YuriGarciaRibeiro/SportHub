using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Application.Common.Interfaces.Security;

namespace SportHub.Tests.Infrastructure;

/// <summary>
/// Testes unitários para o BaseRepository
/// Estes testes verificam as operações básicas de CRUD do repositório base
/// </summary>
public class BaseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly BaseRepository<Sport> _repository;
    private readonly Sport _testSport;

    public BaseRepositoryTests()
    {
        // Configurar banco de dados em memória para testes
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _context = new ApplicationDbContext(options, mockCurrentUserService.Object);
        _repository = new BaseRepository<Sport>(_context);

        _testSport = new Sport
        {
            Id = Guid.NewGuid(),
            Name = "Football",
            Description = "Association football sport",
            ImageUrl = "https://test.com/football.jpg"
        };
    }

    [Fact]
    public async Task AddAsync_Should_Add_Entity_To_Database()
    {
        // Act
        await _repository.AddAsync(_testSport, CancellationToken.None);

        // Assert
        var savedEntity = await _context.Sports.FindAsync(_testSport.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be(_testSport.Name);
        savedEntity.Description.Should().Be(_testSport.Description);
        savedEntity.ImageUrl.Should().Be(_testSport.ImageUrl);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Entity_When_Exists()
    {
        // Arrange
        await _context.Sports.AddAsync(_testSport);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(_testSport.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testSport.Id);
        result.Name.Should().Be(_testSport.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Entity_Exists()
    {
        // Arrange
        await _context.Sports.AddAsync(_testSport);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(_testSport.Id, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_Entity_Not_Exists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId, CancellationToken.None);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity_In_Database()
    {
        // Arrange
        await _context.Sports.AddAsync(_testSport);
        await _context.SaveChangesAsync();

        var updatedName = "Updated Football";
        _testSport.Name = updatedName;

        // Act
        await _repository.UpdateAsync(_testSport, CancellationToken.None);

        // Assert
        var updatedEntity = await _context.Sports.FindAsync(_testSport.Id);
        updatedEntity.Should().NotBeNull();
        updatedEntity!.Name.Should().Be(updatedName);
    }

    [Fact]
    public async Task RemoveAsync_Should_Remove_Entity_From_Database()
    {
        // Arrange
        await _context.Sports.AddAsync(_testSport);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveAsync(_testSport, CancellationToken.None);

        // Assert
        var deletedEntity = await _context.Sports.FindAsync(_testSport.Id);

        if (deletedEntity != null)
        {
            deletedEntity.IsDeleted.Should().BeTrue();
        }
        else
        {
            deletedEntity.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Entities()
    {
        // Arrange
        var sport1 = new Sport
        {
            Id = Guid.NewGuid(),
            Name = "Basketball",
            Description = "Basketball sport",
            ImageUrl = "https://test.com/basketball.jpg"
        };

        var sport2 = new Sport
        {
            Id = Guid.NewGuid(),
            Name = "Tennis",
            Description = "Tennis sport",
            ImageUrl = "https://test.com/tennis.jpg"
        };

        await _context.Sports.AddRangeAsync(_testSport, sport1, sport2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Id == _testSport.Id);
        result.Should().Contain(x => x.Id == sport1.Id);
        result.Should().Contain(x => x.Id == sport2.Id);
    }

    [Fact]
    public async Task GetCountAsync_Should_Return_Correct_Count()
    {
        // Arrange
        var sport1 = new Sport
        {
            Id = Guid.NewGuid(),
            Name = "Basketball",
            Description = "Basketball sport",
            ImageUrl = "https://test.com/basketball.jpg"
        };

        await _context.Sports.AddRangeAsync(_testSport, sport1);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetCountAsync(CancellationToken.None);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var sports = new List<Sport>();
        for (int i = 0; i < 5; i++)
        {
            sports.Add(new Sport
            {
                Id = Guid.NewGuid(),
                Name = $"Sport {i}",
                Description = $"Description {i}",
                ImageUrl = $"https://test.com/sport{i}.jpg"
            });
        }

        await _context.Sports.AddRangeAsync(sports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(1, 2, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsNoTrackingAsync_Should_Return_Entity_From_Repository()
    {
        // Arrange
        await _context.Sports.AddAsync(_testSport);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsNoTrackingAsync(_testSport.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testSport.Id);
        result.Name.Should().Be(_testSport.Name);
    }

    [Fact]
    public async Task GetByIdsAsync_Should_Return_Entities_From_Repository()
    {
        // Arrange
        var sports = new List<Sport> { _testSport };
        await _context.Sports.AddRangeAsync(sports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdsAsync(new[] { _testSport.Id }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(x => x.Id == _testSport.Id);
    }

    [Fact]
    public async Task AddManyAsync_Should_Add_Multiple_Entities()
    {
        // Arrange
        var sports = new List<Sport>
        {
            new Sport { Id = Guid.NewGuid(), Name = "Football", Description = "Football sport", ImageUrl = "https://test.com/football.jpg" },
            new Sport { Id = Guid.NewGuid(), Name = "Baseball", Description = "Baseball sport", ImageUrl = "https://test.com/baseball.jpg" }
        };

        // Act
        await _repository.AddManyAsync(sports, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repository.GetByIdsAsync(sports.Select(x => x.Id), CancellationToken.None);
        result.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
