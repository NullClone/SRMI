using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRMI.Utilities
{
    public static class ReaderExtension
    {
        public static short ReadInt16(this BinaryReader reader, EndianType type = EndianType.LittleEndian)
        {
            if (type == EndianType.LittleEndian)
            {
                return reader.ReadInt16();
            }
            else
            {
                var buffer = new byte[sizeof(short)];

                reader.Read(buffer, 0, buffer.Length);

                return (short)((buffer[1] << 0) | (buffer[0] << 8));
            }
        }

        public static int ReadInt32(this BinaryReader reader, EndianType type = EndianType.LittleEndian)
        {
            if (type == EndianType.LittleEndian)
            {
                return reader.ReadInt32();
            }
            else
            {
                var buffer = new byte[sizeof(int)];

                reader.Read(buffer, 0, buffer.Length);

                return (buffer[3] << 0) | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24);
            }
        }

        public static long ReadInt64(this BinaryReader reader, EndianType type = EndianType.LittleEndian)
        {
            if (type == EndianType.LittleEndian)
            {
                return reader.ReadInt64();
            }
            else
            {
                var buffer = new byte[sizeof(long)];

                reader.Read(buffer, 0, buffer.Length);

                var value1 = ((buffer[7] << 0) | (buffer[6] << 8) | (buffer[5] << 16) | (buffer[4] << 24)) << 0;
                var value2 = ((buffer[3] << 0) | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24)) << 32;

                return value1 | value2;
            }
        }


        public static int ReadInt(this BinaryReader reader, VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.SInt32:
                case VertexAttributeFormat.UInt32:
                    {
                        return reader.ReadInt32();
                    }
                case VertexAttributeFormat.SInt16:
                case VertexAttributeFormat.UInt16:
                    {
                        return reader.ReadInt16();
                    }
                case VertexAttributeFormat.SInt8:
                case VertexAttributeFormat.UInt8:
                    {
                        return reader.ReadByte();
                    }

                default: throw new Exception();
            }
        }

        public static float ReadSingle(this BinaryReader reader, VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                    {
                        return reader.ReadSingle();
                    }
                case VertexAttributeFormat.Float16:
                    {
                        var value = reader.ReadUInt16();

                        return Half.ToSingle(value);
                    }
                case VertexAttributeFormat.SNorm16:
                    {
                        var value = reader.ReadInt16();

                        return Math.Max(value / 32767.0f, -1.0f);
                    }
                case VertexAttributeFormat.UNorm16:
                    {
                        var value = reader.ReadUInt16();

                        return value / 65535.0f;
                    }
                case VertexAttributeFormat.SNorm8:
                    {
                        var value = reader.ReadSByte();

                        return Math.Max(value / 127.0f, -1.0f);
                    }
                case VertexAttributeFormat.UNorm8:
                    {
                        var value = reader.ReadByte();

                        return value / 255.0f;
                    }

                default: throw new Exception();
            }
        }


        public static string ReadString(this BinaryReader reader, int count, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;

            var buffer = reader.ReadBytes(count);

            return encoding.GetString(buffer);
        }

        public static string ReadNullString(this BinaryReader reader, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;

            var buffer = new List<byte>();

            while (reader.BaseStream.Length > reader.BaseStream.Position)
            {
                var b = reader.ReadByte();

                if (b == 0) break;

                buffer.Add(b);
            }

            return encoding.GetString(buffer.ToArray());
        }

        public static string ReadAlignString(this BinaryReader reader, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;

            var count = reader.ReadInt32();

            if (count > 0)
            {
                var remaining = reader.BaseStream.Length - reader.BaseStream.Position;

                if (remaining >= count)
                {
                    var buffer = reader.ReadBytes(count);

                    reader.Align();

                    return encoding.GetString(buffer);
                }
            }

            return null;
        }


        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader)
        {
            return new Matrix4x4(reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4());
        }


        public static void Align(this BinaryReader reader)
        {
            reader.Align(4);
        }

        public static void Align(this BinaryReader reader, int alignment)
        {
            var mod = reader.BaseStream.Position % alignment;

            if (mod != 0)
            {
                reader.BaseStream.Position += alignment - mod;
            }
        }


        public static T ReadEnum<T>(this BinaryReader reader)
        {
            return (T)(object)reader.ReadInt32();
        }


        public static T[] ReadArray<T>(this BinaryReader reader, Func<T> instantiator, EndianType type = EndianType.LittleEndian)
        {
            var count = reader.ReadInt32(type);

            var array = new T[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = instantiator.Invoke();
            }

            return array;
        }

        public static T[] ReadArray<T>(this BinaryReader reader, Func<T> instantiator, int count)
        {
            var array = new T[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = instantiator.Invoke();
            }

            return array;
        }

        public static T[] ReadSerializedArray<T>(this BinaryReader reader, Func<T> instantiator, EndianType type = EndianType.LittleEndian) where T : ISerialized
        {
            var count = reader.ReadInt32(type);

            var array = new T[count];

            for (int i = 0; i < count; i++)
            {
                var instance = instantiator();

                instance.Execute(reader);

                array[i] = instance;
            }

            return array;
        }

        public static T[] ReadSerializedArray<T>(this BinaryReader reader, Func<T> instantiator, int count) where T : ISerialized
        {
            var array = new T[count];

            for (int i = 0; i < count; i++)
            {
                var instance = instantiator();

                instance.Execute(reader);

                array[i] = instance;
            }

            return array;
        }


        public static KeyValuePair<string, T>[] ReadDictionaryArray<T>(this BinaryReader reader) where T : ISerialized, new()
        {
            int count = reader.ReadInt32();

            var array = new KeyValuePair<string, T>[count];

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadAlignString();

                var value = new T();

                value.Execute(reader);

                var result = new KeyValuePair<string, T>(key, value);

                array[i] = result;
            }

            return array;
        }
    }
}