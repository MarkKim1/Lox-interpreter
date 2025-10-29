namespace LoxInterpreter;

public abstract class Expr {
public interface Visitor<R> 
{
    R visitAssignExpr(Assign expr);
    R visitBinaryExpr(Binary expr);
    R visitGroupingExpr(Grouping expr);
    R visitLiteralExpr(Literal expr);
    R visitLogicalExpr(Logical expr);
    R visitUnaryExpr(Unary expr);
    R visitVariableExpr(Variable expr);
}
public abstract R Accept<R>(Visitor<R> visitor);
public class Assign : Expr
{
    public readonly Token? name;
    public readonly Expr? value;
    public Assign ( Token name, Expr value )
    {
        this.name = name;
        this.value = value;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitAssignExpr(this);
    }
}
public class Binary : Expr
{
    public readonly Expr? left;
    public readonly Token? operatorToken;
    public readonly Expr? right;
    public Binary ( Expr left, Token operatorToken, Expr right )
    {
        this.left = left;
        this.operatorToken = operatorToken;
        this.right = right;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitBinaryExpr(this);
    }
}
public class Grouping : Expr
{
    public readonly Expr? expression;
    public Grouping ( Expr expression )
    {
        this.expression = expression;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitGroupingExpr(this);
    }
}
public class Literal : Expr
{
    public readonly object? value;
    public Literal ( object? value )
    {
        this.value = value;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitLiteralExpr(this);
    }
}
public class Logical : Expr
{
    public readonly Expr? left;
    public readonly Token? op;
    public readonly Expr? right;
    public Logical ( Expr left, Token op, Expr right )
    {
        this.left = left;
        this.op = op;
        this.right = right;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitLogicalExpr(this);
    }
}
public class Unary : Expr
{
    public readonly Token? operatorToken;
    public readonly Expr? right;
    public Unary ( Token operatorToken, Expr right )
    {
        this.operatorToken = operatorToken;
        this.right = right;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitUnaryExpr(this);
    }
}
public class Variable : Expr
{
    public readonly Token? name;
    public Variable ( Token name )
    {
        this.name = name;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitVariableExpr(this);
    }
}
}
