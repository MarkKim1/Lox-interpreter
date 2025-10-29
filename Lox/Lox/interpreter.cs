using System.ComponentModel;
using System.Data;
using LoxInterpreter;
class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
{
    private Environment1 environment = new Environment1();

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }
    private void execute(Stmt stmt)
    {
        stmt.Accept(this);
    }
    void executeBlock(List<Stmt> statements, Environment1 environment)
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
        return environment.get(expr.name!);
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
        environment.assign(expr.name!, value);
        return value;
    }

    public object visitBlockStmt(Stmt.Block stmt)
    {
        executeBlock(stmt.statements!, new Environment1(environment));
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

    public object visitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.condition!)))
        {
            execute(stmt.body!);
        }
        return null!;
    }
}