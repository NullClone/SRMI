using SRMI.Utilities;
using System.IO;

namespace SRMI
{
    public class ExternalInfo : ISerialized
    {
        // Properties

        public string AssetPath { get; private set; }

        public byte[] Guid { get; private set; }

        public int Type { get; private set; }

        public string PathNameOrigin { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            AssetPath = reader.ReadNullString();

            Guid = reader.ReadBytes(16);

            Type = reader.ReadInt32();

            PathNameOrigin = reader.ReadNullString();
        }
    }
}