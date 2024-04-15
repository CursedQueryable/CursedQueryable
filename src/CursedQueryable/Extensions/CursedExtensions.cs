using CursedQueryable.Paging;

namespace CursedQueryable.Extensions;

/// <summary>
///     Synchronous extension methods for <see cref="IQueryable{T}" />.
/// </summary>
public static class CursedExtensions
{
    /// <summary>
    ///     Synchronously creates a <see cref="Page{T}" /> from a <see cref="IQueryable{T}" />.
    /// </summary>
    /// <param name="queryable">An <see cref="IQueryable{T}" /> to create the page from.</param>
    /// <param name="opts">Action to configure <see cref="PageOptions" /> for the request.</param>
    /// <typeparam name="T">The type of the elements of queryable.</typeparam>
    /// <returns>A page of results.</returns>
    public static Page<T> ToPage<T>(this IQueryable<T> queryable,
        Action<PageOptions> opts)
        where T : class
    {
        return Dispatch<T>(queryable, opts);
    }

    /// <summary>
    ///     Synchronously creates a <see cref="Page{T}" /> from a <see cref="IQueryable{T}" />.
    /// </summary>
    /// <param name="queryable">An <see cref="IQueryable{T}" /> to create the page from.</param>
    /// <param name="cursor">A continuation token obtained from a previously fetched page.</param>
    /// <param name="direction">The <see cref="Direction" /> of traversal for the request.</param>
    /// <typeparam name="T">The type of the elements of queryable.</typeparam>
    /// <returns>A page of results.</returns>
    public static Page<T> ToPage<T>(this IQueryable<T> queryable,
        string? cursor,
        Direction direction = Direction.Forwards)
        where T : class
    {
        return Dispatch<T>(queryable, opts =>
        {
            opts.Cursor = cursor;
            opts.Direction = direction;
        });
    }

    private static Page<T> Dispatch<T>(IQueryable queryable, Action<PageOptions> opts)
        where T : class
    {
        var mapper = new PageMapper<T>();
        var builder = new PageBuilder<Page<T>, T, PageOptions>(mapper);
        var options = new PageOptions
        {
            FrameworkOptions = CursedExtensionsConfig.Get()
        };
        opts.Invoke(options);
        return builder.ToPage(queryable, options);
    }
}