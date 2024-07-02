using iText.Kernel.Pdf;
using iText.Pdfa;
using System;
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

            int pages = pdfDocument.GetNumberOfPages();
            for (int i = 1; i <= pages; i++)
            {
                try
                {
                    PdfPage page = pdfDocument.GetPage(i);
                    PdfPage copiedPage = page.CopyTo(pdfADocument);
                    pdfADocument.AddPage(copiedPage);
                }
                catch (Exception exception)
                {
                    throw new PdfAConversionErrorException(exception.Message);
                }
            }

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
            try
            {
                return ConverterDocumentoParaPDFA(documento);
            }
            catch(Exception exception)
            {
                throw new PdfAConversionErrorException(exception.Message);
            }
        }


    }
}