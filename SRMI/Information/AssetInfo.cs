using SRMI.Class;
using System.IO;

namespace SRMI
{
    public class AssetInfo : ISerialized
    {
        // Properties

        public int PreloadIndex { get; private set; }

        public int PreloadSize { get; private set; }

        public PPtr<Object> Asset { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            PreloadIndex = reader.ReadInt32();
            PreloadSize = reader.ReadInt32();

            Asset = new PPtr<Object>();
            Asset.Execute(reader);
        }
    }
}