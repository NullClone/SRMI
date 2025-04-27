using System;

namespace SRMI
{
    public static class Half
    {
        public static float ToSingle(ushort value)
        {
            var offset = Table.OffsetTable[value >> 10];

            var mantissa = Table.MantissaTable[offset + (value & 0x3ff)];

            var exponent = Table.ExponentTable[value >> 10];

            var result = BitConverter.GetBytes(mantissa + exponent);

            return BitConverter.ToSingle(result, 0);
        }
    }
}