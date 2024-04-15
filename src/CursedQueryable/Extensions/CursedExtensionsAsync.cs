using CursedQueryable.Paging;

namespace CursedQueryable.Extensions;

/// <summary>
///     Asynchronous extension methods for <see cref="IQueryable{T}" />.
/// </summary>
public static class CursedExtensionsAsync
{
    /// <summary>
    ///     Asynchronously creates a <see cref="Page{T}" /> from a <see cref="IQueryable{T}" />.
    /// </summary>
    /// <param name="queryable">An <see cref="IQueryable{T}" /> to create the page from.</param>
    /// <param name="opts">Action to configure <see cref="PageOptions" /> for the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <typeparam name="T">The type of the elements of queryable.</typeparam>
    /// <returns>A page of results.</returns>
    public static Task<Page<T>> ToPageAsync<T>(this IQueryable<T> queryable,
        Action<PageOptions> opts,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return DispatchAsync<T>(queryable, opts, cancellationToken);
    }

    /// <summary>
    ///     Synchronously creates a <see cref="Page{T}" /> from a <see cref="IQueryable{T}" />.
    /// </summary>
    /// <param name="queryable">An <see cref="IQueryable{T}" /> to create the page from.</param>
    /// <param name="cursor">A continuation token obtained from a previously fetched page.</param>
    /// <param name="direction">The <see cref="Direction" /> of traversal for the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <typeparam name="T">The type of the elements of queryable.</typeparam>
    /// <returns>A page of results.</returns>
    public static Task<Page<T>> ToPageAsync<T>(this IQueryable<T> queryable,
        string? cursor,
        Direction direction = Direction.Forwards,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return DispatchAsync<T>(queryable, opts =>
        {
            opts.Cursor = cursor;
            opts.Direction = direction;
        }, cancellationToken);
    }

    private static async Task<Page<T>> DispatchAsync<T>(IQueryable queryable, Action<PageOptions> opts,
        CancellationToken cancellationToken)
        where T : class
    {
        var mapper = new PageMapper<T>();
        var builder = new PageBuilder<Page<T>, T, PageOptions>(mapper);
        var options = new PageOptions
        {
            FrameworkOptions = CursedExtensionsConfig.Get()
        };
        opts.Invoke(options);
        return await builder.ToPageAsync(queryable, options, cancellationToken);
    }
}