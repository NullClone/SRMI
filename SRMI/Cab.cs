using SRMI.Utilities;
using System;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SRMI
{
    public class Cab
    {
        // Fields

        readonly BinaryReader reader;

        readonly AssetImportContext ctx;

        readonly long basePosition;


        // Properties

        public HeaderInfo Header { get; private set; }

        public TypeInfo[] Types { get; private set; }

        public ObjectInfo[] Objects { get; private set; }

        public ScriptTypeInfo[] ScriptTypes { get; private set; }

        public ExternalInfo[] Externals { get; private set; }

        public TypeInfo[] RefTypes { get; private set; }

        public string UserInformation { get; private set; }


        long Offset { get; set; }


        // Methods

        public Cab(BinaryReader reader, AssetImportContext ctx)
        {
            this.reader = reader;
            this.ctx = ctx;

            basePosition = reader.BaseStream.Position;
        }


        public void Execute(long offset)
        {
            Offset = offset;

            Header = new HeaderInfo
            {
                MetaDataSize = reader.ReadInt32(EndianType.BigEndian),
                FileSize = reader.ReadInt32(EndianType.BigEndian),
                Version = reader.ReadInt32(EndianType.BigEndian),
                DataOffset = reader.ReadInt32(EndianType.BigEndian),
                Endianness = reader.ReadBoolean(),
            };

            if (Header.Endianness == true)
            {
                throw new NotSupportedException();
            }

            reader.Align();

            Header.UnityVersion = reader.ReadNullString();
            Header.BuildTarget = reader.ReadInt32();
            Header.EnableTypeTree = reader.ReadBoolean();

            if (Header.EnableTypeTree == true)
            {
                throw new NotSupportedException();
            }

            Types = reader.ReadSerializedArray(() => new TypeInfo());
            Objects = reader.ReadSerializedArray(() => new ObjectInfo());
            ScriptTypes = reader.ReadSerializedArray(() => new ScriptTypeInfo());
            Externals = reader.ReadSerializedArray(() => new ExternalInfo());
            RefTypes = reader.ReadSerializedArray(() => new TypeInfo());

            UserInformation = reader.ReadNullString();
        }

        public void Read()
        {
            var main = ctx.mainObject as BlockScriptableObject;

            if (main == null) return;

            var baseOffset = Header.DataOffset + basePosition;

            for (int i = 0; i < Objects.Length; i++)
            {
                var type = Types[Objects[i].TypeID];

                Objects[i].ClassID = type.TypeID;
                Objects[i].ScriptTypeIndex = type.ScriptTypeIndex;
                Objects[i].Stripped = type.IsStrippedType;

                if ((ClassIDType)Objects[i].ClassID == ClassIDType.AssetBundle)
                {
                    reader.BaseStream.Seek(baseOffset + Objects[i].ByteStart, SeekOrigin.Begin);

                    var asset = new Class.AssetBundle(Offset);

                    asset.Read(reader);
                    asset.Write(ctx);
                }
            }

            for (int i = 0; i < Objects.Length; i++)
            {
                reader.BaseStream.Seek(baseOffset + Objects[i].ByteStart, SeekOrigin.Begin);

                if (TryGetAsset((ClassIDType)Objects[i].ClassID, out var asset))
                {
                    asset.Read(reader);
                    asset.Write(ctx);

                    foreach ((var key, var value) in main.Container)
                    {
                        if (key.FileIndex > 0 || Objects[i].FileID != key.PathID) continue;

                        if (value.EndsWith(".png") || value.EndsWith(".tga"))
                        {
                            Debug.Log(asset.Name + " : " + value);
                        }
                    }
                }
                else
                {
                    main.SkippedObjectLength += 1;
                }

                main.ObjectLength += 1;
            }
        }

        public bool TryGetAsset(ClassIDType type, out Class.Object asset)
        {
            asset = type switch
            {
                ClassIDType.Mesh => new Class.Mesh(Offset),
                ClassIDType.Texture2D => new Class.Texture2D(Offset),

                _ => null,
            };

            return asset != null;
        }
    }
}