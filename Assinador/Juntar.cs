using iText.Kernel.Pdf;
using System.Collections.Generic;
using System.IO;

namespace CMP.ManipuladorPDF
{

    public static partial class ManipuladorPDF
    {

        private static MemoryStream JuntarArquivos(List<PdfReader> sourceFiles)
        {
            using var outputStream = new MemoryStream();
            using PdfWriter writer = new PdfWriter(outputStream);
            using PdfDocument newPdfDoc = new PdfDocument(writer);
            foreach (PdfReader sourceFile in sourceFiles)
            {
                PdfDocument pdfDoc = new PdfDocument(sourceFile);
                pdfDoc.CopyPagesTo(1, pdfDoc.GetNumberOfPages(), newPdfDoc);
                pdfDoc.Close();
            }

            newPdfDoc.Close();
            return outputStream;
        }

        public static MemoryStream Juntar(this List<string> sourceFiles) 
        {
            List<PdfReader> parameter = new List<PdfReader>();
            foreach (string sourceFile in sourceFiles)
            {
                parameter.Add(new PdfReader(sourceFile));
            }

            return JuntarArquivos(parameter);
        }

        public static MemoryStream Juntar(this List<MemoryStream> sourceFiles)
        {
            List<PdfReader> parameter = new List<PdfReader>();
            foreach (MemoryStream sourceFile in sourceFiles)
            {
                sourceFile.Seek(0, SeekOrigin.Begin);
                parameter.Add(new PdfReader(sourceFile));
            }

            return JuntarArquivos(parameter);
        }

    }
}