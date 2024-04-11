using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using System.IO;
using iText.Layout;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {
        private static MemoryStream SemEfeito(
            PdfReader pdfReader,
            string texto = "SEM EFEITO",
            int tamanho = 120
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter);
            Document doc = new Document(pdfDoc);
            Rectangle pageSize = pdfDoc.GetDefaultPageSize();
            PdfFont font = PDFTrueTypeFont.GetFont("Roboto-Bold");
            float fontSize = (float)tamanho;

            for (int pagina = 1; pagina <= pdfDoc.GetNumberOfPages(); pagina++)
            {
                PdfPage page = pdfDoc.GetPage(pagina);
                float pageWidth = page.GetMediaBox().GetWidth();
                float pageHeight = page.GetMediaBox().GetHeight();

                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, new Rectangle(0,0,pageWidth,pageHeight));

                Paragraph paragraph = new Paragraph(texto)
                    .SetMargin(0)
                    .SetFont(font)
                    .SetFontColor(new DeviceRgb(128, 128, 128))
                    .SetFontSize(tamanho);

                canvas.ShowTextAligned(paragraph, pageWidth/2, pageHeight/2, pagina, TextAlignment.CENTER, VerticalAlignment.MIDDLE, 45f);

            }

            doc.Close();
            return outputStream;
        }

        public static MemoryStream TornarSemEfeito(this string sourceFile, string texto = "SEM EFEITO", int tamanho = 120)
        {
            return SemEfeito(new PdfReader(sourceFile), texto, tamanho);
        }

        public static MemoryStream TornarSemEfeito(this MemoryStream sourceFile, string texto = "SEM EFEITO", int tamanho = 120)
        {
            return SemEfeito(new PdfReader(sourceFile), texto, tamanho);
        }
    }
}
