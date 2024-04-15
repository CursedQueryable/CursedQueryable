namespace CursedQueryable.Exceptions;

/// <summary>
///     Represents errors that occur when decoding and applying cursors.
/// </summary>
public sealed class BadCursorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BadCursorException" /> class
    ///     with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BadCursorException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadCursorException" /> class with a
    ///     specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (Nothing in
    ///     Visual Basic) if no inner exception is specified.
    /// </param>
    public BadCursorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}