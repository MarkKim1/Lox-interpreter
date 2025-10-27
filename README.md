# LoxSharp

### Crafting Interpreter by Robert Nystrom, implementing scripting language using C#

An implementation of Rober Nystrom's [Crafting Interpreter](https://craftinginterpreters.com/) written in C#(.NET).
LoxSharp is a C# prot of the Lox Language - a tree-walk interpreter build from scratch

### How to run the /Lox/Astprinter.cs
The Astprinter Main method is wrapped with AST_PRINTER_DEMO
```bash
dotnet run --project Lox.csproj -p:DefineConstants=AST_PRINTER_DEMO
```

### How to run /Lox/Lox.cs
```bash
dotnet run --project Lox.csproj
```

> [!NOTE]
> requires .NET 8.0 SDK or later.
> Run inside the /Lox directory or use the --project to specify Lox.csproj.
> You can extend this project to include the parser, resolver, and interpreter stages from later chapters of the book.
