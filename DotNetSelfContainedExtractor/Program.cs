using System.Globalization;

namespace DotNetSelfContainedExtractor
{
    internal class Program
    {
        private const string Usage = "\nUsage: DotNetSelfContainedExtractor.exe <self-contained app> <output directory path> [bundle file offset (0x for hex)]";

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(Usage);
            }

            var bundlePath = args[0];
            var outputPath = args[1];

            var offset = -1;

            if (args.Length == 3)
            {
                var offsetStr = args[2];

                var parseResult = offsetStr.StartsWith("0x")
                    ? int.TryParse(offsetStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset)
                    : int.TryParse(offsetStr, CultureInfo.InvariantCulture, out offset);

                if (!parseResult)
                {
                    Console.WriteLine("Failed to parse specified file offset.");
                    Console.WriteLine(Usage);
                    return;
                }
            }

            if (offset == -1 && !Bundle.FindBundleOffset(bundlePath, out offset))
            {
                Console.WriteLine("Failed to automatically locate bundle offset.");
                return;
            }

            var bundle = new Bundle(bundlePath, offset);
            if (!bundle.ReadBundle()) return;

            bundle.ExtractFiles(outputPath);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}