using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {
        private static MemoryStream Pagina(PdfReader pdfReader, int pagina = 1)
        {
            using var outputStream = new MemoryStream();
            using PdfWriter writer = new PdfWriter(outputStream);
            using PdfDocument pdfDoc = new PdfDocument(pdfReader);
            PdfDocument newPdfDoc = new PdfDocument(writer);
            pdfDoc.CopyPagesTo(pagina, pagina, newPdfDoc);
            pdfDoc.Close();
            newPdfDoc.Close();
            return outputStream;
        }

        private static int QuantidadeDePaginas(PdfReader pdfReader)
        {
            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            return pdfDoc.GetNumberOfPages();
        }

        public static MemoryStream ObterPagina(this MemoryStream sourceFile, int pagina = 1)
        {
            sourceFile.Seek(0, SeekOrigin.Begin);
            return Pagina(new PdfReader(sourceFile), pagina);
        }

        public static MemoryStream ObterPagina(this string sourceFile, int pagina = 1)
        {
            return Pagina(new PdfReader(sourceFile), pagina);
        }

        public static int ObterQuantidadeDePaginas(this MemoryStream sourceFile)
        {
            sourceFile.Seek(0, SeekOrigin.Begin);
            return QuantidadeDePaginas(new PdfReader(sourceFile));
        }

        public static int ObterQuantidadeDePaginas(this string sourceFile)
        {
            return QuantidadeDePaginas(new PdfReader(sourceFile));
        }

    }
}