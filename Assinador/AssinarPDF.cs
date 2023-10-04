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

            using (PdfReader reader = new PdfReader(sourceFile))
            {
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

                using (PdfReader reader = new PdfReader(fs))
                {
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
            float width = 200, float height = 50, int? rotate = null, string qrcode = null, bool assinarTodasPaginas = false)
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

                using (PdfReader reader = new PdfReader(fs))
                {
                    return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura, texto, fontSize, width, height, rotate, qrcode);
                }
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
            float width = 200, float height = 50, int? rotate = null, string qrcode = null, bool assinarTodasPaginas = false)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                // Obtém uma coleção de certificados no armazenamento especificado.
                X509Certificate2Collection certificates = store.Certificates;

                Console.WriteLine("Certificados no armazenamento:");
                foreach (X509Certificate2 cert in certificates)
                {
                    if (cert.HasPrivateKey)
                    {
                        Console.WriteLine("Nome: " + cert.Subject);
                        Console.WriteLine("Thumbprint: " + cert.Thumbprint);
                        Console.WriteLine("====================================");
                    }
                }

                using (PdfReader reader = new PdfReader(sourceFile))
                {
                    return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura, texto, fontSize, width, height, rotate, qrcode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
            }
            finally
            {
                store.Close();
            }



            return null;
        }

        private static MemoryStream AssinarInternamente(string caminhoCertificado, string senha, PdfReader reader,
            int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null, string texto = null, float fontSize = 9,
            float width = 200, float height = 50, int? rotate = null, string qrData = null)
        {



           












            if (!dataAssinatura.HasValue)
            {
                dataAssinatura = DateTime.Now;
            }

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
            IX509Certificate[] chain = new IX509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = new X509CertificateBC(ce[k].Certificate);
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                IExternalSignature pks = new PrivateKeySignature(new PrivateKeyBC(pk), DigestAlgorithms.SHA256);
                PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());

                if (!page.HasValue)
                {
                    page = signer.GetDocument().GetNumberOfPages();
                }

                var dadosCertificado = pk12.GetCertificate(alias);

                var subject = dadosCertificado.Certificate.SubjectDN.GetValueList(X509Name.CN);

                if (string.IsNullOrEmpty(texto))
                {
                    texto = $"Assinado digitalmente por\n{subject[subject.Count - 1]}";
                }

                PdfSignatureAppearance appearance = signer.GetSignatureAppearance();

                appearance
                    .SetLocation("Câmara Municipal de Piracicaba - São Paulo")
                    .SetReason("Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.")
                    .SetContact("desenvolvimento@camarapiracicaba.sp.gov.br")
                    .SetSignatureCreator("Biblioteca de Assinatura digital Câmara Municipal de Piracicaba")
                    .SetPageRect(new Rectangle(x, y, width, height))
                    .SetLayer2FontSize(fontSize)
                    .SetPageNumber(page.Value);

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
                }

                signer.SetFieldName(signatureName);
                signer.SetSignDate(dataAssinatura.Value);
                signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

                return outputStream;
            }
        }
    }
}
