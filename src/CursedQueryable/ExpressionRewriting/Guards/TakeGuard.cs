using System.Linq.Expressions;

namespace CursedQueryable.ExpressionRewriting.Guards;

/// <summary>
///     Throws NotSupportedException if there is .Take() in the expression tree followed by an unsupported Queryable method
///     call.
/// </summary>
internal class TakeGuard : ExpressionVisitor
{
    private static readonly IReadOnlyCollection<string> AllowedAfterTakeMethods = new HashSet<string>
    {
        nameof(Queryable.Select)
    };

    private bool _hasTake;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var visited = base.VisitMethodCall(node);

        if (node.Method.DeclaringType == typeof(Queryable))
        {
            if (_hasTake)
            {
                if (!AllowedAfterTakeMethods.Contains(node.Method.Name))
                {
                    throw new NotSupportedException(
                        $"CursedQueryable only supports [{string.Join(", ", AllowedAfterTakeMethods)}] calls after .Take(). Encountered at: {node}");
                }
            }
            else if (node.Method.Name == nameof(Queryable.Take))
                _hasTake = true;
        }

        return visited;
    }
}