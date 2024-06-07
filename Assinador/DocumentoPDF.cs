using System.Collections.Generic;
using System;
using System.IO;
using iText.Kernel.Pdf;

namespace CMP.ManipuladorPDF
{

    public partial class DocumentoPDF
    {
        public byte[] ByteArray { get; set; }
        public string ValidadorURL { get; set; } = "https://validar.camarapiracicaba.sp.gov.br";
        public string Titulo { get; set; } = null;

        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="sourceStream">MemoryStream do arquivo PDF.</param>

        public DocumentoPDF()
        {
            
        }
        public DocumentoPDF(MemoryStream sourceStream)
        {
            ByteArray = sourceStream.ToArray();
        }

        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="filePath">Caminho absoluto do arquivo PDF.</param>

        public DocumentoPDF(string filePath)
        {
            ByteArray = File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="documentByteArray">ByteArray do arquivo PDF.</param>

        public DocumentoPDF(byte[] documentByteArray)
        {
            ByteArray = documentByteArray;
        }

    }

    public static partial class ExtensionMethods
    {

        /// <summary>
        /// Obtém a quantidade de páginas de um documento PDF.
        /// </summary>
        /// <param name="documento">Documento PDF cujas páginas se quer obter.</param>
        /// <returns>int</returns>

        public static int QuantidadeDePaginas(this DocumentoPDF documento)
        {
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            using PdfDocument pdfDocument = new PdfDocument(pdfReader);
            return pdfDocument.GetNumberOfPages();
        }

        public static DocumentoPDF ExtrairPagina(this DocumentoPDF documento, int pagina=1)
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            PdfDocument newPdfDocument = new PdfDocument(pdfWriter);
            pdfDocument.CopyPagesTo(pagina, pagina, newPdfDocument);
            pdfDocument.Close();
            newPdfDocument.Close();
            return new DocumentoPDF(outputStream.ToArray());
        }

        public static DocumentoPDF ExtrairPaginas(this DocumentoPDF documento, int inicio = 1, int fim = 1)
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            PdfDocument newPdfDocument = new PdfDocument(pdfWriter);
            pdfDocument.CopyPagesTo(inicio, fim, newPdfDocument);
            pdfDocument.Close();
            newPdfDocument.Close();
            return new DocumentoPDF(outputStream.ToArray());
        }

    }

}
