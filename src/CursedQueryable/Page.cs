namespace CursedQueryable;

/// <summary>
///     The pagination result for the default implementation of CursedQueryable.
/// </summary>
/// <typeparam name="TNode">The type of the elements as matching the original <see cref="IQueryable{T}" />.</typeparam>
public sealed class Page<TNode>
{
    /// <summary>
    ///     A collection of <see cref="PageEdge" />.
    /// </summary>
    public IList<PageEdge> Edges { get; init; } = default!;

    /// <summary>
    ///     A summary of navigational information for this Page.
    /// </summary>
    public PageInfo Info { get; init; } = default!;

    /// <summary>
    ///     Encapsulates a <see cref="Node" /> and its associated <see cref="Cursor" />.
    /// </summary>
    public class PageEdge
    {
        /// <summary>
        ///     A string token that uniquely identifies the location of the associated <see cref="Node" /> within the original data
        ///     set.
        /// </summary>
        public string Cursor { get; init; } = default!;

        /// <summary>
        ///     A data object matching the type specified for the original <see cref="IQueryable{T}" />.
        /// </summary>
        public TNode Node { get; init; } = default!;
    }

    /// <summary>
    ///     Summarizes pagination information.
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        ///     Whether there there were additional records located after the end of this page of data.
        ///     Will be null if the specified <see cref="Direction" /> was not <see cref="Direction.Forwards" />.
        /// </summary>
        public bool? HasNextPage { get; init; }

        /// <summary>
        ///     Whether there there were additional records located before the start of this page of data.
        ///     Will be null if the specified <see cref="Direction" /> was not <see cref="Direction.Backwards" />.
        /// </summary>
        public bool? HasPreviousPage { get; init; }

        /// <summary>
        ///     The cursor belonging to the first element in <see cref="Edges" />.
        /// </summary>
        public string? StartCursor { get; init; }

        /// <summary>
        ///     The cursor belonging to the last element in <see cref="Edges" />.
        /// </summary>
        public string? EndCursor { get; init; }
    }
}