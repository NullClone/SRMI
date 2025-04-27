using System;
using System.Runtime.InteropServices;

namespace SRMI
{
    public static class Oodle
    {
        [DllImport("Oodle.dll", EntryPoint = "OodleLZ_Decompress")]
        public static extern int Decompress(
            ref byte compressedBuffer,
            int compressedBufferSize,
            ref byte decompressedBuffer,
            int decompressedBufferSize,
            int fuzzSafe,
            int checkCRC,
            int verbosity,
            int rawBuffer,
            int rawBufferSize,
            int fpCallback,
            int callbackUserData,
            int decoderMemory,
            int decoderMemorySize,
            int threadPhase);

        public static Span<byte> Decompress(Span<byte> compressed, Span<byte> decompressed)
        {
            var result = Decompress(ref compressed[0], compressed.Length, ref decompressed[0], decompressed.Length, 1, 0, 0, 0, 0, 0, 0, 0, 0, 3);

            if (result != decompressed.Length)
            {
                throw new Exception("Oodle decompression error");
            }

            return decompressed;
        }
    }
}