using System.Data;
using System.Collections.Generic;

class Environment1
{
    readonly Environment1? enclosing;
    public Environment1()
    {
        enclosing = null;
    }
    public Environment1(Environment1 enclosing)
    {
        this.enclosing = enclosing;
    }
    private readonly Dictionary<string, object> values = new();
    public void define(string name, object value)
    {
        if (values.ContainsKey(name))
        {
            values.Remove(name);
        }
        values.Add(name, value);
    }
    public object get(Token name)
    {
        if (values.ContainsKey(name.lexeme!))
        {
            return values.GetValueOrDefault(name.lexeme!)!;
        }
        if (enclosing != null) return enclosing.get(name);
        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }
    public void assign(Token name, object value)
    {
        if (values.ContainsKey(name.lexeme!))
        {
            values.Add(name.lexeme!, value);
            return;
        }

        if (enclosing != null)
        {
            enclosing.assign(name, value);
            return;
        }

        throw new RuntimeError(name, "Undefined variable'" + name.lexeme + "'.");
    }
}