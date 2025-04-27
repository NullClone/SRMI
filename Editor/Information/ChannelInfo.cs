using System.IO;

namespace SRMI
{
    public class ChannelInfo : ISerialized
    {
        // Properties

        public byte Stream { get; private set; }

        public byte Offset { get; private set; }

        public byte Format { get; private set; }

        public byte RawDimension { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            Stream = reader.ReadByte();
            Offset = reader.ReadByte();
            Format = reader.ReadByte();
            RawDimension = (byte)(reader.ReadByte() & 0xF);
        }
    }
}