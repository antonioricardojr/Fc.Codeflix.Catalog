namespace FC.Codeflix.Catalog.Application.Exceptions;

public class RelatedAggregateException : Exception
{
    
    public RelatedAggregateException(string? message) : base(message)
    {
    }
    
    public static void ThrowIfNull(object? @object, string exceptionMessage)
    {
        if (@object is null)
        {
            throw new RelatedAggregateException(exceptionMessage);
        }
    }
}