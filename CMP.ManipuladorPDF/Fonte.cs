using iText.Kernel.Font;
using iText.Kernel.Pdf;
using System.IO;
using System.Collections.Generic;

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

        private static List<string> ObterTodasAsFontes(
            DocumentoPDF documento
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            List<string> list = new List<string>();
            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
            {
                PdfPage page = pdfDocument.GetPage(pageNum);
                PdfResources resources = page.GetResources();
                foreach (PdfName fontName in resources.GetResourceNames(PdfName.Font))
                {
                    PdfDictionary dictionary = (PdfDictionary)resources.GetResourceObject(PdfName.Font, fontName);
                    PdfFont font = PdfFontFactory.CreateFont(dictionary);
                    list.Add(font.GetFontProgram().GetFontNames().GetFontName());
                }
            }

            return list;
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

        /// <summary>
        /// Obtém todas as fontes de um documento PDF
        /// </summary>
        /// <param name="documento">Documento cujas fontes serão obtidas</param>
        /// <returns>List</returns>
        public static List<string> ObterFontesIncorporadas(
           this DocumentoPDF documento
        )
        {
            return ObterTodasAsFontes(documento);
        }

    }

    public class FontePDF
    {

        public PdfFont Fonte { get; set; } = null;

        public FontePDF(string nome, bool embedded=false)
        {
            byte[] fontBytes;

            if (embedded)
            {
                fontBytes = EmbeddedResource.GetByteArray($"{nome}.ttf");
            }
            else
            {
                fontBytes = File.ReadAllBytes($"{DocumentoPDFConfig.FONT_PATH}/{nome}.ttf");
            }

            Fonte = PdfFontFactory.CreateFont(fontBytes,PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        }

    }




}