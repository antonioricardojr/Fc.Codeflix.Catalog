using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Domain.Repository;

namespace FC.Codeflix.Catalog.Application.UseCases.Genre.UpdateGenre;

public class UpdateGenre : IUpdateGenre
{

    private readonly ICategoryRepository _categoryRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGenre(ICategoryRepository categoryRepository, IGenreRepository genreRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<GenreModelOutput> Handle(UpdateGenreInput request, CancellationToken cancellationToken)
    {
        var genre = await _genreRepository.Get(request.Id, cancellationToken);
        
        genre.Update(request.Name);
        if (request.IsActive is not null && request.IsActive != genre.IsActive)
        {
            if (genre.IsActive)
            {
                genre.Deactivate();
            }
            else
            {
                genre.Activate();
            }
        }

        if (request.CategoryIds is not null)
        {
            genre.RemoveAllCategories();

            if (request.CategoryIds.Count > 0)
            {
                await ValidateCategoryIds(request, cancellationToken);
                request.CategoryIds.ForEach(categoryId => genre.AddCategory(categoryId));    
            }
        }
        await _genreRepository.Update(genre, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return GenreModelOutput.FromGenre(genre);

    }
    
    private async Task ValidateCategoryIds(UpdateGenreInput request, CancellationToken cancellationToken)
    {
        var idsInPersistence = await _categoryRepository.GetIdsListByIds(request.CategoryIds!, cancellationToken);
        if (idsInPersistence.Count < request.CategoryIds!.Count)
        {
            var notFoundIds = request.CategoryIds.FindAll(x => !idsInPersistence.Contains(x));
            throw new RelatedAggregateException($"Related category id (or ids) not found: {String.Join(",", notFoundIds)}");
        }
    }
}