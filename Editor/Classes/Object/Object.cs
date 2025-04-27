using SRMI.Utilities;
using System.IO;
using UnityEditor.AssetImporters;

namespace SRMI.Class
{
    public class Object : IAsset
    {
        // Properties

        public string Name { get; private set; }

        public long Offset { get; private set; }


        // Methods

        public Object(long offset)
        {
            Offset = offset;
        }


        public virtual void Read(BinaryReader reader)
        {
            Name = reader.ReadAlignString();
        }

        public virtual void Write(AssetImportContext ctx)
        {

        }
    }
}