using iText.Kernel.Pdf;
using System;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF RecuperaDocumento(
            this DocumentoPDF documento
        )
        {
            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument brokenPdfDocument = new PdfDocument(pdfReader);
            PdfDocument recoveredPdfDocument = new PdfDocument(pdfWriter);
            int pages = brokenPdfDocument.GetNumberOfPages();
            for(int i = 1; i <= pages; i++)
            {
                try
                {
                    PdfPage page = brokenPdfDocument.GetPage(i);
                    PdfPage copiedPage = page.CopyTo(recoveredPdfDocument);
                    recoveredPdfDocument.AddPage(copiedPage);
                }catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    throw new IrrecuperableBrokenPDFDocumentException();
                }
            }

            brokenPdfDocument.Close();
            recoveredPdfDocument.Close();
            return new DocumentoPDF(outputStream.ToArray());
        }

        /// <summary>
        /// Repara um documento corrupto
        /// </summary>
        /// <param name="documento">Documento para ser reparado.</param>

        public static DocumentoPDF RepararDocumento(
            this DocumentoPDF documento
        )
        {
            return RecuperaDocumento(documento);
        }


    }
}