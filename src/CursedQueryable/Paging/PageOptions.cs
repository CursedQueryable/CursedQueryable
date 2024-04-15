using CursedQueryable.Options;

namespace CursedQueryable.Paging;

/// <summary>
///     Configures pagination options for CursedQueryable requests.
/// </summary>
public class PageOptions
{
    /// <summary>
    ///     <see cref="T:CursedQueryable.Options.FrameworkOptions" /> to use for this request.
    /// </summary>
    public virtual FrameworkOptions FrameworkOptions { get; set; } = new();

    /// <summary>
    ///     The cursor to use when navigating the data set. Results will be located after or before this cursor, depending on
    ///     the specified <see cref="Direction" />. Optional.
    /// </summary>
    public virtual string? Cursor { get; set; }

    /// <summary>
    ///     The <see cref="T:CursedQueryable.Direction" /> to use when navigating the data set.
    /// </summary>
    public virtual Direction Direction { get; set; }
}