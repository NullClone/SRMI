using SRMI.Utilities;
using System.IO;

namespace SRMI
{
    public class ObjectInfo : ISerialized
    {
        // Properties

        public long FileID { get; private set; }

        public int ByteStart { get; private set; }

        public int ByteSize { get; private set; }

        public int TypeID { get; private set; }

        public int ClassID { get; set; }

        public short ScriptTypeIndex { get; set; }

        public bool Stripped { get; set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            reader.Align();

            FileID = reader.ReadInt64();
            ByteStart = reader.ReadInt32();
            ByteSize = reader.ReadInt32();
            TypeID = reader.ReadInt32();
        }
    }
}