using iText.Barcodes;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Signatures;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Assinador
{
    public static class AssinarPDF
    {
        public static MemoryStream AdicionarAssinaturaLateral(string caminhoCertificado, string senha, string sourceFile, string texto, string qrcode)
        {
            int qtdPaginas = 0;
            using (PdfReader reader1 = new PdfReader(sourceFile))
            {
                var pdfDocument = new PdfDocument(reader1);
                qtdPaginas = pdfDocument.GetNumberOfPages();
            }

            using PdfReader reader = new PdfReader(sourceFile);
            MemoryStream retorno = AssinarInternamente(caminhoCertificado, senha, reader, texto: texto, fontSize: 7, width: 50, height: 800, rotate: 90, page: 1, x: 535, qrData: qrcode);

            if (qtdPaginas > 1)
            {
                for (int i = 2; i <= qtdPaginas; i++)
                {
                    retorno = Sign(caminhoCertificado, senha, retorno, texto: texto, fontSize: 7, width: 50, height: 800, rotate: 90, page: i, x: 535, qrcode: qrcode);
                }
            }

            retorno.Close();
            retorno.Dispose();

            return retorno;
        }

        public static MemoryStream AdicionarAssinaturaLateral(string caminhoCertificado, string senha, MemoryStream sourceFile, string texto, string qrcode)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fileName = System.IO.Path.GetTempFileName() + ".pdf";
                int bufferSize = sourceFile.ToArray().Length;
                File.WriteAllBytes(fileName, sourceFile.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                int qtdPaginas = 0;
                using (PdfReader reader1 = new PdfReader(sourceFile))
                {
                    var pdfDocument = new PdfDocument(reader1);
                    qtdPaginas = pdfDocument.GetNumberOfPages();
                }

                using PdfReader reader = new PdfReader(fs);
                MemoryStream retorno = AssinarInternamente(caminhoCertificado, senha, reader, texto: texto, fontSize: 7, width: 50, height: 800, rotate: 90, page: 1, x: 535, qrData: qrcode);

                if (qtdPaginas > 1)
                {
                    for (int i = 2; i <= qtdPaginas; i++)
                    {
                        retorno = Sign(caminhoCertificado, senha, retorno, texto: texto, fontSize: 7, width: 50, height: 800, rotate: 90, page: i, x: 535, qrcode: qrcode);
                    }
                }

                retorno.Close();
                retorno.Dispose();

                return retorno;
            }
            catch (Exception)
            {
                foreach (var fs in fileStreams)
                {
                    fs.Dispose();
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }
            finally
            {
                foreach (var fs in fileStreams)
                {
                    fs.Dispose();
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            return sourceFile;
        }

        public static MemoryStream Sign(string caminhoCertificado, string senha, MemoryStream sourceFile, int? page = null, int x = 30, int y = 30,
            DateTime? dataAssinatura = null, string texto = null, float fontSize = 9,
            float width = 200, float height = 50, int? rotate = null, string qrcode = null, bool a3 = false)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fileName = System.IO.Path.GetTempFileName() + ".pdf";
                int bufferSize = sourceFile.ToArray().Length;
                File.WriteAllBytes(fileName, sourceFile.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                using PdfReader reader = new PdfReader(fs);
                return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura, texto, fontSize, width, height, rotate, qrcode, a3);
            }
            catch (Exception)
            {
                foreach (var fs in fileStreams)
                {
                    fs.Dispose();
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }
            finally
            {
                foreach (var fs in fileStreams)
                {
                    fs.Dispose();
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            return sourceFile;
        }

        public static MemoryStream Sign(string caminhoCertificado, string senha, string sourceFile, int? page = null, int x = 30, int y = 30,
            DateTime? dataAssinatura = null, string texto = null, float fontSize = 9,
            float width = 200, float height = 50, int? rotate = null, string qrcode = null, bool a3 = false)
        {
            using PdfReader reader = new PdfReader(sourceFile);
            return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura, texto, fontSize, width, height, rotate, qrcode, a3);
        }

        private static MemoryStream AssinarInternamente(string caminhoCertificado, string senha, PdfReader reader,
            int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null, string texto = null, float fontSize = 9,
            float width = 200, float height = 50, int? rotate = null, string qrData = null, bool a3 = false)
        {
            if (!dataAssinatura.HasValue)
            {
                dataAssinatura = DateTime.Now;
            }

            string assinante = "";
            IX509Certificate[] chain;
            IExternalSignature pks;

            if (!a3)
            {
                char[] PASSWORD = senha.ToCharArray();

                Pkcs12Store pk12 = new Pkcs12Store(new FileStream(caminhoCertificado, FileMode.Open, FileAccess.Read), PASSWORD);
                string alias = null;
                foreach (object a in pk12.Aliases)
                {
                    alias = ((string)a);
                    if (pk12.IsKeyEntry(alias))
                    {
                        break;
                    }
                }

                ICipherParameters pk = pk12.GetKey(alias).Key;
                X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
                chain = new IX509Certificate[ce.Length];
                for (int k = 0; k < ce.Length; ++k)
                {
                    chain[k] = new X509CertificateBC(ce[k].Certificate);
                }
                pks = new PrivateKeySignature(new PrivateKeyBC(pk), DigestAlgorithms.SHA256);

                var dadosCertificado = pk12.GetCertificate(alias);
                var subject = dadosCertificado.Certificate.SubjectDN.GetValueList(X509Name.CN);
                assinante = subject[subject.Count - 1].ToString();
            }
            else
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2 certificate = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, caminhoCertificado, true)[0];
                var cert = new Org.BouncyCastle.X509.X509CertificateParser().ReadCertificate(certificate.GetRawCertData());
                var subject = cert.CertificateStructure.Subject.GetValueList(X509Name.CN);
                assinante = subject[subject.Count - 1].ToString();
                chain = new IX509Certificate[1];
                chain[0] = new X509CertificateBC(cert);
                pks = new X509Certificate2RSASignature(certificate);
            }

            using MemoryStream outputStream = new MemoryStream();
            PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());

            if (!page.HasValue)
            {
                page = signer.GetDocument().GetNumberOfPages();
            }

            var a4Height = 841.92d;
            var a4Width = 595.32d;

            var dimensoesPaginaQueVouAssinar = signer.GetDocument().GetPage(page.Value).GetPageSizeWithRotation();

            var proporcaoHeight = dimensoesPaginaQueVouAssinar.GetHeight() / a4Height;
            var proporcaoWidth = dimensoesPaginaQueVouAssinar.GetWidth() / a4Width;

            x = (int)Math.Round(x * proporcaoWidth, 0);
            y = (int)Math.Round(y * Math.Pow(proporcaoHeight, 5), 0);

            if (string.IsNullOrEmpty(texto))
            {
                texto = $"Assinado digitalmente por\n{assinante}";
            }

            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();

            appearance
                .SetLocation("Câmara Municipal de Piracicaba - São Paulo")
                .SetReason("Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.")
                .SetContact("desenvolvimento@camarapiracicaba.sp.gov.br")
                .SetSignatureCreator("Biblioteca de Assinatura digital Câmara Municipal de Piracicaba")
                .SetPageRect(new Rectangle(x, y, width, height))
                .SetLayer2FontSize(fontSize)
                .SetPageNumber(page.Value)
                .SetLayer2Font(ObterPdfFont.Obter());

            string signatureName = signer.GetNewSigFieldName();

            if (rotate.HasValue)
            {
                appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);

                PdfFormXObject layer2Object = appearance.GetLayer2();
                Rectangle rect = layer2Object.GetBBox().ToRectangle();
                PdfCanvas pdfCanvas = new PdfCanvas(layer2Object, signer.GetDocument());

                if (rotate == 90)
                    pdfCanvas.ConcatMatrix(0, 1, -1, 0, rect.GetWidth(), 0);
                else if (rotate == 180)
                    pdfCanvas.ConcatMatrix(-1, 0, 0, -1, rect.GetWidth(), rect.GetHeight());
                else if (rotate == 270)
                    pdfCanvas.ConcatMatrix(0, -1, 1, 0, 0, rect.GetHeight());

                Rectangle rotatedRect = 0 == (rotate / 90) % 2 ? new Rectangle(rect.GetWidth(), rect.GetHeight()) : new Rectangle(rect.GetHeight(), rect.GetWidth());
                Canvas appearanceCanvas = new Canvas(pdfCanvas, rotatedRect);

                Paragraph text = new Paragraph();
                text.SetFontSize(fontSize).Add(texto);
                text.SetFont(ObterPdfFont.Obter());

                if (!string.IsNullOrEmpty(qrData))
                {
                    text.SetFixedPosition(50, 5, height - 100);
                }
                appearanceCanvas.Add(text);

                if (!string.IsNullOrEmpty(qrData))
                {
                    BarcodeQRCode qrCode = new BarcodeQRCode(qrData);
                    qrCode.Regenerate();
                    Image qrImage = new Image(qrCode.CreateFormXObject(signer.GetDocument()));
                    qrImage.SetFixedPosition(5, 5);
                    appearanceCanvas.Add(qrImage);
                }
            }
            else
            {
                appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);
                appearance.SetLayer2Text(texto);
                appearance.SetLayer2Font(ObterPdfFont.Obter());
            }

            signer.SetFieldName(signatureName);
            signer.SetSignDate(dataAssinatura.Value);
            signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

            return outputStream;
        }
    }

    class X509Certificate2RSASignature : IExternalSignature
    {
        public X509Certificate2RSASignature(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public Org.BouncyCastle.X509.X509Certificate[] GetChain()
        {
            var bcCertificate = new Org.BouncyCastle.X509.X509Certificate(X509CertificateStructure.GetInstance(certificate.RawData));
            return new Org.BouncyCastle.X509.X509Certificate[] { bcCertificate };
        }

        public string GetSignatureAlgorithmName()
        {
            return "RSA";
        }

        public ISignatureMechanismParams GetSignatureMechanismParameters()
        {
            return null;
        }

        public string GetDigestAlgorithmName()
        {
            return "SHA512";
        }

        public byte[] Sign(byte[] message)
        {
            using RSA rsa = certificate.GetRSAPrivateKey();
            return rsa.SignData(message, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        X509Certificate2 certificate;
    }
}
