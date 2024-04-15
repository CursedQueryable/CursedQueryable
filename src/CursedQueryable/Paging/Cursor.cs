using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CursedQueryable.Exceptions;

namespace CursedQueryable.Paging;

/// <summary>
///     A static helper class to encode/decode cursors.
/// </summary>
public static class Cursor
{
    /// <summary>
    ///     Creates a cursor for an <see cref="T:CursedQueryable.Paging.ICursedWrapper" /> instance.
    /// </summary>
    /// <param name="wrapper"></param>
    /// <returns>
    ///     A string value representing the unique cursor for this <see cref="T:CursedQueryable.Paging.ICursedWrapper" />
    ///     instance.
    /// </returns>
    /// <remarks>A cursor is basically a Base64 JSON array.</remarks>
    public static string Encode(ICursedWrapper wrapper)
    {
        var json = JsonSerializer.Serialize(new[]
        {
            wrapper.Hash as object,
            wrapper.Keys,
            wrapper.Cols
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     Decodes a cursor into a <see cref="JsonArray" />.
    /// </summary>
    /// <param name="cursor">A cursor, typically obtained from a previous CursedQueryable request.</param>
    /// <returns>A <see cref="JsonArray" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the array does not have the correct length (of 3).</exception>
    /// <exception cref="BadCursorException">Thrown when the input string cannot be decoded.</exception>
    public static JsonArray Decode(string cursor)
    {
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var json = Encoding.UTF8.GetString(bytes);
            var node = JsonNode.Parse(json)!.AsArray();

            if (node.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(cursor),
                    "Incorrect number of decoded cursor array entries.");
            }

            return node;
        }
        catch (Exception ex)
        {
            throw new BadCursorException($"Unable to decode cursor '{cursor}'", ex);
        }
    }
}