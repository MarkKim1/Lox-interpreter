using System.Data;
using System.Runtime.CompilerServices;
using LoxInterpreter;

class Resolver : Expr.Visitor<object>, Stmt.Visitor<object>
{
    private readonly Stack<Dictionary<string, bool>> scopes = [];
    private FunctionType currentFunction = FunctionType.NONE;
    private readonly Interpreter? inpterpreter;
    public Resolver(Interpreter inpterpreter)
    {
        this.inpterpreter = inpterpreter;
    }
    public object visitBlockStmt(Stmt.Block stmt)
    {
        beginScope();
        resolve(stmt.statements!);
        endScope();
        return null!;
    }
    public object visitExpressionStmt(Stmt.Expression stmt)
    {
        resolve(stmt.expression!);
        return null!;
    }
    public object visitVarStmt(Stmt.Var stmt)
    {
        declare(stmt.name!);
        if (stmt.initializer != null)
        {
            resolve(stmt.initializer);
        }
        define(stmt.name!);
        return null!;
    }
    public object visitWhileStmt(Stmt.While stmt)
    {
        resolve(stmt.condition!);
        resolve(stmt.body!);
        return null!;
    }
    public object visitFunctionStmt(Stmt.Function stmt)
    {
        declare(stmt.name!);
        define(stmt.name!);

        resolveFunction(stmt, FunctionType.FUNCTION);
        return null!;
    }
    public object visitIfStmt(Stmt.If stmt)
    {
        resolve(stmt.condition!);
        resolve(stmt.thenBranch!);
        if (stmt.elseBranch != null) resolve(stmt.elseBranch);

        return null!;
    }
    public object visitPrintStmt(Stmt.Print stmt)
    {
        resolve(stmt.expression!);
        return null!;
    }
    public object visitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            Lox.Error(stmt.keyword!, "Can't return from top-level code.");
        }
        if (stmt.value != null)
        {
            resolve(stmt.value);
        }
        return null!;
    }
    public object visitAssignExpr(Expr.Assign expr)
    {
        resolve(expr.value!);
        resolveLocal(expr, expr.name!);
        return null!;
    }
    public object visitBinaryExpr(Expr.Binary expr)
    {
        resolve(expr.left!);
        resolve(expr.right!);
        return null!;
    }
    public object visitCallExpr(Expr.Call expr)
    {
        resolve(expr.callee!);
        foreach (Expr argument in expr.arguments!)
        {
            resolve(argument);
        }
        return null!;
    }
    public object visitGroupingExpr(Expr.Grouping expr)
    {
        resolve(expr.expression!);
        return null!;
    }
    public object visitLiteralExpr(Expr.Literal expr)
    {
        return null!;
    }
    public object visitLogicalExpr(Expr.Logical expr)
    {
        resolve(expr.left!);
        resolve(expr.right!);
        return null!;
    }
    public object visitUnaryExpr(Expr.Unary expr)
    {
        resolve(expr.right!);
        return null!;
    }
    public object visitVariableExpr(Expr.Variable expr)
    {
        if (scopes.Count > 0 && scopes.Peek().ContainsKey(expr.name!.lexeme!) && scopes.Peek()[expr.name!.lexeme!] == false)
        {
            Lox.Error(expr.name, "Can't read local variable in its own initializer");
        }
        resolveLocal(expr, expr.name!);
        return null!;
    }
    private void resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }
    private void resolve(Expr expr)
    {
        expr.Accept(this);
    }
    private void resolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;
        beginScope();
        foreach (Token param in function.parameters!)
        {
            declare(param);
            define(param);
        }
        resolve(function.body!);
        endScope();
        currentFunction = enclosingFunction;
    }
    private void beginScope()
    {
        scopes.Push([]);
    }
    private void endScope()
    {
        _ = scopes.Pop();
    }
    private void declare(Token name)
    {
        if (scopes.Count == 0) return;

        Dictionary<string, bool> scope = scopes.Peek();
        if (scope.ContainsKey(name.lexeme!))
        {
            Lox.Error(name, "Already a variable with this name in this scope.");
        }
        scope.TryAdd(name.lexeme!, false);
    }
    private void define(Token name)
    {
        if (scopes.Count == 0) return;
        scopes.Peek()[name.lexeme!] = true;
    }
    private void resolveLocal(Expr expr, Token name)
    {
        int distance = 0;
        // Stack&lt;T&gt; enumerates from top (innermost) to bottom (outermost).
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name.lexeme!))
            {
                inpterpreter!.resolve(expr, distance);
                return;
            }
            distance++;
        }
    }
    public void resolve(List<Stmt> statements)
    {
        foreach (Stmt statement in statements)
        {
            resolve(statement);
        }
    }
    private enum FunctionType
    {
        NONE,
        FUNCTION
    }
}
