using System.ComponentModel;
using LoxInterpreter;
class Interpreter : Expr.Visitor<object>
{
    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.value!;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression!);
    }
    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }
    public object VisitBinaryExpr(Expr.Binary expr)
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

    public object VisitUnaryExpr(Expr.Unary expr)
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
    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }
    public void interpret(Expr expression)
    {
        try
        {
            object value = Evaluate(expression);
            Console.WriteLine(stringify(value));
        }
        catch (RuntimeError error)
        {
            Lox.runtimeError(error);
        }
    }

}