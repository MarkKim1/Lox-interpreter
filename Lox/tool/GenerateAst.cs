public class GenerateAst
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }
        string outputDir = args[0];

        DefineAst(outputDir, "Expr", new List<string>
        {
            "Binary   : Expr left, Token operatorToken, Expr right",
            "Grouping : Expr expression",
            "Literal  : object value",
            "Unary    : Token operatorToken, Expr right"
        });
    }
    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        // create the path
        string path = outputDir + "/" + baseName + ".cs";
        StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);

        writer.WriteLine("namespace LoxInterpreter;");
        writer.WriteLine();
        writer.WriteLine("public abstract class " + baseName + " {");
        DefineVisitor(writer, baseName, types);
        writer.WriteLine("public abstract R Accept<R>(Visitor<R> visitor);");
        //Console.WriteLine("printing class names");
        foreach (string type in types)
        {
            string className = type.Split(":")[0].Trim();
            //Console.WriteLine(className);

            string fields = type.Split(":")[1].Trim();
            //Console.WriteLine(fields);

            DefineType(writer, baseName, className, fields);
        }

        writer.WriteLine("}");
        writer.Close();
    }
    private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("public interface Visitor<R> ");
        writer.WriteLine("{");

        foreach (string type in types)
        {
            string typeName = type.Split(":")[0].Trim();
            writer.WriteLine("    R visit" + typeName + baseName + "(" + typeName + " " + baseName.ToLower() + ");");
        }
        writer.WriteLine("}");
    }
    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
    {
        #region [Class Declaration]
        writer.WriteLine($"public class {className} : {baseName}");
        writer.WriteLine("{");


        string[] fields = fieldList.Split(", ");

        //variable declaration
        foreach (string field in fields)
        {
            string fieldType = field.Split(" ")[0];
            string fieldTypeName = field.Split(" ")[1];
            writer.WriteLine($"    public readonly {fieldType}? {fieldTypeName};");
        }
        #region [Constructor]
        if (className == "Literal" && fieldList.StartsWith("object"))
        {
            fieldList = "object? " + fieldList.Split(" ")[1];
        }
        writer.WriteLine($"    public {className} ( {fieldList} )");
        writer.WriteLine("    {");

        foreach (string field in fields)
        {
            string name = field.Split(" ")[1];

            writer.WriteLine($"        this.{name} = {name};");
            Console.WriteLine(field);
        }

        Console.WriteLine();
        writer.WriteLine("    }");
        #endregion [Constructor]    

        //override Accpet
        writer.WriteLine("    public override R Accept<R>(Visitor<R> visitor)");
        writer.WriteLine("    {");
        writer.WriteLine($"        return visitor.visit{className}{baseName}(this);");
        writer.WriteLine("    }");

        writer.WriteLine("}");
        #endregion [Class Declaration]

    }
}