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

    public static int BitLength(this int a)
    {
        return Math.BitLength(a);
    }
}