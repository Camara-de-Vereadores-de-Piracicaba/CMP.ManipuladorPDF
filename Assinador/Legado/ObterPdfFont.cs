using CMP.ManipuladorPDF;
using iText.Kernel.Font;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CMP.ManipuladorPDFLegado
{
    internal static class ObterPdfFont
    {
        public static PdfFont Obter()
        {
            return new FontePDF("Roboto-Regular").Fonte;
        }
    }
}
