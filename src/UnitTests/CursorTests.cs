using System.Text;
using CursedQueryable.Exceptions;
using CursedQueryable.Paging;
using FluentAssertions;
using Xunit;

namespace CursedQueryable.UnitTests;

[Trait("Category", "Cursed Framework - Unit Tests")]
public class CursorTests
{
    [Fact]
    public void Should_encode()
    {
        var wrapper = new CursedWrapper<object>
        {
            Hash = 1337,
            Keys = [1, Guid.Parse("00000000-0000-0000-0000-000000000001"), "Test1"],
            Cols = [new DateTime(3030, 1, 1).ToString("o"), null, "Test2"]
        };

        var base64 = Cursor.Encode(wrapper);
        var bytes = Convert.FromBase64String(base64);
        var json = Encoding.UTF8.GetString(bytes);

        json.Should()
            .Be(
                "[1337,[1,\"00000000-0000-0000-0000-000000000001\",\"Test1\"],[\"3030-01-01T00:00:00.0000000\",null,\"Test2\"]]");
    }

    [Fact]
    public void Should_encode_no_cols()
    {
        var wrapper = new CursedWrapper<object>
        {
            Hash = 1337,
            Keys = [1, Guid.Parse("00000000-0000-0000-0000-000000000001"), "Test1"]
        };

        var base64 = Cursor.Encode(wrapper);
        var bytes = Convert.FromBase64String(base64);
        var json = Encoding.UTF8.GetString(bytes);

        json.Should().Be("[1337,[1,\"00000000-0000-0000-0000-000000000001\",\"Test1\"],null]");
    }

    [Fact]
    public void Should_not_decode_null()
    {
        string? base64 = null;

        Assert.Throws<BadCursorException>(() => Cursor.Decode(base64!));
    }

    [Fact]
    public void Should_not_decode_empty_string()
    {
        var base64 = " ";

        Assert.Throws<BadCursorException>(() => Cursor.Decode(base64));
    }

    [Fact]
    public void Should_not_decode_garbage()
    {
        var base64 = "lol";

        Assert.Throws<BadCursorException>(() => Cursor.Decode(base64));
    }

    [Fact]
    public void Should_not_decode_empty_array()
    {
        // "[]"
        var base64 = "W10=";

        Assert.Throws<BadCursorException>(() => Cursor.Decode(base64));
    }

    [Fact]
    public void Should_not_decode_partial_array_1()
    {
        // "[1]"
        var base64 = "WzFd";

        Assert.Throws<BadCursorException>(() => Cursor.Decode(base64));
    }

    [Fact]
    public void Should_not_decode_partial_array_2()
    {
        // "[1,[]]"
        var base64 = "WzEsW11d";

        Assert.Throws<BadCursorException>(() => Cursor.Decode(base64));
    }

    [Fact]
    public void Should_decode()
    {
        // "[1,[],[]]"
        var base64 = "WzEsW10sW11d";
        var decoded = Cursor.Decode(base64);

        decoded.Should().NotBeNull();
    }

    [Fact]
    public void Should_decode_cols_null()
    {
        // "[1,[],null]"
        var base64 = "WzEsW10sbnVsbF0=";
        var decoded = Cursor.Decode(base64);

        decoded.Should().NotBeNull();
    }
}