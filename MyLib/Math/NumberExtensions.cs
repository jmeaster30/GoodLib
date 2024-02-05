namespace MyLib.Math;

public static class NumberExtensions
{
    public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }
    
    public static float Remap(this int value, int fromMin, int fromMax, int toMin, int toMax)
    {
        return (float)(value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    public static int Floor(this float value)
    {
        return (int)System.Math.Floor(value);
    }
    
    public static int Floor(this double value)
    {
        return (int)System.Math.Floor(value);
    }
    
    public static int Floor(this decimal value)
    {
        return (int)System.Math.Floor(value);
    }
    
    public static int Ceiling(this float value)
    {
        return (int)System.Math.Ceiling(value);
    }

    public static int Ceiling(this double value)
    {
        return (int)System.Math.Ceiling(value);
    }
    
    public static int Ceiling(this decimal value)
    {
        return (int)System.Math.Ceiling(value);
    }

    public static int Min(this int a, int b)
    {
        return System.Math.Min(a, b);
    }
    
    public static float Min(this float a, float b)
    {
        return System.Math.Min(a, b);
    }
    
    public static double Min(this double a, double b)
    {
        return System.Math.Min(a, b);
    }
    
    public static decimal Min(this decimal a, decimal b)
    {
        return System.Math.Min(a, b);
    }
    
    public static int Max(this int a, int b)
    {
        return System.Math.Max(a, b);
    }
    
    public static float Max(this float a, float b)
    {
        return System.Math.Max(a, b);
    }
    
    public static double Max(this double a, double b)
    {
        return System.Math.Max(a, b);
    }
    
    public static decimal Max(this decimal a, decimal b)
    {
        return System.Math.Max(a, b);
    }

    public static int BitLength(this int a)
    {
        return Math.BitLength(a);
    }
}