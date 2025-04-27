using System.IO;

namespace SRMI
{
    public class TypeInfo : ISerialized
    {
        // Properties

        public int TypeID { get; private set; }

        public bool IsStrippedType { get; private set; }

        public short ScriptTypeIndex { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            TypeID = reader.ReadInt32();
            IsStrippedType = reader.ReadBoolean();
            ScriptTypeIndex = reader.ReadInt16();

            if (TypeID == 114)
            {
                reader.BaseStream.Seek(32, SeekOrigin.Current);
            }
            else
            {
                reader.BaseStream.Seek(16, SeekOrigin.Current);
            }
        }
    }
}