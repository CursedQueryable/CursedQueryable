using CursedQueryable.Exceptions;
using CursedQueryable.IntegrationTests.Abstract.BasicTests;
using CursedQueryable.IntegrationTests.Data.Entities;
using CursedQueryable.Options;
using CursedQueryable.Paging;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingCursors : BasicTestsBase
{
    [Fact]
    public async Task Cursor_with_null_cols_is_ok()
    {
        var queryable = _rootQueryable
            .Take(1);

        var page = await ToPage(queryable);

        var cursor = page.Edges.First().Cursor;
        var decoded = Cursor.Decode(cursor);

        cursor = Cursor.Encode(new CursedWrapper<Cat>
        {
            Hash = decoded[0]!.GetValue<int>(),
            Keys = decoded[1]!.AsArray().Select(d => d!.GetValue<object>()).ToArray(),
            Cols = null
        });

        await ToPage(queryable, cursor);
    }

    [Fact]
    public async Task Bad_cursor_ignored_when_behaviour_is_ignore()
    {
        var queryable = _rootQueryable
            .OrderBy(cat => cat.Sex)
            .Take(1);

        await ToPage(queryable, "fail", o => o.FrameworkOptions.BadCursorBehaviour = BadCursorBehaviour.Ignore);
    }

    [Fact]
    public async Task Bad_cursor_throws_when_behaviour_is_throw()
    {
        var queryable = _rootQueryable
            .OrderBy(cat => cat.Sex)
            .Take(1);

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable, "fail"));
    }

    [Fact]
    public async Task Cursor_with_mismatched_hash_is_bad()
    {
        var queryable1 = _rootQueryable
            .OrderBy(cat => cat.Sex)
            .Take(1);

        var page = await ToPage(queryable1);
        var cursor = page.Edges.First().Cursor;

        var queryable2 = _rootQueryable
            .OrderBy(cat => cat.Name)
            .Take(1);

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable2, cursor));
    }

    [Fact]
    public async Task Cursor_with_mismatched_type_is_bad()
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
            Keys = ["fail"],
            Cols = decoded[2]!.AsArray().Select(n => n?.GetValue<object>()).ToArray()
        });

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable, cursor));
    }

    [Fact]
    public async Task Cursor_with_incorrect_num_keys_is_bad()
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
            Keys = [.. decoded[1]!.AsArray().Select(n => n?.GetValue<object>()), "Extra"],
            Cols = decoded[2]!.AsArray().Select(n => n?.GetValue<object>()).ToArray()
        });

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable, cursor));
    }

    [Fact]
    public async Task Cursor_with_incorrect_null_keys_is_bad()
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
            Keys = null,
            Cols = decoded[2]!.AsArray().Select(n => n?.GetValue<object>()).ToArray()
        });

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable, cursor));
    }

    [Fact]
    public async Task Cursor_with_incorrect_num_cols_is_bad()
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
            Cols = [.. decoded[2]!.AsArray().Select(n => n?.GetValue<object>()), "Extra"]
        });

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable, cursor));
    }

    [Fact]
    public async Task Cursor_with_incorrect_null_cols_is_bad()
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
            Cols = null
        });

        await Assert.ThrowsAsync<BadCursorException>(() => ToPage(queryable, cursor));
    }
}