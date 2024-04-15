using System.Linq.Expressions;

namespace CursedQueryable.ExpressionRewriting.Components;

/// <summary>
///     Tracks ordering statements in the expression tree to allow access to a simplified ordering chain.
/// </summary>
internal class Ordering
{
    private readonly List<Info> _chain = [];
    private bool _completed;

    public IReadOnlyCollection<Info> Chain => _chain;

    public void OnTraversingUp(Expression node)
    {
        if (node is MethodCallExpression methodCall
            && methodCall.Method.DeclaringType == typeof(Queryable)
            && methodCall.Method.Name is nameof(Queryable.OrderBy)
                or nameof(Queryable.OrderByDescending)
                or nameof(Queryable.ThenBy)
                or nameof(Queryable.ThenByDescending))
            HandleOrderingNode(methodCall);
    }

    private void HandleOrderingNode(MethodCallExpression node)
    {
        // Once the chain is complete, ignore any redundant ordering nodes
        if (_completed)
            return;

        var operand = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;

        var direction = node.Method.Name is nameof(Queryable.OrderByDescending) or nameof(Queryable.ThenByDescending)
            ? Direction.Backwards
            : Direction.Forwards;

        var info = new Info { Expression = operand.Body, Direction = direction };

        // Insert at 0 because we're traversing up the tree and nodes will be in reverse order as a result
        _chain.Insert(0, info);

        if (node.Method.Name is nameof(Queryable.OrderBy) or nameof(Queryable.OrderByDescending))
            _completed = true;
    }

    public class Info
    {
        public Expression Expression { get; init; } = default!;
        public Direction Direction { get; init; }
    }
}