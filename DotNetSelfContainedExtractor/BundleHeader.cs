namespace DotNetSelfContainedExtractor;

public readonly struct BundleHeader
{

    [Flags]
    public enum HeaderFlags
    {
        None,
        NetCore3Compat
    }

    public struct Location
    {
        public long Offset;
        public long Size;

        public bool IsValid => Offset != 0;

        public Location(BinaryReader reader)
        {
            Offset = reader.ReadInt64();
            Size = reader.ReadInt64();
        }
    }

    public readonly uint MajorVersion;
    public readonly uint MinorVersion;
    public readonly int EmbeddedFilesCount;
    public readonly string BundleId;
    public readonly Location DepsJsonLocation;
    public readonly Location RuntimeConfigJsonLocation;
    public readonly HeaderFlags Flags;

    public bool IsValid => EmbeddedFilesCount > 0 && MinorVersion == 0 && MajorVersion is 6 or 2;
    public bool IsNetCore3Compat => (Flags & HeaderFlags.NetCore3Compat) != 0;

    public BundleHeader(BinaryReader reader)
    {
        MajorVersion = reader.ReadUInt32();
        MinorVersion = reader.ReadUInt32();
        EmbeddedFilesCount = reader.ReadInt32();
        if (!IsValid)
        {
            throw new InvalidDataException(
                $"Failed to parse bundle. Parsed data: Version: {MajorVersion}.{MinorVersion}, Embedded file count: {EmbeddedFilesCount}");
        }

        BundleId = reader.ReadPathString();
        DepsJsonLocation = new Location(reader);
        RuntimeConfigJsonLocation = new Location(reader);
        Flags = (HeaderFlags)reader.ReadUInt64();
    }
}