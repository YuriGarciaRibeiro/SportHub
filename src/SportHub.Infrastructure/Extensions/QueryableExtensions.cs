namespace SportHub.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> If<T>(
        this IQueryable<T> query,
        bool condition,
        Func<IQueryable<T>, IQueryable<T>> transform)
    {
        return condition ? transform(query) : query;
    }
}