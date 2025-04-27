using System.IO;

namespace SRMI
{
    public interface ISerialized
    {
        void Execute(BinaryReader reader);
    }
}