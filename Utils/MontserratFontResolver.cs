using System;
using System.IO;
using PdfSharp.Fonts;

namespace PickupAPi.Utils
{
    public class MontserratFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (isBold && isItalic)
                return new FontResolverInfo("Arial#BoldItalic");
            if (isBold)
                return new FontResolverInfo("Arial#Bold");
            if (isItalic)
                return new FontResolverInfo("Arial#Italic");
            return new FontResolverInfo("Arial#Regular");
        }

        public byte[] GetFont(string faceName)
        {
            try
            {
                string fileName;
                switch (faceName)
                {
                    case "Arial#Regular":    fileName = "arial.ttf"; break;
                    case "Arial#Bold":       fileName = "arialbd.ttf"; break;
                    case "Arial#Italic":     fileName = "ariali.ttf"; break;
                    case "Arial#BoldItalic": fileName = "arialbi.ttf"; break;
                    default:                 fileName = "arial.ttf"; break;
                }

                var fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                var path = Path.Combine(fontsDir, fileName);

                Console.WriteLine($"[FontResolver] Loading {faceName} from {path} (exists={File.Exists(path)})");

                if (!File.Exists(path))
                {
                    Console.WriteLine($"[FontResolver] Font file not found: {path}");
                    return null;
                }

                var bytes = File.ReadAllBytes(path);
                Console.WriteLine($"[FontResolver] Loaded {bytes.Length} bytes for {faceName}");
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FontResolver] ERROR: {ex.Message}");
                return null;
            }
        }
    }
}
