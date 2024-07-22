using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest
{
    private readonly CreateGenreTestFixture _fixture;

    public CreateGenreTest(CreateGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(Create))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task Create()
    {
        var input = _fixture.GetExampleInput();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var uniofWorkMock = _fixture.GetUnitOfWorkMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        categoryRepositoryMock.Setup(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(input.CategoryIds!);
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(genreRepositoryMock.Object, uniofWorkMock.Object, categoryRepositoryMock.Object);

        var datetimeBefore = DateTime.Now;
        var output = await useCase.Handle(input, CancellationToken.None);
        var datetimeAfter = DateTime.Now.AddSeconds(1);

        genreRepositoryMock.Verify(x => x.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), Times.Once);
        uniofWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
        
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Id.Should().NotBeEmpty();
        output.Categories.Should().HaveCount(0);
        output.CreatedAt.Should().NotBeSameDateAs(default);
        (output.CreatedAt >= datetimeBefore).Should().BeTrue();
        (output.CreatedAt <= datetimeAfter).Should().BeTrue();
    }
    
    [Fact(DisplayName = nameof(CreateWithRelatedCategories))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task CreateWithRelatedCategories()
    {
        var input = _fixture.GetExampleInputWithCategories();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var uniofWorkMock = _fixture.GetUnitOfWorkMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        categoryRepositoryMock.Setup(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(input.CategoryIds!);
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(genreRepositoryMock.Object, uniofWorkMock.Object, categoryRepositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepositoryMock.Verify(x => x.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), Times.Once);
        uniofWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
        
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Id.Should().NotBeEmpty();
        output.Categories.Should().HaveCount(input.CategoryIds!.Count);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }
    
    [Fact(DisplayName = nameof(CreateThrowWhenRelatedCategoryNofFound))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task CreateThrowWhenRelatedCategoryNofFound()
    {
        var input = _fixture.GetExampleInputWithCategories();
        int ˆ1;
        var exampleGuid = input.CategoryIds![input.CategoryIds.Count - 1];
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var uniofWorkMock = _fixture.GetUnitOfWorkMock();
        categoryRepositoryMock.Setup(x => 
                x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Guid>) input.CategoryIds.FindAll(x => x != exampleGuid));
        
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(
            genreRepositoryMock.Object, uniofWorkMock.Object, categoryRepositoryMock.Object);
        var action = async () => await useCase.Handle(input, CancellationToken.None);
        action.Should().ThrowAsync<RelatedAggregateException>().WithMessage($"Related Category id(s) not found: {exampleGuid}");
        
        categoryRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("Application", "CreateGenre - Use Cases")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task ThrowWhenNameIsInvalid(string? invalidName)
    {
        var input = _fixture.GetExampleInput(invalidName);
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var uniofWorkMock = _fixture.GetUnitOfWorkMock();
        
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(
            genreRepositoryMock.Object, uniofWorkMock.Object, categoryRepositoryMock.Object);
        var action = async () => await useCase.Handle(input, CancellationToken.None);
        action.Should().ThrowAsync<EntityValidationException>().WithMessage($"{invalidName} should not be null or empty");
    }
}