using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.Barcodes;
using static CMP.ManipuladorPDF.ManipuladorPDF;
using iText.Layout.Properties;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {

        public enum PosicaoAssinatura
        {
            SOMENTE_INTERNA,
            FIXA_LATERAL_DIREITA,
            FIXA_LATERAL_ESQUERDA,
            LIVRE
        }

        private const string CRL_URL = "https://crl.cacert.org/revoke.crl";

        //private const string TSA_GLOBALSIGN = "http://rfc3161timestamp.globalsign.com/advanced";
        //private const string TSA_GLOBALSIGN2 = "http://timestamp.globalsign.com/tsa/r6advanced1";
        //private const string TSA_GLOBALSIGN3 = "http://aatl-timestamp.globalsign.com/tsa/aohfewat2389535fnasgnlg5m23";
        private const string TSA_DIGICERT = "http://timestamp.digicert.com";
        //private const string TSA_SECTIGO = "https://timestamp.sectigo.com";
        //private const string TSA_ENTRUST = "http://timestamp.entrust.net/TSS/RFC3161sha2TS";
        //private const string TSA_DOCUSIGN = "http://kstamp.keynectis.com/KSign/";
        //private const string TSA_QUOVADIS = "http://ts.quovadisglobal.com/eu";
        //private const string TSA_SSLDOTCOM = "http://ts.ssl.com";
        //private const string TSA_IDENTRUST = "http://timestamp.identrust.com";

        private const string TSA_URL = TSA_DIGICERT;

        public const int TODAS_AS_PAGINAS = 0;

        private static MemoryStream Assinar(
            PdfReader reader,
            Certificado certificado,
            Assinatura assinatura
        )
        {
            using MemoryStream outputStream = new MemoryStream();
            PdfSigner pdfSigner = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());

            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSA_URL, null, null, 8192, DigestAlgorithms.SHA256);
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            OcspClientBouncyCastle ocspClient = new OcspClientBouncyCastle(ocspVerifier);

            var crlClient = new CrlClientOnline("https://crl.cacert.org/revoke.crl");
            List<ICrlClient> crlList = new List<ICrlClient>() { crlClient };

            string signatureName = pdfSigner.GetNewSigFieldName();

            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            Rectangle mediaBox = pdfSigner.GetDocument().GetPage(1).GetMediaBox();
            float documentWidth = mediaBox.GetWidth();
            float documentHeight = mediaBox.GetHeight();

            int pagina = assinatura.Pagina;
            if (
                pagina <= 0 ||
                pagina > pdfSigner.GetDocument().GetNumberOfPages()
            )
            {
                pagina = 1;
            }

            appearance
                .SetLocation(assinatura.Local)
                .SetReason(assinatura.Razao)
                .SetContact(assinatura.EmailContato)
                .SetSignatureCreator(assinatura.Criador)
                .SetPageNumber(pagina);

            appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);

            int alturaAssinaturaHorizontal = 40;
            Rectangle lateralSignaturePosition = new Rectangle(new Rectangle(documentWidth - alturaAssinaturaHorizontal, 0, alturaAssinaturaHorizontal, documentHeight));

            if (assinatura.Dados.Posicao == PosicaoAssinatura.FIXA_LATERAL_DIREITA)
            {
                appearance.SetPageRect(lateralSignaturePosition);
                SetBorderSignature(pdfSigner, assinatura.Dados);
            }
            else if (assinatura.Dados.Posicao == PosicaoAssinatura.FIXA_LATERAL_ESQUERDA)
            {
                lateralSignaturePosition = new Rectangle(alturaAssinaturaHorizontal, 0, alturaAssinaturaHorizontal, documentHeight);
                appearance.SetPageRect(lateralSignaturePosition);
                SetBorderSignature(pdfSigner, assinatura.Dados);
            }
            else if (assinatura.Dados.Posicao == PosicaoAssinatura.LIVRE)
            {
                appearance.SetPageRect(new Rectangle(assinatura.Dados.X, assinatura.Dados.Y, 200, 37));
                SetFreeSignature(pdfSigner, assinatura.Dados);
            }

            if (assinatura.CarimboLateral)
            {
                PdfDocument document = pdfSigner.GetDocument();
                int paginas = document.GetNumberOfPages();

                for( int i = 1; i <= paginas; i++)
                {
                    if (i == pagina && assinatura.Dados.Posicao != PosicaoAssinatura.LIVRE)
                        continue;

                    PdfCanvas _canvas = new PdfCanvas(document.GetPage(i));
                    Canvas canvas = new Canvas(_canvas, lateralSignaturePosition);
                    DrawBorderSignatureInCanvas(canvas, document, assinatura.Dados, (int)(documentWidth - alturaAssinaturaHorizontal));
                }

            }

            pdfSigner.SetFieldName(signatureName);
            pdfSigner.SignDetached(certificado.PKS, certificado.Chain, crlList, ocspClient, tsaClient, 0, PdfSigner.CryptoStandard.CMS);

            return outputStream;
        }

        private static MemoryStream AdicionaLTV(MemoryStream _inputStream)
        {

            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle(TSA_URL, null, null, 8192, DigestAlgorithms.SHA256);
            OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
            OcspClientBouncyCastle ocspClient = new OcspClientBouncyCastle(ocspVerifier);
            var crlClient = new CrlClientOnline(CRL_URL);
            List<ICrlClient> crlList = new List<ICrlClient>() { crlClient };

            MemoryStream inputStream = new MemoryStream(_inputStream.ToArray());
            using MemoryStream outputStream = new MemoryStream();
            PdfSigner ltvPdfSigner = new PdfSigner(new PdfReader(inputStream), outputStream, new StampingProperties().UseAppendMode());
            PdfDocument ltvDoc = ltvPdfSigner.GetDocument();
            
            LtvVerification ltvVerification = new LtvVerification(ltvDoc);
            SignatureUtil signatureUtil = new SignatureUtil(ltvDoc);
            IList<string> names = signatureUtil.GetSignatureNames();
            foreach (var name in names)
            {
                ltvVerification.AddVerification(
                    signatureName: name,
                    ocsp: ocspClient,
                    crl: crlClient,
                    certOption: LtvVerification.CertificateOption.WHOLE_CHAIN,
                    level: LtvVerification.Level.OCSP_CRL,
                    certInclude: LtvVerification.CertificateInclusion.NO
                );
            }

            ltvVerification.Merge();

            //Console.WriteLine(ltvPdfSigner.GetDocument().GetCatalog().GetPdfObject().Get(PdfName.DSS));


            ltvPdfSigner.Timestamp(tsaClient, "Signature2");



            return outputStream;
        }

        private static Canvas DrawBorderSignatureInCanvas(Canvas signature, PdfDocument pdfDocument, DadosAssinatura dados, int right = 0)
        {
            int lineFloor = 25;
            int fontSpace = 50;
            int fontSize = 6;
            void addTextLine(string _text, int _line)
            {
                Paragraph text = new Paragraph();
                text.SetFontSize(fontSize).Add(_text);
                text.SetFont(PDFTrueTypeFont.GetFont());
                signature.ShowTextAligned(
                    p: text,
                    pageNumber: 1,
                    x: lineFloor - (_line * (fontSize + 1)) + right,
                    y: fontSpace,
                    textAlign: TextAlignment.LEFT,
                    vertAlign: VerticalAlignment.TOP,
                    radAngle: (float)Conversion.ToRadians(90)
                );
            }

            DateTime data = DateTime.Now;
            addTextLine($"Assinado digitalmente por {dados.Nome} e protocolado na Câmara Municipal de Piracicaba em {data}, sob o nº {dados.Protocolo}.", dados.EnderecoValidacao ? 1 : 0);
            if (dados.EnderecoValidacao)
            {
                addTextLine($"Para autenticar este documento, utilize o qrcode impresso ou entre em https://validar.camarapiracicaba.sp.gov.br.", 0);
            }

            BarcodeQRCode qrcode = new BarcodeQRCode($"https://validar.camarapiracicaba.sp.gov.br/{dados.Protocolo}");
            Image qrImage = new Image(qrcode.CreateFormXObject(pdfDocument));
            qrImage.SetFixedPosition(right, 12);
            signature.Add(qrImage);
            return signature;
        }

        private static void SetBorderSignature(PdfSigner pdfSigner, DadosAssinatura dados)
        {
            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            float documentHeight = pdfSigner.GetDocument().GetPage(1).GetMediaBox().GetHeight();
            var layer2 = appearance.GetLayer2();
            PdfCanvas canvas = new PdfCanvas(layer2, pdfSigner.GetDocument());
            Canvas signature = new Canvas(canvas, new Rectangle(0, 0, 20, documentHeight));
            DrawBorderSignatureInCanvas(signature, pdfSigner.GetDocument(), dados);
        }

        private static void SetFreeSignature(PdfSigner pdfSigner, DadosAssinatura dados)
        {
            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            float documentHeight = pdfSigner.GetDocument().GetPage(1).GetMediaBox().GetHeight();
            var layer2 = appearance.GetLayer2();
            PdfCanvas canvas = new PdfCanvas(layer2, pdfSigner.GetDocument());
            PdfFont font = PdfFontFactory.CreateFont();
            Canvas signature = new Canvas(canvas, new Rectangle(dados.X, dados.Y, 200, 40));
            void addTextLine(string _text, int y, int size = 8, int offset = 0, bool bold = false)
            {
                Paragraph text = new Paragraph();
                text.SetFontSize(size).Add(_text);
                text.SetFont(PDFTrueTypeFont.GetFont(bold ? "Roboto-Bold" : "Roboto-Regular"));
                signature.ShowTextAligned(text, 38, 4+(y*3)+offset, TextAlignment.LEFT);
            }

            DateTime data = DateTime.Now;
            addTextLine("Documento assinado digitalmente por:", 3, 4, 12, false);
            addTextLine(dados.Nome.ToUpper(), 2, 6, 7, true);
            addTextLine($"Assinado em {data}", 1, 5, 4, false);
            addTextLine("Verifique em validar.camarapiracicaba.sp.gov.br", 0 , 4, 0, true);
            BarcodeQRCode qrcode = new BarcodeQRCode($"https://validar.camarapiracicaba.sp.gov.br/{dados.Protocolo}");
            Image qrImage = new Image(qrcode.CreateFormXObject(pdfSigner.GetDocument()));
            qrImage.SetFixedPosition(0,0);
            signature.Add(qrImage);
        }

        private static List<CertificadosAssinatura> ObterAssinaturas(PdfReader pdfReader)
        {
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            List<CertificadosAssinatura> signatures = new List<CertificadosAssinatura>();
            var signatureUtil = new SignatureUtil(pdfDocument);
            var signatureNames = signatureUtil.GetSignatureNames();
            foreach (var signatureName in signatureNames) 
            {
                var signature = signatureUtil.GetSignature(signatureName);
                var conteudo = signature.GetContents();
                var dataAssinatura = signature.GetDate().GetValue();
                byte[] signatureBytes = conteudo.GetValueBytes();
                var signedData = new SignedCms();
                signedData.Decode(signatureBytes);
                var signerInfos = signedData.SignerInfos;

                var dados = new CertificadosAssinatura
                {
                    CadeiaCertificados = new List<X509Certificate2>(),
                    CertificadoAssinante = signerInfos[0].Certificate,
                    DataAssinatura = PdfDate.Decode(dataAssinatura),
                };

                foreach (var certificate in signedData.Certificates) 
                {
                    dados.CadeiaCertificados.Add(certificate);
                }

                signatures.Add(dados);
            }

            return signatures;
        }
        public static List<CertificadosAssinatura> ObterAssinaturasDigitais(this string filePath) 
        {
            return ObterAssinaturas(new PdfReader(filePath));
        }
        public static List<CertificadosAssinatura> ObterAssinaturasDigitais(this MemoryStream memoryStream)
        {
            return ObterAssinaturas(new PdfReader(memoryStream));
        }

        public static MemoryStream AssinarPDF(
           MemoryStream sourceFile,
           string certificado,
           string senha,
           Assinatura assinatura
        )
        {
            MemoryStream stream = Assinar(
                new PdfReader(sourceFile),
                ObterCertificado(certificado, senha),
                assinatura
            );
            return AdicionaLTV(stream);
        }

        public static MemoryStream AssinarPDF(
           string sourceFile,
           string certificado,
           string senha,
           Assinatura assinatura
        )
        {
            MemoryStream stream = Assinar(
               new PdfReader(sourceFile),
               ObterCertificado(certificado, senha),
               assinatura
           );
           return AdicionaLTV(stream);
        }

    }

    public class Assinatura {
        public string Local { get; set; } = "Câmara Municipal de Piracicaba - São Paulo";
        public string Razao { get; set; } = "Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.";
        public string EmailContato { get; set; } = "desenvolvimento@camarapiracicaba.sp.gov.br";
        public string Criador { get; set; } = "Biblioteca de Assinatura Digital da Câmara Municipal de Piracicaba";
        public int Pagina { get; set; } = 1;
        public bool CarimboLateral { get; set; } = false;
        public DadosAssinatura Dados { get; set; }
    }

    public class DadosAssinatura
    {
        public string Nome { get; set; }
        public string Protocolo { get; set; }
        public List<string> Texto { get; set; }
        public PosicaoAssinatura Posicao { get; set; } = PosicaoAssinatura.FIXA_LATERAL_DIREITA;
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public bool EnderecoValidacao { get; set; } = false;
    }
    
    public class CertificadosAssinatura
    {
        public DateTime DataAssinatura { get; set; }
        public X509Certificate2 CertificadoAssinante { get; set; }
        public List<X509Certificate2> CadeiaCertificados { get; set; }
    }

}
