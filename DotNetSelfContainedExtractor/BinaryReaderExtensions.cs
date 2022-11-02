using System.Text;

namespace DotNetSelfContainedExtractor;

public static class BinaryReaderExtensions
{
    // While .ReadString could be used, the 2-byte max length is something potentially useful.
    public static string ReadPathString(this BinaryReader reader)
    {
        var pathLength = reader.ReadPathLength();
        return Encoding.UTF8.GetString(reader.ReadBytes(pathLength));
    }

    public static int ReadPathLength(this BinaryReader reader)
    {
        int length;

        var first = reader.ReadByte();
        if ((first & 0x80) == 0)
            length = first;
        else
        {
            var second = reader.ReadByte();
            if ((second & 0x80) != 0)
            {
                Console.WriteLine("Error: Bundle path length attempted to read beyond two bytes.");
                throw new InvalidDataException("Failed to read path length.");
            }

            length = (second << 7) | (first & 0x7f);
        }

        if (length is <= 0 or > 4095)
        {
            Console.WriteLine("Error: Bundle path length is zero or too long.");
            throw new InvalidDataException("Read invalid path length from bundle.");
        }

        return length;
    }
}