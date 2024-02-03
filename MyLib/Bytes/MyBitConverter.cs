using System.Text;

namespace MyLib.Bytes;

public static class MyBitConverter
{
    public static byte[] ToBytes<T>(this T value) where T : struct
    {
        return value switch
        {
            byte b => new[] { b },
            char c => BitConverter.GetBytes(c),
            bool bo => BitConverter.GetBytes(bo),
            float f => BitConverter.GetBytes(f),
            double d => BitConverter.GetBytes(d),
            Half h => BitConverter.GetBytes(h),
            short i16 => BitConverter.GetBytes(i16),
            int i32 => BitConverter.GetBytes(i32),
            long i64 => BitConverter.GetBytes(i64),
            ushort ui16 => BitConverter.GetBytes(ui16),
            uint ui32 => BitConverter.GetBytes(ui32),
            ulong ui64 => BitConverter.GetBytes(ui64),
            _ => throw new ArgumentException("Type is not supported for bytes conversion", nameof(value))
        };
    }
}