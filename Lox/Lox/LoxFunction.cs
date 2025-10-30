using LoxInterpreter;

class LoxFunction : LoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly Environment1 closure;

    public LoxFunction(Stmt.Function declaration, Environment1 closure)
    {
        this.closure = closure;
        this.declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
    }

    // Report the number of parameters (used by visitCallExpr() arity check)
    public int arity()
    {
        return declaration.parameters!.Count; // or declaration.parameters!.Count if that's your field name
    }

    // Execute the function body in a new function-local environment
    public object call(Interpreter interpreter, List<object> arguments)
    {
        // New environment for this call; parent is where globals live (per your current design)
        var environment = new Environment1(closure);
        // Bind parameters to passed arguments
        for (int i = 0; i < declaration.parameters!.Count; i++)
        {
            environment.define(declaration.parameters[i].lexeme!, arguments[i]);
        }
        // Run the body inside the function-local environment
        try
        {
            interpreter.executeBlock(declaration.body!, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.value!;
        }
        // No return values yet; nil in Lox => null in C#
        return null!;
    }

    // IMPORTANT: must be 'ToString' (PascalCase) to override in C#
    public override string ToString()
    {
        return "<fn " + declaration.name!.lexeme + ">";
    }
}