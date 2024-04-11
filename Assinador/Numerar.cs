using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;

namespace CMP.ManipuladorPDF {

    public enum PosicionamentoNumeracao
    {
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT,
        TOP_LEFT
    }

    public static partial class ManipuladorPDF 
    {

        private static MemoryStream IncluirNumeracao(
            PdfReader pdfReader, 
            PosicionamentoNumeracao posicao = PosicionamentoNumeracao.TOP_RIGHT,
            int margem = 20, 
            int numeracaoInicial = 1,
            int tamanho= 9,
            string prefixo = "Página "
        )
            {
            using var outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter);
            Document document = new Document(pdfDoc);

            float documentWidth = pdfDoc.GetFirstPage().GetMediaBox().GetWidth();
            float documentHeight = pdfDoc.GetFirstPage().GetMediaBox().GetHeight();

            for (var pagina = numeracaoInicial; pagina <= pdfDoc.GetNumberOfPages()+(numeracaoInicial-1); pagina++)
            {

                TextAlignment align = TextAlignment.RIGHT;
                float bottom = documentHeight-margem;
                if (posicao == PosicionamentoNumeracao.BOTTOM_RIGHT) 
                {
                    bottom = margem;
                }
                else if (posicao == PosicionamentoNumeracao.BOTTOM_LEFT)
                {
                    bottom = margem;
                    align = TextAlignment.LEFT;
                } 
                else if (posicao == PosicionamentoNumeracao.TOP_LEFT)
                {
                    align = TextAlignment.LEFT;
                }

                Paragraph paragraph = new Paragraph($"{prefixo}{pagina}")
                    .SetTextAlignment(align)
                    .SetMargin(0)
                    .SetFontSize(tamanho)
                    .SetFixedPosition(margem, bottom, documentWidth-(margem*2));

                document.Add(paragraph);

            }

            document.Close();
            pdfDoc.Close();

            return outputStream;

        }

        public static MemoryStream Numerar(
            this MemoryStream sourceFile, 
            PosicionamentoNumeracao posicao=PosicionamentoNumeracao.TOP_RIGHT, 
            int margem=20, 
            int numeracaoInicial=1, 
            int tamanho=9, 
            string prefixo="Página "
        ) {
            return IncluirNumeracao(new PdfReader(sourceFile), posicao, margem, numeracaoInicial, tamanho, prefixo);
        }

        public static MemoryStream Numerar(
            this string sourceFile, 
            PosicionamentoNumeracao posicao = PosicionamentoNumeracao.TOP_RIGHT, 
            int margem = 20, 
            int numeracaoInicial = 1, 
            int tamanho = 9, 
            string prefixo = "Página "
        ) {
            return IncluirNumeracao(new PdfReader(sourceFile), posicao, margem, numeracaoInicial, tamanho, prefixo);
        }

    }
}
