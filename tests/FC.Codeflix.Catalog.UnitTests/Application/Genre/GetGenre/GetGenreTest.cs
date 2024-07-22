using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Application.UseCases.Genre.GetGenre;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.GetGenre;

[Collection(nameof(GetGenreTestFixture))]
public class GetGenreTest
{
    private readonly GetGenreTestFixture _fixture;

    public GetGenreTest(GetGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("Application", "GetGenre - UseCases")]
    public async Task GetGenre()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var exampleCategories = _fixture.GetExampleCategoriesList();
        var exampleGenre = _fixture.GetExampleGenre(categoryIds: exampleCategories.Select(c => c.Id).ToList());
        genreRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleGenre);
        categoryRepositoryMock.Setup(x => x.GetListByIds(
            It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleCategories);
        var useCase = new Catalog.Application.UseCases.Genre.GetGenre.GetGenre(genreRepositoryMock.Object, categoryRepositoryMock.Object);

        var input = new GetGenreInput(exampleGenre.Id);

        GenreModelOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().Be(exampleGenre.Id);
        output.Name.Should().Be(exampleGenre.Name);
        output.IsActive.Should().Be(exampleGenre.IsActive);
        output.CreatedAt.Should().Be(exampleGenre.CreatedAt);
        output.Categories.Should().HaveCount(exampleGenre.Categories.Count);
        foreach (var categoryId in exampleGenre.Categories)
        {
            output.Categories.Select(c => c.Id).Should().Contain(categoryId);

        }
        
        genreRepositoryMock.Verify(x => 
            x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact(DisplayName = nameof(ThrowWhenNotFound))]
    [Trait("Application", "GetGenre - UseCases")]
    public async Task ThrowWhenNotFound()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var getCategoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var exampleGenre = _fixture.GetExampleGenre(categoryIds: _fixture.GetRandomIdsList());
        genreRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Genre '{exampleGenre.Id}' not found"));
        var useCase = new Catalog.Application.UseCases.Genre.GetGenre.GetGenre(genreRepositoryMock.Object, getCategoryRepositoryMock.Object);

        var input = new GetGenreInput(exampleGenre.Id);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{exampleGenre.Id}' not found");
        
        genreRepositoryMock.Verify(x => 
            x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}