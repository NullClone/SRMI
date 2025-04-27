using SRMI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRMI.Class
{
    public class Mesh : Object
    {
        #region Properties

        public SubMesh[] SubMeshes { get; private set; }

        public BlendShapeVertex[] BlendShapeVertices { get; private set; }

        public BlendShape[] BlendShapes { get; private set; }

        public BlendShapeChannel[] BlendShapeChannels { get; private set; }

        public float[] FullWeights { get; private set; }

        public Matrix4x4[] BindPose { get; private set; }

        public int[] BoneNameHashes { get; private set; }

        public int RootBoneNameHash { get; private set; }

        public MinMaxAABB[] BonesAABB { get; private set; }

        public int[] VariableBoneCountWeights { get; private set; }

        public byte MeshCompression { get; private set; }

        public bool IsReadable { get; private set; }

        public bool KeepVertices { get; private set; }

        public bool KeepIndices { get; private set; }

        public IndexFormat IndexFormat { get; private set; }

        public int[] IndexBuffer { get; private set; }

        public int VertexCount { get; private set; }

        public ChannelInfo[] VertexChannels { get; private set; }

        public StreamInfo[] VertexStreams { get; private set; }

        public byte[] VertexData { get; private set; }

        public int UVInfo { get; private set; }

        public Vector3 LocalCenter { get; private set; }

        public Vector3 LocalExtent { get; private set; }

        public int MeshUsageFlags { get; private set; }

        public byte[] BakedConvexCollisionMesh { get; private set; }

        public byte[] BakedTriangleCollisionMesh { get; private set; }

        public float[] MeshMetrics { get; private set; }

        #endregion


        // Methods

        public Mesh(long offset) : base(offset) { }


        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            SubMeshes = reader.ReadSerializedArray(() => new SubMesh());

            BlendShapeVertices = reader.ReadSerializedArray(() => new BlendShapeVertex());
            BlendShapes = reader.ReadSerializedArray(() => new BlendShape());
            BlendShapeChannels = reader.ReadSerializedArray(() => new BlendShapeChannel());
            FullWeights = reader.ReadArray(reader.ReadSingle);

            BindPose = reader.ReadArray(reader.ReadMatrix4x4);
            BoneNameHashes = reader.ReadArray(reader.ReadInt32);
            RootBoneNameHash = reader.ReadInt32();
            BonesAABB = reader.ReadSerializedArray(() => new MinMaxAABB());
            VariableBoneCountWeights = reader.ReadArray(reader.ReadInt32);

            MeshCompression = reader.ReadByte();
            IsReadable = reader.ReadBoolean();
            KeepVertices = reader.ReadBoolean();
            KeepIndices = reader.ReadBoolean();

            reader.Align();

            IndexFormat = reader.ReadEnum<IndexFormat>();

            if (IndexFormat == IndexFormat.UInt16)
            {
                var indexBufferSize = reader.ReadInt32();

                IndexBuffer = new int[indexBufferSize / 2];

                for (int i = 0; i < IndexBuffer.Length; i++)
                {
                    IndexBuffer[i] = reader.ReadInt16();
                }

                reader.Align();
            }
            else
            {
                var indexBufferSize = reader.ReadInt32();

                IndexBuffer = new int[indexBufferSize / 4];

                for (int i = 0; i < IndexBuffer.Length; i++)
                {
                    IndexBuffer[i] = reader.ReadInt32();
                }
            }

            VertexCount = reader.ReadInt32();
            VertexChannels = reader.ReadSerializedArray(() => new ChannelInfo());
            VertexData = reader.ReadArray(reader.ReadByte);

            reader.Align();

            if (MeshCompression == 0)
            {
                reader.BaseStream.Seek(160, SeekOrigin.Current);
            }
            else
            {
                Debug.Log($"Mesh Compression : [{(ModelImporterMeshCompression)MeshCompression}]");

                return;
            }

            UVInfo = reader.ReadInt32();
            LocalCenter = reader.ReadVector3();
            LocalExtent = reader.ReadVector3();
            MeshUsageFlags = reader.ReadInt32();

            BakedConvexCollisionMesh = reader.ReadArray(reader.ReadByte);

            reader.Align();

            BakedTriangleCollisionMesh = reader.ReadArray(reader.ReadByte);

            reader.Align();

            MeshMetrics = reader.ReadArray(reader.ReadSingle, 2);

            ReadData(reader);
        }

        public override void Write(AssetImportContext ctx)
        {
            base.Write(ctx);

            if (MeshCompression != 0) return;

            GetStreams();

            var mesh = new UnityEngine.Mesh
            {
                name = Name,
                bindposes = BindPose,
            };

            var stream = new MemoryStream(VertexData);
            var reader = new BinaryReader(stream);

            SetVertex(mesh, reader);

            reader.Dispose();
            stream.Dispose();

            SetIndices(mesh);

            mesh.RecalculateNormals();
            mesh.UploadMeshData(true);

            ctx.AddObjectToAsset(mesh.name, mesh);
        }


        void ReadData(BinaryReader reader)
        {
            reader.Align();

            var streamDataOffset = reader.ReadInt32();
            var streamDataSize = reader.ReadInt32();
            var streamDataPath = reader.ReadAlignString();

            if (streamDataPath != null && Offset > 0)
            {
                reader.BaseStream.Seek(Offset + streamDataOffset, SeekOrigin.Begin);

                VertexData = reader.ReadBytes(streamDataSize);
            }
        }

        void GetStreams()
        {
            VertexStreams = new StreamInfo[VertexChannels.Max(x => x.Stream) + 1];

            int offset = 0;

            for (int i = 0; i < VertexStreams.Length; i++)
            {
                int stride = 0;
                int mask = 0;

                foreach (var channel in VertexChannels)
                {
                    if (channel.Stream == i && channel.RawDimension > 0)
                    {
                        var formatSize = (VertexAttributeFormat)channel.Format switch
                        {
                            VertexAttributeFormat.Float32 or
                            VertexAttributeFormat.SInt32 or
                            VertexAttributeFormat.UInt32 => 4,
                            VertexAttributeFormat.Float16 or
                            VertexAttributeFormat.SInt16 or
                            VertexAttributeFormat.SNorm16 or
                            VertexAttributeFormat.UInt16 or
                            VertexAttributeFormat.UNorm16 => 2,
                            VertexAttributeFormat.SInt8 or
                            VertexAttributeFormat.SNorm8 or
                            VertexAttributeFormat.UInt8 or
                            VertexAttributeFormat.UNorm8 => 1,

                            _ => throw new Exception(),
                        };

                        stride += channel.RawDimension * formatSize;

                        mask |= 1 << i;
                    }
                }

                VertexStreams[i] = new StreamInfo
                {
                    Offset = offset,
                    Stride = stride,
                    ChannelMask = mask,
                };

                offset = (offset + (VertexCount * stride) + 15) & ~15;
            }
        }

        void SetVertex(UnityEngine.Mesh mesh, BinaryReader reader)
        {
            var boneWeights = new BoneWeight[VertexCount];

            for (int i = 0; i < VertexChannels.Length; i++)
            {
                var channel = VertexChannels[i];

                var stream = VertexStreams[channel.Stream];

                if (channel.RawDimension > 0 && ((stream.ChannelMask >> i) & 0) == 0)
                {
                    reader.BaseStream.Seek(channel.Offset + stream.Offset, SeekOrigin.Begin);

                    var format = (VertexAttributeFormat)channel.Format;

                    switch ((VertexAttribute)i)
                    {
                        case VertexAttribute.Position:
                            {
                                if (channel.RawDimension == 3)
                                {
                                    var vertices = new Vector3[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        vertices[j] = new Vector3(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.vertices = vertices;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.Normal:
                            {
                                if (channel.RawDimension == 3 || channel.RawDimension == 4)
                                {
                                    var normals = new Vector3[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        normals[j] = new Vector3(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.normals = normals;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.Tangent:
                            {
                                if (channel.RawDimension == 4)
                                {
                                    var tangents = new Vector4[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        tangents[j] = new Vector4(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.tangents = tangents;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.Color:
                            {
                                if (channel.RawDimension == 4)
                                {
                                    var colors = new Color[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        colors[j] = new Color(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.colors = colors;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord0:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord1:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv2 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord2:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv3 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord3:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv4 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord4:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv5 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord5:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv6 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord6:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv7 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.TexCoord7:
                            {
                                if (channel.RawDimension == 2)
                                {
                                    var uv = new Vector2[VertexCount];

                                    for (int j = 0; j < VertexCount; j++)
                                    {
                                        var position = reader.BaseStream.Position;

                                        uv[j] = new Vector2(
                                            reader.ReadSingle(format),
                                            reader.ReadSingle(format));

                                        reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                        reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                    }

                                    mesh.uv8 = uv;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }

                                break;
                            }
                        case VertexAttribute.BlendWeight:
                            {
                                for (int j = 0; j < VertexCount; j++)
                                {
                                    var position = reader.BaseStream.Position;

                                    boneWeights[j].weight0 = reader.ReadSingle(format);
                                    boneWeights[j].weight1 = (channel.RawDimension > 1) ? reader.ReadSingle(format) : 0;
                                    boneWeights[j].weight2 = (channel.RawDimension > 2) ? reader.ReadSingle(format) : 0;
                                    boneWeights[j].weight3 = (channel.RawDimension > 3) ? reader.ReadSingle(format) : 0;

                                    reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                    reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                }

                                break;
                            }
                        case VertexAttribute.BlendIndices:
                            {
                                for (int j = 0; j < VertexCount; j++)
                                {
                                    var position = reader.BaseStream.Position;

                                    boneWeights[j].boneIndex0 = reader.ReadInt(format);
                                    boneWeights[j].boneIndex1 = (channel.RawDimension > 1) ? reader.ReadInt(format) : 0;
                                    boneWeights[j].boneIndex2 = (channel.RawDimension > 2) ? reader.ReadInt(format) : 0;
                                    boneWeights[j].boneIndex3 = (channel.RawDimension > 3) ? reader.ReadInt(format) : 0;

                                    reader.BaseStream.Seek(position, SeekOrigin.Begin);

                                    reader.BaseStream.Seek(stream.Stride, SeekOrigin.Current);
                                }

                                break;
                            }
                    }
                }
            }

            mesh.boneWeights = boneWeights;
        }

        void SetIndices(UnityEngine.Mesh mesh)
        {
            mesh.indexFormat = IndexFormat;
            mesh.subMeshCount = SubMeshes.Length;

            for (int i = 0; i < SubMeshes.Length; i++)
            {
                var subMesh = SubMeshes[i];

                var topology = subMesh.Topology;

                var indexCount = subMesh.IndexCount;

                var firstIndex = (IndexFormat == IndexFormat.UInt16) ? subMesh.FirstByte / 2 : subMesh.FirstByte / 4;

                var indices = new List<int>();

                if (topology == MeshTopology.Triangles)
                {
                    for (int j = 0; j < subMesh.IndexCount; j += 3)
                    {
                        indices.Add(IndexBuffer[firstIndex + j]);
                        indices.Add(IndexBuffer[firstIndex + j + 1]);
                        indices.Add(IndexBuffer[firstIndex + j + 2]);
                    }
                }
                else if (topology == MeshTopology.Quads)
                {
                    for (int j = 0; j < subMesh.IndexCount; j += 4)
                    {
                        indices.Add(IndexBuffer[firstIndex + j]);
                        indices.Add(IndexBuffer[firstIndex + j + 1]);
                        indices.Add(IndexBuffer[firstIndex + j + 2]);
                        indices.Add(IndexBuffer[firstIndex + j]);
                        indices.Add(IndexBuffer[firstIndex + j + 2]);
                        indices.Add(IndexBuffer[firstIndex + j + 3]);
                    }

                    indexCount = indexCount / 2 * 3;
                }
                else
                {
                    throw new NotSupportedException();
                }

                if (indexCount > 65535 && IndexFormat == IndexFormat.UInt16)
                {
                    Debug.LogError("32 bit not supported");

                    return;
                }

                var descriptor = new SubMeshDescriptor
                {
                    baseVertex = subMesh.BaseVertex,
                    bounds = new Bounds(subMesh.Center, subMesh.Extent),
                    firstVertex = subMesh.FirstVertex,
                    indexCount = indexCount,
                    indexStart = firstIndex,
                    topology = topology,
                    vertexCount = subMesh.VertexCount,
                };

                mesh.SetIndices(indices, 0, indexCount, topology, i);

                mesh.SetSubMesh(i, descriptor);
            }
        }
    }

    public class SubMesh : ISerialized
    {
        // Properties

        public int FirstByte { get; private set; }

        public int IndexCount { get; private set; }

        public MeshTopology Topology { get; private set; }

        public int BaseVertex { get; private set; }

        public int FirstVertex { get; private set; }

        public int VertexCount { get; private set; }

        public Vector3 Center { get; private set; }

        public Vector3 Extent { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            FirstByte = reader.ReadInt32();
            IndexCount = reader.ReadInt32();
            Topology = reader.ReadEnum<MeshTopology>();
            BaseVertex = reader.ReadInt32();
            FirstVertex = reader.ReadInt32();
            VertexCount = reader.ReadInt32();
            Center = reader.ReadVector3();
            Extent = reader.ReadVector3();
        }
    }

    public class BlendShape : ISerialized
    {
        // Properties

        public int FirstVertex { get; private set; }

        public int VertexCount { get; private set; }

        public bool HasNormals { get; private set; }

        public bool HasTangents { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            FirstVertex = reader.ReadInt32();
            VertexCount = reader.ReadInt32();
            HasNormals = reader.ReadBoolean();
            HasTangents = reader.ReadBoolean();

            reader.Align();
        }
    }

    public class BlendShapeVertex : ISerialized
    {
        // Properties

        public Vector3 Vertex { get; private set; }

        public Vector3 Normal { get; private set; }

        public Vector3 Tangent { get; private set; }

        public int Index { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            Vertex = reader.ReadVector3();
            Normal = reader.ReadVector3();
            Tangent = reader.ReadVector3();
            Index = reader.ReadInt32();
        }
    }

    public class BlendShapeChannel : ISerialized
    {
        // Properties

        public string Name { get; private set; }

        public int NameHash { get; private set; }

        public int FrameIndex { get; private set; }

        public int FrameCount { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            Name = reader.ReadAlignString();
            NameHash = reader.ReadInt32();
            FrameIndex = reader.ReadInt32();
            FrameCount = reader.ReadInt32();
        }
    }

    public class MinMaxAABB : ISerialized
    {
        // Properties

        public Vector3 Min { get; private set; }

        public Vector3 Max { get; private set; }


        // Methods

        public void Execute(BinaryReader reader)
        {
            Min = reader.ReadVector3();
            Max = reader.ReadVector3();
        }
    }

    public class Vector
    {
        // Properties

        public int NumItems { get; private set; }

        public float Range { get; private set; }

        public float Start { get; private set; }

        public byte[] Data { get; private set; }

        public byte BitSize { get; private set; }


        // Methods

        public void Read(BinaryReader reader, bool isFloat = true)
        {
            NumItems = reader.ReadInt32();

            if (isFloat)
            {
                Range = reader.ReadSingle();
                Start = reader.ReadSingle();
            }

            Data = reader.ReadArray(reader.ReadByte);

            reader.Align();

            BitSize = reader.ReadByte();

            reader.Align();
        }
    }
}