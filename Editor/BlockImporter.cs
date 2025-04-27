using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SRMI
{
    [ScriptedImporter(0, "block")]
    public class BlockImporter : ScriptedImporter
    {
        // Fields

        public string m_Directory;

        public bool m_IgnoreDirectory;

        public AssetImportContext m_Context;


        // Methods

        public override void OnImportAsset(AssetImportContext ctx)
        {
            Initialize(ctx);

            var stream = new FileStream(assetPath, FileMode.Open);
            var reader = new BinaryReader(stream);

            try
            {
                var block = new Block(reader, ctx);

                block.Execute();
            }
            finally
            {
                reader.Dispose();
                stream.Dispose();
            }
        }


        void Initialize(AssetImportContext ctx)
        {
            var block = ScriptableObject.CreateInstance<BlockScriptableObject>();
            block.name = "Main";

            m_Context = ctx;
            m_Context.AddObjectToAsset(block.name, block);
            m_Context.SetMainObject(block);
        }
    }
}