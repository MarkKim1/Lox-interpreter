# Lox-interpreter in C#

### Crafting Interpreter by Robert Nystrom, implementing scripting language using C#

1. Project Overview
This project is a full re-implementation of the jlox interpreter from Robert Nystrom's Crafting Interpreter, rewritten in C#

2. Directory Structure
```bash
Lox/
├── AstPrinter.cs
├── Environment.cs
├── Expr.cs
├── Interpreter.cs
├── Lox.cs
├── LoxCallable.cs
├── LoxClass.cs
├── LoxFunction.cs
├── LoxInstance.cs
├── Parser.cs
├── Resolver.cs
├── Return.cs
├── RuntimeError.cs
├── Scanner.cs
├── Stmt.cs
├── Token.cs
├── TokenType.cs
└── test.txt  (example program)
```

3. How the Interpreter Works
- Scanning
Scanner.cs contverts raw characters into a sequence of Token objects
- Parsing
Parser.cs converts the token stream into an AST (instances of Expr and Stmt)
- Resolving
Resolver.cs performs static analysis to determine where variables should be found (their depth in the environment chain)
- Interpreting
Interpreter.cs walks the AST and evaluates
- Environment.cs

### How to run with file (Lox Interpreter/lox/Lox directory)
```bash
dotnet run test.txt
```
### How to run REPL (Lox Interpreter/lox/Lox directory)
```bash
dotnet run 
```

### Credits
This interpreter is based on the language and architecture from:

Crafting Interpreters by Robert Nystrom
https://craftinginterpreters.com

The code structure, algorithms, and patterns mirror the book’s jlox implementation, adapted into a modern C# environment.


> [!NOTE]
> requires .NET 8.0 SDK or later.
> Run inside the /Lox directory or use the --project to specify Lox.csproj.
