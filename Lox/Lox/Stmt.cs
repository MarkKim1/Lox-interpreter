namespace LoxInterpreter;

public abstract class Stmt {
public interface Visitor<R> 
{
    R visitBlockStmt(Block stmt);
    R visitExpressionStmt(Expression stmt);
    R visitPrintStmt(Print stmt);
    R visitVarStmt(Var stmt);
}
public abstract R Accept<R>(Visitor<R> visitor);
public class Block : Stmt
{
    public readonly List<Stmt>? statements;
    public Block ( List<Stmt> statements )
    {
        this.statements = statements;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitBlockStmt(this);
    }
}
public class Expression : Stmt
{
    public readonly Expr? expression;
    public Expression ( Expr expression )
    {
        this.expression = expression;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitExpressionStmt(this);
    }
}
public class Print : Stmt
{
    public readonly Expr? expression;
    public Print ( Expr expression )
    {
        this.expression = expression;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitPrintStmt(this);
    }
}
public class Var : Stmt
{
    public readonly Token? name;
    public readonly Expr? initializer;
    public Var ( Token name, Expr initializer )
    {
        this.name = name;
        this.initializer = initializer;
    }
    public override R Accept<R>(Visitor<R> visitor)
    {
        return visitor.visitVarStmt(this);
    }
}
}
