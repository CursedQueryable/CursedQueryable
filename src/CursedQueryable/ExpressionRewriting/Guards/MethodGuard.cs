using System.Linq.Expressions;

namespace CursedQueryable.ExpressionRewriting.Guards;

/// <summary>
///     Throws NotSupportedException if there is an unsupported Queryable method call in the expression tree.
/// </summary>
internal class MethodGuard : ExpressionVisitor
{
    private static readonly IReadOnlyCollection<string> SupportedQueryableMethods = new HashSet<string>
    {
        nameof(Queryable.OrderBy),
        nameof(Queryable.OrderByDescending),
        nameof(Queryable.ThenBy),
        nameof(Queryable.ThenByDescending),
        nameof(Queryable.Select),
        nameof(Queryable.Skip),
        nameof(Queryable.Take),
        nameof(Queryable.Where)
    };

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(Queryable) && !SupportedQueryableMethods.Contains(node.Method.Name))
        {
            throw new NotSupportedException(
                $"CursedQueryable does not support Queryable method '{node.Method.Name}'. Encountered at: {node}");
        }

        return base.VisitMethodCall(node);
    }
}