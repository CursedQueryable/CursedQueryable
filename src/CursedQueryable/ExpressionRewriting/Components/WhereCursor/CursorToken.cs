using System.Linq.Expressions;
using CursedQueryable.Options;

namespace CursedQueryable.ExpressionRewriting.Components.WhereCursor;

/// <summary>
///     Encapsulates the relationship between a property of a database entity and a value parsed from an incoming cursor.
/// </summary>
internal class CursorToken
{
    private static readonly Expression Zero = Expression.Constant(0);

    private Expression ValueConstant => Expression.Constant(Value, Type);

    /// <summary>
    ///     An expression that accesses the property value on an entity.
    /// </summary>
    public Expression PropertyExpression { get; init; } = default!;

    /// <summary>
    ///     The property type, as defined on the original entity.
    /// </summary>
    public Type Type { get; init; } = default!;

    /// <summary>
    ///     The boxed property value to use in comparisons, as defined in the cursor.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    ///     The direction of ordering, as defined by the use of .OrderBy() vs .OrderByDescending() in the queryable.
    /// </summary>
    public Direction Direction { get; init; }

    public Expression GetEqualsComparison()
    {
        return Expression.Equal(PropertyExpression, ValueConstant);
    }

    public Expression GetDirectionalComparison(NullBehaviour nullBehaviour)
    {
        // This combines both the NullBehaviour and Direction to determine whether null values are to be treated as less
        // or greater than non-null values.
        Func<Expression, Expression, Expression> nullComparer =
            (int)Direction == (int)nullBehaviour
                ? Expression.NotEqual
                : Expression.Equal;

        Func<Expression, Expression, Expression> valueComparer =
            Direction == Direction.Backwards
                ? Expression.LessThan
                : Expression.GreaterThan;

        if (Value == null)
            return nullComparer(PropertyExpression, ValueConstant);

        if (Type == typeof(string))
        {
            var compareMethod = Type.GetMethod(nameof(string.Compare), [Type, Type])!;
            var compareCall = Expression.Call(compareMethod, PropertyExpression, ValueConstant);
            return valueComparer(compareCall, Zero);
        }

        if (Nullable.GetUnderlyingType(Type) is { } underlyingType)
        {
            var nullConstant = Expression.Constant(null, Type);

            var compareMethod = typeof(Nullable)
                .GetMethod(nameof(Nullable.Compare))!
                .MakeGenericMethod(underlyingType);

            var compareCall = Expression.Call(compareMethod, PropertyExpression, ValueConstant);
            var nullCheck = nullComparer(PropertyExpression, nullConstant);
            var valueCheck = valueComparer(compareCall, Zero);

            return Expression.OrElse(nullCheck, valueCheck);
        }

        return valueComparer(PropertyExpression, ValueConstant);
    }
}