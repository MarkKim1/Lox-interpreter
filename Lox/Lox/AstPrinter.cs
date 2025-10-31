using System.Text;
using LoxInterpreter;

public class AstPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string visitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string visitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.operatorToken!.lexeme!, expr.left!, expr.right!);
    }

    public string visitCallExpr(Expr.Call expr)
    {
        throw new NotImplementedException();
    }

    public string visitGetExpr(Expr.Get expr)
    {
        throw new NotImplementedException();
    }

    public string visitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.expression!);
    }

    public string visitLiteralExpr(Expr.Literal expr)
    {
        if (expr.value == null) return "nil";
        return expr.value.ToString()!;
    }

    public string visitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }

    public string visitSetExpr(Expr.Set expr)
    {
        throw new NotImplementedException();
    }

    public string visitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.operatorToken!.lexeme!, expr.right!);
    }

    public string visitVariableExpr(Expr.Variable expr)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();
        builder.Append("(").Append(name);
        foreach (var expr in exprs)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }
        builder.Append(')');
        return builder.ToString();
    }
    // #if AST_PRINTER_DEMO
    //     public static void Main(string[] args)
    //     {
    //         Expr expression = new Expr.Binary(
    //             new Expr.Unary(
    //                 new Token(TokenType.MINUS, "-", null, 1),
    //                 new Expr.Literal(123)),
    //             new Token(TokenType.STAR, "*", null, 1),
    //             new Expr.Grouping(
    //                 new Expr.Literal(45.67)));
    //         AstPrinter printer = new AstPrinter();
    //         Console.WriteLine(printer.visitBinaryExpr((Expr.Binary)expression));
    //     }
    // #endif
}