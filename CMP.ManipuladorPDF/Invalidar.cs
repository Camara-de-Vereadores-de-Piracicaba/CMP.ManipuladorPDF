﻿using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using iText.Layout;
using System.IO;
using iText.Layout.Properties;
using System.Collections.Generic;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF InvalidarArquivo(
            DocumentoPDF documento,
            string texto
        )
        {
            documento = documento.DesencriptarCasoNecessario();
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)),pdfWriter);
            Rectangle pageSize = pdfDocument.GetDefaultPageSize();
            //PdfFont font = PDFTrueTypeFont.GetFont("calibrib");
            float fontSize = 140;
            for (int pagina = 1; pagina <= pdfDocument.GetNumberOfPages(); pagina++)
            {
                PdfPage page = pdfDocument.GetPage(pagina);
                float pageWidth = page.GetMediaBox().GetWidth();
                float pageHeight = page.GetMediaBox().GetHeight();
                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, new Rectangle(0, 0, pageWidth, pageHeight));
                Paragraph paragraph = new Paragraph(texto)
                    .SetMargin(0)
                    //.SetFont(font)
                    .SetFontColor(new DeviceRgb(255, 50, 50))
                    .SetFontSize(fontSize);
                canvas.ShowTextAligned(paragraph, pageWidth / 2, pageHeight / 2, pagina, TextAlignment.CENTER, VerticalAlignment.MIDDLE, 45f);
            }

            pdfDocument.Close();
            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Invalidar um Documento PDF.
        /// </summary>
        /// <param name="documento">Documento para invalidar.</param>
        /// <param name="texto">Texto que aparecerá no meio da página.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Invalidar(this DocumentoPDF documento, string texto)
        {
            return InvalidarArquivo(documento, texto);
        }

        /// <summary>
        /// Escreve uma frase em diagonal sobre um Documento PDF.
        /// </summary>
        /// <param name="documento">Documento para marcar.</param>
        /// <param name="texto">Texto que aparecerá no meio da página.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Marcar(this DocumentoPDF documento, string texto)
        {
            return InvalidarArquivo(documento, texto);
        }

        /// <summary>
        /// Escreve a frase "Sem efeito" em um Documento PDF.
        /// </summary>
        /// <param name="documento">Documento para tornar sem efeito.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF TornarSemEfeito(this DocumentoPDF documento)
        {
            return Invalidar(documento, "SEM EFEITO");
        }

        /// <summary>
        /// Escreve a frase "Cópia" em um Documento PDF.
        /// </summary>
        /// <param name="documento">Documento para tornar cópia.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF MarcarComoCopia(this DocumentoPDF documento)
        {
            return Invalidar(documento, "CÓPIA");
        }

        /// <summary>
        /// Escreve a frase "Modelo" em um Documento PDF.
        /// </summary>
        /// <param name="documento">Documento para marcar como modelo.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF MarcarComoModelo(this DocumentoPDF documento)
        {
            return Invalidar(documento, "MODELO");
        }

    }
}