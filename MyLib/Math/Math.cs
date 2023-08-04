namespace MyLib.Math;

public static class Math
{
    public static int BitLength(int value)
    {
        return (int)System.Math.Log(value, 2) + 1;
    }
}