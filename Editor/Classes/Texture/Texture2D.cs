using SRMI.Utilities;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SRMI.Class
{
    public class Texture2D : Texture
    {
        #region Properties

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int CompleteImageSize { get; private set; }

        public TextureFormat TextureFormat { get; private set; }

        public int MipCount { get; private set; }

        public bool IsReadable { get; private set; }

        public bool IgnoreMipmapLimit { get; private set; }

        public bool StreamingMipmaps { get; private set; }

        public int StreamingMipmapsPriority { get; private set; }

        public int ImageCount { get; private set; }

        public int Dimension { get; private set; }

        public FilterMode FilterMode { get; private set; }

        public int AnisoLevel { get; private set; }

        public float MipMapBias { get; private set; }

        public TextureWrapMode WrapModeU { get; private set; }

        public TextureWrapMode WrapModeV { get; private set; }

        public TextureWrapMode WrapModeW { get; private set; }

        public int LightmapFormat { get; private set; }

        public int ColorSpace { get; private set; }

        public byte[] Data { get; private set; }


        #endregion


        // Methods

        public Texture2D(long offset) : base(offset) { }


        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            CompleteImageSize = reader.ReadInt32();

            TextureFormat = reader.ReadEnum<TextureFormat>();
            MipCount = reader.ReadInt32();
            IsReadable = reader.ReadBoolean();
            IgnoreMipmapLimit = reader.ReadBoolean();
            StreamingMipmaps = reader.ReadBoolean();

            reader.Align();

            StreamingMipmapsPriority = reader.ReadInt32();
            ImageCount = reader.ReadInt32();
            Dimension = reader.ReadInt32();

            FilterMode = reader.ReadEnum<FilterMode>();
            AnisoLevel = reader.ReadInt32();
            MipMapBias = reader.ReadSingle();
            WrapModeU = reader.ReadEnum<TextureWrapMode>();
            WrapModeV = reader.ReadEnum<TextureWrapMode>();
            WrapModeW = reader.ReadEnum<TextureWrapMode>();

            LightmapFormat = reader.ReadInt32();
            ColorSpace = reader.ReadInt32();

            ReadData(reader);
        }

        public override void Write(AssetImportContext ctx)
        {
            base.Write(ctx);

            var texture = new UnityEngine.Texture2D(Width, Height, TextureFormat, MipCount, false)
            {
                name = Name,
                anisoLevel = AnisoLevel,
                mipMapBias = MipMapBias,
                ignoreMipmapLimit = IgnoreMipmapLimit,
                filterMode = FilterMode,
                wrapModeU = WrapModeU,
                wrapModeV = WrapModeV,
                wrapModeW = WrapModeW,
            };

            texture.LoadRawTextureData(Data);
            texture.Apply(true, true);

            ctx.AddObjectToAsset(texture.name, texture);
        }


        void ReadData(BinaryReader reader)
        {
            var imageDataSize = reader.ReadInt32();

            if (imageDataSize == 0)
            {
                var streamDataOffset = reader.ReadInt32();
                var streamDataSize = reader.ReadInt32();
                var streamDataPath = reader.ReadAlignString();

                if (streamDataPath != null && Offset > 0)
                {
                    reader.BaseStream.Seek(Offset + streamDataOffset, SeekOrigin.Begin);

                    Data = reader.ReadBytes(streamDataSize);

                    return;
                }
            }

            Data = reader.ReadBytes(imageDataSize);
        }
    }
}