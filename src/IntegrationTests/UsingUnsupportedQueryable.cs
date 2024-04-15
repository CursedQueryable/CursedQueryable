using CursedQueryable.IntegrationTests.Abstract.BasicTests;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingUnsupportedQueryable : BasicTestsBase
{
    [Fact]
    public async Task GroupBy_throws()
    {
        var queryable = _rootQueryable
            .GroupBy(cat => cat.Sex)
            .Select(g => new
            {
                Sex = g.Key,
                Max = g.Max(c => c.Name)
            });

        await Assert.ThrowsAsync<NotSupportedException>(() => ToPage(queryable));
    }

    [Fact]
    public async Task SelectMany_throws()
    {
        var queryable = _rootQueryable
            .SelectMany(cat => cat.Kittens);

        await Assert.ThrowsAsync<NotSupportedException>(() => ToPage(queryable));
    }

    [Fact]
    public async Task Take_OrderBy_throws()
    {
        var queryable = _rootQueryable
            .Take(10)
            .OrderBy(cat => cat.Name);

        await Assert.ThrowsAsync<NotSupportedException>(() => ToPage(queryable));
    }

    [Fact]
    public async Task Take_Skip_throws()
    {
        var queryable = _rootQueryable
            .Take(10)
            .Skip(1);

        await Assert.ThrowsAsync<NotSupportedException>(() => ToPage(queryable));
    }

    [Fact]
    public async Task Skip_OrderBy_throws()
    {
        var queryable = _rootQueryable
            .Skip(10)
            .OrderBy(cat => cat.Name);

        await Assert.ThrowsAsync<NotSupportedException>(() => ToPage(queryable));
    }
}