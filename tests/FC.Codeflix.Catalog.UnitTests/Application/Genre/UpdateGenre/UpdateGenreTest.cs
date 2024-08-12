using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Genre.UpdateGenre;
using FC.Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTest
{
    private readonly UpdateGenreTestFixture _fixture;

    public UpdateGenreTest(UpdateGenreTestFixture fixture)
    {
        _fixture = fixture;
    }


    [Fact(DisplayName = nameof(Update))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task Update()
    {
        var exampleGenre = _fixture.GetExampleGenre();
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = _fixture.GetRandomBoolean();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive);
        
        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(0);
        genreRepositoryMock.Verify(x => 
            x.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        unitOfWorkMock.Verify(x => 
                x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

    }
    
    
    [Fact(DisplayName = nameof(ThrowWhenNotFound))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task ThrowWhenNotFound()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var exampleId = Guid.NewGuid();
        var input = new UpdateGenreInput(exampleId, _fixture.GetValidGenreName(), true);
        
        genreRepositoryMock.Setup(x => x.Get(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new NotFoundException($"Genre not found {exampleId}"));
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            _fixture.GetCategoryRepositoryMock().Object, 
            genreRepositoryMock.Object,
            _fixture.GetUnitOfWorkMock().Object);

        
        var action = async () => await useCase.Handle(input, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre not found {exampleId}");

    }
    
    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task ThrowWhenNameIsInvalid(string? invalidName)
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var exampleGenre = _fixture.GetExampleGenre();

        var input = new UpdateGenreInput(exampleGenre.Id, invalidName!, true);
        genreRepositoryMock.Setup(x =>
                x.Get(It.Is<Guid>(x => x == exampleGenre.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleGenre);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            _fixture.GetCategoryRepositoryMock().Object, 
            genreRepositoryMock.Object,
            _fixture.GetUnitOfWorkMock().Object);

        
        var action = async () => await useCase.Handle(input, CancellationToken.None);
        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage($"Name should not be null or empty");

    }
    
    [Theory(DisplayName = nameof(UpdateGenreOnlyName))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateGenreOnlyName(bool isActive)
    {
        var exampleGenre = _fixture.GetExampleGenre(isActive);
        var newNameExample = _fixture.GetValidGenreName();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample);
        
        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(isActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(0);
        genreRepositoryMock.Verify(x => 
                x.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        unitOfWorkMock.Verify(x => 
                x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

    }
    
    [Fact(DisplayName = nameof(UpdateGenreAddingCategoryIds))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreAddingCategoryIds()
    {
        var exampleGenre = _fixture.GetExampleGenre();
        var exampleCategoryIds = _fixture.GetRandomIdsList();
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = _fixture.GetRandomBoolean();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        
        categoryRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleCategoryIds);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive, exampleCategoryIds);
        
        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(exampleCategoryIds.Count);
        exampleCategoryIds.ForEach(expectedId =>
        {
            output.Categories.Select(c => c.Id).Should().Contain(expectedId);
        });
        genreRepositoryMock.Verify(x => 
                x.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        unitOfWorkMock.Verify(x => 
                x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

    }
    
    [Fact(DisplayName = nameof(UpdateGenreReplacingCategoryIds))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreReplacingCategoryIds()
    {
        var oldCategoryIds = _fixture.GetRandomIdsList();
        var exampleGenre = _fixture.GetExampleGenre(true, oldCategoryIds);
        var newCategoryIds = _fixture.GetRandomIdsList();
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = _fixture.GetRandomBoolean();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        
        categoryRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(newCategoryIds);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive, newCategoryIds);
        
        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(newCategoryIds.Count);
        newCategoryIds.ForEach(expectedId =>
        {
            output.Categories.Select(c => c.Id).Should().Contain(expectedId);
        });
        oldCategoryIds.ForEach(expectedId =>
        {
            output.Categories.Select(c => c.Id).Should().NotContain(expectedId);
        });
        genreRepositoryMock.Verify(x => 
                x.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        unitOfWorkMock.Verify(x => 
                x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

    }
    
    
    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var oldCategoryIds = _fixture.GetRandomIdsList();
        var exampleGenre = _fixture.GetExampleGenre(true, oldCategoryIds);
        var newCategoryIds = _fixture.GetRandomIdsList(10);
        var returnedCategoryIds = newCategoryIds.GetRange(0, newCategoryIds.Count - 2);
        var idsNotReturned = newCategoryIds.GetRange(newCategoryIds.Count - 3, 2);
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = _fixture.GetRandomBoolean();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        categoryRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(returnedCategoryIds);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive, newCategoryIds);
        
        var action = async () => await useCase.Handle(input, CancellationToken.None);


        action.Should().ThrowAsync<RelatedAggregateException>().WithMessage($"Related Category id(s) not found: {String.Join(",", idsNotReturned)}");
        genreRepositoryMock.Verify(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        categoryRepositoryMock.Verify(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact(DisplayName = nameof(UpdateGenreWithoutCategoryIds))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithoutCategoryIds()
    {
        var categoryIds = _fixture.GetRandomIdsList();
        var exampleGenre = _fixture.GetExampleGenre(true, categoryIds);
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = _fixture.GetRandomBoolean();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive);
        
        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(categoryIds.Count);
        categoryIds.ForEach(expectedId =>
        {
            output.Categories.Select(c => c.Id).Should().Contain(expectedId);
        });
        genreRepositoryMock.Verify(x => 
                x.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        unitOfWorkMock.Verify(x => 
                x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

    }
    
    [Fact(DisplayName = nameof(UpdateGenreWithEmptyCategoryIds))]
    [Trait("Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithEmptyCategoryIds()
    {
        var categoryIds = _fixture.GetRandomIdsList();
        var exampleGenre = _fixture.GetExampleGenre(true, categoryIds);
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = _fixture.GetRandomBoolean();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        genreRepositoryMock.Setup(x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(exampleGenre);
        
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            categoryRepositoryMock.Object, 
            genreRepositoryMock.Object,
            unitOfWorkMock.Object);

        var input = new UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive, new List<Guid>());
        
        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(0);
        categoryIds.ForEach(expectedId =>
        {
            output.Categories.Select(c => c.Id).Should().NotContain(expectedId);
        });
        genreRepositoryMock.Verify(x => 
                x.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        unitOfWorkMock.Verify(x => 
                x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

    }
}