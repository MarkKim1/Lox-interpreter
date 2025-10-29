using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Transactions;

class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new();
    private int start;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        ["and"] = TokenType.AND,
        ["class"] = TokenType.CLASS,
        ["else"] = TokenType.ELSE,
        ["false"] = TokenType.FALSE,
        ["for"] = TokenType.FOR,
        ["fun"] = TokenType.FUN,
        ["if"] = TokenType.IF,
        ["nil"] = TokenType.NIL,
        ["or"] = TokenType.OR,
        ["print"] = TokenType.PRINT,
        ["return"] = TokenType.RETURN,
        ["super"] = TokenType.SUPER,
        ["this"] = TokenType.THIS,
        ["true"] = TokenType.TRUE,
        ["var"] = TokenType.VAR,
        ["while"] = TokenType.WHILE
    };

    public Scanner(string source)
    {
        this.source = source;
    }
    public List<Token> scanTokens()
    {
        while (!isAtEnd())
        {
            start = current;
            scanToken();
        }
        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }

    private void scanToken()
    {
        char c = advance();
        switch (c)
        {
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.SEMICOLON); break;
            case '*': addToken(TokenType.STAR); break;
            case '!': addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            case '/':
                if (match('/'))
                {
                    while (peek() != '\n' && !isAtEnd())
                    {
                        advance();
                    }
                }
                else
                {
                    addToken(TokenType.SLASH);
                }
                break;
            // using STRING() instead of string() : Invalid expression term
            case '"': STRING(); break;
            default:
                if (isDigit(c))
                {
                    number();
                }
                else if (isAlpha(c))
                {
                    identifier();
                }
                else
                {
                    Lox.Error(line, "Unexpected character.");
                }
                break;
        }
    }
    private void identifier()
    {
        while (isAlphaNumeric(peek())) advance();
        string text = source.Substring(start, current - start);
        if (!keywords.TryGetValue(text, out var type))
            type = TokenType.IDENTIFIER;
        addToken(type);
    }
    private void number()
    {
        while (isDigit(peek())) advance();

        if (peek() == '.' && isDigit(peekNext()))
        {
            advance();
            while (isDigit(peek())) advance();
        }
        addToken(TokenType.NUMBER, double.Parse(source.Substring(start, current - start)));
    }
    private void STRING()
    {
        while (peek() != '"' && !isAtEnd())
        {
            if (peek() == '\n') line++;
            advance();
        }
        if (isAtEnd())
        {
            Lox.Error(line, "Unterminated string.");
            return;
        }
        // The closing ".
        advance();
        // Trim the surrounding quotes.
        int length = current - start - 2;
        string value = source.Substring(start + 1, length);
        //System.Console.WriteLine($"value: {value}");
        addToken(TokenType.STRING, value);
    }
    private bool match(char expected)
    {
        if (isAtEnd()) return false;
        if (source[current] != expected)
        {
            return false;
        }
        current++;
        return true;
    }
    private char peek()
    {
        if (isAtEnd()) return '\0';
        return source[current];
    }
    private char peekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }
    private bool isAlphaNumeric(char c)
    {
        return isAlpha(c) || isDigit(c);
    }
    private bool isAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
       (c >= 'A' && c <= 'Z') ||
        c == '_';
    }
    private bool isDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool isAtEnd()
    {
        return current >= source.Length;
    }
    private char advance()
    {
        return source[current++];
    }
    private void addToken(TokenType type)
    {
        addToken(type, null);
    }
    private void addToken(TokenType type, object? literal)
    {
        try
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }


    }
}