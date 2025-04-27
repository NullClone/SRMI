using SRMI.Utilities;
using System.IO;

namespace SRMI
{
    public class ScriptTypeInfo : ISerialized
    {
        // Properties

        public int LocalSerializedFileIndex { get; private set; }

        public long LocalIdentifierInFile { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            LocalSerializedFileIndex = reader.ReadInt32();

            reader.Align();

            LocalIdentifierInFile = reader.ReadInt64();
        }
    }
}