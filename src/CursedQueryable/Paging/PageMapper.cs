namespace CursedQueryable.Paging;

/// <summary>
///     Mapping interface use when converting <see cref="ICursedWrapper{T}" />.
/// </summary>
/// <typeparam name="TPage">The type of the result page.</typeparam>
/// <typeparam name="TNode">The type of the elements as matching the original <see cref="IQueryable{T}" />.</typeparam>
/// <typeparam name="TOptions">The type of the options to use, inheriting from <see cref="PageOptions" />.</typeparam>
public interface IPageMapper<TPage, in TNode, TOptions>
    where TPage : class
    where TNode : class
    where TOptions : PageOptions
{
    /// <summary>
    ///     Synchronously map to a resultant TPage.
    /// </summary>
    /// <param name="enumerable">An enumerable of <see cref="ICursedWrapper{T}" /> to use in mapping.</param>
    /// <param name="context">A <see cref="PageMappingContext{T}" /> to use in mapping.</param>
    /// <returns>A TPage.</returns>
    TPage Map(IEnumerable<ICursedWrapper<TNode>> enumerable,
        PageMappingContext<TOptions> context);

    /// <summary>
    ///     Asynchronously map to a resultant TPage.
    /// </summary>
    /// <param name="asyncEnumerable">An async enumerable of <see cref="ICursedWrapper{T}" /> to use in mapping.</param>
    /// <param name="context">A <see cref="PageMappingContext{T}" /> to use in mapping.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A TPage.</returns>
    Task<TPage> MapAsync(IAsyncEnumerable<ICursedWrapper<TNode>> asyncEnumerable,
        PageMappingContext<TOptions> context,
        CancellationToken cancellationToken);
}

/// <summary>
///     The default implementation of <see cref="IPageMapper{TPage,TNode,TOptions}" />.
/// </summary>
/// <typeparam name="TNode">The type of the elements as matching the original <see cref="IQueryable{T}" />.</typeparam>
public sealed class PageMapper<TNode> : IPageMapper<Page<TNode>, TNode, PageOptions>
    where TNode : class
{
    private static readonly Func<ICursedWrapper<TNode>, Page<TNode>.PageEdge> ConvertToEdge =
        wrapper => new Page<TNode>.PageEdge
        {
            Cursor = Cursor.Encode(wrapper),
            Node = wrapper.Node
        };

    /// <summary>
    ///     Synchronously map to a resultant <see cref="Page{T}" />
    /// </summary>
    /// <param name="enumerable">An enumerable of <see cref="ICursedWrapper{T}" /> to use in mapping.</param>
    /// <param name="context">A <see cref="PageMappingContext{T}" /> to use in mapping.</param>
    /// <returns>A <see cref="Page{T}" /></returns>
    public Page<TNode> Map(IEnumerable<ICursedWrapper<TNode>> enumerable,
        PageMappingContext<PageOptions> context)
    {
        var edges = enumerable
            .Select(ConvertToEdge)
            .ToList();

        return ConvertToPage(edges, context);
    }

    /// <summary>
    ///     Asynchronously map to a resultant <see cref="Page{T}" />
    /// </summary>
    /// <param name="asyncEnumerable">An async enumerable of <see cref="ICursedWrapper{T}" /> to use in mapping.</param>
    /// <param name="context">A <see cref="PageMappingContext{T}" /> to use in mapping.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Page{T}" /></returns>
    public async Task<Page<TNode>> MapAsync(IAsyncEnumerable<ICursedWrapper<TNode>> asyncEnumerable,
        PageMappingContext<PageOptions> context,
        CancellationToken cancellationToken)
    {
        var edges = await asyncEnumerable
            .Select(ConvertToEdge)
            .ToListAsync(cancellationToken);

        return ConvertToPage(edges, context);
    }

    private static Page<TNode> ConvertToPage(List<Page<TNode>.PageEdge> edges,
        PageMappingContext<PageOptions> context)
    {
        var amountTrimmed = TrimEdges(edges, context.OriginalTake);

        if (context.Options.Direction == Direction.Backwards)
        {
            edges.Reverse();

            return new Page<TNode>
            {
                Edges = edges,
                Info = new Page<TNode>.PageInfo
                {
                    HasNextPage = null,
                    HasPreviousPage = amountTrimmed > 0,
                    StartCursor = edges.FirstOrDefault()?.Cursor,
                    EndCursor = edges.LastOrDefault()?.Cursor
                }
            };
        }

        return new Page<TNode>
        {
            Edges = edges,
            Info = new Page<TNode>.PageInfo
            {
                HasNextPage = amountTrimmed > 0,
                HasPreviousPage = null,
                StartCursor = edges.FirstOrDefault()?.Cursor,
                EndCursor = edges.LastOrDefault()?.Cursor
            }
        };
    }

    private static int TrimEdges(List<Page<TNode>.PageEdge> edges, int? originalTake)
    {
        // If the number of results exceeds the original .Take() value, then there is a next page of data.
        // Those extra results need to be discarded, however.
        if (edges.Count > originalTake)
        {
            var amountTrimmed = edges.Count - originalTake.Value;
            edges.RemoveRange(originalTake.Value, amountTrimmed);
            return amountTrimmed;
        }

        return 0;
    }
}