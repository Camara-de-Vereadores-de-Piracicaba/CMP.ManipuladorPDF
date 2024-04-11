using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using System.IO;
using iText.Layout;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using Org.BouncyCastle.Asn1.X509;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {

        public enum PosicionamentoTexto
        {
            RODAPE,
            CABECALHO
        }

        public static MemoryStream AdicionarTexto(
            PdfReader pdfReader,
            string texto,
            PosicionamentoTexto posicao,
            int pagina = 0,
            string fonte = "Roboto-Regular",
            TextAlignment alinhamento = TextAlignment.LEFT,
            int tamanho = 12,
            int margem = 20
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter);
            Document document = new Document(pdfDoc);
            Rectangle pageSize = pdfDoc.GetDefaultPageSize();
            PdfFont font = PDFTrueTypeFont.GetFont(fonte);
            float fontSize = (float)tamanho;

            void Escrever(int pagina)
            {
                PdfPage page = pdfDoc.GetPage(pagina);
                float pageWidth = page.GetMediaBox().GetWidth();
                float pageHeight = page.GetMediaBox().GetHeight();

                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, new Rectangle(0, 0, pageWidth, pageHeight));

                int bottom = margem;

                if (posicao == PosicionamentoTexto.CABECALHO)
                {
                    bottom = (int)pageHeight - (margem * 2);
                }

                Paragraph paragraph = new Paragraph(texto)
                        .SetTextAlignment(alinhamento)
                        .SetFontSize(tamanho)
                        .SetFixedPosition(margem, bottom, pageWidth);

                document.Add(paragraph);
            }

            if (pagina != 0)
            {
                Escrever(pagina);
                document.Close();
                return outputStream;
            }

            for (int p = 1; p <= pdfDoc.GetNumberOfPages(); p++)
            {
                Escrever(p);
            }

            document.Close();
            return outputStream;
        }

        public static MemoryStream AdicionarRodape(
            this MemoryStream sourceFile,
            string texto,
            int pagina = 0,
            string fonte = "Roboto-Regular",
            TextAlignment alinhamento = TextAlignment.LEFT,
            int tamanho = 12,
            int margem = 20
        )
        {
            return AdicionarTexto(new PdfReader(sourceFile), texto, PosicionamentoTexto.RODAPE, pagina, fonte, alinhamento, tamanho, margem);
        }

        public static MemoryStream AdicionarRodape(
            this string sourceFile, 
            string texto,
            int pagina = 0, 
            string fonte = "Roboto-Regular",
            TextAlignment alinhamento = TextAlignment.LEFT,
            int tamanho = 12,
            int margem = 20
        )
        {
            return AdicionarTexto(new PdfReader(sourceFile), texto, PosicionamentoTexto.RODAPE, pagina, fonte, alinhamento, tamanho, margem);
        }

    }
}
