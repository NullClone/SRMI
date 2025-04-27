namespace SRMI
{
    public class HeaderInfo
    {
        public int MetaDataSize { get; set; }

        public int FileSize { get; set; }

        public int Version { get; set; }

        public int DataOffset { get; set; }

        public bool Endianness { get; set; }

        public string UnityVersion { get; set; }

        public int BuildTarget { get; set; }

        public bool EnableTypeTree { get; set; }
    }
}