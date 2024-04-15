namespace CursedQueryable.Paging;

/// <summary>
///     Provides context to be used during mapping.
/// </summary>
/// <typeparam name="TOptions">The type of the options to use, inheriting from <see cref="PageOptions" />.</typeparam>
public sealed class PageMappingContext<TOptions>
    where TOptions : PageOptions
{
    /// <summary>
    ///     The value specified in any .Take() used in the original <see cref="IQueryable{T}" />.
    /// </summary>
    /// <remarks>
    ///     If the number of results brought back is less than or equal to this value, there is no next/previous page of data.
    ///     Will be null if there was no .Take() specified in the original <see cref="IQueryable{T}" />.
    /// </remarks>
    public int? OriginalTake { get; init; }

    /// <summary>
    ///     The options to use during mapping.
    /// </summary>
    public TOptions Options { get; init; } = default!;
}