using System;
using System.IO;
using System.Web;
using System.Diagnostics;
using PdfSharp.Fonts;

namespace PickupAPi.Utils
{
    public class ArialFontResolver : IFontResolver, IFontResolverMarker
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            Debug.WriteLine($"[FontResolver] ResolveTypeface: '{familyName}' bold={isBold} italic={isItalic}");
            Console.WriteLine($"[FontResolver] ResolveTypeface: '{familyName}' bold={isBold} italic={isItalic}");

            if (string.Equals(familyName, "Arial", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic) return new FontResolverInfo("Arial#BoldItalic");
                if (isBold) return new FontResolverInfo("Arial#Bold");
                if (isItalic) return new FontResolverInfo("Arial#Italic");
                return new FontResolverInfo("Arial#Regular");
            }

            // Fallback to Arial for any other family
            return new FontResolverInfo("Arial#Regular");
        }

        public byte[] GetFont(string faceName)
        {
            Debug.WriteLine($"[FontResolver] GetFont: '{faceName}'");
            Console.WriteLine($"[FontResolver] GetFont: '{faceName}'");

            switch (faceName)
            {
                case "Arial#Regular":   return LoadSystemFont("arial.ttf");
                case "Arial#Bold":      return LoadSystemFont("arialbd.ttf");
                case "Arial#Italic":    return LoadSystemFont("ariali.ttf");
                case "Arial#BoldItalic": return LoadSystemFont("arialbi.ttf");
            }
            return LoadSystemFont("arial.ttf");
        }

        private static byte[] LoadSystemFont(string fileName)
        {
            var fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            var path = Path.Combine(fontsDir, fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"System font not found: {fileName}", path);

            byte[] bytes = File.ReadAllBytes(path);
            Debug.WriteLine($"[FontResolver] Loaded {fileName}: {bytes.Length} bytes");
            Console.WriteLine($"[FontResolver] Loaded {fileName}: {bytes.Length} bytes");
            return bytes;
        }
    }
}
