using System;
using System.Buffers.Binary;

namespace SRMI
{
    public static class Mr0k
    {
        public static Span<byte> Decrypt(Span<byte> data)
        {
            var key1 = new byte[0x10];
            var key2 = new byte[0x10];
            var key3 = new byte[0x10];

            data.Slice(4, 0x10).CopyTo(key1);
            data.Slice(0x74, 0x10).CopyTo(key2);
            data.Slice(0x84, 0x10).CopyTo(key3);

            var encryptedBlockSize = Math.Min(0x10 * ((data.Length - 0x94) >> 7), 0x400);

            for (int i = 0; i < Table.InitVector.Length; i++)
            {
                key2[i] ^= Table.InitVector[i];
            }

            AES.Decrypt(key1, Table.ExpansionKey);
            AES.Decrypt(key3, Table.ExpansionKey);

            for (int i = 0; i < key1.Length; i++)
            {
                key1[i] ^= key3[i];
            }

            key1.CopyTo(data.Slice(0x84, 0x10));

            var seed1 = BinaryPrimitives.ReadUInt64LittleEndian(key2);
            var seed2 = BinaryPrimitives.ReadUInt64LittleEndian(key3);

            var seed = seed2 ^ seed1 ^ (seed1 + (uint)data.Length - 20);

            var encryptedBlock = data.Slice(0x94, encryptedBlockSize);

            var seedSpan = BitConverter.GetBytes(seed);

            for (int i = 0; i < encryptedBlockSize; i++)
            {
                encryptedBlock[i] ^= (byte)(seedSpan[i % seedSpan.Length] ^ Table.BlockKey[i % Table.BlockKey.Length]);
            }

            data = data[0x14..];

            return data;
        }
    }
}