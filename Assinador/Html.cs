using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Pdfa;
using System.IO;

namespace CMP.ManipuladorPDF
{
    internal static class HtmlToPDFFonts 
    {

        private static readonly string[] FontList = 
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
            "CourierPrime-Italic"
        };

        public static DefaultFontProvider GetAllFonts() 
        {
            DefaultFontProvider fontProvider = new DefaultFontProvider(false, false, false);
            foreach(string font in FontList) 
            {
                fontProvider.AddFont(EmbeddedResource.GetByteArray($"{font}.ttf"));
            }

            return fontProvider;
        }
    }

    public static partial class ManipuladorPDF
    {

        public static MemoryStream ConverterParaPDF(this string html, bool removerPorcentagem=true)
        {
            if (removerPorcentagem) 
            {
                html = html.Replace("%", "");
            }
            
            ConverterProperties converterProperties = new ConverterProperties();
            converterProperties.SetBaseUri("");
            using MemoryStream stream = new MemoryStream();
            using Stream sRGBColorStream = EmbeddedResource.GetStream("sRGB Color Space Profile.icm");
            using MemoryStream msTeste = new MemoryStream();
            PdfOutputIntent pdfOutputIntent = new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", sRGBColorStream);
            PdfADocument pdfDocument = new PdfADocument(
                new PdfWriter(stream), 
                PdfAConformanceLevel.PDF_A_3A,
                pdfOutputIntent
            );
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            pdfDocument.SetTagged();
            converterProperties.SetFontProvider(HtmlToPDFFonts.GetAllFonts());
            HtmlConverter.ConvertToPdf(html, pdfDocument, converterProperties);
            return stream;
        }

    }
}
