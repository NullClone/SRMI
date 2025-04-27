using SRMI.Utilities;
using System.IO;

namespace SRMI
{
    public class BlockInfo : ISerialized
    {
        // Properties

        public int CompressedSize { get; private set; }

        public int UncompressedSize { get; private set; }

        public int CompressionFlags { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            UncompressedSize = reader.ReadInt32(EndianType.BigEndian);
            CompressedSize = reader.ReadInt32(EndianType.BigEndian);
            CompressionFlags = reader.ReadInt16(EndianType.BigEndian);
        }
    }
}