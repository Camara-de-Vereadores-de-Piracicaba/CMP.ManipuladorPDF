using iText.Kernel.Pdf;
using iText.Signatures;
using System.Collections.Generic;
using System.IO;
using CMP.ManipuladorPDF.Certificados;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using iText.Layout;
using System;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Xobject;
using iText.IO.Image;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private const string CRL_URL = "https://crl.cacert.org/revoke.crl";

        [Obsolete]
        private static DocumentoPDF AssinarDocumento(
            this DocumentoPDF documento,
            Certificado certificado,
            int x,
            int y,
            int pagina,
            string local,
            string razao
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSAServers.TSA_GLOBALSIGN, null, null, 8192, DigestAlgorithms.SHA256);
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            OcspClientBouncyCastle ocspClient = new OcspClientBouncyCastle(ocspVerifier);
            var crlClient = new CrlClientOnline(CRL_URL);
            List<ICrlClient> crlList = new List<ICrlClient>() { crlClient };
            PdfSigner pdfSigner = new PdfSigner(pdfReader, outputStream, new StampingProperties().UseAppendMode());
            Rectangle rectangle = new Rectangle(x, y, 200, 37);
            pdfSigner.SetPageRect(rectangle);
            float documentHeight = pdfSigner.GetDocument().GetPage(pagina).GetMediaBox().GetHeight();

            //Obsoleto
            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            PdfFormXObject layer = appearance.GetLayer2();

            //Novo jeito, não obsoleto
            //PdfFormXObject layer = new PdfFormXObject(new Rectangle(0, 0, 200, 37));

            PdfCanvas canvas = new PdfCanvas(layer, pdfSigner.GetDocument());
            PdfFont font = PdfFontFactory.CreateFont();
            Canvas signature = new Canvas(canvas, new Rectangle(0, 0, 200, 37));
            void addTextLine(string _text, int y, int size = 8, int offset = 0, bool bold = false)
            {
                Paragraph text = new Paragraph();
                text.SetFontSize(size).Add(_text);
                text.SetFont(PDFTrueTypeFont.GetFont(bold ? "calibrib" : "calibri"));
                signature.ShowTextAligned(text, 30, 6 + (y * 3) + offset, TextAlignment.LEFT);
            }
            var info = CertificateInfo.GetSubjectFields(certificado.Chain[0]);
            string _name = info.GetField("CN");
            string _email = info.GetField("E");
            DateTime data = DateTime.Now;
            addTextLine("Documento assinado digitalmente por:", 4, 4, 9, false);
            addTextLine(_name, 3, 6, 5, true);
            addTextLine(_email, 2, 4, 4, true);
            addTextLine($"Assinado em {data}", 1, 4, 1, false);
            addTextLine("Verifique em validar.camarapiracicaba.sp.gov.br", 0, 4, 0, false);
            ImageData imageData = ImageDataFactory.Create("https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/brasao_camara.png");
            Image image = new Image(imageData);
            image.SetFixedPosition(2, 4).ScaleAbsolute(25, 30);
            signature.Add(image);
            if (pagina>0)
            {
                pdfSigner
                    .SetLocation(local)
                    .SetReason(razao)
                    .SetPageNumber(pagina);

                /*
                 * Não obsoleto
                PdfSignatureFormField pdfSignatureFormField = new SignatureFormFieldBuilder(pdfSigner.GetDocument(),"signature")
                    .SetWidgetRectangle(pdfSigner.GetPageRect())
                    .SetPage(3)
                    .CreateSignature();
                PdfAcroForm acroForm = PdfAcroForm.GetAcroForm(pdfSigner.GetDocument(),true);
                acroForm.AddField(pdfSignatureFormField);
                pdfSignatureFormField.SetSignatureAppearanceLayer(layer);
                pdfSigner.SetFieldName("signature");
                pdfSigner.GetSignatureField().SetSignatureAppearanceLayer(layer);
                */
                
            }

            pdfSigner.SignDetached(certificado.PKS, certificado.Chain, crlList, ocspClient, tsaClient, 0, PdfSigner.CryptoStandard.CMS);
            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Assina um Documento PDF 
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>
        /// <param name="certificado">Caminho do certificado.</param>
        /// <param name="senha">Senha do certificado.</param>
        /// <param name="pagina">Página onde a assinatura vai aparecer. Defina como zero (0) para uma assinatura invisível.</param>
        /// <param name="x">Posição X da assinatura.</param>
        /// <param name="y">Posição Y da assinatura. As coordenadas são de baixo para cima.</param>
        /// <param name="local">Local físico onde o documento está sendo assinado.</param>
        /// <param name="razao">Motivo pelo qual o documento está sendo assinado.</param>

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento, 
            string certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0,
            string local = "Câmara Municipal de Piracicaba",
            string razao = "Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020."
        )
        {
            Certificado _certificado = new Certificado(certificado, senha);
            return AssinarDocumento(documento, _certificado, x, y, pagina, local, razao);
        }

        /// <summary>
        /// Assina um Documento PDF 
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>
        /// <param name="certificado">ByteArray do arquivo do certificado.</param>
        /// <param name="senha">Senha do certificado.</param>
        /// <param name="pagina">Página onde a assinatura vai aparecer. Defina como zero (0) para uma assinatura invisível.</param>
        /// <param name="x">Posição X da assinatura.</param>
        /// <param name="y">Posição Y da assinatura. As coordenadas são de baixo para cima.</param>
        /// <param name="local">Local físico onde o documento está sendo assinado.</param>
        /// <param name="razao">Motivo pelo qual o documento está sendo assinado.</param>

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento,
            byte[] certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0,
            string local = "Câmara Municipal de Piracicaba",
            string razao = "Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020."
        )
        {
            Certificado _certificado = new Certificado(certificado, senha);
            return AssinarDocumento(documento, _certificado, x, y, pagina, local, razao);
        }

        /// <summary>
        /// Assina um Documento PDF 
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>
        /// <param name="certificado">MemoryStream com o certificado.</param>
        /// <param name="senha">Senha do certificado.</param>
        /// <param name="pagina">Página onde a assinatura vai aparecer. Defina como zero (0) para uma assinatura invisível.</param>
        /// <param name="x">Posição X da assinatura.</param>
        /// <param name="y">Posição Y da assinatura. As coordenadas são de baixo para cima.</param>
        /// <param name="local">Local físico onde o documento está sendo assinado.</param>
        /// <param name="razao">Motivo pelo qual o documento está sendo assinado.</param>

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento,
            MemoryStream certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0,
            string local = "Câmara Municipal de Piracicaba",
            string razao = "Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020."
        )
        {
            Certificado _certificado = new Certificado(certificado, senha);
            return AssinarDocumento(documento, _certificado, x, y, pagina, local, razao);
        }

    }
}
