using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.ExpressionRewriting.Common;

namespace CursedQueryable.ExpressionRewriting.Components.WhereCursor;

/// <summary>
///     Injects any required calls into the expression tree that seek the dataset to the correct cursor position.
/// </summary>
internal class WhereCursorInjector(CursedRewriter.Context context, Ordering ordering) : IExpressionInjector
{
    private int? _injectionPoint;
    private int _injectionPointMinimum = 1;

    public void OnTraversingUp(int position, Expression node)
    {
        if (node is not MethodCallExpression mce || mce.Method.DeclaringType != typeof(Queryable))
            return;

        if (IsBreakingNode(mce))
        {
            _injectionPointMinimum = position + 1;
            _injectionPoint = null;
        }

        if (_injectionPoint == null && IsWhereNode(mce))
            _injectionPoint = position;
    }

    public Expression OnTraversingDown(int position, Expression node, IEntityDescriptor entityDescriptor)
    {
        _injectionPoint ??= _injectionPointMinimum;

        if (position != _injectionPoint || context.Cursor == null)
            return node;

        var whereCursorContext = new WhereCursorBuilder.Context
        {
            Antecedent = node,
            EntityDescriptor = entityDescriptor,
            BadCursorBehaviour = context.BadCursorBehaviour,
            Cursor = context.Cursor!,
            ColProps = ordering.Chain,
            Direction = context.Direction,
            NullBehaviour = context.NullBehaviour
        };

        var builder = new WhereCursorBuilder(whereCursorContext);

        if (builder.TryBuild(out var injected))
            return injected;

        return node;
    }

    private static bool IsBreakingNode(MethodCallExpression node)
    {
        return node.Method.Name
            is nameof(Queryable.Select)
            or nameof(Queryable.Skip)
            or nameof(Queryable.Take);
    }

    private static bool IsWhereNode(MethodCallExpression node)
    {
        return node.Method.Name
            is nameof(Queryable.Where);
    }
}