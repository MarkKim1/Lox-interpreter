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

        return expr.operatorToken!.type switch
        {
            TokenType.GREATER => (double)left > (double)right,
            TokenType.GREATER_EQUAL => (double)left >= (double)right,
            TokenType.LESS => (double)left < (double)right,
            TokenType.LESS_EQUAL => (double)left <= (double)right,
            TokenType.MINUS => (double)left - (double)right,
            TokenType.PLUS => AddOperands(left, right),
            TokenType.SLASH => (double)left / (double)right,
            TokenType.STAR => (double)left * (double)right,
            TokenType.BANG_EQUAL => !isEqual(left, right),
            TokenType.EQUAL_EQUAL => isEqual(left, right), // TODO : need to implement isEqual on Chapter 7
            // Unreachable.
            _ => null!,
        };
    }
    //asdfasdfasdf

    // Custom method to handle addtion of different operand types
    private object AddOperands(object left, object right)
    {
        if (left is double && right is double)
        {
            return (double)left + (double)right;
        }
        if (left is string && right is string)
        {
            return (string)left + (string)right;
        }
        return (int)left + (int)right; // Fallback for other types (e.g., int)
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

}