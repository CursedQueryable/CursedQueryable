using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.Exceptions;
using CursedQueryable.IntegrationTests.Abstract.BasicTests;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingNoEntityDescriptor : BasicTestsBase
{
    protected override IEntityDescriptorProvider Provider => new NoDescriptorProvider();

    [Fact]
    public async Task EntityDescriptor_not_found_throws()
    {
        await Assert.ThrowsAsync<EntityDescriptorNotFoundException>(() => ToPage(_rootQueryable));
    }

    public class NoDescriptorProvider : IEntityDescriptorProvider
    {
        public bool TryGetEntityDescriptor(Expression expression, out IEntityDescriptor entityDescriptor)
        {
            entityDescriptor = null!;
            return false;
        }
    }
}