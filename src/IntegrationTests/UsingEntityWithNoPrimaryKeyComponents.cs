using System.Linq.Expressions;
using System.Reflection;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.IntegrationTests.Abstract.BasicTests;
using CursedQueryable.IntegrationTests.Data.Entities;
using CursedQueryable.Paging;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingEntityWithNoPrimaryKeyComponents : BasicTestsBase
{
    protected override IEntityDescriptorProvider Provider => new NoPrimaryKeyProvider();

    [Fact]
    public async Task Should_work_without_primary_key()
    {
        var queryable = _rootQueryable
            .OrderBy(cat => cat.Sex)
            .Take(1);

        var page = await ToPage(queryable);
        var cursor = page.Edges.First().Cursor;
        var decoded = Cursor.Decode(cursor);

        cursor = Cursor.Encode(new CursedWrapper<Cat>
        {
            Hash = decoded[0]!.GetValue<int>(),
            Keys = decoded[1]!.AsArray().Select(n => n!.GetValue<object>()).ToArray(),
            Cols = [.. decoded[2]!.AsArray().Select(n => n?.GetValue<object>())]
        });

        await ToPage(queryable, cursor);
    }

    private class NoPrimaryKeyProvider : IEntityDescriptorProvider
    {
        public bool TryGetEntityDescriptor(Expression expression, out IEntityDescriptor entityDescriptor)
        {
            entityDescriptor = new NoPrimaryKeyDescriptor();
            return true;
        }

        private class NoPrimaryKeyDescriptor : IEntityDescriptor
        {
            public Type Type { get; } = typeof(Cat);
            public IReadOnlyCollection<PropertyInfo> PrimaryKeyComponents { get; } = [];
        }
    }
}