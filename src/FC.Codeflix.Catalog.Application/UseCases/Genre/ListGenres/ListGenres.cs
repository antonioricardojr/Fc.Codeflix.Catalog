using FC.Codeflix.Catalog.Domain.Repository;

namespace FC.Codeflix.Catalog.Application.UseCases.Genre.ListGenres;

public class ListGenres : IListGenres
{
    private readonly IGenreRepository _genreRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ListGenres(IGenreRepository genreRepository, ICategoryRepository categoryRepository)
    {
        _genreRepository = genreRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ListGenresOutput> Handle(ListGenresInput request, CancellationToken cancellationToken)
    {
        var searchOutput = await _genreRepository.Search(request.ToSearchInput(), cancellationToken);
        ListGenresOutput output = ListGenresOutput.FromSearchOutput(searchOutput);
        List<Guid> relatedCategoryIds = searchOutput.Items.SelectMany(item => item.Categories)
            .Distinct()
            .ToList();
        if (relatedCategoryIds.Count > 0)
        {
            IReadOnlyList<Domain.Entity.Category> categories = 
                await _categoryRepository.GetListByIds(relatedCategoryIds, cancellationToken);
        
            output.FillWithCategoryNames(categories);    
        }
        

        return output;

    }
}