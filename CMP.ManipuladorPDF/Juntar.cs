using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Pdfa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF JuntarArquivos(
            DocumentoPDF documento1,
            DocumentoPDF documento2
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument mergedPdfDocument = new PdfDocument(pdfWriter);
            using PdfDocument pdfDocument1 = new PdfDocument(new PdfReader(new MemoryStream(documento1.ByteArray)));
            using PdfDocument pdfDocument2 = new PdfDocument(new PdfReader(new MemoryStream(documento2.ByteArray)));
            PdfMerger pdfMerger = new PdfMerger(mergedPdfDocument);
            pdfMerger.Merge(pdfDocument1, 1, pdfDocument1.GetNumberOfPages());
            pdfMerger.Merge(pdfDocument2, 1, pdfDocument2.GetNumberOfPages());
            pdfDocument1.Close();
            pdfDocument2.Close();
            pdfMerger.Close();
            mergedPdfDocument.Close();
            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Junta múltiplos documentos PDF.
        /// </summary>
        /// <param name="documentos">Lista de documentos para juntar.</param>
        /// <returns>DocumentoPDF</returns>
        public static DocumentoPDF Juntar(
           this List<DocumentoPDF> documentos
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument mergedPdfDocument = new PdfDocument(pdfWriter);
            PdfMerger pdfMerger = new PdfMerger(mergedPdfDocument);
            foreach (DocumentoPDF documento in documentos)
            {
                PdfDocument pdfDocument = new PdfDocument(new PdfReader(documento.ConverterParaMemoryStream()));
                pdfMerger.Merge(pdfDocument, 1, pdfDocument.GetNumberOfPages());
                pdfDocument.Close();
            }

            pdfMerger.Close();
            mergedPdfDocument.Close();
            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Junta dois documentos PDF.
        /// </summary>
        /// <param name="documento1">Primeiro documento.</param>
        /// <param name="documento2">Segundo documento.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Juntar(
            this DocumentoPDF documento1, 
            DocumentoPDF documento2
        )
        {
            return JuntarArquivos(documento1, documento2);
        }

        /// <summary>
        /// Junta dois documentos PDF.
        /// </summary>
        /// <param name="documento1">Primeiro documento.</param>
        /// <param name="documento2">Caminho absoluto do segundo documento.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Juntar(
            this DocumentoPDF documento1,
            string documento2
        )
        {
            return JuntarArquivos(documento1, new DocumentoPDF(documento2));
        }

        /// <summary>
        /// Junta dois documentos PDF.
        /// </summary>
        /// <param name="documento1">Primeiro documento.</param>
        /// <param name="documento2">ByteArray do segundo documento.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Juntar(
            this DocumentoPDF documento1,
            byte[] documento2
        )
        {
            return JuntarArquivos(documento1, new DocumentoPDF(documento2));
        }

        /// <summary>
        /// Junta dois documentos PDF.
        /// </summary>
        /// <param name="documento1">Primeiro documento.</param>
        /// <param name="documento2">MemoryStream do segundo documento.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Juntar(
            this DocumentoPDF documento1,
            MemoryStream documento2
        )
        {
            return JuntarArquivos(documento1, new DocumentoPDF(documento2));
        }

    }

}