using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Domain.Entity;
using FC.Codeflix.Catalog.Domain.Repository;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace FC.Codeflix.Catalog.Infra.Data.EF.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly CodeflixCatalogDbContext _context;
    private DbSet<Genre> Genres => _context.Set<Genre>();
    private DbSet<GenresCategories> GenresCategories => _context.Set<GenresCategories>();

    public GenreRepository(CodeflixCatalogDbContext context)
    {
        _context = context;
    }

    public async Task Insert(Genre genre, CancellationToken cancellationToken)
    {
        await _context.Genres.AddAsync(genre, cancellationToken);
        if (genre.Categories.Count > 0)
        {
            var relations = genre.Categories.Select(categoryId => new GenresCategories(categoryId, genre.Id));
            await GenresCategories.AddRangeAsync(relations);
        }
    }

    public async Task<Genre> Get(
        Guid id, 
        CancellationToken cancellationToken
    )
    {
        var genre = await Genres
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        NotFoundException.ThrowIfNull(genre, $"Genre '{id}' not found.");
        var categoryIds = await GenresCategories
            .Where(x => x.GenreId == genre!.Id)
            .Select(x => x.CategoryId)
            .ToListAsync(cancellationToken);
        categoryIds.ForEach(genre!.AddCategory);
        return genre;
    }

    public Task Delete(Genre genre, CancellationToken cancellationToken)
    {
        GenresCategories.RemoveRange(GenresCategories.Where(x => x.GenreId == genre.Id));
        Genres.Remove(genre);
        return Task.CompletedTask;
    }

    public async Task Update(Genre genre, CancellationToken cancellationToken)
    {
       Genres.Update(genre);
       GenresCategories.RemoveRange(GenresCategories.Where(x => x.GenreId == genre.Id));
       if (genre.Categories.Count > 0)
       {
           var relations = genre.Categories
               .Select(categoryId => new GenresCategories(categoryId, genre.Id));
           await GenresCategories.AddRangeAsync(relations, cancellationToken);
       }
    }

    public async Task<SearchOutput<Genre>> Search(SearchInput input, CancellationToken cancellationToken)
    {
        var toSkip = (input.Page - 1) * input.PerPage;
        var query = Genres.AsNoTracking();
        query = AddOrderToQuery(query, input.OrderBy, input.Order);
        if (!String.IsNullOrWhiteSpace(input.Search))
        {
            query = query.Where(x => x.Name.Contains(input.Search));
        }
        var total = await query.CountAsync();
        var items = await query
            .AsNoTracking()
            .Skip(toSkip)
            .Take(input.PerPage).ToListAsync(cancellationToken);
        var relations = await GenresCategories
            .Where(relation => items.Select(i => i.Id).Contains(relation.GenreId))
            .ToListAsync(cancellationToken);
        var relationsByGenreIdGroup = relations.GroupBy(x => x.GenreId).ToList();
        relationsByGenreIdGroup.ForEach(relationGroup =>
        {
            var genre = items.Find(genre => genre.Id == relationGroup.Key);
            if (genre is null) return;
            relationGroup.ToList().ForEach(relation => genre.AddCategory(relation.CategoryId));
        });
        return new SearchOutput<Genre>(input.Page, input.PerPage, total, items);
    }
    
    private IQueryable<Genre> AddOrderToQuery(
        IQueryable<Genre> query,
        string orderProperty,
        SearchOrder order)
    {
        var orderedQuery = (orderProperty.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => query.OrderBy(x => x.Name),
            ("name", SearchOrder.Desc) => query.OrderByDescending(x => x.Name),
            ("id", SearchOrder.Asc) => query.OrderBy(x => x.Id),
            ("id", SearchOrder.Desc) => query.OrderByDescending(x => x.Id),
            ("createdat", SearchOrder.Asc) => query.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name)
        };

        return orderedQuery.ThenBy(x => x.CreatedAt);
    }
}