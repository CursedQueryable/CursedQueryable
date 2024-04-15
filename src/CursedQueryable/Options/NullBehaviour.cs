namespace CursedQueryable.Options;

/// <summary>
///     Specifies how NULL values behave when they are sorted by the underlying database.
/// </summary>
public enum NullBehaviour
{
    /// <summary>
    ///     NULL values are treated as smaller than everything else; i.e. ASC ordering will put NULLs at the top of the set.
    /// </summary>
    /// <remarks>
    ///     This is the default behaviour of SQL Server, MySQL, and SQLite.
    /// </remarks>
    SmallerThanNonNullable,

    /// <summary>
    ///     NULL values are treated as larger than everything else; i.e. ASC ordering will put NULLs at the bottom of the set.
    /// </summary>
    /// <remarks>
    ///     This is the default behaviour of Postgres and Oracle.
    /// </remarks>
    LargerThanNonNullable
}