using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Font;
using iText.Pdfa;
using iText.StyledXmlParser.Resolver.Font;
using System;
using System.IO;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF Html2PDF(
            this string html,
            bool tagged = true,
            string title = "Documento PDF",
            string author = "Câmara Municipal de Piracicaba",
            string subject = "Documento criado pelo sistema de documentos digitais da Câmara Municipal de Piracicaba",
            string creator = "Câmara Municipal de Piracicaba",
            string keywords = "Documento PDF, Câmara Municipal de Piracicaba"
        )
        {

            if (html.Contains("<style scoped>"))
            {
                html = html.Replace("<style scoped>", "<style scoped>*{font-family:\"Aptos\"}");
                html = html.Replace(": %;", ": 0%;");
            }

            using MemoryStream outputStream = new MemoryStream();
            PdfWriter pdfWriter = new PdfWriter(outputStream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
            Stream sRGBColorStream = EmbeddedResource.GetStream("sRGB Color Space Profile.icm");
            PdfOutputIntent intent = new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", sRGBColorStream);
            PdfADocument pdfDocument = new PdfADocument(pdfWriter, PdfAConformance.PDF_A_4, intent);

            pdfDocument.SetDefaultPageSize(PageSize.A4);
            pdfDocument.GetCatalog().SetLang(new PdfString("pt-BR"));

            if (tagged)
                pdfDocument.SetTagged();

            PdfDocumentInfo info = pdfDocument.GetDocumentInfo();
            info
                .SetTitle(title)
                .SetAuthor(author)
                .SetSubject(subject)
                .SetCreator(creator)
                .SetKeywords(keywords)
                .AddCreationDate();

            ConverterProperties properties = CriarPropriedadesConversao();

            try
            {
                HtmlConverter.ConvertToPdf(html, pdfDocument, properties);
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("All the fonts must be embedded"))
                {
                    string message = exception.Message.Split(":")[1];
                    throw new FontNotExistException(message);
                }

                throw new HtmlConverterException(exception.Message);

            }

            return new DocumentoPDF(outputStream);
        }

        private static ConverterProperties CriarPropriedadesConversao()
        {
            ConverterProperties properties = new ConverterProperties();

            FontProvider fontProvider = new BasicFontProvider();

            int fontCount = fontProvider.AddDirectory(DocumentoPDFConfig.FONT_PATH);

            if (fontCount == 0)
            {
                throw new FontDirectoryEmptyException();
            }

            properties.SetFontProvider(fontProvider);

            return properties;
        }

        /// <summary>
        /// Converte HTML em PDF comum, sem PDF/A. Para páginas mescladas em documentos que já não
        /// são PDF/A: ali a conformidade se perde no merge, e a validação só serviria para derrubar
        /// a conversão por um caractere sem glifo na fonte (emoji, símbolo colado do Word etc.).
        /// </summary>
        internal static DocumentoPDF ConverterParaPdfSimples(this string html)
        {
            using MemoryStream outputStream = new MemoryStream();
            PdfDocument pdfDocument = new PdfDocument(new PdfWriter(outputStream));
            pdfDocument.SetDefaultPageSize(PageSize.A4);

            HtmlConverter.ConvertToPdf(html, pdfDocument, CriarPropriedadesConversao());

            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Converte uma string com html para um documento PDF.
        /// </summary>
        /// <param name="html">String com o html dentro.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF ConverterParaPdf(
            this string html,
            string titulo = "Documento PDF",
            string autor = "Câmara Municipal de Piracicaba",
            string assunto = "Documento criado pelo sistema de documentos digitais da Câmara Municipal de Piracicaba",
            string criador = "Câmara Municipal de Piracicaba",
            string palavrasChave = "Documento PDF, Câmara Municipal de Piracicaba"
        )
        {
            return Html2PDF(html, false, titulo, autor, assunto, criador, palavrasChave);
        }

        public static DocumentoPDF ConverterParaPdfTagged(
            this string html,
            string titulo = "Documento PDF",
            string autor = "Câmara Municipal de Piracicaba",
            string assunto = "Documento criado pelo sistema de documentos digitais da Câmara Municipal de Piracicaba",
            string criador = "Câmara Municipal de Piracicaba",
            string palavrasChave = "Documento PDF, Câmara Municipal de Piracicaba"
        )
        {
            return Html2PDF(html, true, titulo, autor, assunto, criador, palavrasChave);
        }

    }

}