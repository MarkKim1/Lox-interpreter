using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using LoxInterpreter;
public class Lox
{
    private static readonly Interpreter interpreter = new Interpreter();
    private static bool hadError = false;
    private static bool hadRuntimeError = false;

    public static void Main(string[] args)
    {
        try
        {
            //Console.WriteLine("asdfasdf");
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                runPrompt();
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public static void runFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Run(Encoding.Default.GetString(bytes));

        // Indicate an error in the exit code.
        if (hadError) Environment.Exit(65);
        if (hadRuntimeError) Environment.Exit(70);

    }
    public static void runPrompt()
    {
        for (; ; )
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            hadError = false;
        }
    }
    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.scanTokens();

        Parser parser = new Parser(tokens);
        Expr expression = parser.parse();

        if (hadError) return;

        //Console.WriteLine(new AstPrinter().Print(expression));
        interpreter.interpret(expression);

    }
    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }
    public static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        hadError = true;
    }
    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.lexeme + "'", message);
        }
    }
    public static void runtimeError(RuntimeError error)
    {
        Console.Error.WriteLine($"{error.Message}\n[line {error.token.line}]");
        hadRuntimeError = true;
    }
}