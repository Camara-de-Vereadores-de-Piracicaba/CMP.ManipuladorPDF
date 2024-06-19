using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Pdfa;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF ConverterDocumentoParaPDFA(
            this DocumentoPDF documento
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            Stream sRGBColorStream = EmbeddedResource.GetStream("sRGB Color Space Profile.icm");
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            PdfADocument pdfADocument = new PdfADocument(
                pdfWriter,
                PdfAConformanceLevel.PDF_A_4,
                new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", sRGBColorStream)
            );
            PdfMerger pdfMerger = new PdfMerger(pdfADocument);
            pdfMerger.Merge(pdfDocument, 1, pdfDocument.GetNumberOfPages());
            pdfDocument.Close();
            pdfADocument.Close();
            return new DocumentoPDF(outputStream.ToArray());
        }

        /// <summary>
        /// Converte um documento para PDF/A
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>

        public static DocumentoPDF ConverterParaPDFA(
            this DocumentoPDF documento
        )
        {
            return ConverterDocumentoParaPDFA(documento);
        }


    }
}