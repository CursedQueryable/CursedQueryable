using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using CursedQueryable.Exceptions;

namespace CursedQueryable.EntityDescriptors;

/// <summary>
///     Provides IEntityDescriptor metadata instances for IQueryable expressions.
/// </summary>
public interface IEntityDescriptorProvider
{
    /// <summary>
    ///     Tries to get an <see cref="IEntityDescriptor" /> for the input Expression.
    /// </summary>
    /// <param name="expression">
    ///     The Expression to use when getting an <see cref="IEntityDescriptor" />.
    /// </param>
    /// <param name="entityDescriptor">
    ///     When this method returns true, contains an instance of <see cref="IEntityDescriptor" />; otherwise, null.
    /// </param>
    /// <returns>
    ///     true if the Expression was able to provide an <see cref="IEntityDescriptor" />; otherwise, false.
    /// </returns>
    bool TryGetEntityDescriptor(Expression expression, [MaybeNullWhen(false)] out IEntityDescriptor entityDescriptor);
}

internal class NoEntityDescriptorProvider : IEntityDescriptorProvider
{
    public bool TryGetEntityDescriptor(Expression expression, out IEntityDescriptor entityDescriptor)
    {
        throw new NoEntityDescriptorProviderException();
    }
}