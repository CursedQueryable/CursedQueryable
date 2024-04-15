using System.Text.Json.Nodes;
using CursedQueryable.Exceptions;
using CursedQueryable.ExpressionRewriting.Components;
using CursedQueryable.Paging;

namespace CursedQueryable.ExpressionRewriting.Guards;

internal class CursorGuard(Type type, IEnumerable<Ordering.Info> ordering)
{
    public JsonArray Decode(string cursor)
    {
        var decoded = Cursor.Decode(cursor);

        // The hash doesn't need to be secure or anything, this is mostly just to provide
        // feedback if there's a mismatch.
        var expectedHash = GetHash();
        var actualHash = decoded[0]!.GetValue<int>();

        if (expectedHash != actualHash)
        {
            throw new BadCursorException(
                "The specified cursor appears to be for a different data set.");
        }

        return decoded;
    }

    public int GetHash()
    {
        List<string> parts =
        [
            type.Name,
            ..ordering.Select(tuple => tuple.Expression.ToString())
        ];

        var str = string.Join(",", parts);
        return CalculateHash(str);
    }

    // https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
    private static int CalculateHash(string str)
    {
        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + hash2 * 1566083941;
        }
    }
}