namespace DotNetSelfContainedExtractor;

public struct BundleFileEntry
{
    public enum FileType
    {
        Unknown,
        Assembly,
        NativeBinary,
        DepsJson,
        RuntimeConfigJson,
        Symbols,
        Last
    }

    public readonly long Offset;
    public readonly long Size;
    public readonly long CompressedSize;
    public readonly FileType Type;
    public readonly string RelativePath;

    public bool IsValid => Offset > 0 && Size > 0 && CompressedSize >= 0 && Type != FileType.Last;

    public BundleFileEntry(BinaryReader reader, uint majorVersion)
    {
        Offset = reader.ReadInt64();
        Size = reader.ReadInt64();

        if (majorVersion >= 6)
            CompressedSize = reader.ReadInt64();

        Type = (FileType) reader.ReadByte();

        if (!IsValid)
            throw new InvalidDataException(
                $"Failed to parse bundle file entry.\n Offset: {Offset} Size: {Size} CompressedSize: {CompressedSize} Type: {Type}");

        RelativePath = reader.ReadPathString();
    }
}