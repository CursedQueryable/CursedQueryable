using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.Exceptions;
using CursedQueryable.ExpressionRewriting.Common;
using CursedQueryable.ExpressionRewriting.Components;
using CursedQueryable.ExpressionRewriting.Components.OrderByKeys;
using CursedQueryable.ExpressionRewriting.Components.SelectWrapper;
using CursedQueryable.ExpressionRewriting.Components.WhereCursor;
using CursedQueryable.ExpressionRewriting.Guards;
using CursedQueryable.Options;

namespace CursedQueryable.ExpressionRewriting;

/// <summary>
///     Examines and rewrites the entire expression tree to fit the required behaviour for CursedQueryable.
/// </summary>
internal class CursedRewriter : ExpressionVisitor
{
    private static readonly IReadOnlyCollection<Type> Guards =
        [typeof(MethodGuard), typeof(SkipGuard), typeof(TakeGuard)];

    private readonly Context _context;
    private readonly IReadOnlyCollection<IExpressionInjector> _injectors;
    private readonly Ordering _ordering;
    private IEntityDescriptor? _entityDescriptor;
    private int? _originalTake;
    private int _recursion;

    private CursedRewriter(Context context)
    {
        _context = context;
        _ordering = new Ordering();
        _injectors =
        [
            new OrderByKeysInjector(_context, _ordering),
            new WhereCursorInjector(_context, _ordering),
            new SelectWrapperInjector(_ordering)
        ];
    }

    public static Result Rewrite(Context context, Expression expression)
    {
        foreach (var guardType in Guards)
        {
            var guard = (ExpressionVisitor)Activator.CreateInstance(guardType);
            guard.Visit(expression);
        }

        var rewriter = new CursedRewriter(context);
        var visited = rewriter.Visit(expression);

        return new Result
        {
            Expression = visited,
            OriginalTake = rewriter._originalTake
        };
    }

    public override Expression Visit(Expression node)
    {
        var position = ++_recursion;

        _ordering.OnTraversingUp(node);

        foreach (var injector in _injectors)
            injector.OnTraversingUp(position, node);

        node = base.Visit(node)!;

        // Once we've traversed all the way to the top of the expression tree, we *should* be able to use the root node
        // to get an entity descriptor
        if (_entityDescriptor == null)
        {
            if (_context.Provider.TryGetEntityDescriptor(node, out var entityDescriptor))
                _entityDescriptor = entityDescriptor;
            else
            {
                throw new EntityDescriptorNotFoundException(
                    $"Unable to resolve EntityDescriptor for top level expression: {node}");
            }
        }

        foreach (var injector in _injectors)
            node = injector.OnTraversingDown(position, node, _entityDescriptor);

        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var antecedent = Visit(node.Arguments[0]);

        if (node.Method.DeclaringType != typeof(Queryable))
            return PropagateChanges(node, antecedent);

        return node.Method.Name switch
        {
            nameof(Queryable.Take) => HandleTakeNode(node, antecedent),
            nameof(Queryable.OrderBy) => HandleOrderNode(node, antecedent),
            nameof(Queryable.OrderByDescending) => HandleOrderNode(node, antecedent),
            nameof(Queryable.ThenBy) => HandleOrderNode(node, antecedent),
            nameof(Queryable.ThenByDescending) => HandleOrderNode(node, antecedent),
            _ => PropagateChanges(node, antecedent)
        };
    }

    private MethodCallExpression HandleTakeNode(MethodCallExpression node, Expression antecedent)
    {
        var take = (int)((ConstantExpression)node.Arguments[1]).Value;
        _originalTake ??= take;

        var args = node.Arguments.ToList();
        args[1] = Expression.Constant(take + 1);

        node = Expression.Call(null, node.Method, args);

        return PropagateChanges(node, antecedent);
    }

    private MethodCallExpression HandleOrderNode(MethodCallExpression node, Expression antecedent)
    {
        if (_context.Direction == Direction.Backwards)
        {
            // Replace the order node with the flipped version
            var args = node.Arguments.ToList();

            var method = node.Method.Name switch
            {
                nameof(Queryable.OrderBy) => Consts.OrderByDescendingMethodInfo,
                nameof(Queryable.OrderByDescending) => Consts.OrderByMethodInfo,
                nameof(Queryable.ThenBy) => Consts.ThenByDescendingMethodInfo,
                _ => Consts.ThenByMethodInfo
            };

            var t1 = node.Type.GetGenericArguments()[0];
            var t2 = ((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).ReturnType;
            method = method.MakeGenericMethod(t1, t2);
            node = Expression.Call(null, method, args);
        }

        return PropagateChanges(node, antecedent);
    }

    /// <summary>
    ///     Ensures any changes in the antecedent node are propagated into the current node.
    /// </summary>
    private static MethodCallExpression PropagateChanges(MethodCallExpression node, Expression antecedent)
    {
        if (node.Arguments[0] == antecedent)
            return node;

        // If antecedent has changed, replace the node with one that references the new antecedent
        var method = node.Method;
        var args = node.Arguments.ToList();
        args[0] = antecedent;

        // If the antecedent output type has also changed, update the node method call to match
        // (this will be because antecedentOutputType has changed to CursedWrapper<T>)
        var methodTypes = node.Method.GetGenericArguments();
        var nodeInputType = methodTypes[0];
        var antecedentOutputType = antecedent.Type.GetGenericArguments()[0];

        if (antecedentOutputType != nodeInputType)
        {
            methodTypes[0] = antecedentOutputType;

            method = method.GetGenericMethodDefinition()
                .MakeGenericMethod(methodTypes);

            // Ensure all node args with lambdas are replaced with lambdas expecting CursedWrapper<T>
            for (var i = 1; i < args.Count; i++)
            {
                var replacer = new LambdaToWrappedLambdaReplacer();
                args[i] = replacer.Visit(args[i])!;
            }
        }

        return Expression.Call(null, method, args);
    }

    public class Context
    {
        public string? Cursor { get; init; }
        public IEntityDescriptorProvider Provider { get; init; } = default!;
        public Direction Direction { get; init; }
        public NullBehaviour NullBehaviour { get; init; }
        public BadCursorBehaviour BadCursorBehaviour { get; init; }
    }

    public class Result
    {
        public int? OriginalTake { get; init; }
        public Expression Expression { get; init; } = default!;
    }
}