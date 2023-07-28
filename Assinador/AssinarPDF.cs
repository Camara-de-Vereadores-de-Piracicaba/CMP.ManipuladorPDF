using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Assinador
{
    public static class AssinarPDF
    {
        public static MemoryStream Sign(string caminhoCertificado, string senha, MemoryStream sourceFile, int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null)
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
                    return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura);
                }
            }
            catch (Exception ex)
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

        public static MemoryStream Sign(string caminhoCertificado, string senha, string sourceFile, int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null)
        {
            using (PdfReader reader = new PdfReader(sourceFile))
            {
                return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura);
            }
        }

        private static MemoryStream AssinarInternamente(string caminhoCertificado, string senha, PdfReader reader, int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null)
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

            AsymmetricKeyEntry key = pk12.GetKey(alias);
            ICipherParameters pk = key.Key;

            #region Construindo o caminho do certificado do assinante até o certificado de emissor 
            X509Certificate2 subscriberCert = new X509Certificate2(caminhoCertificado, senha);
            X509Chain chain2 = new X509Chain();
            chain2.Build(subscriberCert);

            X509Certificate2Collection certificates = new X509Certificate2Collection();
            foreach (X509ChainElement element in chain2.ChainElements)
            {
                certificates.Add(element.Certificate);
            }

            List<X509Certificate> certPath = new List<X509Certificate>();
            foreach (X509Certificate2 cert in certificates)
            {
                certPath.Add(new X509CertificateParser().ReadCertificate(cert.RawData));
            }
            #endregion

            using (MemoryStream outputStream = new MemoryStream())
            {
                PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());

                if (!page.HasValue)
                {
                    page = signer.GetDocument().GetNumberOfPages();
                }

                PdfSignatureAppearance appearance = signer.GetSignatureAppearance();

                var dadosCertificado = pk12.GetCertificate(alias);

                var subject = dadosCertificado.Certificate.SubjectDN.GetValueList(X509Name.CN);

                appearance
                    .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION)
                    .SetLayer2Text($"Assinado digitalmente por\n{subject[subject.Count - 1]}")
                    .SetLocation("Câmara Municipal de Piracicaba - São Paulo")
                    .SetReason("Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.")
                    .SetContact("desenvolvimento@camarapiracicaba.sp.gov.br")
                    .SetLayer2FontSize(9)
                    .SetSignatureCreator("Biblioteca de Assinatura digital Câmara Municipal de Piracicaba")
                    .SetPageRect(new Rectangle(x, y, 200, 50))
                    .SetPageNumber(page.Value);

                signer.SetFieldName(signer.GetNewSigFieldName());
                signer.SetSignDate(dataAssinatura.Value);

                var privateKey = new PrivateKeySignature(pk, DigestAlgorithms.SHA512);

                signer.SignDetached(privateKey, certPath.ToArray(), null, null, null, 0, PdfSigner.CryptoStandard.CMS);
                return outputStream;
            }
        }
    }
}
