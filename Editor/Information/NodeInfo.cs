using SRMI.Utilities;
using System.IO;

namespace SRMI
{
    public class NodeInfo : ISerialized
    {
        // Properties

        public string Path { get; private set; }

        public long Size { get; private set; }

        public long Offset { get; private set; }

        public int Flags { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            Offset = reader.ReadInt64(EndianType.BigEndian);
            Size = reader.ReadInt64(EndianType.BigEndian);
            Flags = reader.ReadInt32(EndianType.BigEndian);
            Path = reader.ReadNullString();
        }
    }
}