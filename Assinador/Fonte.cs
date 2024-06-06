using iText.Kernel.Font;
using iText.Kernel.Pdf;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF IncorporarFonteNoPDF(
            DocumentoPDF documento,
            string fonte
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
            PdfFont font = new FontePDF(fonte).Fonte;

            pdfDocument.GetFirstPage().GetResources().AddFont(pdfDocument, font);

            pdfDocument.Close();

            return new DocumentoPDF(outputStream.ToArray());
        }

        /// <summary>
        /// Incorpora uma fonte a um documento PDF.
        /// </summary>
        /// <param name="documento">Documento cuja fonte será incorporada</param>
        /// <param name="fonte">Nome da fonte</param>
        /// <returns>DocumentoPDF</returns>
        public static DocumentoPDF IncorporarFonte(
           this DocumentoPDF documento,
           string fonte
        )
        {
            return IncorporarFonteNoPDF(documento, fonte);
        }

    }

    public class FontePDF
    {

        public PdfFont Fonte { get; set; } = null;

        public FontePDF(string nome)
        {
            Fonte = PdfFontFactory.CreateFont(
                File.ReadAllBytes($"{DocumentoPDFConfig.FONT_PATH}/{nome}.ttf"),
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED
            );
        }

    }

}