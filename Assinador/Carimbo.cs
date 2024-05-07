﻿using iText.Kernel.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Properties;
using iText.Layout.Renderer;
using iText.Layout.Layout;
using iText.IO.Image;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using Rectangle = iText.Kernel.Geom.Rectangle;
using Image = iText.Layout.Element.Image;
using iText.Kernel.Geom;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        public enum PosicaoCarimbo
        {
            DIREITA,
            ESQUERDA,
            TOPO,
            BASE
        }
        private static DocumentoPDF Carimbo(
            this DocumentoPDF documento,
            List<string> linhas,
            PosicaoCarimbo posicao,
            string? qrcode = null,
            int altura = 10
        ){
            int tamanhoQRCode = 40;
            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
            int totalPaginas = pdfDocument.GetNumberOfPages();
            Rectangle pageSize = pdfDocument.GetDefaultPageSize();
            PdfFont font = PDFTrueTypeFont.GetFont("calibri");
            for (int pagina = 1; pagina <= totalPaginas; pagina++)
            {
                int distanciaBorda = 6;
                int tamanhoFonte = 6;
                PdfPage page = pdfDocument.GetPage(pagina);
                var mediaBox = page.GetCropBox();
                float pageWidth = mediaBox.GetWidth();
                float pageHeight = mediaBox.GetHeight();
                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, new Rectangle(0, 0, pageWidth, pageHeight));
                if(qrcode != null)
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrcode, QRCodeGenerator.ECCLevel.H);
                    QRCode _qrCode = new QRCode(qrCodeData);
                    Bitmap qrCodeImage = _qrCode.GetGraphic(4,Color.Black,Color.White,false);
                    MemoryStream ms = new MemoryStream();
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    ImageData imageData = ImageDataFactory.CreatePng(ms.ToArray());
                    Image _qrcode = new Image(imageData);
                    int qx = (int)pageWidth - tamanhoQRCode - distanciaBorda;
                    if (posicao == PosicaoCarimbo.ESQUERDA)
                    {
                        qx = 0;
                    }
                    _qrcode
                        .ScaleAbsolute(tamanhoQRCode, tamanhoQRCode)
                        .SetFixedPosition(qx, altura);
                    canvas.Add(_qrcode);
                    altura = altura + tamanhoQRCode + 5;
                }
                void Texto(string texto, int linha)
                {
                    Paragraph text = new Paragraph();
                    text.SetFontSize(tamanhoFonte).Add(texto);
                    text.SetFont(PDFTrueTypeFont.GetFont("calibri"));
                    IRenderer renderer = text.CreateRendererSubTree();
                    LayoutResult result = renderer.SetParent(canvas.GetRenderer()).Layout(new LayoutContext(new LayoutArea(pagina, new Rectangle(pageWidth, pageHeight))));
                    Rectangle boundingBox = result.GetOccupiedArea().GetBBox();
                    int height = (int)boundingBox.GetHeight();
                    int width = (int)boundingBox.GetWidth();
                    float x = pageWidth - (height / 2) - distanciaBorda - (linha * (tamanhoFonte + 1));
                    if (posicao == PosicaoCarimbo.ESQUERDA)
                    {
                        x = distanciaBorda - (linha * (tamanhoFonte + 1));
                    }
                    canvas.ShowTextAligned(
                        p: text,
                        pageNumber: pagina,
                        x: x,
                        y: altura,
                        textAlign: TextAlignment.LEFT,
                        vertAlign: VerticalAlignment.TOP,
                        radAngle: 1.5708f
                    );
                }
                for(int i = 0; i <= linhas.Count()-1; i++)
                {
                    string _texto = linhas[i];
                    if (i == 0)
                    {
                        _texto = $"Página {pagina} de {totalPaginas}. {_texto}";
                    }
                    Texto(_texto, i);
                }
            }
            pdfDocument.Close();
            return new DocumentoPDF(outputStream);
        }


        /// <summary>
        /// Carimba um documento PDF com um texto na lateral.
        /// </summary>
        /// <param name="documento">Documento PDF para carimbar.</param>
        /// <param name="linhas">Uma lista de strings com as linhas de texto do carimbo.</param>
        /// <param name="posicao">A posição no carimbo, na direita ou na esquerda da página.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Carimbar(this DocumentoPDF documento, List<string> linhas, PosicaoCarimbo posicao = PosicaoCarimbo.DIREITA)
        {
            return Carimbo(documento, linhas, posicao);
        }

        /// <summary>
        /// Carimba um documento PDF com um texto na lateral.
        /// </summary>
        /// <param name="documento">Documento PDF para carimbar.</param>
        /// <param name="linha">O texto a ser carimbado.</param>
        /// <param name="posicao">A posição no carimbo, na direita ou na esquerda da página.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF Carimbar(this DocumentoPDF documento, string linha, PosicaoCarimbo posicao = PosicaoCarimbo.DIREITA)
        {
            List<string> linhas = new List<string>();
            linhas.Add(linha);
            return Carimbo(documento, linhas, posicao);
        }

        /// <summary>
        /// Carimba um documento PDF com um texto e um QRCode na lateral.
        /// </summary>
        /// <param name="documento">Documento PDF para carimbar.</param>
        /// <param name="linhas">Uma lista de strings com as linhas de texto do carimbo.</param>
        /// <param name="qrcode">O texto que o QRCode mostrará.</param>
        /// <param name="posicao">A posição no carimbo, na direita ou na esquerda da página.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF CarimbarComQRCode(this DocumentoPDF documento, List<string> linhas, string qrcode, PosicaoCarimbo posicao = PosicaoCarimbo.DIREITA)
        {
            return Carimbo(documento, linhas, posicao, qrcode);
        }

        /// <summary>
        /// Carimba um documento PDF com um texto e um QRCode na lateral.
        /// </summary>
        /// <param name="documento">Documento PDF para carimbar.</param>
        /// <param name="linha">O texto a ser carimbado.</param>
        /// <param name="qrcode">O texto que o QRCode mostrará.</param>
        /// <param name="posicao">A posição no carimbo, na direita ou na esquerda da página.</param>
        /// <returns>DocumentoPDF</returns>

        public static DocumentoPDF CarimbarComQRCode(this DocumentoPDF documento, string linha, string qrcode, PosicaoCarimbo posicao = PosicaoCarimbo.DIREITA)
        {
            List<string> linhas = new List<string>();
            linhas.Add(linha);
            return Carimbo(documento, linhas, posicao, qrcode);
        }

    }

}