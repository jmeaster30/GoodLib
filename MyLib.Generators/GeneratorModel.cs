namespace MyLib.Generators;

public class GeneratorModel
{
    public string? ClassName { get; set; }
    public string? Path { get; set; }
    public string? Namespace { get; set; }
    public List<FunctionModel> Functions { get; set; } = new ();
}

public class FunctionModel
{
    public string? FunctionName { get; set; }
    public string? ReturnType { get; set; }
    public string? ParamGenType { get; set; }
    public List<string> ParamTypes { get; set; } = new ();
    public List<string> ParamNames { get; set; } = new ();
    public string? Body { get; set; }
}