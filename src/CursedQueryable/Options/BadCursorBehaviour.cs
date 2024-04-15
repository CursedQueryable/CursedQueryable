namespace CursedQueryable.Options;

/// <summary>
///     Specifies what the error handling behaviour should be when
///     <see cref="T:CursedQueryable.Exceptions.BadCursorException" /> is thrown.
/// </summary>
public enum BadCursorBehaviour
{
    /// <summary>
    ///     Cursors that are unable to be decoded/applied will have any errors suppressed and will instead be treated as null.
    /// </summary>
    Ignore,

    /// <summary>
    ///     Cursors that are unable to be decoded/applied will not have
    ///     <see cref="T:CursedQueryable.Exceptions.BadCursorException" /> suppressed, allowing it to be handled elsewhere
    ///     in the application.
    /// </summary>
    ThrowException
}