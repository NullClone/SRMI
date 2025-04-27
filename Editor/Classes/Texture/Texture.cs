using SRMI.Utilities;
using System.IO;
using UnityEditor.AssetImporters;

namespace SRMI.Class
{
    public class Texture : Object
    {
        // Properties

        public int ForcedFallbackFormat { get; private set; }

        public bool DownscaleFallback { get; private set; }


        // Methods

        public Texture(long offset) : base(offset) { }


        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            ForcedFallbackFormat = reader.ReadInt32();
            DownscaleFallback = reader.ReadBoolean();

            reader.Align();
        }

        public override void Write(AssetImportContext ctx)
        {
            base.Write(ctx);
        }
    }
}