// See https://aka.ms/new-console-template for more information

using MyLib.Generators;
using Newtonsoft.Json;

var templateFiles = Directory.GetFiles(args[0], "*.json", SearchOption.AllDirectories);

foreach (var fileName in templateFiles)
{
    if (fileName.StartsWith("./obj/") || fileName.StartsWith("./bin/"))
        continue;
    
    Console.WriteLine($"Generating source for '{fileName}'...");

    var contents = File.ReadAllText(fileName);
    var model = JsonConvert.DeserializeObject<GeneratorModel>(contents);

    using var writer = new StreamWriter($"{model.Path}/{model.ClassName}.cs");
    
    writer.WriteLine($"namespace {model.Namespace};");
    writer.WriteLine($"// This file was auto-generated. Make sure to edit '{fileName}' if you need to make changes");
    writer.WriteLine($"public static class {model.ClassName}");
    writer.Write("{");

    foreach (var functionModel in model.Functions)
    {
        Console.WriteLine($"\tGenerating Function '{functionModel.FunctionName}'..");
        if (functionModel.ParamGenType == "single")
        {
            foreach (var paramType in functionModel.ParamTypes.Select((v, i) => (i, v)))
            {
                writer.WriteLine("");
                writer.Write($"\tpublic static {functionModel.ReturnType} {functionModel.FunctionName}(");
                writer.Write($"this {paramType.v} {functionModel.ParamNames[0]}");
                writer.WriteLine(")");
                writer.WriteLine("\t{");
                writer.WriteLine($"\t\t{functionModel.Body}");
                writer.WriteLine("\t}");
            }
        }
        else
        {
            var comboParamTypes = combo(functionModel.ParamTypes, functionModel.ParamNames.Count);

            foreach (var paramTypes in comboParamTypes)
            {
                var biggestType = paramTypes.Aggregate(paramTypes[0], (current, paramType) => biggestTypeFunction(paramType, current));

                writer.Write($"\tpublic static {(functionModel.ReturnType == "BIGGEST_OF_INPUT" ? biggestType : functionModel.ReturnType)} {functionModel.FunctionName}(");

                var zippedTypes = paramTypes.Zip(functionModel.ParamNames);
                foreach (var (idx, param) in zippedTypes.Select((v, i) => (i, v)))
                {
                    if (idx == 0)
                        writer.Write("this ");
                    writer.Write($"{param.First} {param.Second}");
                    if (idx < paramTypes.Count - 1)
                        writer.Write(", ");
                }
                
                writer.WriteLine(")");
                writer.WriteLine("\t{");

                var inVariableName = false;
                var variableName = "";
                var body = "";
                foreach (var t in functionModel.Body)
                {
                    if (inVariableName) 
                    {
                        if (char.IsLetterOrDigit(t))
                        {
                            variableName += t;
                        }
                        else
                        {
                            var pt = zippedTypes.First(x => x.Second == variableName);
                            if (pt.First != biggestType && neededCast(pt.First).Contains(biggestType))
                                body += $" ({biggestType})";
                            body += t;
                            inVariableName = false;
                            variableName = "";
                        }
                    }
                    else if (t == '%')
                    {
                        inVariableName = true;
                    }
                    else
                    {
                        body += t;
                    }
                }
                
                writer.WriteLine($"\t\t{body}");
                writer.WriteLine("\t}\n");
            }
        }
    }
    
    writer.WriteLine("}");
    writer.Flush();
    writer.Close();
}

return;

string biggestTypeFunction(string a, string b) => typeNameConversion(a) < typeNameConversion(b) ? b : a;

int typeNameConversion(string a)
{
    return a switch
    {
        "byte" => 0,
        "ushort" => 1,
        "short" => 2,
        "uint" => 3,
        "int" => 4,
        "ulong" => 5,
        "long" => 6,
        "float" => 7,
        "double" => 8,
        "decimal" => 9
    };
}

List<string> neededCast(string a)
{
    return a switch
    {
        "uint" => new List<string> { "int" },
        "ushort" => new List<string> { "short" },
        "ulong" => new List<string> { "long" },
        "short" => new List<string> { "ulong", "uint" },
        "int" => new List<string> { "ulong" },
        "float" => new List<string> { "decimal" },
        "double" => new List<string> { "decimal" },
        _ => new List<string>(),
    };
}

List<List<T>> combo<T>(List<T> values, int dimension)
{
    return dimension switch
    {
        < 1 => new List<List<T>>(),
        1 => values.Select(x => new List<T> { x }).ToList(),
        _ => values.SelectMany(x =>
            {
                return combo(values, dimension - 1)
                    .Select(y =>
                    {
                        y.Add(x);
                        return y;
                    });
            })
            .ToList()
    };
}