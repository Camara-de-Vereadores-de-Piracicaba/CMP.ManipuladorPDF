using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static string ReconhecerOCR(
            this DocumentoPDF documento
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
            int totalPaginas = pdfDocument.GetNumberOfPages();
            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            string text = "";
            for (int pagina = 1; pagina <= totalPaginas; pagina++)
            {
                PdfPage page = pdfDocument.GetPage(pagina);
                text += PdfTextExtractor.GetTextFromPage(page, strategy);
            }
            return text;
        }

        /// <summary>
        /// Realiza o reconhecimento OCR de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento para ter os caracteres reconhecidos.</param>
        /// <returns>DocumentoPDF</returns>
        public static string OCR(
           this DocumentoPDF documento
        )
        {
            return documento.ReconhecerOCR();
        }

        

    }

}