using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.Exceptions;
using Xunit;

namespace CursedQueryable.UnitTests;

[Trait("Category", "Cursed Framework - Unit Tests")]
public class NoEntityDescriptorProviderTests
{
    private readonly NoEntityDescriptorProvider _provider = new();

    [Fact]
    public void TryGetEntityDescriptor_throws()
    {
        var expression = Expression.Empty();

        Assert.Throws<NoEntityDescriptorProviderException>(() => _provider.TryGetEntityDescriptor(expression, out _));
    }
}