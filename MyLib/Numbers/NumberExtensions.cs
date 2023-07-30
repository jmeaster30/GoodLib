namespace MyLib.Numbers;

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
        return (int)Math.Floor(value);
    }

    public static int Ceiling(this double value)
    {
        return (int) Math.Ceiling(value);
    }

    public static int Min(this int a, int b)
    {
        return Math.Min(a, b);
    }
    
    public static int Max(this int a, int b)
    {
        return Math.Max(a, b);
    }
}