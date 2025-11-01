using LoxInterpreter;

public class LoxFunction : LoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly Environment1 closure;
    private readonly bool isInitializer;

    public LoxFunction(Stmt.Function declaration, Environment1 closure, bool isInitializer)
    {
        this.isInitializer = isInitializer;
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
            if (isInitializer) return closure.getAt(0, "this");
            return returnValue.value!;
        }
        if (isInitializer) return closure.getAt(0, "this");
        return null!;
    }

    // IMPORTANT: must be 'ToString' (PascalCase) to override in C#
    public override string ToString()
    {
        return "<fn " + declaration.name!.lexeme + ">";
    }

    public LoxFunction bind(LoxInstance instance)
    {
        Environment1 environment = new Environment1(closure);
        environment.define("this", instance);
        return new LoxFunction(declaration, environment, isInitializer);
    }
}
