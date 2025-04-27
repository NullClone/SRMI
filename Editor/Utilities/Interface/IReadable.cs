using System.IO;

namespace SRMI
{
    public interface IReadable
    {
        void Read(BinaryReader reader);
    }
}