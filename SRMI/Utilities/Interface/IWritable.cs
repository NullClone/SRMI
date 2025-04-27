using UnityEditor.AssetImporters;

namespace SRMI
{
    public interface IWritable
    {
        void Write(AssetImportContext ctx);
    }
}