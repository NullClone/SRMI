using System;

namespace SRMI
{
    public static class AES
    {
        public static void Decrypt(byte[] value, byte[] keys)
        {
            var result = new byte[0x10];

            Array.Copy(value, result, 0x10);

            XorRoundKey(result, keys, 0);

            for (int i = 0; i < 9; i++)
            {
                SubBytesInv(result);
                ShiftRowsInv(result);
                MixColInv(result, 0x00);
                MixColInv(result, 0x04);
                MixColInv(result, 0x08);
                MixColInv(result, 0x0C);
                XorRoundKey(result, keys, i + 1);
            }

            SubBytesInv(result);
            ShiftRowsInv(result);
            XorRoundKey(result, keys, 0xA);

            Array.Copy(result, value, 0x10);
        }


        static void SubBytesInv(byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = Table.LookupSBoxInv[value[i]];
            }
        }

        static void XorRoundKey(byte[] value, byte[] keys, int round)
        {
            for (int i = 0; i < 0x10; i++)
            {
                value[i] ^= keys[i + (round * 0x10)];
            }
        }

        static void ShiftRowsInv(byte[] value)
        {
            var temp = new byte[0x10];

            Array.Copy(value, temp, 0x10);

            for (int i = 0; i < 0x10; i++)
            {
                value[i] = temp[Table.ShiftRowsTableInv[i]];
            }
        }

        static void MixColInv(byte[] value, int off)
        {
            var a0 = value[off + 0];
            var a1 = value[off + 1];
            var a2 = value[off + 2];
            var a3 = value[off + 3];

            value[off + 0] = (byte)(Table.LookupG14[a0] ^ Table.LookupG9[a3] ^ Table.LookupG13[a2] ^ Table.LookupG11[a1]);
            value[off + 1] = (byte)(Table.LookupG14[a1] ^ Table.LookupG9[a0] ^ Table.LookupG13[a3] ^ Table.LookupG11[a2]);
            value[off + 2] = (byte)(Table.LookupG14[a2] ^ Table.LookupG9[a1] ^ Table.LookupG13[a0] ^ Table.LookupG11[a3]);
            value[off + 3] = (byte)(Table.LookupG14[a3] ^ Table.LookupG9[a2] ^ Table.LookupG13[a1] ^ Table.LookupG11[a0]);
        }
    }
}