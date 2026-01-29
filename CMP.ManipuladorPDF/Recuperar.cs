using iText.Kernel.Pdf;
using System;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF ReparaDocumento(this DocumentoPDF documento)
        {
            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument brokenPdfDocument = new PdfDocument(pdfReader);
            PdfDocument recoveredPdfDocument = new PdfDocument(pdfWriter);
            int pages = brokenPdfDocument.GetNumberOfPages();
            for (int i = 1; i <= pages; i++)
            {
                try
                {
                    PdfPage page = brokenPdfDocument.GetPage(i);
                    PdfPage copiedPage = page.CopyTo(recoveredPdfDocument);
                    recoveredPdfDocument.AddPage(copiedPage);
                }
                catch (Exception)
                {
                    throw new IrrecuperableBrokenPDFDocumentException();
                }
            }

            brokenPdfDocument.Close();
            recoveredPdfDocument.Close();
            return new DocumentoPDF(outputStream.ToArray());
        }

        private static DocumentoPDF RecuperarDocumento(this DocumentoPDF documento)
        {
            documento = documento.DesencriptarCasoNecessario();

            using MemoryStream outputStream = new MemoryStream();
            using (MemoryStream inputStream = new MemoryStream(documento.ByteArray))
            {
                using PdfReader pdfReader = new PdfReader(inputStream);
                pdfReader.SetUnethicalReading(true);

                using PdfWriter pdfWriter = new PdfWriter(outputStream);
                pdfWriter.SetSmartMode(true);

                using PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);

                PdfObject catalogObject = pdfDocument.GetCatalog().GetPdfObject();

                if (catalogObject is PdfDictionary catalogDict)
                {
                    if (catalogDict.ContainsKey(PdfName.EmbeddedFiles))
                        catalogDict.Remove(PdfName.EmbeddedFiles);

                    if (catalogDict.ContainsKey(PdfName.JavaScript))
                        catalogDict.Remove(PdfName.JavaScript);

                    if (catalogDict.ContainsKey(PdfName.OpenAction))
                        catalogDict.Remove(PdfName.OpenAction);

                    if (catalogDict.ContainsKey(PdfName.AA))
                        catalogDict.Remove(PdfName.AA);

                    if (catalogDict.ContainsKey(PdfName.AcroForm))
                    {
                        PdfObject acroFormObject = catalogDict.Get(PdfName.AcroForm);
                        if (acroFormObject is PdfDictionary acroFormDict)
                        {
                            if (acroFormDict.ContainsKey(PdfName.XFA))
                                acroFormDict.Remove(PdfName.XFA);
                        }
                    }
                }

                int numberOfPages = pdfDocument.GetNumberOfPages();
                for (int i = 1; i <= numberOfPages; i++)
                {
                    PdfPage page = pdfDocument.GetPage(i);
                    PdfDictionary pageDict = page.GetPdfObject();

                    if (pageDict.ContainsKey(PdfName.AA))
                        pageDict.Remove(PdfName.AA);

                    if (pageDict.ContainsKey(PdfName.Trans))
                        pageDict.Remove(PdfName.Trans);
                }

                pdfDocument.Close();
            }

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
            return RecuperarDocumento(documento);
        }


    }
}