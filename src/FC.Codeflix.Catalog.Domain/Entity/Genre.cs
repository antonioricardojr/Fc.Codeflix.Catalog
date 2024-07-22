using FC.Codeflix.Catalog.Domain.SeedWork;
using FC.Codeflix.Catalog.Domain.Validation;

namespace FC.Codeflix.Catalog.Domain.Entity;

public class Genre : AggregateRoot
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<Guid> Categories => _categories.AsReadOnly();
    private List<Guid> _categories = new ();

    public Genre(string name, bool isActive = true)
    {
        Name = name;
        IsActive = isActive;
        CreatedAt = DateTime.Now;
        Validate();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Update(string name)
    {
        Name = name;
        Validate();
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Name, nameof(Name));
    }

    public void AddCategory(Guid categoryGuid)
    {
        _categories.Add(categoryGuid);
        Validate();
    }

    public void RemoveCategory(Guid exampleGuid)
    {
        _categories.Remove(exampleGuid);
        Validate();
    }

    public void RemoveAllCategories()
    {
        _categories.Clear();
        Validate();
    }
}