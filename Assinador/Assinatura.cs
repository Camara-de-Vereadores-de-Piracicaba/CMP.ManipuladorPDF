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
using iText.Layout.Borders;
using iText.Kernel.Colors;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {

        public enum PosicaoAssinatura
        {
            FIXA_LATERAL_DIREITA,
            FIXA_LATERAL_ESQUERDA,
            LIVRE
        }

        private static MemoryStream Assinar(
            PdfReader reader,
            Certificado certificado,
            Assinatura assinatura
        )
        {
            
            using MemoryStream outputStream = new MemoryStream();   
            PdfSigner pdfSigner = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());
            TSAClientBouncyCastle tsaClient = new TSAClientBouncyCastle("https://freetsa.org/tsr", null, null, 8192, DigestAlgorithms.SHA256);
            OcspClientBouncyCastle ocspClient = new OcspClientBouncyCastle(null);
            var crlClient = new CrlClientOnline("https://crl.cacert.org/revoke.crl");
            List<ICrlClient> crlList = new List<ICrlClient>() { crlClient };

            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            Rectangle mediaBox = pdfSigner.GetDocument().GetPage(1).GetMediaBox();
            float documentWidth = mediaBox.GetWidth();
            float documentHeight = mediaBox.GetHeight();
            
            appearance
                .SetLocation(assinatura.Local)
                .SetReason(assinatura.Razao)
                .SetContact(assinatura.EmailContato)
                .SetSignatureCreator(assinatura.Criador)
                .SetPageNumber(1);
            appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);

            Console.Write(assinatura.Dados);

            if (assinatura.Dados.Posicao == PosicaoAssinatura.FIXA_LATERAL_DIREITA)
            {
                appearance.SetPageRect(new Rectangle(documentWidth - 60, 0, 60, documentHeight));
                SetFreeSignature(pdfSigner, assinatura.Dados);
            }
            else if (assinatura.Dados.Posicao == PosicaoAssinatura.FIXA_LATERAL_ESQUERDA)
            {
                appearance.SetPageRect(new Rectangle(60, 0, 60, documentHeight));
                SetFreeSignature(pdfSigner, assinatura.Dados);
            }
            else if (assinatura.Dados.Posicao == PosicaoAssinatura.LIVRE)
            {
                appearance.SetPageRect(new Rectangle(assinatura.Dados.X, assinatura.Dados.Y, 200, 50));
                SetFreeSignature(pdfSigner, assinatura.Dados);
            }

            pdfSigner.SignDetached(
                externalSignature: certificado.PKS,
                chain: certificado.Chain,
                estimatedSize: 0,
                sigtype: PdfSigner.CryptoStandard.CMS,
                crlList: crlList,
                ocspClient: ocspClient,
                tsaClient: tsaClient
            );

            return outputStream;

        }

        private static void SetBorderSignature(PdfSigner pdfSigner, DadosAssinatura dados)
        {
            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            float documentHeight = pdfSigner.GetDocument().GetPage(1).GetMediaBox().GetHeight();
            var layer2 = appearance.GetLayer2();
            PdfCanvas canvas = new PdfCanvas(layer2, pdfSigner.GetDocument());
            PdfFont font = PdfFontFactory.CreateFont();
            Canvas signature = new Canvas(canvas, new Rectangle(0, 0, 20, documentHeight));
            int lineFloor = 42;
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
                    x: lineFloor - (_line * (fontSize + 1)),
                    y: fontSpace,
                    textAlign: TextAlignment.LEFT,
                    vertAlign: VerticalAlignment.TOP,
                    radAngle: (float)Conversion.ToRadians(90)
                );
            }

            for(int i = 1; i <= dados.Texto.Count; i++)
            {
                addTextLine(dados.Texto[i-1], i);
            }

            BarcodeQRCode qrcode = new BarcodeQRCode("https://sistemas.camarapiracicaba.sp.gov.br/digital/validar/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            Image qrImage = new Image(qrcode.CreateFormXObject(pdfSigner.GetDocument()));
            qrImage.SetFixedPosition(0, 5);
            signature.Add(qrImage);

        }

        private static void SetFreeSignature(PdfSigner pdfSigner, DadosAssinatura dados)
        {
            PdfSignatureAppearance appearance = pdfSigner.GetSignatureAppearance();
            float documentHeight = pdfSigner.GetDocument().GetPage(1).GetMediaBox().GetHeight();
            var layer2 = appearance.GetLayer2();
            PdfCanvas canvas = new PdfCanvas(layer2, pdfSigner.GetDocument());
            PdfFont font = PdfFontFactory.CreateFont();
            Canvas signature = new Canvas(canvas, new Rectangle(dados.X, dados.Y, 200, 50));
            void addTextLine(string _text, int y, int size = 8, int offset = 0, bool bold = false)
            {
                Paragraph text = new Paragraph();
                text.SetFontSize(size).Add(_text);
                text.SetFont(PDFTrueTypeFont.GetFont(bold ? "Roboto-Bold" : "Roboto-Regular"));
                signature.ShowTextAligned(text, 46, 6+(y*3)+offset, TextAlignment.LEFT);
            }
            addTextLine("Documento assinado digitalmente por:", 4, 5, 15, false);
            addTextLine(dados.Nome, 3, 6, 8, true);
            addTextLine($"Assinado em", 2, 5, 4, false);
            addTextLine("Verifique a autenticidade em:", 1, 4, 1, false);
            addTextLine("validar.camarapiracicaba.sp.gov.br", 0, 4, 0, true);
            BarcodeQRCode qrcode = new BarcodeQRCode($"https://validar.camarapiracicaba.sp.gov.br/{dados.Protocolo}");
            Image qrImage = new Image(qrcode.CreateFormXObject(pdfSigner.GetDocument()));
            qrImage.SetFixedPosition(0,0);
            signature.Add(qrImage);
        }

        public static MemoryStream AssinarPDF(
            MemoryStream sourceFile,
            string certificado,
            string senha,
            Assinatura assinatura
        )
        {
            return Assinar(
                new PdfReader(sourceFile),
                ObterCertificado(certificado,senha),
                assinatura
            );
        }

        private static List<DadosAssinaturaDTO> ObterAssinaturas(PdfReader pdfReader)
        {
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            List<DadosAssinaturaDTO> signatures = new List<DadosAssinaturaDTO>();
            var signatureUtil = new SignatureUtil(pdfDocument);
            var signatureNames = signatureUtil.GetSignatureNames();
            foreach (var signatureName in signatureNames) {
                var signature = signatureUtil.GetSignature(signatureName);
                var conteudo = signature.GetContents();
                var dataAssinatura = signature.GetDate().GetValue();
                byte[] signatureBytes = conteudo.GetValueBytes();
                var signedData = new SignedCms();
                signedData.Decode(signatureBytes);
                var signerInfos = signedData.SignerInfos;
                var dados = new DadosAssinaturaDTO {
                    CadeiaCertificados = new List<X509Certificate2>(),
                    CertificadoAssinante = signerInfos[0].Certificate,
                    DataAssinatura = PdfDate.Decode(dataAssinatura),
                };
                foreach (var certificate in signedData.Certificates) {
                    dados.CadeiaCertificados.Add(certificate);
                }
                signatures.Add(dados);
            }
            return signatures;
        }
        public static List<DadosAssinaturaDTO> ObterAssinaturasDigitais(this string filePath) 
        {
            return ObterAssinaturas(new PdfReader(filePath));
        }
        public static List<DadosAssinaturaDTO> ObterAssinaturasDigitais(this MemoryStream memoryStream)
        {
            return ObterAssinaturas(new PdfReader(memoryStream));
        }
    }

    public class Assinatura {
        public string Local { get; set; } = "Câmara Municipal de Piracicaba - São Paulo";
        public string Razao { get; set; } = "Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.";
        public string EmailContato { get; set; } = "desenvolvimento@camarapiracicaba.sp.gov.br";
        public string Criador { get; set; } = "Biblioteca de Assinatura Digital da Câmara Municipal de Piracicaba";
        public DadosAssinatura Dados { get; set; }
    }

    public class DadosAssinatura
    {
        public string Nome { get; set; }
        public string Protocolo { get; set; }
        public List<string> Texto { get; set; };
        public PosicaoAssinatura Posicao { get; set; } = PosicaoAssinatura.FIXA_LATERAL_DIREITA;
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
    }

    












    public class DadosAssinaturaDTO
    {
        public DateTime DataAssinatura { get; set; }
        public X509Certificate2 CertificadoAssinante { get; set; }
        public List<X509Certificate2> CadeiaCertificados { get; set; }
    }
    public class AssinarPDFResponse2
    {
        public MemoryStream PDFAssinado { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
    }

}
