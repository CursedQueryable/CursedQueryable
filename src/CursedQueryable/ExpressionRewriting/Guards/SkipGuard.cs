using System.Linq.Expressions;

namespace CursedQueryable.ExpressionRewriting.Guards;

/// <summary>
///     Throws NotSupportedException if there is .Skip() in the expression tree followed by an unsupported Queryable method
///     call.
/// </summary>
internal class SkipGuard : ExpressionVisitor
{
    private static readonly IReadOnlyCollection<string> AllowedAfterSkipMethods = new HashSet<string>
    {
        nameof(Queryable.Take),
        nameof(Queryable.Select)
    };

    private bool _hasSkip;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var visited = base.VisitMethodCall(node);

        if (node.Method.DeclaringType == typeof(Queryable))
        {
            if (_hasSkip)
            {
                if (!AllowedAfterSkipMethods.Contains(node.Method.Name))
                {
                    throw new NotSupportedException(
                        $"CursedQueryable only supports [{string.Join(", ", AllowedAfterSkipMethods)}] calls after .Skip(). Encountered at: {node}");
                }
            }
            else if (node.Method.Name == nameof(Queryable.Skip))
                _hasSkip = true;
        }

        return visited;
    }
}