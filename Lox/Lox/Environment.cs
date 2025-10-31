using System.Data;
using System.Collections.Generic;

class Environment1
{
    readonly Environment1? enclosing;
    private readonly Dictionary<string, object> values = [];
    public Environment1()
    {
        enclosing = null;
    }
    public Environment1(Environment1 enclosing)
    {
        this.enclosing = enclosing;
    }

    public void define(string name, object value)
    {
        values.Remove(name);
        values.Add(name, value);
    }
    Environment1 ancestor(int? distance)
    {
        Environment1 environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing!;
        }
        return environment;
    }
    public object getAt(int? distance, string name)
    {
        var env = ancestor(distance);
        // Resolver guarantees the key exists at this distance.
        // Using TryGetValue to be safe.
        if (env.values.TryGetValue(name, out var value))
            return value;
        return null!;
    }
    public void assignAt(int? distance, Token name, object value)
    {
        var env = ancestor(distance);
        env.values[name.lexeme!] = value;
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
            values[name.lexeme!] = value;
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