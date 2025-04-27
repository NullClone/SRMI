using System.IO;

namespace SRMI.Class
{
    public struct PPtr<T> : ISerialized where T : Object
    {
        // Properties

        public int FileIndex { get; private set; }

        public long PathID { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            FileIndex = reader.ReadInt32();
            PathID = reader.ReadInt64();
        }
    }
}