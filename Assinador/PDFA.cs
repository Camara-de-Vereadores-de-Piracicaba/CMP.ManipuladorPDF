using iText.Kernel.Pdf;
using iText.Pdfa;
using iText.Pdfa.Checker;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF ConverterDocumentoParaPDFA(
            this DocumentoPDF documento
        )
        {




            /*
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            Stream sRGBColorStream = EmbeddedResource.GetStream("sRGB Color Space Profile.icm");
            StampingProperties stamp = new StampingProperties();
            PdfADocument pdfDocument = new PdfADocument(pdfReader, pdfWriter);
            pdfDocument.Close();
            return new DocumentoPDF(outputStream);
            */

            return new DocumentoPDF();

        }

        private static bool ChecarDocumentoPDFA(
            this DocumentoPDF documento
        )
        {

            MemoryStream inputStream = new MemoryStream(documento.ByteArray);
            inputStream.Position = 0;
            PdfWriter pdfWriter = new PdfWriter(inputStream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
            Stream sRGBColorStream = EmbeddedResource.GetStream("sRGB Color Space Profile.icm");
            PdfADocument pdfDocument = new PdfADocument(pdfWriter,PdfAConformanceLevel.PDF_A_4,new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", sRGBColorStream));

            return false;
        }

        /// <summary>
        /// Checa se um documento é um PDF-A
        /// </summary>
        /// <param name="documento">Documento para checar</param>

        public static bool ChecarPDFA(
            this DocumentoPDF documento
        )
        {
            return documento.ChecarDocumentoPDFA();
        }

        /// <summary>
        /// Checa se um documento é um PDF-A
        /// </summary>
        /// <param name="documento">Documento para checar</param>

        public static DocumentoPDF ConverterParaPDFA(
            this DocumentoPDF documento
        )
        {
            return documento.ConverterDocumentoParaPDFA();
        }

    }
}