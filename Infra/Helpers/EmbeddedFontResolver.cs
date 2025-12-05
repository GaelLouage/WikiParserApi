using PdfSharp.Fonts;
using System.Reflection;

namespace Infra.Helpers
{
    public class EmbeddedFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            string resourceName = faceName switch
            {
                "Courier New" => "Infra.Fonts.CourierNew.ttf",
                "Arial" => "Infra.Fonts.Arial.ttf",
                _ => throw new InvalidOperationException($"Font '{faceName}' not found")
            };

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Font resource '{resourceName}' not found");

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Courier New", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("Courier New");
            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("Arial");
            return null;
        }
    }
}
