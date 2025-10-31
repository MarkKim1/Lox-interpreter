using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using LoxInterpreter;

sealed class ClockFunction : LoxCallable
{
    public int arity() => 0;

    public object call(Interpreter interpreter, List<object> arguments)
        => (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

    public string toString()
    {
        return "<native fn>";
    }
}
public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
{
    public readonly Environment1 globals = new();
    private Environment1 environment = new();
    private readonly Dictionary<Expr, int> locals = [];
    public Interpreter()
    {
        globals.define("clock", new ClockFunction());
        environment = globals;
    }
    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }
    private void execute(Stmt stmt)
    {
        stmt.Accept(this);
    }
    public void resolve(Expr expr, int depth)
    {
        locals.Add(expr, depth);
    }
    public void executeBlock(List<Stmt> statements, Environment1 environment)
    {
        Environment1 previous = this.environment;
        try
        {
            this.environment = environment;

            foreach (Stmt statement in statements)
            {
                execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }
    private void checkNumberOperands(Token op, object left, object right)
    {
        if (left is Double && right is Double) return;
        throw new RuntimeError(op, "Operands must be numbers.");
    }
    private void checkNumberOperands(Token obj, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(obj, "Operand must be a number.");
    }
    private bool isEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }
    private string stringify(object obj)
    {
        if (obj == null) return "nil";
        if (obj is double)
        {
            string? text = obj.ToString();
            if (text!.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }
        return obj.ToString()!;
    }
    // Custom method to handle addtion of different operand types
    private object AddOperands(Token op, object left, object right)
    {
        if (left is double && right is double)
        {
            return (double)left + (double)right;
        }
        if (left is string && right is string)
        {
            return (string)left + (string)right;
        }
        throw new RuntimeError(op, "Operands must be two numbers or two strings.");
    }
    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }
    // public void interpret(Expr expression)
    // {
    //     try
    //     {
    //         object value = Evaluate(expression);
    //         Console.WriteLine(stringify(value));
    //     }
    //     catch (RuntimeError error)
    //     {
    //         Lox.runtimeError(error);
    //     }
    // }
    public void interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            Lox.runtimeError(error);
        }
    }

    public object visitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.left!);
        object right = Evaluate(expr.right!);

        switch (expr.operatorToken!.type)
        {
            case TokenType.GREATER:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left <= (double)right;
            case TokenType.MINUS:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                return AddOperands(expr.operatorToken!, left, right);
            case TokenType.SLASH:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                checkNumberOperands(expr.operatorToken!, left, right);
                return (double)left * (double)right;
            case TokenType.BANG_EQUAL:
                return !isEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return isEqual(left, right);
            // Unreachable.
            default:
                return null!;
        }
    }

    public object visitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression!);
    }

    public object visitLiteralExpr(Expr.Literal expr)
    {
        return expr.value!;
    }

    public object visitUnaryExpr(Expr.Unary expr)
    {
        object right = Evaluate(expr.right!);

        return expr.operatorToken!.type switch
        {
            TokenType.MINUS => -(double)right,
            TokenType.BANG => !IsTruthy(right),
            // Unreachable.
            _ => null!,
        };
    }

    public object visitVariableExpr(Expr.Variable expr)
    {
        return lookUpVariable(expr.name!, expr);
    }
    private object lookUpVariable(Token name, Expr expr)
    {

        int? distance;
        if (locals.ContainsKey(expr))
        {
            distance = locals[expr];
            return environment.getAt(distance, name.lexeme!);
        }
        else
        {
            return globals.get(name);
        }
    }
    object Stmt.Visitor<object>.visitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression!);
        return null!;
    }

    object Stmt.Visitor<object>.visitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.expression!);
        System.Console.WriteLine(stringify(value));
        return null!;
    }

    public object visitVarStmt(Stmt.Var stmt)
    {
        object value = null!;
        if (stmt.initializer != null)
        {
            value = Evaluate(stmt.initializer);
        }
        environment.define(stmt.name!.lexeme!, value);
        return null!;
    }

    public object visitAssignExpr(Expr.Assign expr)
    {
        object value = Evaluate(expr.value!);
        //Console.WriteLine(value);
        int? distance = null;
        if (locals.ContainsKey(expr))
        {
            distance = locals[expr];
        }
        if (distance != null)
        {
            environment.assignAt(distance, expr.name!, value);
        }
        else
        {
            globals.assign(expr.name!, value);
        }
        return value;
    }

    public object visitBlockStmt(Stmt.Block stmt)
    {
        executeBlock(stmt.statements!, new Environment1(environment));
        return null!;
    }
    public object visitClassStmt(Stmt.Class stmt)
    {
        environment.define(stmt.name!.lexeme!, null!);
        Dictionary<string, LoxFunction> methods = [];
        foreach (Stmt.Function method in stmt.methods!)
        {
            LoxFunction function = new(method, environment);
            methods.Add(method.name!.lexeme!, function);
        }
        LoxClass klass = new LoxClass(stmt.name!.lexeme!, methods);
        environment.assign(stmt.name, klass);
        return null!;
    }
    public object visitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.condition!)))
        {
            execute(stmt.thenBranch!);
        }
        else if (stmt.elseBranch != null)
        {
            execute(stmt.elseBranch);
        }
        return null!;
    }

    public object visitLogicalExpr(Expr.Logical expr)
    {
        object left = Evaluate(expr.left!);

        if (expr.op!.type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }
        return Evaluate(expr.right!);
    }
    public object visitSetExpr(Expr.Set expr)
    {
        object obj = Evaluate(expr.obj!);
        if (obj is not LoxInstance)
        {
            throw new RuntimeError(expr.name!, "Only instance have fields");
        }
        object value = Evaluate(expr.value!);
        ((LoxInstance)obj).set(expr.name!, value);
        return value;
    }

    public object visitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.condition!)))
        {
            execute(stmt.body!);
        }
        return null!;
    }

    public object visitCallExpr(Expr.Call expr)
    {
        object callee = Evaluate(expr.callee!);
        List<object> arguments = [];
        foreach (Expr argument in expr.arguments!)
        {
            arguments.Add(Evaluate(argument));
        }
        LoxCallable function = (LoxCallable)callee;
        if (arguments.Count != function.arity())
        {
            throw new RuntimeError(expr.paran!, "Expected " +
                    function.arity() + " arguments but got " +
                    arguments.Count + ".");
        }
        if (callee is not LoxCallable)
        {
            throw new RuntimeError(expr.paran!, "Can only call functions and classes");
        }
        return function.call(this, arguments);
    }
    public object visitGetExpr(Expr.Get expr)
    {
        object obj = Evaluate(expr.obj!);
        if (obj is LoxInstance)
        {
            return ((LoxInstance)obj).get(expr.name!);
        }
        throw new RuntimeError(expr.name!, "Only instances have porperties.");
    }

    public object visitFunctionStmt(Stmt.Function stmt)
    {
        LoxFunction function = new(stmt, environment);
        environment.define(stmt.name!.lexeme!, function);
        return null!;
    }

    public object visitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.value! != null) value = Evaluate(stmt.value);

        throw new Return(value!);
    }
}