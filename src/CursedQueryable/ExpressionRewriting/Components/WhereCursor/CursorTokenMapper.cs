using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using CursedQueryable.Exceptions;
using CursedQueryable.ExpressionRewriting.Common;

namespace CursedQueryable.ExpressionRewriting.Components.WhereCursor;

/// <summary>
///     Maps the values from an incoming cursor to the properties of a database entity.
/// </summary>
internal class CursorTokenMapper(Expression parameter, Direction direction)
{
    public IReadOnlyList<CursorToken> MapKeys(IReadOnlyCollection<PropertyInfo> properties, JsonArray? cursorKeys)
    {
        try
        {
            if ((cursorKeys?.Count ?? 0) != properties.Count)
                throw new Exception($"Expected {properties.Count} tokens, found {cursorKeys?.Count ?? 0}.");

            return properties
                .Select((propertyInfo, i) => new CursorToken
                {
                    PropertyExpression = Expression.Property(parameter, propertyInfo),
                    Type = propertyInfo.PropertyType,
                    Value = cursorKeys![i].Deserialize(propertyInfo.PropertyType),
                    Direction = direction
                })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new BadCursorException("Cursor had mismatched primary key tokens.", ex);
        }
    }

    public IReadOnlyList<CursorToken> MapCols(
        IReadOnlyCollection<Ordering.Info> properties,
        JsonArray? cursorCols)
    {
        try
        {
            if ((cursorCols?.Count ?? 0) != properties.Count)
                throw new Exception($"Expected {properties.Count} tokens, found {cursorCols?.Count ?? 0}.");

            return properties
                .Select((col, i) => new CursorToken
                {
                    PropertyExpression = new ParameterReplacer(parameter).Visit(col.Expression)!,
                    Type = col.Expression.Type,
                    Value = cursorCols![i].Deserialize(col.Expression.Type),
                    Direction = direction == Direction.Forwards ? col.Direction : 1 - col.Direction
                })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new BadCursorException("Cursor had mismatched ordering column tokens.", ex);
        }
    }
}