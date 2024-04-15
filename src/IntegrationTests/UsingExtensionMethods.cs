using CursedQueryable.Extensions;
using CursedQueryable.IntegrationTests.Abstract.BasicTests.Helpers;
using CursedQueryable.IntegrationTests.Data;
using CursedQueryable.IntegrationTests.Data.Entities;
using FluentAssertions;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Unit Tests")]
//[Collection(nameof(StaticCollection))]
public class UsingExtensionMethods
{
    private readonly IQueryable<Cat> _rootQueryable;

    public UsingExtensionMethods()
    {
        _rootQueryable = TestData.GenerateCats().ToList().AsQueryable();

        CursedExtensionsConfig.Configure(o => { o.Provider = new TestProvider(); });
    }

    [Fact]
    public void ToPage_cursor()
    {
        var queryable = _rootQueryable
            .Take(5);

        var page = queryable.ToPage(default(string));

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

    [Fact]
    public void ToPage_configurator()
    {
        var queryable = _rootQueryable
            .Take(5);

        var page = queryable.ToPage(_ => { });

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

    [Fact]
    public async Task ToPageAsync_cursor()
    {
        var queryable = _rootQueryable
            .Take(5);

        var page = await queryable.ToPageAsync(default(string));

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

    [Fact]
    public async Task ToPageAsync_configurator()
    {
        var queryable = _rootQueryable
            .Take(5);

        var page = await queryable.ToPageAsync(_ => { });

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
}