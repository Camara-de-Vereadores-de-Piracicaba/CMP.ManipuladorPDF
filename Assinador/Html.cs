using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using System.IO;
using iText.Html2pdf;
using iText.Pdfa;
using iText.Html2pdf.Resolver.Font;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF CriarDeStringHtml(
            this string html,
            string[] fontList
        )
        {
            ConverterProperties converterProperties = new ConverterProperties();
            converterProperties.SetBaseUri("");
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using Stream sRGBColorStream = EmbeddedResource.GetStream("sRGB Color Space Profile.icm");
            PdfOutputIntent pdfOutputIntent = new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", sRGBColorStream);
            PdfADocument pdfDocument = new PdfADocument(pdfWriter,PdfAConformanceLevel.PDF_A_3A,pdfOutputIntent);
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            pdfDocument.SetTagged();
            fontList ??= new string[]
            {
                "calibri",
                "calibrib",
                "calibrii",
                "calibriz",
                "times",
                "timesbd",
                "timesbi",
                "timesi",
                "CourierPrime-Regular",
                "CourierPrime-Bold",
                "CourierPrime-BoldItalic",
                "CourierPrime-Italic",
                "Roboto-Regular",
                "Roboto-Bold",
                "Roboto-BoldItalic",
                "Roboto-Italic"
            };
            DefaultFontProvider fontProvider = new DefaultFontProvider(false, false, false);
            foreach (string font in fontList)
            {
                fontProvider.AddFont(EmbeddedResource.GetByteArray($"{font}.ttf"));
            }
            converterProperties.SetFontProvider(fontProvider);
            HtmlConverter.ConvertToPdf(html, pdfDocument, converterProperties);
            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Converte uma string com html dentro para um documento PDF.
        /// </summary>
        /// <param name="html">String com o html dentro.</param>
        /// <param name="fontes">Array opcional de fontes para usar. Se nulo, inclui todas as fontes.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF HtmlParaPdf(this string html, string[] fontes = null)
        {
            return CriarDeStringHtml(html,fontes);
        }

        /// <summary>
        /// Converte uma string com html dentro para um documento PDF, retirando o caracter "%" da string.
        /// </summary>
        /// <param name="html">String com o html dentro.</param>
        /// <param name="fontes">Array opcional de fontes para usar. Se nulo, inclui todas as fontes.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF HtmlParaPdfSemPorcentagem(this string html, string[] fontes = null)
        {
            html = html.Replace("%", "");
            return CriarDeStringHtml(html, fontes);
        }

    }
}