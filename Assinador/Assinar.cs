using iText.Kernel.Pdf;
using iText.Signatures;
using System.IO;
using CMP.Certificados;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Forms.Form.Element;
using System;
using iText.Layout.Renderer;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static DocumentoPDF AssinarDocumento(
            this DocumentoPDF documento,
            Certificado certificado,
            int x,
            int y,
            int pagina
        )
        {
            using MemoryStream signatureStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            using PdfWriter pdfWriter = new PdfWriter(signatureStream);
            
            var info = CertificateInfo.GetSubjectFields(certificado.Chain[0]);
            string _name = info.GetField("CN");
            if (_name == null)
            {
                _name = "";
            }

            string _email = info.GetField("E");
            if(_email == null)
            {
                _email = "";
            }

            DateTime data = DateTime.Now;

            var font = PDFTrueTypeFont.GetFont("calibri");
            var fontBold = PDFTrueTypeFont.GetFont("calibrib");

            Div root = new Div()
                .SetWidth(220)
                .SetHeight(50);
            root.SetNextRenderer(new FlexContainerRenderer(root));

            Div logo = new Div()
                .SetWidth(40)
                .SetRelativePosition(0,0,0,0)
                .Add(
                    new Image(ImageDataFactory.Create("https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/brasao_camara.png"))
                        .SetRelativePosition(2,2,0,0)
                        .ScaleAbsolute(33, 41)
                );

            Div text = new Div()
                .SetWidth(180)
                .Add(new Paragraph("Documento assinado digitalmente").SetFont(font).SetFontSize(7).SetRelativePosition(0, 2, 0, 0).SetMargin(0))
                .Add(new Paragraph(_name.ToUpper()).SetFont(fontBold).SetMultipliedLeading(0.8f).SetFontSize(9).SetRelativePosition(0, 1, 0, 0).SetMargin(0))
                .Add(new Paragraph($"Assinado em {data}").SetFont(font).SetFontSize(6).SetRelativePosition(0, 0, 0, 0).SetMargin(0))
                .Add(new Paragraph("Verifique em validar.camarapiracicaba.sp.gov.br").SetFont(font).SetFontSize(5).SetRelativePosition(0, -3, 0, 0).SetMargin(0));

            root.Add(logo);
            root.Add(text);

            string signatureName = "Signature_" + System.IO.Path.GetRandomFileName().Replace(".","").Substring(0,8);

            SignerProperties signerProperties = new SignerProperties();
            signerProperties.SetFieldName(signatureName);

            SignatureFieldAppearance appearance = new SignatureFieldAppearance(signerProperties.GetFieldName());
            appearance
                .SetContent(root)
                .SetHeight(50)
                .SetWidth(220)
                .SetInteractive(true);

            signerProperties
                .SetSignatureAppearance(appearance)
                .SetLocation("Câmara Municipal de Piracicaba")
                .SetReason("Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.")
                .SetPageRect(new Rectangle(x,y,220,49));

            if (pagina > 0)
            {
                signerProperties.SetPageNumber(pagina);
            }

            MemoryStream outputStream = new MemoryStream();
            PdfPadesSigner padesSigner = new PdfPadesSigner(pdfReader, outputStream);
            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSAServers.TSA_DEFAULT, null, null, 8192, DigestAlgorithms.SHA256);
            padesSigner.SignWithBaselineLTAProfile(signerProperties, certificado.Chain, certificado.PKS, tsaClient);
            return new DocumentoPDF(outputStream);
        }

        private static DocumentoPDF ProcessarAssinatura(
            DocumentoPDF documento,
            dynamic certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0
        )
        {
            try
            {
                Certificado _certificado = new Certificado(certificado, senha);
                return AssinarDocumento(documento, _certificado, x, y, pagina);
            }
            catch(Exception exception)
            {
                if (exception.Message == "PKCS12 key store MAC invalid - wrong password or corrupted file.")
                {
                    throw new AssinaturaException("Senha incorreta.");
                }
                else if (exception.Message.Contains("Could not find file"))
                {
                    throw new AssinaturaException("Certificado não encontrado.");
                }
                else if (exception.Message.Contains("unexpected end-of-contents marker"))
                {
                    throw new AssinaturaException("Certificado inválido.");
                }

                throw new AssinaturaException(exception.Message);
            }
            
        }

        /// <summary>
        /// Assina um Documento PDF 
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>
        /// <param name="certificado">Certificado.</param>
        /// <param name="pagina">Página onde a assinatura vai aparecer. Defina como zero (0) para uma assinatura invisível.</param>
        /// <param name="x">Posição X da assinatura.</param>
        /// <param name="y">Posição Y da assinatura. As coordenadas são de baixo para cima.</param>

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento,
            Certificado certificado,
            int pagina = 1,
            int x = 0,
            int y = 0
        )
        {
            return ProcessarAssinatura(documento, certificado.ByteArray, certificado.Senha, pagina, x, y);
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

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento, 
            string certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0
        )
        {
            return ProcessarAssinatura(documento, certificado, senha, pagina, x, y);
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

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento,
            byte[] certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0
        )
        {
            return ProcessarAssinatura(documento, certificado, senha, pagina, x, y);
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

        public static DocumentoPDF Assinar(
            this DocumentoPDF documento,
            MemoryStream certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0
        )
        {
            return ProcessarAssinatura(documento, certificado, senha, pagina, x, y);
        }

    }
}
