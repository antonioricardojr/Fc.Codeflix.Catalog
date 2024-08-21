using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace EndToEndTests.Api.Genre.Common;

public class GenrePersistence
{
    private readonly CodeflixCatalogDbContext _context;
    public GenrePersistence(CodeflixCatalogDbContext context)
    {
        _context = context;
    }
    
    public Task<FC.Codeflix.Catalog.Domain.Entity.Genre?> GetById(Guid id)
        => _context.Genres.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    
    public async Task BulkInsert(List<FC.Codeflix.Catalog.Domain.Entity.Genre> exampleGenresList)
    {
        await _context.Genres.AddRangeAsync(exampleGenresList);
        await _context.SaveChangesAsync();
    }
    
    public async Task BulkInsertGenresCategoriesRelationsList(List<GenresCategories> genresCategoriesList)
    {
        await _context.GenresCategories.AddRangeAsync(genresCategoriesList);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GenresCategories>> GetGenresCategoriesRelationsByGenreId(Guid id)
    {
        return await _context.GenresCategories.AsNoTracking().Where(relation => relation.GenreId == id).ToListAsync();
    }
}