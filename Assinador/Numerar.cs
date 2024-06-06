using iText.Kernel.Pdf;
using System.IO;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Properties;
using Rectangle = iText.Kernel.Geom.Rectangle;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        public enum PosicaoNumero
        {
            TOPO_ESQUERDA,
            TOPO_DIREITA,
            RODAPE_ESQUERDA,
            RODAPE_DIREITA
        }

        private static DocumentoPDF NumerarDocumento(
            this DocumentoPDF documento,
            PosicaoNumero posicao,
            int tamanhoFonte,
            int paginaInicial,
            int primeiroNumero,
            int margem,
            string prefixo
        )
        {

            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
            int totalPaginas = pdfDocument.GetNumberOfPages();

            int numero = primeiroNumero;

            if(paginaInicial > totalPaginas)
            {
                paginaInicial = totalPaginas;
            }

            if(paginaInicial < 1) 
            {
                paginaInicial = 1;
            }

            for (int pagina = paginaInicial; pagina <= totalPaginas; pagina++)
            {

                PdfPage page = pdfDocument.GetPage(pagina);
                var mediaBox = page.GetCropBox();
                float pageWidth = mediaBox.GetWidth();
                float pageHeight = mediaBox.GetHeight();
                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, new Rectangle(0, 0, pageWidth, pageHeight));

                Paragraph text = new Paragraph();
                text.SetFontSize(tamanhoFonte).Add($"{prefixo}{numero}");
                //text.SetFont(PDFTrueTypeFont.GetFont("calibri"));

                float x = margem;
                float y = margem;
                TextAlignment alignment = TextAlignment.LEFT;

                if (posicao == PosicaoNumero.RODAPE_DIREITA)
                {
                    x = pageWidth - margem;
                    alignment = TextAlignment.RIGHT;
                }
                else if (posicao == PosicaoNumero.TOPO_ESQUERDA)
                {
                    y = pageHeight - margem;
                }
                else if (posicao == PosicaoNumero.TOPO_DIREITA)
                {
                    x = pageWidth - margem;
                    y = pageHeight - margem;
                    alignment = TextAlignment.RIGHT;
                }

                canvas.ShowTextAligned(text, x, y, alignment, VerticalAlignment.MIDDLE);

                numero++;

            }

            pdfDocument.Close();

            return new DocumentoPDF(outputStream);

        }

        /// <summary>
        /// Numera as páginas de um documento PDF.
        /// </summary>
        /// <param name="documento">O documento PDF que será numerado.</param>
        /// <param name="posicao">A posição na página.</param>
        /// <param name="tamanhoFonte">Tamanho da fonte</param>
        /// <param name="paginaInicial">Página cuja numeração se iniciará.</param>
        /// <param name="primeiroNumero">Primeiro número cuja numeração se iniciará.</param>
        /// <param name="margem">A distância de margem dos números.</param>
        /// <param name="prefixo">Prefixo a ser adicionado a todo número. Por exemplo, "Página ".</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Numerar(
            this DocumentoPDF documento,
            PosicaoNumero posicao = PosicaoNumero.TOPO_DIREITA,
            int tamanhoFonte = 9,
            int paginaInicial = 1,
            int primeiroNumero = 1,
            int margem = 20,
            string prefixo = ""
        )
        {
            return NumerarDocumento(documento,posicao,tamanhoFonte,paginaInicial,primeiroNumero,margem,prefixo);
        }

    }

}
