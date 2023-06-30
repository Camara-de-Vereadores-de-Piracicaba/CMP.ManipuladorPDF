using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using System.IO;

namespace ConversorHTML
{
    public static class HtmlToPDF
    {
        public static MemoryStream Convert(this string html)
        {
            ConverterProperties converterProperties = new ConverterProperties();
            MemoryStream stream = new MemoryStream();
            PdfDocument pdfDocument = new PdfDocument(new PdfWriter(stream));
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            HtmlConverter.ConvertToPdf(html.Replace("px", "").Replace("%", ""), pdfDocument, converterProperties);
            stream.Close();
            return stream;
        }
    }
}
