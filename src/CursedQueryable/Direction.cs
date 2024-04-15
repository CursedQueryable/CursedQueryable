namespace CursedQueryable;

/// <summary>
///     Determines the direction of pagination.
/// </summary>
public enum Direction
{
    /// <summary>
    ///     The returned dataset will be rows located immediately <i>after</i> any supplied cursor.
    /// </summary>
    Forwards,

    /// <summary>
    ///     The returned dataset will be rows located immediately <i>before</i> any supplied cursor.
    /// </summary>
    Backwards
}