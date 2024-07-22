using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Application.UseCases.Genre.DeleteGenre;
using FC.Codeflix.Catalog.Application.UseCases.Genre.GetGenre;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.DeleteGenre;


[Collection(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTest
{
    private readonly DeleteGenreTestFixture _fixture;

    public DeleteGenreTest(DeleteGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("Application", "DeleteGenre - UseCases")]
    public async Task DeleteGenre()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre(categoryIds: _fixture.GetRandomIdsList());
        
        
        genreRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleGenre);
        var useCase = new Catalog.Application.UseCases.Genre.DeleteGenre.DeleteGenre(genreRepositoryMock.Object, unitOfWork.Object);

        var input = new DeleteGenreInput(exampleGenre.Id);

        await useCase.Handle(input, CancellationToken.None);
        
        genreRepositoryMock.Verify(x => 
            x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        
        genreRepositoryMock.Verify(x => 
            x.Delete(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact(DisplayName = nameof(ThrowWhenNotFound))]
    [Trait("Application", "DeleteGenre - UseCases")]
    public async Task ThrowWhenNotFound()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre(categoryIds: _fixture.GetRandomIdsList());
        
        
        genreRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Genre '{exampleGenre.Id}' not found"));
        var useCase = new Catalog.Application.UseCases.Genre.DeleteGenre.DeleteGenre(genreRepositoryMock.Object, unitOfWork.Object);

        var input = new DeleteGenreInput(exampleGenre.Id);

        var action = async () => await useCase.Handle(input, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{exampleGenre.Id}' not found");
        genreRepositoryMock.Verify(x => 
            x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}