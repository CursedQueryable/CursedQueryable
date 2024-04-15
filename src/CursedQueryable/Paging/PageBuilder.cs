using CursedQueryable.ExpressionRewriting;

namespace CursedQueryable.Paging;

/// <summary>
///     Builder interface to use when paginating with CursedQueryable.
/// </summary>
/// <typeparam name="TPage">The type of the result page.</typeparam>
/// <typeparam name="TOptions">The type of the options to use, inheriting from <see cref="PageOptions" />.</typeparam>
public interface IPageBuilder<TPage, in TOptions>
    where TPage : class
    where TOptions : PageOptions
{
    /// <summary>
    ///     Synchronously rewrites, executes and remaps an <see cref="IQueryable" /> to TPage.
    /// </summary>
    /// <param name="original">The <see cref="IQueryable" /> instance to rewrite.</param>
    /// <param name="options">The options to use for this request.</param>
    /// <returns>A TPage.</returns>
    TPage ToPage(IQueryable original, TOptions options);

    /// <summary>
    ///     Asynchronously rewrites, executes and remaps an <see cref="IQueryable" /> to TPage.
    /// </summary>
    /// <param name="original">The <see cref="IQueryable" /> instance to rewrite.</param>
    /// <param name="options">The options to use for this request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A TPage.</returns>
    Task<TPage> ToPageAsync(IQueryable original, TOptions options, CancellationToken cancellationToken = default);
}

/// <summary>
///     The default implementation of <see cref="IPageBuilder{TPage,TOptions}" />.
/// </summary>
/// <param name="mapper">The <see cref="IPageMapper{TPage,TNode,TOptions}" /> to use during mapping.</param>
/// <typeparam name="TPage">The type of the result page.</typeparam>
/// <typeparam name="TNode">The type of the elements as matching the original <see cref="IQueryable{T}" />.</typeparam>
/// <typeparam name="TOptions">The type of the options to use, inheriting from <see cref="PageOptions" />.</typeparam>
public sealed class PageBuilder<TPage, TNode, TOptions>(IPageMapper<TPage, TNode, TOptions> mapper)
    : IPageBuilder<TPage, TOptions>
    where TPage : class
    where TNode : class
    where TOptions : PageOptions
{
    /// <summary>
    ///     Synchronously rewrites, executes and remaps an <see cref="IQueryable" /> to TPage.
    /// </summary>
    /// <param name="original">The <see cref="IQueryable" /> instance to rewrite.</param>
    /// <param name="options">The options to use for this request.</param>
    /// <returns>A TPage.</returns>
    public TPage ToPage(IQueryable original, TOptions options)
    {
        var (queryable, originalTake) = Rewrite(original, options);

        var enumerable = queryable.AsEnumerable();

        var mappingContext = new PageMappingContext<TOptions>
        {
            OriginalTake = originalTake,
            Options = options
        };

        return mapper.Map(enumerable, mappingContext);
    }

    /// <summary>
    ///     Asynchronously rewrites, executes and remaps an <see cref="IQueryable" /> to TPage.
    /// </summary>
    /// <param name="original">The <see cref="IQueryable" /> instance to rewrite.</param>
    /// <param name="options">The options to use for this request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A TPage.</returns>
    public Task<TPage> ToPageAsync(IQueryable original, TOptions options,
        CancellationToken cancellationToken = default)
    {
        var (queryable, originalTake) = Rewrite(original, options);

        var asyncEnumerable = queryable.ToAsyncEnumerable();

        var mappingContext = new PageMappingContext<TOptions>
        {
            OriginalTake = originalTake,
            Options = options
        };

        return mapper.MapAsync(asyncEnumerable, mappingContext, cancellationToken);
    }

    private static (IQueryable<CursedWrapper<TNode>> Queryable, int?) Rewrite(IQueryable queryable, PageOptions options)
    {
        var rewriterContext = new CursedRewriter.Context
        {
            Cursor = options.Cursor,
            Direction = options.Direction,
            NullBehaviour = options.FrameworkOptions.NullBehaviour,
            Provider = options.FrameworkOptions.Provider,
            BadCursorBehaviour = options.FrameworkOptions.BadCursorBehaviour
        };

        var result = CursedRewriter.Rewrite(rewriterContext, queryable.Expression);
        var newQueryable = queryable.Provider.CreateQuery<CursedWrapper<TNode>>(result.Expression);

        return (newQueryable, result.OriginalTake);
    }
}