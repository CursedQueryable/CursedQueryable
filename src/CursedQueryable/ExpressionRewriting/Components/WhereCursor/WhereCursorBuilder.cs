using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.ExpressionRewriting.Common;
using CursedQueryable.ExpressionRewriting.Guards;
using CursedQueryable.Options;

namespace CursedQueryable.ExpressionRewriting.Components.WhereCursor;

/// <summary>
///     Generates expressions based on cursor and entity data to navigate the data set to the correct location.
///     This output is for use with generating a new .Where() call.
/// </summary>
internal class WhereCursorBuilder(WhereCursorBuilder.Context context)
{
    public bool TryBuild([MaybeNullWhen(false)] out MethodCallExpression node)
    {
        try
        {
            var guard = new CursorGuard(context.EntityDescriptor.Type, context.ColProps);
            var cursor = guard.Decode(context.Cursor);
            var lambda = GetLambda(cursor);
            var method = Consts.WhereMethodInfo.MakeGenericMethod(context.EntityDescriptor.Type);
            node = Expression.Call(null, method, context.Antecedent, lambda);
            return true;
        }
        catch
        {
            if (context.BadCursorBehaviour == BadCursorBehaviour.ThrowException)
                throw;
        }

        node = null;
        return false;
    }

    private LambdaExpression GetLambda(JsonArray cursor)
    {
        var parameter = Expression.Parameter(context.EntityDescriptor.Type, nameof(WhereCursorBuilder));
        var mapper = new CursorTokenMapper(parameter, context.Direction);

        var keyCursorTokens = mapper.MapKeys(context.EntityDescriptor.PrimaryKeyComponents, cursor[1]?.AsArray());
        var colCursorTokens = mapper.MapCols(context.ColProps, cursor[2]?.AsArray());

        var keysExpr = GetCascadingExpression(keyCursorTokens, context.NullBehaviour);
        var colsExpr = GetCascadingExpression(colCursorTokens, context.NullBehaviour);

        var chain = keysExpr;

        if (colsExpr != null)
        {
            chain = chain != null
                ? Expression.OrElse(colsExpr, Expression.AndAlso(GetAllEqualExpression(colCursorTokens)!, chain))
                : colsExpr;
        }

        var funcType = typeof(Func<,>).MakeGenericType(context.EntityDescriptor.Type, typeof(bool));
        return Expression.Lambda(funcType, chain!, parameter);
    }

    private static Expression? GetAllEqualExpression(IEnumerable<CursorToken> cursorTokens)
    {
        // Outputs an expression similar to:
        // 1 cursorToken:  x => x.Col1 == val1
        // 2 cursorTokens: x => x.Col1 == val1 && x.Col2 == val2
        // 3 cursorTokens: x => x.Col1 == val1 && x.Col2 == val2 && x.Col3 == val3
        // etc.

        Expression? chain = null;

        foreach (var cursorToken in cursorTokens)
        {
            var expr = cursorToken.GetEqualsComparison();

            chain = chain != null
                ? Expression.AndAlso(chain, expr)
                : expr;
        }

        return chain;
    }

    private static Expression? GetCascadingExpression(IEnumerable<CursorToken> cursorTokens,
        NullBehaviour nullBehaviour)
    {
        // Outputs an expression similar to:
        // 1 cursorToken:  x => x.Col1 > val1
        // 2 cursorTokens: x => x.Col1 > val1 || (x.Col1 == val1 && x.Col2 > val2)
        // 3 cursorTokens: x => x.Col1 > val1 || (x.Col1 == val1 && (x.Col2 > val2 || (x.Col2 == val2 && x.Col3 > val3)))
        // etc.
        // Note that depending on direction of the cursorToken, the comparison will be 'less than' instead of 'greater than'.

        Expression? chain = null;
        var reversed = cursorTokens.Reverse().ToList();

        for (var i = 0; i < reversed.Count; i++)
        {
            var cursorToken = reversed[i];
            var expr = cursorToken.GetDirectionalComparison(nullBehaviour);

            if (i + 1 < reversed.Count)
            {
                var otherProp = reversed[i + 1];
                var otherExpr = otherProp.GetEqualsComparison();
                expr = Expression.AndAlso(otherExpr, expr);
            }

            chain = chain != null
                ? Expression.OrElse(expr, chain)
                : expr;
        }

        return chain;
    }

    internal class Context
    {
        public IEntityDescriptor EntityDescriptor { get; init; } = default!;
        public BadCursorBehaviour BadCursorBehaviour { get; init; }
        public string Cursor { get; init; } = default!;
        public IReadOnlyCollection<Ordering.Info> ColProps { get; init; } = default!;
        public Direction Direction { get; init; }
        public NullBehaviour NullBehaviour { get; init; }
        public Expression Antecedent { get; init; } = default!;
    }
}