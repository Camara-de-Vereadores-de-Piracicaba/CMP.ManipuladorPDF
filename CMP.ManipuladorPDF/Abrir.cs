using iText.Kernel.Pdf;
using System.IO;
using System.Text;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF DesencriptarPDF(
            this DocumentoPDF documento,
            string senha = null
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            ReaderProperties properties = new ReaderProperties();
            if (senha != null)
                properties.SetPassword(Encoding.ASCII.GetBytes(senha));

            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray), properties);

            if (DocumentoPDFConfig.UNETHICAL_READING)
                pdfReader.SetUnethicalReading(true);

            PdfDocument decryptedDocument = new PdfDocument(pdfReader, pdfWriter);
            decryptedDocument.GetCatalog().GetPdfObject().Remove(PdfName.Encrypt);
            decryptedDocument.Close();

            return new DocumentoPDF(outputStream);
        }

        internal static DocumentoPDF DesencriptarCasoNecessario(this DocumentoPDF documento)
        {
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            if (DocumentoPDFConfig.UNETHICAL_READING)
                pdfReader.SetUnethicalReading(true);
            PdfDocument pdfDocument = new PdfDocument(pdfReader);

            PdfReader checkReader = pdfDocument.GetReader();

            if (checkReader.IsEncrypted() || !checkReader.IsOpenedWithFullPermission())
                documento = documento.Desencriptar();

            pdfDocument.Close();
            return documento;
        }

        internal static DocumentoPDF RemoverTagsParaAssinatura(this DocumentoPDF documento)
        {
            using MemoryStream input = new MemoryStream(documento.ByteArray);
            using PdfReader reader = new PdfReader(input);

            if (DocumentoPDFConfig.UNETHICAL_READING)
                reader.SetUnethicalReading(true);

            using MemoryStream output = new MemoryStream();
            using PdfWriter writer = new PdfWriter(output);

            using PdfDocument pdfDocument = new PdfDocument(reader, writer);

            PdfDictionary catalog = pdfDocument.GetCatalog().GetPdfObject();
            PdfName structTreeRootKey = PdfName.StructTreeRoot;

            PdfObject structTreeRoot = catalog.Get(structTreeRootKey);
            if (structTreeRoot != null)
            {
                catalog.Remove(structTreeRootKey);

                PdfName markInfoKey = PdfName.MarkInfo;
                PdfDictionary markInfo = catalog.GetAsDictionary(markInfoKey);
                markInfo?.Put(PdfName.Marked, PdfBoolean.FALSE);
            }

            pdfDocument.Close();
            return new DocumentoPDF(output);
        }

        /// <summary>
        /// Desencripta um documento que só está fechado para edição.
        /// </summary>
        /// <param name="documento">Documento PDF para desencriptar.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Desencriptar(this DocumentoPDF documento)
        {
            return DesencriptarPDF(documento);
        }

        /// <summary>
        /// Desencripta um documento que possui uma senha
        /// </summary>
        /// <param name="documento">Documento PDF para desencriptar.</param>
        /// <param name="senha">Senha do documento para desencriptar.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Desencriptar(this DocumentoPDF documento, string senha)
        {
            return DesencriptarPDF(documento, senha);
        }



    }

}
