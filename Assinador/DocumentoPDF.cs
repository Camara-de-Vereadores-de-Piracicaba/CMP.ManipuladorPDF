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

        /// <summary>
        /// Cria uma referência a um documento PDF.
        /// </summary>
        /// <param name="sourceStream">MemoryStream do arquivo PDF.</param>

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

    }

}
