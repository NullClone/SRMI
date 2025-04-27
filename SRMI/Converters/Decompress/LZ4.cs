using System;

namespace SRMI
{
    public static class LZ4
    {
        public static Span<byte> Decompress(Span<byte> cmp, Span<byte> dec)
        {
            int cmpPos = 0;
            int decPos = 0;

            do
            {
                int encCount = (cmp[cmpPos] >> 0) & 0xf;
                int litCount = (cmp[cmpPos++] >> 4) & 0xf;

                if (litCount == 0xf)
                {
                    byte sum;

                    do
                    {
                        litCount += sum = cmp[cmpPos++];
                    }
                    while (sum == 0xff);
                }

                cmp.Slice(cmpPos, litCount).CopyTo(dec[decPos..]);

                cmpPos += litCount;
                decPos += litCount;

                if (cmpPos >= cmp.Length) break;

                int back = (cmp[cmpPos++] << 0) | (cmp[cmpPos++] << 8);

                if (encCount == 0xf)
                {
                    byte sum;

                    do
                    {
                        encCount += sum = cmp[cmpPos++];
                    }
                    while (sum == 0xff);
                }

                encCount += 4;

                int encPos = decPos - back;

                if (encCount <= back)
                {
                    dec.Slice(encPos, encCount).CopyTo(dec[decPos..]);

                    decPos += encCount;
                }
                else
                {
                    while (encCount-- > 0)
                    {
                        dec[decPos++] = dec[encPos++];
                    }
                }
            }
            while (cmpPos < cmp.Length && decPos < dec.Length);

            if (decPos != dec.Length)
            {
                throw new Exception("LZ4 decompression error");
            }

            return dec;
        }
    }
}