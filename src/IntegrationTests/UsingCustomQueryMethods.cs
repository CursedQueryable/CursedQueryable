using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.IntegrationTests.Abstract.BasicTests;
using CursedQueryable.IntegrationTests.Abstract.BasicTests.Helpers;
using CursedQueryable.IntegrationTests.Data.Entities;
using FluentAssertions;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingCustomQueryMethods : BasicTestsBase
{
    protected override IEntityDescriptorProvider Provider => new TestProvider();

    [Fact]
    public async Task Rewriter_ignores_custom_method()
    {
        var queryable = CustomMethod(_rootQueryable)
            .Take(5);

        var page = await ToPage(queryable);

        page.Should().NotBeNull();
        page.Should().BeOfType<Page<Cat>>();
        page.Edges.Should().NotBeNull();
        page.Edges.Count.Should().Be(5);

        page.Info.Should().NotBeNull();
        page.Info.HasNextPage.Should().BeTrue();
        page.Info.HasPreviousPage.Should().BeNull();
        page.Info.StartCursor.Should().Be(page.Edges.First().Cursor);
        page.Info.EndCursor.Should().Be(page.Edges.Last().Cursor);
    }

    private static IQueryable<TSource> CustomMethod<TSource>(IQueryable<TSource> source)
    {
        var methodInfo = new Func<IQueryable<TSource>, IQueryable<TSource>>(CustomMethod).Method;
        var call = Expression.Call(null, methodInfo, source.Expression);
        return source.Provider.CreateQuery<TSource>(call);
    }

    // ReSharper disable once UnusedMember.Local - accessed via reflection
    private static IEnumerable<TSource> CustomMethod<TSource>(IEnumerable<TSource> source)
    {
        return source;
    }
}