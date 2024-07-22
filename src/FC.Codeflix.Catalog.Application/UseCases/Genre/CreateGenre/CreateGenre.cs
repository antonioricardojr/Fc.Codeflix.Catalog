using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Domain.Repository;

namespace FC.Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;

public class CreateGenre : ICreateGenre
{
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _categoryRepository;

    public CreateGenre(IGenreRepository genreRepository, IUnitOfWork unitOfWork, ICategoryRepository categoryRepository)
    {
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
    }

    public async Task<GenreModelOutput> Handle(CreateGenreInput request, CancellationToken cancellationToken)
    {
        var genre = new Domain.Entity.Genre(request.Name, request.IsActive);

        if ((request.CategoryIds?.Count ?? 0) > 0)
        {
            ValidateCategoryIds(request, cancellationToken);
            request.CategoryIds?.ForEach(categoryId => genre.AddCategory(categoryId));
        }        
        await _genreRepository.Insert(genre, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return GenreModelOutput.FromGenre(genre);
    }

    private async void ValidateCategoryIds(CreateGenreInput request, CancellationToken cancellationToken)
    {
        var idsInPersistence = await _categoryRepository.GetIdsListByIds(request.CategoryIds!, cancellationToken);
        if (idsInPersistence.Count < request.CategoryIds!.Count)
        {
            var notFoundIds = request.CategoryIds.FindAll(x => !idsInPersistence.Contains(x));
            throw new RelatedAggregateException($"Related Category id(s) not found: {String.Join(",", notFoundIds)}");
        }
    }
}