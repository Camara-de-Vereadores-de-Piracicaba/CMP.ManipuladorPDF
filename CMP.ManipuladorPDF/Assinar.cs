using iText.Kernel.Pdf;
using iText.Signatures;
using System.IO;
using CMP.Certificados;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.IO.Image;
using System;
using iText.Layout.Renderer;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Forms.Form.Element;

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
            _name ??= "";

            DateTime data = DateTime.Now;

            Div root = CarimboDeIdentidade(_name,data);

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
                .SetLocation(SignatureText.LOCATION)
                .SetReason(SignatureText.REASON);

            if (pagina > 0)
            {
                signerProperties.SetSignatureAppearance(appearance);
                signerProperties.SetPageRect(new Rectangle(x, y, 220, 49));
                signerProperties.SetPageNumber(pagina);
            }

            MemoryStream outputStream = new MemoryStream();
            PdfPadesSigner padesSigner = new PdfPadesSigner(pdfReader, outputStream);

            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSAServers.TSA_DEFAULT, null, null, 8192, DigestAlgorithms.SHA256);

            try
            {
                if (certificado.PKS != null)
                {
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
                }
                else if(certificado.RSASignature!=null)
                {
                    if (profile == "LT")
                    {
                        padesSigner.SignWithBaselineLTProfile(signerProperties, certificado.Chain, certificado.RSASignature, tsaClient);
                    }
                    else if (profile == "T")
                    {
                        padesSigner.SignWithBaselineTProfile(signerProperties, certificado.Chain, certificado.RSASignature, tsaClient);
                    }
                    else if (profile == "B")
                    {
                        padesSigner.SignWithBaselineBProfile(signerProperties, certificado.Chain, certificado.RSASignature);
                    }
                    else
                    {
                        padesSigner.SignWithBaselineLTAProfile(signerProperties, certificado.Chain, certificado.RSASignature, tsaClient);
                    }
                }
            }catch(Exception exception)
            {
                if(exception.Message.Contains("All the fonts must be embedded."))
                {
                    throw new InvalidPDFDocumentException();
                }

                throw new SignatureException(exception.Message);
            }

            return new DocumentoPDF(outputStream);
        }

        [Obsolete]
        public static DocumentoPDF AssinarNoModoLegado(
            this DocumentoPDF documento,
            Certificado certificado,
            int pagina = 1,
            int x = 0,
            int y =0
        )
        {
            using MemoryStream signatureStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            using PdfWriter pdfWriter = new PdfWriter(signatureStream);
            PdfSigner signer = new PdfSigner(pdfReader, signatureStream, new StampingProperties().UseAppendMode());
            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
            PdfFont font = new FontePDF(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT,true).Fonte;
            string signatureName = "Signature_" + System.IO.Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
            appearance
                .SetLocation(SignatureText.LOCATION)
                .SetReason(SignatureText.REASON)
                .SetContact(SignatureText.CONTACT)
                .SetSignatureCreator(SignatureText.CREATOR)
                .SetPageRect(new Rectangle(x, y, 220, 50))
                .SetLayer2FontSize(12)
                .SetPageNumber(pagina)
                .SetLayer2Font(font);

            Rectangle rectangle = new Rectangle(0, 0, 220, 50);
            PdfFormXObject layer = appearance.GetLayer2();
            PdfCanvas pdfCanvas = new PdfCanvas(layer, signer.GetDocument());
            Canvas canvas = new Canvas(pdfCanvas, rectangle);
            var info = CertificateInfo.GetSubjectFields(certificado.Chain[0]);
            string name = info.GetField("CN");
            name ??= "";
            DateTime data = DateTime.Now;
            Div root = CarimboDeIdentidade(name, data);
            canvas.Add(root);
            signer.SetFieldName(signatureName);
            signer.SetSignDate(DateTime.Now);
            signer.SignDetached(certificado.PKS, certificado.Chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
            return new DocumentoPDF(signatureStream);
        }

        private static Div CarimboDeIdentidade(
            string nome,
            DateTime data
        )
        {

            PdfFont font = new FontePDF(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT,true).Fonte;
            PdfFont fontBold = new FontePDF(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT_BOLD,true).Fonte;

            Div root = new Div()
                .SetWidth(220)
                .SetHeight(50);
            root.SetNextRenderer(new FlexContainerRenderer(root));

            Div logo = new Div()
                .SetWidth(40)
                .SetRelativePosition(0, 0, 0, 0)
                .Add(
                    new Image(ImageDataFactory.Create("https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/brasao_camara.png"))
                        .SetRelativePosition(2, 2, 0, 0)
                        .ScaleAbsolute(33, 41)
                );

            Div text = new Div()
            .SetWidth(180)
                .Add(new Paragraph("Documento assinado digitalmente").SetFont(font).SetFontSize(7).SetRelativePosition(0, 2, 0, 0).SetMargin(0))
                .Add(new Paragraph(nome.ToUpper()).SetMultipliedLeading(0.8f).SetFont(fontBold).SetFontSize(9).SetRelativePosition(0, 1, 0, 0).SetMargin(0))
                .Add(new Paragraph($"Assinado em {data}").SetFont(font).SetFontSize(7).SetRelativePosition(0, 2, 0, 0).SetMargin(0))
                .Add(new Paragraph("Verifique em validar.camarapiracicaba.sp.gov.br").SetFont(font).SetFontSize(7).SetRelativePosition(0, -3, 0, 0).SetMargin(0));

            root.Add(logo);
            root.Add(text);

            return root;
        }

        private static DocumentoPDF ProcessarAssinatura(
            DocumentoPDF documento,
            Certificado certificado,
            int pagina = 1,
            int x = 0,
            int y = 0,
            string profile = "LTA"
        )
        {
            try
            {
                return AssinarDocumento(documento, certificado, x, y, pagina, profile);
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
                        throw new SignatureException(exception.Message);
                    }
                }
                else if (exception.Message.Contains("unexpected end-of-contents marker"))
                {
                    throw new SignatureException("Certificado inválido.");
                }
                else if (exception.Message.Contains("All the fonts must be embedded"))
                {
                    throw new FontNotExistException(exception.Message);
                }

                throw new SignatureException(exception.Message);
            }
            
        }

        public static class SignatureText
        {

            public static string LOCATION { get; set; } = "Câmara Municipal de Piracicaba";
            public static string REASON { get; set; } = "Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.";
            public static string CONTACT { get; set; } = "desenvolvimento@camarapiracicaba.sp.gov.br";
            public static string CREATOR { get; set; } = "Biblioteca de Assinatura Digital da Câmara Municipal de Piracicaba";

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
            return ProcessarAssinatura(documento, certificado, pagina, x, y, profile);
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

            Certificado _certificado = new Certificado();
            try
            {
                _certificado = new Certificado(certificado, senha);
            }
            catch (Exception exception)
            {
                if (exception.Message == "PKCS12 key store MAC invalid - wrong password or corrupted file.")
                    throw new CertificateWrongPasswordException();
            }

            return ProcessarAssinatura(documento, _certificado, pagina, x, y, profile);

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
            Certificado _certificado = new Certificado();
            try
            {
                _certificado = new Certificado(certificado, senha);
            }
            catch (Exception exception)
            {
                if (exception.Message == "PKCS12 key store MAC invalid - wrong password or corrupted file.")
                    throw new CertificateWrongPasswordException();
            }
            return ProcessarAssinatura(documento, _certificado, pagina, x, y, profile);
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
            Certificado _certificado = new Certificado();
            try
            {
                _certificado = new Certificado(certificado, senha);
            }
            catch (Exception exception)
            {
                if (exception.Message == "PKCS12 key store MAC invalid - wrong password or corrupted file.")
                    throw new CertificateWrongPasswordException();
            }
            return ProcessarAssinatura(documento, _certificado, pagina, x, y, profile);
        }

        /// <summary>
        /// Assina um Documento PDF no modo legado (Você não deveria estar usando isso...)
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>
        /// <param name="certificado">Certificado do signatário.</param>
        /// <param name="pagina">Página onde a assinatura vai aparecer. Defina como zero (0) para uma assinatura invisível.</param>
        /// <param name="x">Posição X da assinatura.</param>
        /// <param name="y">Posição Y da assinatura. As coordenadas são de baixo para cima.</param>

        [Obsolete]
        public static DocumentoPDF AssinarLegado(
            this DocumentoPDF documento,
            Certificado certificado,
            int pagina = 1,
            int x = 0,
            int y = 0
        )
        {
            return AssinarNoModoLegado(documento, certificado, pagina, x, y);
        }

    }


}
