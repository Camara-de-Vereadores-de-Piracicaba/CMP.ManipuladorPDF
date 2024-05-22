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
using iText.Kernel.Font;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF AssinarDocumento(
            this DocumentoPDF documento,
            Certificado certificado,
            int x,
            int y,
            int pagina,
            string profile = "LTA"
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

            PdfFont font = PdfFontFactory.CreateFont(
                File.ReadAllBytes($"{DocumentoPDFConfig.FONT_PATH}/{DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT}.ttf"),
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED
            );
            PdfFont fontBold = PdfFontFactory.CreateFont(
                File.ReadAllBytes($"{DocumentoPDFConfig.FONT_PATH}/{DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT_BOLD}.ttf"),
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED
            );

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
                .Add(new Paragraph(_name.ToUpper()).SetMultipliedLeading(0.8f).SetFont(fontBold).SetFontSize(9).SetRelativePosition(0, 1, 0, 0).SetMargin(0))
                .Add(new Paragraph($"Assinado em {data}").SetFont(font).SetFontSize(7).SetRelativePosition(0, 2, 0, 0).SetMargin(0))
                .Add(new Paragraph("Verifique em validar.camarapiracicaba.sp.gov.br").SetFont(font).SetFontSize(7).SetRelativePosition(0, -3, 0, 0).SetMargin(0));

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

            if (profile == "LT")
            {
                padesSigner.SignWithBaselineLTProfile(signerProperties, certificado.Chain, certificado.PKS, tsaClient);
            }
            else if (profile == "T")
            {
                padesSigner.SignWithBaselineTProfile(signerProperties, certificado.Chain, certificado.PKS, tsaClient);
            }
            else if (profile == "B")
            {
                padesSigner.SignWithBaselineBProfile(signerProperties, certificado.Chain, certificado.PKS);
            }
            else
            {
                padesSigner.SignWithBaselineLTAProfile(signerProperties, certificado.Chain, certificado.PKS, tsaClient);
            }

            return new DocumentoPDF(outputStream);
        }

        private static DocumentoPDF ProcessarAssinatura(
            DocumentoPDF documento,
            dynamic certificado,
            string senha,
            int pagina = 1,
            int x = 0,
            int y = 0,
            string profile = "LTA"
        )
        {
            try
            {
                Certificado _certificado = new Certificado(certificado, senha);
                return AssinarDocumento(documento, _certificado, x, y, pagina, profile);
            }
            catch(Exception exception)
            {
                
                if (exception.Message == "PKCS12 key store MAC invalid - wrong password or corrupted file.")
                {
                    throw new CertificateWrongPasswordException();
                }
                else if (exception.Message.Contains("Could not find file"))
                {
                    if (exception.Message.Contains(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT)) 
                    {
                        throw new FontNotExistException(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT);
                    }
                    else if (exception.Message.Contains(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT_BOLD))
                    {
                        throw new FontNotExistException(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT_BOLD);
                    }
                    else
                    {
                        throw new AssinaturaException("Certificado não encontrado.");
                    }
                }
                else if (exception.Message.Contains("unexpected end-of-contents marker"))
                {
                    throw new AssinaturaException("Certificado inválido.");
                }

                throw new AssinaturaException(exception.Message);
            }
            
        }

        public static class SignatureType
        {
            public static string SIGNATURE_LTA { get; set; } = "LTA";
            public static string SIGNATURE_LT { get; set; } = "LT";
            public static string SIGNATURE_T { get; set; } = "T";
            public static string SIGNATURE_B { get; set; } = "B";
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
            int y = 0,
            string profile = "LTA"
        )
        {
            return ProcessarAssinatura(documento, certificado.ByteArray, certificado.Senha, pagina, x, y, profile);
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
            int y = 0,
            string profile = "LTA"
        )
        {
            return ProcessarAssinatura(documento, certificado, senha, pagina, x, y, profile);
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
            int y = 0,
            string profile = "LTA"
        )
        {
            return ProcessarAssinatura(documento, certificado, senha, pagina, x, y, profile);
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
            int y = 0,
            string profile = "LTA"
        )
        {
            return ProcessarAssinatura(documento, certificado, senha, pagina, x, y, profile);
        }

    }
}
