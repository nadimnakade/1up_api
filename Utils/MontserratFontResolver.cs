using System;
using System.IO;
using System.Web;
using PdfSharp.Fonts;
using System.Runtime.InteropServices;

namespace PickupAPi.Utils
{
    // Maps the logical family name "Montserrat" to TTF files in ~/fonts
    public class MontserratFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (string.Equals(familyName, "Montserrat", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic)
                    return new FontResolverInfo("Montserrat#BoldItalic");
                if (isBold)
                    return new FontResolverInfo("Montserrat#Bold");
                if (isItalic)
                    return new FontResolverInfo("Montserrat#Italic");
                return new FontResolverInfo("Montserrat#Regular");
            }

            // Support Arial explicitly so existing styles continue to work
            if (string.Equals(familyName, "Arial", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic)
                    return new FontResolverInfo("Arial#BoldItalic");
                if (isBold)
                    return new FontResolverInfo("Arial#Bold");
                if (isItalic)
                    return new FontResolverInfo("Arial#Italic");
                return new FontResolverInfo("Arial#Regular");
            }

            // Fallback to Montserrat regular for any other family
            return new FontResolverInfo("Montserrat#Regular");
        }

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "Montserrat#Regular":
                    return LoadFontBytes("~/fonts/Montserrat-Regular.ttf");
                case "Montserrat#Bold":
                    return LoadFontBytes("~/fonts/Montserrat-Bold.ttf");
                case "Montserrat#Italic":
                    return LoadFontBytes("~/fonts/Montserrat-Italic.ttf");
                case "Montserrat#BoldItalic":
                    return LoadFontBytes("~/fonts/Montserrat-BoldItalic.ttf");
                case "Arial#Regular":
                    return LoadSystemFontBytes("arial.ttf");
                case "Arial#Bold":
                    return LoadSystemFontBytes("arialbd.ttf");
                case "Arial#Italic":
                    return LoadSystemFontBytes("ariali.ttf");
                case "Arial#BoldItalic":
                    return LoadSystemFontBytes("arialbi.ttf");
            }
            return null;
        }

        private static byte[] LoadFontBytes(string virtualPath)
        {
            var path = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath(virtualPath)
                : System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new FileNotFoundException($"Font file not found: {virtualPath}", path);

            return File.ReadAllBytes(path);
        }

        private static byte[] LoadSystemFontBytes(string fileName)
        {
            // Windows Fonts directory
            var fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            var path = Path.Combine(fontsDir, fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"System font file not found: {fileName}", path);

            return File.ReadAllBytes(path);
        }
    }
}