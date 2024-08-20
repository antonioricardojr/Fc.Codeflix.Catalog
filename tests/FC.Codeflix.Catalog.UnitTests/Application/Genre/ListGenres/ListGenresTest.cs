using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Application.UseCases.Genre.ListGenres;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.ListGenres;

[Collection(nameof(ListGenresTestFixture))]
public class ListGenresTest
{
    private readonly ListGenresTestFixture _fixture;

    public ListGenresTest(ListGenresTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(DisplayName = nameof(ListGenres))]
    [Trait("Application", "ListGenre - UseCases")]
    public async Task ListGenres()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var exampleListGenres = _fixture.GetExampleGenreList();
        var exampleCategories = _fixture.GetExampleCategoriesList(10);
        Random random = new Random();

        foreach (var genre in exampleListGenres)
        {
            genre.RemoveAllCategories();
            exampleCategories.OrderBy(x => random.Next()).Take(3).ToList().ForEach(category =>
            {
                genre.AddCategory(category.Id);
            });
        }
        
        var input = _fixture.GetExampleInput();
        
        var outputRepositorySearch = new SearchOutput<Catalog.Domain.Entity.Genre>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: exampleListGenres,
            total: (new Random()).Next(50, 200)
        );
        genreRepositoryMock.Setup(x => x.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(outputRepositorySearch);
        categoryRepositoryMock.Setup(x => x.GetListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleCategories);
        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(genreRepositoryMock.Object, categoryRepositoryMock.Object);


        ListGenresOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputRepositorySearch.CurrentPage);
        output.PerPage.Should().Be(outputRepositorySearch.PerPage);
        output.Total.Should().Be(outputRepositorySearch.Total);
        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);
        
        ((List<GenreModelOutput>) output.Items).ForEach(outputItem =>
        {
            var repositorygenre = outputRepositorySearch.Items
                .FirstOrDefault(x => x.Id == outputItem.Id);
            
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(repositorygenre!.Name);
            outputItem.IsActive.Should().Be(repositorygenre.IsActive);
            outputItem.CreatedAt.Should().Be(repositorygenre.CreatedAt);
            outputItem.Id.Should().Be(repositorygenre.Id);

            outputItem.Categories.ToList().ForEach(outputCategory =>
            {
                var expectedCategory = exampleCategories.FirstOrDefault(c => c.Id == outputCategory.Id);
                outputCategory.Name.Should().Be(expectedCategory!.Name);
            });
        });
        
        genreRepositoryMock.Verify(x => x.Search(It.Is<SearchInput>(searchInput =>
            searchInput.Page == input.Page &&
            searchInput.PerPage == input.PerPage &&
            searchInput.Search == input.Search &&
            searchInput.OrderBy == input.Sort &&
            searchInput.Order == input.Dir
        ), It.IsAny<CancellationToken>()), Times.Once);
        
        categoryRepositoryMock.Verify(x => x.GetListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact(DisplayName = nameof(ListGenresWhenEmpty))]
    [Trait("Application", "ListGenre - UseCases")]
    public async Task ListGenresWhenEmpty()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var input = _fixture.GetExampleInput();
        var outputRepositorySearch = new SearchOutput<Catalog.Domain.Entity.Genre>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: new List<Catalog.Domain.Entity.Genre>(),
            total: (new Random()).Next(50, 200)
        );
        
        genreRepositoryMock.Setup(x => x.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(outputRepositorySearch);
        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(genreRepositoryMock.Object, categoryRepositoryMock.Object);


        ListGenresOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputRepositorySearch.CurrentPage);
        output.PerPage.Should().Be(outputRepositorySearch.PerPage);
        output.Total.Should().Be(outputRepositorySearch.Total);
        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);
        
        ((List<GenreModelOutput>) output.Items).ForEach(outputItem =>
        {
            var repositorygenre = outputRepositorySearch.Items
                .FirstOrDefault(x => x.Id == outputItem.Id);
            
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(repositorygenre!.Name);
            outputItem.IsActive.Should().Be(repositorygenre.IsActive);
            outputItem.CreatedAt.Should().Be(repositorygenre.CreatedAt);
            outputItem.Id.Should().Be(repositorygenre.Id);
        });
        
        genreRepositoryMock.Verify(x => x.Search(It.Is<SearchInput>(searchInput =>
            searchInput.Page == input.Page &&
            searchInput.PerPage == input.PerPage &&
            searchInput.Search == input.Search &&
            searchInput.OrderBy == input.Sort &&
            searchInput.Order == input.Dir
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact(DisplayName = nameof(ListUsingDefaultInputValues))]
    [Trait("Application", "ListGenre - UseCases")]
    public async Task ListUsingDefaultInputValues()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var outputRepositorySearch = new SearchOutput<Catalog.Domain.Entity.Genre>(
            currentPage: 1,
            perPage:15,
            items: new List<Catalog.Domain.Entity.Genre>(),
            total: 0
        );
        
        genreRepositoryMock.Setup(x => x.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(outputRepositorySearch);
        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(genreRepositoryMock.Object, categoryRepositoryMock.Object);


        ListGenresOutput output = await useCase.Handle(new ListGenresInput(), CancellationToken.None);
        
        
        genreRepositoryMock.Verify(x => x.Search(It.Is<SearchInput>(searchInput =>
            searchInput.Page == 1 &&
            searchInput.PerPage == 15 &&
            searchInput.Search == "" &&
            searchInput.OrderBy == "" &&
            searchInput.Order == SearchOrder.Asc
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}