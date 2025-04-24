using iText.Kernel.Pdf;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF AdicionarSenha(
            this DocumentoPDF documento,
            string senha
        )
        {

            WriterProperties writerProperties = new WriterProperties()
                   .SetStandardEncryption(
                       System.Text.Encoding.ASCII.GetBytes(senha),
                       System.Text.Encoding.ASCII.GetBytes(senha),
                       EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_COPY,
                       EncryptionConstants.ENCRYPTION_AES_128
                   );

            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream, writerProperties);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
            pdfDocument.Close();

            return new DocumentoPDF(outputStream);

        }

        /// <summary>
        /// Adiciona proteção por senha em um documento PDF.
        /// </summary>
        /// <param name="documento">O documento PDF que será numerado.</param>
        /// <param name="senha">A senha do documento.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF ProtegerComSenha(
            this DocumentoPDF documento,
            string senha
        )
        {
            return AdicionarSenha(documento, senha);
        }

    }

}
