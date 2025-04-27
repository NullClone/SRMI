using SRMI.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;

namespace SRMI.Class
{
    public class AssetBundle : Object
    {
        // Properties

        public PPtr<Object>[] PreloadTable { get; private set; }

        public KeyValuePair<string, AssetInfo>[] Container { get; private set; }


        // Methods

        public AssetBundle(long offset) : base(offset) { }


        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            PreloadTable = reader.ReadSerializedArray(() => new PPtr<Object>());

            Container = reader.ReadDictionaryArray<AssetInfo>();
        }

        public override void Write(AssetImportContext ctx)
        {
            base.Write(ctx);

            var main = ctx.mainObject as BlockScriptableObject;

            if (main == null) return;

            foreach ((var key, var value) in Container)
            {
                for (int i = value.PreloadIndex; i < value.PreloadIndex + value.PreloadSize; i++)
                {
                    main.Container.Add(new KeyValuePair<PPtr<Object>, string>(PreloadTable[i], key));
                }
            }
        }
    }
}