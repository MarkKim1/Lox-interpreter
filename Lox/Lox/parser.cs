using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using LoxInterpreter;

using Microsoft.CSharp.RuntimeBinder;

class Parser
{
    private class ParseError : Exception { }
    private readonly List<Token> tokens;
    private int current = 0;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }
    public List<Stmt> parse()
    {
        List<Stmt> statements = new();
        while (!isAtEnd())
        {
            statements.Add(declaration());
        }
        return statements;
    }
    private Expr expression()
    {
        return assignment();
    }
    private Stmt declaration()
    {
        try
        {
            if (match(TokenType.CLASS)) return classDeclaration();
            if (match(TokenType.FUN)) return function("function");
            if (match(TokenType.VAR)) return varDeclaration();
            return statement();
        }
        catch (ParseError)
        {
            synchronize();
            return null!;
        }
    }
    private Stmt classDeclaration()
    {
        Token name = consume(TokenType.IDENTIFIER, "Expect class name.");
        consume(TokenType.LEFT_BRACE, "Expect '{' before class body");

        List<Stmt.Function> methods = [];
        while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
        {
            methods.Add(function("method"));
        }
        consume(TokenType.RIGHT_BRACE, "Expect '}' after class body");

        return new Stmt.Class(name, methods);
    }
    private Stmt statement()
    {
        if (match(TokenType.FOR)) return forStatement();
        if (match(TokenType.IF)) return ifStatement();
        if (match(TokenType.PRINT)) return printStatement();
        if (match(TokenType.RETURN)) return returnStatement();
        if (match(TokenType.WHILE)) return whileStatement();
        if (match(TokenType.LEFT_BRACE)) return new Stmt.Block(block());
        return expressionsStatement();
    }
    private Stmt forStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (match(TokenType.VAR))
        {
            initializer = varDeclaration();
        }
        else
        {
            initializer = expressionsStatement();
        }

        Expr? condition = null;
        if (!check(TokenType.SEMICOLON))
        {
            condition = expression();
        }
        consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!check(TokenType.RIGHT_PAREN))
        {
            increment = expression();
        }
        consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
        Stmt body = statement();

        if (increment != null)
        {
            body = new Stmt.Block(new List<Stmt>{
                body,
                new Stmt.Expression(increment)
            });
        }

        if (condition == null) condition = new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block(new List<Stmt> { initializer, body });
        }

        return body;
    }
    private Stmt ifStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = statement();
        Stmt? elseBranch = null;
        if (match(TokenType.ELSE))
        {
            elseBranch = statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch!);
    }
    private Stmt printStatement()
    {
        Expr value = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }
    private Stmt returnStatement()
    {
        Token keyword = previous();
        Expr? value = null;
        if (!check(TokenType.SEMICOLON))
        {
            value = expression();
        }

        consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value!);
    }
    private Stmt varDeclaration()
    {
        Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (match(TokenType.EQUAL))
        {
            initializer = expression();
        }

        consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer!); // TODO: need to declare Stmt.Var
    }
    private Stmt whileStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Stmt body = statement();

        return new Stmt.While(condition, body);
    }
    private Stmt expressionsStatement()
    {
        Expr expr = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }
    private Stmt.Function function(string kind)
    {
        Token name = consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
        consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
        List<Token> parameters = [];
        if (!check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    error(peek(), "Can't have mre than 255 paramaters");
                }
                parameters.Add(consume(TokenType.IDENTIFIER,
                                    "Expect parameter name."
                ));
            } while (match(TokenType.COMMA));
        }
        consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

        consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
        List<Stmt> body = block();
        return new Stmt.Function(name, parameters, body);
    }
    private List<Stmt> block()
    {
        List<Stmt> statements = new();
        while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
        {
            statements.Add(declaration());
        }
        consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }
    private Expr assignment()
    {
        //Expr expr = equality();
        Expr expr = or();
        if (match(TokenType.EQUAL))
        {
            Token equals = previous();
            Expr value = assignment();

            if (expr is Expr.Variable)
            {
                Token name = ((Expr.Variable)expr).name!;
                return new Expr.Assign(name, value);
            }
            else if (expr is Expr.Get get)
            {
                return new Expr.Set(get.obj!, get.name!, value);
            }
            error(equals, "Invalid assignment target.");
        }
        return expr;
    }
    private Expr or()
    {
        Expr expr = and();

        while (match(TokenType.OR))
        {
            Token op = previous();
            Expr right = and();
            expr = new Expr.Logical(expr, op, right);
        }
        return expr;
    }
    private Expr and()
    {
        Expr expr = equality();

        while (match(TokenType.AND))
        {
            Token op = previous();
            Expr right = equality();
            expr = new Expr.Logical(expr, op, right);
        }
        return expr;
    }

    private Expr equality()
    {
        Expr expr = comparison();

        while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token oper = previous();
            Expr right = comparison();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }
    private Expr comparison()
    {
        Expr expr = term();

        while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token oper = previous();
            Expr right = term();
            expr = new Expr.Binary(expr, oper, right);
        }
        return expr;
    }
    private Expr term()
    {
        Expr expr = factor();

        while (match(TokenType.MINUS, TokenType.PLUS))
        {
            Token oper = previous();
            Expr right = factor();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }
    private Expr factor()
    {
        Expr expr = unary();
        while (match(TokenType.SLASH, TokenType.STAR))
        {
            Token oper = previous();
            Expr right = unary();
            expr = new Expr.Binary(expr, oper, right);
        }
        return expr;
    }
    private Expr unary()
    {
        if (match(TokenType.BANG, TokenType.MINUS))
        {
            Token oper = previous();
            Expr right = unary();
            return new Expr.Unary(oper, right);
        }
        return call();
    }
    private Expr finishCall(Expr callee)
    {
        List<Expr> arguments = new();
        if (!check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count() >= 255)
                {
                    error(peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(expression());
            } while (match(TokenType.COMMA));
        }
        Token paren = consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
        return new Expr.Call(callee, paren, arguments);
    }
    private Expr call()
    {
        Expr expr = primary();

        while (true)
        {
            if (match(TokenType.LEFT_PAREN))
            {
                expr = finishCall(expr);
            }
            else if (match(TokenType.DOT))
            {
                Token name = consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }
        return expr;
    }
    private Expr primary()
    {
        if (match(TokenType.FALSE)) return new Expr.Literal(false);
        if (match(TokenType.TRUE)) return new Expr.Literal(true);
        if (match(TokenType.NIL)) return new Expr.Literal(null);

        if (match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.Literal(previous().literal);
        }
        if (match(TokenType.THIS)) return new Expr.This(previous());
        if (match(TokenType.IDENTIFIER))
        {
            return new Expr.Variable(previous());
        }
        if (match(TokenType.LEFT_PAREN))
        {
            Expr expr = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw error(peek(), "Expect expression.");
    }
    private Token consume(TokenType type, String message)
    {
        if (check(type)) return advance();

        throw error(peek(), message);
    }

    private bool match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (check(type))
            {
                advance();
                return true;
            }
        }

        return false;
    }
    private bool check(TokenType type)
    {
        if (isAtEnd()) return false;
        return peek().type == type;
    }
    private Token advance()
    {
        if (!isAtEnd()) current++;
        return previous();
    }
    private bool isAtEnd()
    {
        return peek().type == TokenType.EOF;
    }
    private Token peek()
    {
        return tokens[current];
    }
    private Token previous()
    {
        return tokens[current - 1];
    }
    private ParseError error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }
    private void synchronize()
    {
        advance();

        while (!isAtEnd())
        {
            if (previous().type == TokenType.SEMICOLON) return;

            switch (peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            advance();
        }
    }

}