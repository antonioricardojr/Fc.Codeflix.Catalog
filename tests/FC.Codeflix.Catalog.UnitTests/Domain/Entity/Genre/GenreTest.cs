using FC.Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Domain.Entity.Genre;

[Collection(nameof(GenreTestFixture))]
public class GenreTest
{
    private readonly GenreTestFixture _fixture;

    public GenreTest(GenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "Genre - Aggregates")]
    public void Instantiate()
    {
        
        var genreName = _fixture.GetValidName();
        var dateTimeBefore = DateTime.Now;
        var genre = new Catalog.Domain.Entity.Genre(genreName);
        var dateTimeAfter = DateTime.Now.AddSeconds(1);
        
        genre.Should().NotBeNull();
        genre.Name.Should().Be(genreName);
        genre.IsActive.Should().BeTrue();
        genre.Id.Should().NotBe(default(Guid));
        genre.CreatedAt.Should().NotBe(default(DateTime));
        genre.CreatedAt.Should().BeAfter(dateTimeBefore);
        genre.CreatedAt.Should().BeBefore(dateTimeAfter);
        genre.IsActive.Should().BeTrue();
    }
    
    [Theory(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "Genre - Aggregates")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void InstantiateThrowWhenNameEmpty(string? invalidName)
    {
        
        var action = () => new Catalog.Domain.Entity.Genre(invalidName!);
        action.Should().Throw<EntityValidationException>().WithMessage("Name should not be null or empty");
    }
    
    [Theory(DisplayName = nameof(Activate))]
    [Trait("Domain", "Genre - Aggregates")]
    [InlineData(true)]
    [InlineData(false)]
    public void Activate(bool isActive)
    {

        var genre = _fixture.GetExampleGenre();

        genre.Activate();
        
        genre.Should().NotBeNull();
        genre.Name.Should().Be(genre.Name);
        genre.IsActive.Should().BeTrue();
        genre.Id.Should().NotBe(default(Guid));
        genre.CreatedAt.Should().NotBe(default(DateTime));
    }
    
    [Theory(DisplayName = nameof(Deactivate))]
    [Trait("Domain", "Genre - Aggregates")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deactivate(bool isActive)
    {

        var genre = _fixture.GetExampleGenre();

        genre.Deactivate();
        
        genre.Should().NotBeNull();
        genre.Name.Should().Be(genre.Name);
        genre.IsActive.Should().BeFalse();
        genre.Id.Should().NotBe(default(Guid));
        genre.CreatedAt.Should().NotBe(default(DateTime));
    }
    
        
    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "Genre - Aggregates")]
    public void Update()
    {

        var genre = _fixture.GetExampleGenre();
        var newGenreName = _fixture.GetValidName();

        genre.Update(newGenreName);
        
        genre.Should().NotBeNull();
        genre.Name.Should().Be(newGenreName);
        genre.Id.Should().NotBe(default(Guid));
        genre.CreatedAt.Should().NotBe(default(DateTime));
    }
    
    [Theory(DisplayName = nameof(UpdateThrowWhenNameIsEmpty))]
    [Trait("Domain", "Genre - Aggregates")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateThrowWhenNameIsEmpty(string? invalidName)
    {
        var genre = _fixture.GetExampleGenre();
        var action = () => genre.Update(invalidName!);
        action.Should().Throw<EntityValidationException>().WithMessage("Name should not be null or empty");
        
    }
    
    [Fact(DisplayName = nameof(AddCategory))]
    [Trait("Domain", "Genre - Aggregates")]
    public void AddCategory()
    {
        
        var genre = _fixture.GetExampleGenre();
        var categoryGuid = Guid.NewGuid();

        genre.AddCategory(categoryGuid);

        genre.Categories.Should().HaveCount(1);
        genre.Categories.Should().Contain(categoryGuid);
    }
    
    [Fact(DisplayName = nameof(AddTwoCategories))]
    [Trait("Domain", "Genre - Aggregates")]
    public void AddTwoCategories()
    {
        
        var genre = _fixture.GetExampleGenre();
        var categoryGuid1 = Guid.NewGuid();
        var categoryGuid2 = Guid.NewGuid();

        genre.AddCategory(categoryGuid1);
        genre.AddCategory(categoryGuid2);

        genre.Categories.Should().HaveCount(2);
        genre.Categories.Should().Contain(categoryGuid1);
        genre.Categories.Should().Contain(categoryGuid2);
    }
    
    
    [Fact(DisplayName = nameof(RemoveCategory))]
    [Trait("Domain", "Genre - Aggregates")]
    public void RemoveCategory()
    {

        var exampleGuid = Guid.NewGuid();
        var genre = _fixture.GetExampleGenre(categoryIds: new List<Guid>() {Guid.NewGuid(), Guid.NewGuid(), exampleGuid, Guid.NewGuid()});

        genre.Categories.Should().HaveCount(4);
        genre.RemoveCategory(exampleGuid);
        genre.Categories.Should().HaveCount(3);
        genre.Categories.Should().NotContain(exampleGuid);
    }
    
    [Fact(DisplayName = nameof(RemoveAllCategories))]
    [Trait("Domain", "Genre - Aggregates")]
    public void RemoveAllCategories()
    {

        var genre = _fixture.GetExampleGenre(categoryIds: new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()});

        genre.Categories.Should().HaveCount(3);
        genre.RemoveAllCategories();
        genre.Categories.Should().HaveCount(0);
    }
    
}