using iText.Kernel.Font;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CMP.ManipuladorPDF
{
    internal static class ObterPdfFont
    {
        private const string NameFontFamily = "Roboto-Regular.ttf";

        public static PdfFont Obter()
        {
            var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith(NameFontFamily));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (MemoryStream msTeste = new MemoryStream())
                {
                    stream.CopyTo(msTeste);
                    return PdfFontFactory.CreateFont(msTeste.ToArray(), PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                }
            }
        }
    }
}
