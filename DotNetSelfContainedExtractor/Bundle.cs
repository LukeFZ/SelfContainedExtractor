using System.IO.Compression;

namespace DotNetSelfContainedExtractor;

public class Bundle
{
    private readonly string _bundlePath;
    private readonly long _bundleOffset;

    private BundleHeader _header;
    public readonly List<BundleFileEntry> EmbeddedFiles;

    public Bundle(string bundlePath, long bundleOffset)
    {
        _bundlePath = bundlePath;
        _bundleOffset = bundleOffset;
        EmbeddedFiles = new List<BundleFileEntry>();
    }

    public static bool FindBundleOffset(string bundlePath, out int bundleOffset)
    {
        var testGuidBytes = new byte[] // Encoding.UTF8.GetBytes("d38cc827-e34f-4453-9df4-1e796e9f1d07")
        {
            0x33, 0x38, 0x63, 0x63, 0x38, 0x32, 0x37, 0x2d, 0x65, 0x33, 0x34, 0x66, 0x2d, 0x34, 0x34, 0x35, 0x33, 0x2d,
            0x39, 0x64, 0x66, 0x34, 0x2d, 0x31, 0x65, 0x37, 0x39, 0x36, 0x65, 0x39, 0x66, 0x31, 0x64, 0x30, 0x37
        };

        var bundleBytes = File.ReadAllBytes(bundlePath);

        // Checking for 32bit/x86 executable
        var peOffset = BitConverter.ToInt32(bundleBytes, 0x3c);
        var peMachineType = BitConverter.ToUInt16(bundleBytes, peOffset + 4);

        var guidBundleOffset = peMachineType == 0x14c
            ? 0x1 + 0x8 + 0x20 + 0x8   // test enable byte + (0x4 padding + 0x4 ptr) + 0x20 bundle sig + 0x8 bundle offset
            : 0x1 + 0x10 + 0x20 + 0x8;  // test enable byte + (0x8 padding + 0x8 ptr) + 0x20 bundle sig + 0x8 bundle offset

        for (var i = 0; i < bundleBytes.Length; i++)
        {
            if (!testGuidBytes.Where((pb, pbi) => pb != bundleBytes[i + pbi]).Any())
            {
                bundleOffset = BitConverter.ToInt32(bundleBytes, i - guidBundleOffset);
                return true;
            }
        }

        bundleOffset = -1;
        return false;
    }

    public bool ReadBundle()
    {
        var bundleData = File.ReadAllBytes(_bundlePath);
        using var bundleStream = new MemoryStream(bundleData);
        using var bundleReader = new BinaryReader(bundleStream);

        bundleReader.BaseStream.Seek(_bundleOffset, SeekOrigin.Begin);

        try
        {
            _header = new BundleHeader(bundleReader);
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine("Error while parsing bundle header.");
            Console.WriteLine(ex.Message);
            return false;
        }

        Console.WriteLine($"Bundle details:");
        Console.WriteLine($"Bundle ID: {_header.BundleId}");
        Console.WriteLine($"Version: {_header.MajorVersion}.{_header.MinorVersion}");
        Console.WriteLine($"Embedded files count: {_header.EmbeddedFilesCount}");
        //Console.WriteLine($".deps.json offset: {_header.DepsJsonLocation.Offset}");
        //Console.WriteLine($".runtimeconfig.json offset: {_header.RuntimeConfigJsonLocation.Offset}");

        try
        {
            for (int i = 0; i < _header.EmbeddedFilesCount; i++)
            {
                var entry = new BundleFileEntry(bundleReader, _header.MajorVersion);
                Console.WriteLine($"Embedded file info: Name: {entry.RelativePath}, Size: {entry.Size}, Type: {entry.Type}");
                EmbeddedFiles.Add(entry);
            }
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine("Error while parsing embedded bundle files.");
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;
    }

    public void ExtractFiles(string outputDirectory)
    {
        var bundleData = File.ReadAllBytes(_bundlePath);
        using var bundleStream = new MemoryStream(bundleData);
        using var bundleReader = new BinaryReader(bundleStream);

        Directory.CreateDirectory(outputDirectory);

        foreach (var entry in EmbeddedFiles)
        {
            using var outputStream = File.OpenWrite(Path.Join(outputDirectory, entry.RelativePath));

            bundleReader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
            if (entry.CompressedSize != 0)
            {
                var compressedData = bundleReader.ReadBytes((int) entry.CompressedSize);
                using var compressedStream = new MemoryStream(compressedData);
                using var zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress, false);
                zlibStream.CopyTo(outputStream);
            }
            else
            {
                outputStream.Write(bundleReader.ReadBytes((int) entry.Size));
            }

            Console.WriteLine($"Successfully extracted file {entry.RelativePath}.");
        }
    }
}