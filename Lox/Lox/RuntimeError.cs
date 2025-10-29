using LoxInterpreter;

public class RuntimeError : System.Exception
{
    public readonly Token token;
    public RuntimeError(Token token, string message) : base(message)
    {
        this.token = token;
    }
}