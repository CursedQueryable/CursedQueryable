namespace CursedQueryable.Paging;

/// <summary>
///     Encapsulates metadata used to create cursors unique to a particular entity and dataset.
/// </summary>
public interface ICursedWrapper
{
    /// <summary>
    ///     An integer value representing a unique hash for the dataset this cursor belongs to.
    /// </summary>
    int Hash { get; }

    /// <summary>
    ///     A boxed array containing all Primary Key property values for the row.
    /// </summary>
    object?[]? Keys { get; }

    /// <summary>
    ///     A boxed array containing all ordered column values for the row.
    /// </summary>
    object?[]? Cols { get; }
}

/// <summary>
///     Encapsulates an entity/projection, as well as metadata used to create cursors unique to it.
/// </summary>
public interface ICursedWrapper<out T> : ICursedWrapper
{
    /// <summary>
    ///     The data node. This will either be the entity data itself or a projection of it.
    /// </summary>
    T Node { get; }
}

internal class CursedWrapper<T> : ICursedWrapper<T>
{
    public T Node { get; init; } = default!;
    public int Hash { get; init; }
    public object?[]? Keys { get; init; }
    public object?[]? Cols { get; init; }
}