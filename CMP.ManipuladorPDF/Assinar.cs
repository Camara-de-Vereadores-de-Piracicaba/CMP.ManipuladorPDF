using CMP.Certificados;
using iText.Forms.Form.Element;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Crypto;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Renderer;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {
        private static SemaphoreSlim _tsaSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Verifica se o documento já possui pelo menos uma assinatura digital
        /// (campos de assinatura efetivamente preenchidos, com ByteRange).
        /// </summary>
        private static bool PossuiAssinaturas(
            this DocumentoPDF documento
        )
        {
            try
            {
                using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
                if (DocumentoPDFConfig.UNETHICAL_READING)
                    pdfReader.SetUnethicalReading(true);
                using PdfDocument pdfDocument = new PdfDocument(pdfReader);
                SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);
                return signatureUtil.GetSignatureNames().Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool FonteEstaEmbutida(PdfDictionary fontDict)
        {
            PdfDictionary descriptor = fontDict.GetAsDictionary(PdfName.FontDescriptor);
            if (descriptor == null)
            {
                PdfArray descendants = fontDict.GetAsArray(PdfName.DescendantFonts);
                if (descendants != null && descendants.Size() > 0)
                    descriptor = descendants.GetAsDictionary(0)?.GetAsDictionary(PdfName.FontDescriptor);
            }
            if (descriptor == null) return false;
            return descriptor.ContainsKey(PdfName.FontFile)
                || descriptor.ContainsKey(PdfName.FontFile2)
                || descriptor.ContainsKey(PdfName.FontFile3);
        }

        /// <summary>
        /// Prepara, em modo append (incremental), um documento já assinado para receber
        /// uma nova assinatura sem que o validador de conformidade do iText recuse
        /// defeitos herdados — típicos de PDFs assinados pela Adobe. Faz duas coisas:
        ///
        /// 1. Remove do /DR do AcroForm as fontes padrão não embutidas (Helvetica /Helv,
        ///    ZapfDingbats /ZaDb), que causam "All the fonts must be embedded".
        /// 2. Remove a árvore de tags (StructTreeRoot) e desmarca MarkInfo, evitando que o
        ///    checker PDF/UA rejeite a estrutura de acessibilidade herdada (ex.:
        ///    "Parent-child (H2:P) relation is not allowed").
        ///
        /// Por ser incremental, as assinaturas anteriores permanecem criptograficamente
        /// válidas (os bytes já assinados não são alterados).
        /// </summary>
        private static DocumentoPDF PrepararDocumentoAssinadoParaReassinatura(
            this DocumentoPDF documento
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));
            if (DocumentoPDFConfig.UNETHICAL_READING)
                pdfReader.SetUnethicalReading(true);
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter, new StampingProperties().UseAppendMode());

            PdfDictionary catalog = pdfDocument.GetCatalog().GetPdfObject();
            PdfDictionary acroForm = catalog.GetAsDictionary(PdfName.AcroForm);
            PdfDictionary dr = acroForm?.GetAsDictionary(PdfName.DR);
            PdfDictionary fonts = dr?.GetAsDictionary(PdfName.Font);

            if (fonts != null)
            {
                List<PdfName> remover = new List<PdfName>();
                foreach (PdfName key in fonts.KeySet())
                {
                    PdfDictionary fontDict = fonts.GetAsDictionary(key);
                    if (fontDict != null && !FonteEstaEmbutida(fontDict))
                        remover.Add(key);
                }

                foreach (PdfName key in remover)
                    fonts.Remove(key);

                if (remover.Count > 0)
                {
                    // Marca a cadeia até o objeto indireto que contém a mudança, para que o
                    // append mode a reescreva (dicionários diretos serializam inline no
                    // indireto que os contém).
                    fonts.SetModified();
                    dr.SetModified();
                    acroForm.SetModified();
                    catalog.SetModified();
                }
            }

            // Remove a árvore de tags (acessibilidade). Assinar em append mode preserva as
            // tags do documento, e o validador PDF/UA rejeitaria a estrutura herdada da
            // Adobe (ex.: "Parent-child (H2:P) relation is not allowed"). Isso equivale ao
            // RemoverTagsParaAssinatura, mas incremental para não invalidar assinaturas.
            if (catalog.ContainsKey(PdfName.StructTreeRoot))
            {
                catalog.Remove(PdfName.StructTreeRoot);
                PdfDictionary markInfo = catalog.GetAsDictionary(PdfName.MarkInfo);
                markInfo?.Put(PdfName.Marked, PdfBoolean.FALSE);
                catalog.SetModified();
            }

            pdfDocument.Close();
            return new DocumentoPDF(outputStream.ToArray());
        }

        private static DocumentoPDF AssinarDocumento(
            this DocumentoPDF documento,
            Certificado certificado,
            int x,
            int y,
            int pagina,
            string profile = "LTA",
            bool tryRecovery = true,
            bool signatureTagRemoval = true
        )
        {

            documento = documento.DesencriptarCasoNecessario();

            if (signatureTagRemoval)
                documento = documento.RemoverTagsParaAssinatura();

            MemoryStream inputStream = new MemoryStream(documento.ByteArray);
            using PdfReader pdfReader = new PdfReader(inputStream, new ReaderProperties());
            pdfReader.SetCloseStream(false);

            CertificateInfo.X500Name info = CertificateInfo.GetSubjectFields(certificado.Chain[0]);
            string _name = info.GetField("CN");
            _name ??= "";

            DateTime data = DateTime.Now;

            Div root = CarimboDeIdentidade(_name, data);

            string signatureName = "Signature_" + System.IO.Path.GetRandomFileName().Replace(".", "").Substring(0, 8);

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
                padesSigner.Sign(certificado, signerProperties, profile);
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("All the fonts must be embedded."))
                {
                    throw new InvalidPDFDocumentException(exception.Message);
                }
                else if (exception.Message.Contains("Append mode requires a document without errors, even if recovery is possible"))
                {
                    if (tryRecovery)
                    {
                        DocumentoPDF documentoRecuperado = new DocumentoPDF(documento.ConverterParaByteArray()).RepararDocumento();
                        documento = AssinarDocumento(documentoRecuperado, certificado, x, y, pagina, profile, false);
                        return new DocumentoPDF(documento.ConverterParaByteArray());
                    }

                    throw new IrrecuperableBrokenPDFDocumentException();
                }

                throw new SignatureException(exception.Message);
            }

            byte[] resultBytes = outputStream.ToArray();
            outputStream.Dispose();
            return new DocumentoPDF(resultBytes);
        }

        private static Div CarimboDeIdentidade(
            string nome,
            DateTime data
        )
        {

            PdfFont font = new FontePDF(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT, true).Fonte;
            PdfFont fontBold = new FontePDF(DocumentoPDFConfig.SIGNATURE_DEFAULT_FONT_BOLD, true).Fonte;

            Div root = new Div()
                .SetWidth(220)
                .SetHeight(50)
                .SetBackgroundColor(ColorConstants.WHITE);
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
            catch (Exception exception)
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
                    if (documento.PossuiAssinaturas())
                    {
                        try
                        {
                            DocumentoPDF documentoPreparado = documento.PrepararDocumentoAssinadoParaReassinatura();
                            return AssinarDocumento(documentoPreparado, certificado, x, y, pagina, profile, true, false);
                        }
                        catch (Exception)
                        {
                            throw new FontNotExistException(exception.Message);
                        }
                    }
                    else
                    {
                        throw new FontNotExistException(exception.Message);
                    }
                }
                else if (exception.Message.Contains("Append mode requires a document without errors, even if recovery is possible"))
                {
                    throw new BrokenPDFDocumentException();
                }
                else if (exception.Message.Contains("The file header shall begin at byte zero and shall consist of"))
                {
                    try
                    {
                        documento = documento.ConverterDocumentoParaPDFA();
                        return AssinarDocumento(documento, certificado, x, y, pagina, SignatureType.SIGNATURE_LTA);
                    }
                    catch (Exception)
                    {
                        throw new InvalidPDFHeaderDocumentException();
                    }
                }
                else if (exception.Message.Contains("The SSL connection could not be established"))
                {
                    try
                    {
                        return AssinarDocumento(documento, certificado, x, y, pagina, SignatureType.SIGNATURE_B);
                    }
                    catch (Exception)
                    {
                        throw new OCSPSignatureVerifyConnectionException();
                    }
                }
                else if (exception.Message.Contains("Cannot close document with already flushed PDF Catalog"))
                {
                    try
                    {
                        DocumentoPDF documentoRecuperado;
                        documentoRecuperado = documento.RemoverFontDescriptorsInvalidos();
                        return AssinarDocumento(documentoRecuperado, certificado, x, y, pagina, SignatureType.SIGNATURE_B, true, false);
                    }
                    catch (Exception)
                    {
                        throw new SignatureException(exception.Message);
                    }
                }
                else
                {
                    DocumentoPDF documentoRecuperado;
                    try
                    {
                        documentoRecuperado = documento.RecuperarDocumento();
                        return AssinarDocumento(documentoRecuperado, certificado, x, y, pagina, SignatureType.SIGNATURE_B, true, false);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            documentoRecuperado = documento.ConverterDocumentoParaPDFA();
                            return AssinarDocumento(documentoRecuperado, certificado, x, y, pagina, SignatureType.SIGNATURE_B, true, false);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                documentoRecuperado = documento.ConverterDocumentoParaPDFA();
                                return AssinarDocumento(documentoRecuperado, certificado, x, y, pagina, SignatureType.SIGNATURE_B, true, true);
                            }
                            catch (Exception)
                            {
                                throw new SignatureException(exception.Message);
                            }
                        }
                    }
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

        public static async Task<DocumentoPDF> AssinarAsync(this DocumentoPDF documento,
            Certificado certificado,
            int pagina = 1,
            int x = 0,
            int y = 0,
            string profile = "LTA")
        {
            await _tsaSemaphore.WaitAsync();
            try
            {
                return ProcessarAssinatura(documento, certificado, pagina, x, y, profile);
            }
            finally
            {
                _tsaSemaphore.Release();
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

    }

    public static class PdfPadesSignerExtension
    {
        public static void Sign(
            this PdfPadesSigner padesSigner,
            Certificado certificado,
            SignerProperties signerProperties,
            string profile)
        {

            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSAServers.TSA_DEFAULT, null, null, 8192, DigestAlgorithms.SHA256);

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
            else if (certificado.RSASignature != null)
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
        }
    }

}
