using FC.Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace EndToEndTests.Api.Category.Common;

public class CategoryPersistence
{
    private readonly CodeflixCatalogDbContext _context;

    public CategoryPersistence(CodeflixCatalogDbContext context)
    {
        _context = context;
    }

    public Task<FC.Codeflix.Catalog.Domain.Entity.Category?> GetById(Guid id)
        => _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task BulkInsert(List<FC.Codeflix.Catalog.Domain.Entity.Category> exampleCategoriesList)
    {
        await _context.Categories.AddRangeAsync(exampleCategoriesList);
        await _context.SaveChangesAsync();
    }
}