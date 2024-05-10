using iText.Kernel.Pdf;
using iText.Signatures;
using System.Collections.Generic;
using System.IO;
using CMP.ManipuladorPDF.Certificados;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.Layout;
using iText.IO.Image;
using iText.Forms;
using iText.Forms.Form.Element;
using iText.Kernel.Colors;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using iText.Layout.Properties;
using iText.Layout.Renderer;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private const string CRL_URL = "https://crl.cacert.org/revoke.crl";

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
            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSAServers.TSA_GLOBALSIGN, null, null, 8192, DigestAlgorithms.SHA256);
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            OcspClientBouncyCastle ocspClient = new OcspClientBouncyCastle(ocspVerifier);
            var crlClient = new CrlClientOnline(CRL_URL);
            List<ICrlClient> crlList = new List<ICrlClient>() { crlClient };
            var info = CertificateInfo.GetSubjectFields(certificado.Chain[0]);
            string _name = info.GetField("CN");
            string _email = info.GetField("E");
            DateTime data = DateTime.Now;

            var font = PDFTrueTypeFont.GetFont("calibri");
            var fontBold = PDFTrueTypeFont.GetFont("calibrib");

            Div root = new Div()
                .SetWidth(220)
                .SetHeight(50);
            root.SetNextRenderer(new FlexContainerRenderer(root));

            Div logo = new Div()
                .SetWidth(38)
                .SetRelativePosition(0,0,0,0)
                .Add(
                    new Image(ImageDataFactory.Create("https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/brasao_camara.png"))
                        .SetRelativePosition(2,2,0,0)
                        .ScaleAbsolute(33, 41)
                );

            Div text = new Div()
                .Add(new Paragraph("Documento assinado digitalmente por:").SetFont(font).SetFontSize(5).SetRelativePosition(0, 5, 0, 0).SetMargin(0))
                .Add(new Paragraph(_name).SetFont(fontBold).SetFontSize(8).SetRelativePosition(0, 1, 0, 0).SetMargin(0))
                .Add(new Paragraph(_email).SetFont(fontBold).SetFontSize(5).SetRelativePosition(0, -3, 0, 0).SetMargin(0))
                .Add(new Paragraph($"Assinado em {data}").SetFont(font).SetFontSize(4).SetRelativePosition(0, 0, 0, 0).SetMargin(0))
                .Add(new Paragraph("Verifique em validar.camarapiracicaba.sp.gov.br").SetFont(font).SetFontSize(4).SetRelativePosition(0, -3, 0, 0).SetMargin(0));
            
            root.Add(logo);
            root.Add(text);

            SignerProperties signerProperties = new SignerProperties().SetFieldName("Signature1");
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
                .SetPageNumber(1)
                .SetPageRect(new Rectangle(x,y,220,49));

            MemoryStream outputStream = new MemoryStream();
            var padesSigner = new PdfPadesSigner(pdfReader, outputStream);

            padesSigner.SignWithBaselineBProfile(signerProperties, certificado.Chain, certificado.PKS);

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
