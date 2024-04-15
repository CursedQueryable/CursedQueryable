using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.ExpressionRewriting.Common;

namespace CursedQueryable.ExpressionRewriting.Components.OrderByKeys;

/// <summary>
///     Injects any required calls into the expression tree that order the dataset by primary key(s).
/// </summary>
internal class OrderByKeysInjector(CursedRewriter.Context context, Ordering ordering) : IExpressionInjector
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

        if (_injectionPoint == null && IsOrderingNode(mce))
            _injectionPoint = position;
    }

    public Expression OnTraversingDown(int position, Expression node, IEntityDescriptor entityDescriptor)
    {
        _injectionPoint ??= _injectionPointMinimum;

        if (position != _injectionPoint)
            return node;

        var orderByKeysContext = new OrderByKeysBuilder.Context
        {
            Antecedent = node,
            Type = entityDescriptor.Type,
            KeyProps = entityDescriptor.PrimaryKeyComponents,
            StartNewChain = !ordering.Chain.Any(),
            Direction = context.Direction
        };

        var builder = new OrderByKeysBuilder(orderByKeysContext);
        return builder.Build();
    }

    private static bool IsBreakingNode(MethodCallExpression node)
    {
        return node.Method.Name
            is nameof(Queryable.Select)
            or nameof(Queryable.Skip)
            or nameof(Queryable.Take);
    }

    private static bool IsOrderingNode(MethodCallExpression node)
    {
        return node.Method.Name
            is nameof(Queryable.OrderBy)
            or nameof(Queryable.OrderByDescending)
            or nameof(Queryable.ThenBy)
            or nameof(Queryable.ThenByDescending);
    }
}