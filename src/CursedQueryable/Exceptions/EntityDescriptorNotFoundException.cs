namespace CursedQueryable.Exceptions;

/// <summary>
///     Represents errors that occur when finding an EntityDescriptor for an expression tree.
/// </summary>
public sealed class EntityDescriptorNotFoundException(string message) : Exception(message);