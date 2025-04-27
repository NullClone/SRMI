using SRMI.Utilities;
using System;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;

namespace SRMI
{
    public class Block
    {
        // Fields

        readonly BinaryReader reader;

        readonly AssetImportContext ctx;


        // Properties

        BlockInfo[] Blocks { get; set; }

        NodeInfo[] Nodes { get; set; }


        // Methods

        public Block(BinaryReader reader, AssetImportContext ctx)
        {
            this.reader = reader;
            this.ctx = ctx;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
        }


        public void Execute()
        {
            var signature = reader.ReadNullString();

            if (signature != "ENCR") return;

            reader.BaseStream.Seek(8, SeekOrigin.Current);

            var compressedSize = reader.ReadInt32(EndianType.BigEndian);
            var uncompressedSize = reader.ReadInt32(EndianType.BigEndian);
            var compressionFlags = reader.ReadInt32(EndianType.BigEndian);

            var blockStream = new MemoryStream();

            switch ((CompressionType)(compressionFlags & 0x3F))
            {
                case CompressionType.None:
                    {
                        var compressedBuffer = reader.ReadBytes(compressedSize);

                        blockStream.Write(compressedBuffer);

                        break;
                    }
                case CompressionType.Lz4:
                case CompressionType.Lz4HC:
                    {
                        var compressedBuffer = reader.ReadBytes(compressedSize);

                        var uncompressedBuffer = LZ4.Decompress(compressedBuffer, new byte[uncompressedSize]);

                        blockStream.Write(uncompressedBuffer);

                        break;
                    }
                case CompressionType.Lz4Mr0k:
                    {
                        var compressedBuffer = reader.ReadBytes(compressedSize);

                        var uncompressedBuffer = Mr0k.Decrypt(compressedBuffer);

                        uncompressedBuffer = LZ4.Decompress(uncompressedBuffer, new byte[uncompressedSize]);

                        blockStream.Write(uncompressedBuffer);

                        break;
                    }

                default: throw new IOException("Unsupported compression type");
            }

            var blockReader = new BinaryReader(blockStream);

            blockReader.BaseStream.Seek(0, SeekOrigin.Begin);

            Blocks = blockReader.ReadSerializedArray(() => new BlockInfo(), EndianType.BigEndian);
            Nodes = blockReader.ReadSerializedArray(() => new NodeInfo(), EndianType.BigEndian);

            blockReader.Dispose();
            blockStream.Dispose();

            Read();

            Execute();
        }

        public void Read()
        {
            Create(out var stream);

            for (int i = 0; i < Blocks.Length; i++)
            {
                switch ((CompressionType)(Blocks[i].CompressionFlags & 0x3F))
                {
                    case CompressionType.None:
                        {
                            var compressedBuffer = reader.ReadBytes(Blocks[i].CompressedSize);

                            stream.Write(compressedBuffer);

                            break;
                        }
                    case CompressionType.OodleHSR:
                        {
                            var compressedBuffer = reader.ReadBytes(Blocks[i].CompressedSize);

                            var uncompressedBuffer = Oodle.Decompress(compressedBuffer, new byte[Blocks[i].UncompressedSize]);

                            stream.Write(uncompressedBuffer);

                            break;
                        }
                    case CompressionType.OodleMr0k:
                        {
                            var compressedBuffer = reader.ReadBytes(Blocks[i].CompressedSize);

                            var uncompressedBuffer = Mr0k.Decrypt(compressedBuffer);

                            uncompressedBuffer = Oodle.Decompress(uncompressedBuffer, new byte[Blocks[i].UncompressedSize]);

                            stream.Write(uncompressedBuffer);

                            break;
                        }

                    default: throw new IOException("Unsupported compression type");
                }
            }

            if (Nodes.Length > 0 || Nodes.Length < 3)
            {
                var reader = new BinaryReader(stream);

                reader.BaseStream.Seek(Nodes[0].Offset, SeekOrigin.Begin);

                var offset = (Nodes.Length == 1) ? -1 : Nodes[1].Offset;

                var cab = new Cab(reader, ctx);

                cab.Execute(offset);
                cab.Read();

                reader.Dispose();
                stream.Dispose();
            }
            else
            {
                throw new ArgumentOutOfRangeException("");
            }
        }

        public void Create(out Stream stream)
        {
            var uncompressedSize = Blocks.Sum(t => t.UncompressedSize);

            if (uncompressedSize >= int.MaxValue)
            {
                var path = ctx.assetPath.Replace("block", "tmp");

                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            }
            else
            {
                stream = new MemoryStream();
            }
        }
    }
}